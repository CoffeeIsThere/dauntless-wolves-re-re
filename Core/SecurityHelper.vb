Imports System.Security.Cryptography
Imports System.Text

' Password hashing.
'
' NEW passwords : PBKDF2-SHA256 with a random salt, stored as
'                 PBKDF2$<rounds>$<saltBase64>$<hashBase64>      (about 83 characters; the column is varchar(255))
' OLD passwords : plain SHA-256 hex (for example set with SQL SHA2('pw', 256)).
'                 They still verify, and AuthService upgrades them to the new format after a successful login.
'
' The old plain-text fallback ("typed password equals stored value") has been REMOVED on purpose:
' it let anybody log in by typing a stored hash, or the seed placeholder text.
Public Class SecurityHelper

    Private Const Prefix As String = "PBKDF2"
    Private Const CurrentRounds As Integer = 100000
    Private Const SaltBytes As Integer = 16
    Private Const HashBytes As Integer = 32

    ' Hash for NEW passwords (use this when creating or changing a password)
    Public Shared Function HashPassword(rawPassword As String) As String
        If String.IsNullOrEmpty(rawPassword) Then Return ""

        Dim salt(SaltBytes - 1) As Byte
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(salt)
        End Using

        Dim hash As Byte() = Derive(rawPassword, salt, CurrentRounds, HashBytes)
        Return Prefix & "$" & CurrentRounds.ToString() & "$" & Convert.ToBase64String(salt) & "$" & Convert.ToBase64String(hash)
    End Function

    ' Plain SHA-256 hex. Kept for old accounts and for matching SQL SHA2(..., 256).
    Public Shared Function HashSHA256(rawPassword As String) As String
        If String.IsNullOrEmpty(rawPassword) Then Return ""
        Using sha As SHA256 = SHA256.Create()
            Dim bytes As Byte() = sha.ComputeHash(Encoding.UTF8.GetBytes(rawPassword))
            Dim sb As New StringBuilder()
            For Each b As Byte In bytes
                sb.Append(b.ToString("x2"))
            Next
            Return sb.ToString()
        End Using
    End Function

    Public Shared Function VerifyPassword(rawPassword As String, storedHash As String) As Boolean
        If String.IsNullOrEmpty(rawPassword) OrElse String.IsNullOrEmpty(storedHash) Then Return False

        ' New format: PBKDF2$rounds$salt$hash
        If storedHash.StartsWith(Prefix & "$", StringComparison.Ordinal) Then
            Dim parts() As String = storedHash.Split("$"c)
            If parts.Length <> 4 Then Return False

            Dim rounds As Integer
            If Not Integer.TryParse(parts(1), rounds) OrElse rounds <= 0 Then Return False

            Try
                Dim salt As Byte() = Convert.FromBase64String(parts(2))
                Dim expected As Byte() = Convert.FromBase64String(parts(3))
                Dim actual As Byte() = Derive(rawPassword, salt, rounds, expected.Length)
                Return ConstantTimeEquals(actual, expected)
            Catch ex As FormatException
                Return False
            End Try
        End If

        ' Old format: SHA-256 hex
        Return String.Equals(HashSHA256(rawPassword), storedHash, StringComparison.OrdinalIgnoreCase)
    End Function

    ' True when the stored hash is old SHA-256 or uses fewer rounds than today
    Public Shared Function NeedsRehash(storedHash As String) As Boolean
        If String.IsNullOrEmpty(storedHash) Then Return False
        If Not storedHash.StartsWith(Prefix & "$", StringComparison.Ordinal) Then Return True

        Dim parts() As String = storedHash.Split("$"c)
        Dim rounds As Integer
        Return parts.Length = 4 AndAlso Integer.TryParse(parts(1), rounds) AndAlso rounds < CurrentRounds
    End Function

    Private Shared Function Derive(rawPassword As String, salt As Byte(), rounds As Integer, length As Integer) As Byte()
        Using kdf As New Rfc2898DeriveBytes(rawPassword, salt, rounds, HashAlgorithmName.SHA256)
            Return kdf.GetBytes(length)
        End Using
    End Function

    ' Compares every byte (no early exit), so timing does not leak information
    Private Shared Function ConstantTimeEquals(a As Byte(), b As Byte()) As Boolean
        Dim diff As Integer = a.Length Xor b.Length
        For i As Integer = 0 To Math.Min(a.Length, b.Length) - 1
            diff = diff Or (a(i) Xor b(i))
        Next
        Return diff = 0
    End Function

End Class