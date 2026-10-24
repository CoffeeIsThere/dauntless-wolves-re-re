Imports System.Security.Cryptography
Imports System.Text

Public Class SecurityHelper

    ''' <summary>
    ''' Hashes a plain-text password string using SHA-256.
    ''' </summary>
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

    ''' <summary>
    ''' Verifies password against stored SHA-256 hash or plain-text fallback (for testing/unhashed seed data).
    ''' </summary>
    Public Shared Function VerifyPassword(rawPassword As String, storedHash As String) As Boolean
        If String.IsNullOrEmpty(rawPassword) OrElse String.IsNullOrEmpty(storedHash) Then Return False

        ' 1. Direct plain-text comparison fallback (for unhashed seed testing accounts)
        If rawPassword = storedHash Then Return True

        ' 2. SHA-256 hash comparison
        Dim inputHash As String = HashSHA256(rawPassword)
        Return String.Equals(inputHash, storedHash, StringComparison.OrdinalIgnoreCase)
    End Function

End Class