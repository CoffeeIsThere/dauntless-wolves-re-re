Imports System.Security.Cryptography
Imports System.Text

Public Class PasswordHasher

    ''' <summary>
    ''' Verifies a raw password against a stored database hash or plain-text record.
    ''' </summary>
    Public Shared Function VerifyPassword(rawPassword As String, storedHash As String) As Boolean
        If String.IsNullOrWhiteSpace(storedHash) OrElse String.IsNullOrWhiteSpace(rawPassword) Then
            Return False
        End If

        ' 1. Plain-text match (for testing / unhashed seed data)
        If rawPassword = storedHash Then Return True

        ' 2. Built-in SHA256 hash verification
        Dim hashedInput As String = HashSHA256(rawPassword)
        If hashedInput.Equals(storedHash, StringComparison.OrdinalIgnoreCase) Then Return True

        Return False
    End Function

    Public Shared Function HashSHA256(input As String) As String
        Using sha256 As SHA256 = sha256.Create()
            Dim bytes As Byte() = sha256.ComputeHash(Encoding.UTF8.GetBytes(input))
            Dim sb As New StringBuilder()
            For Each b As Byte In bytes
                sb.Append(b.ToString("x2"))
            Next
            Return sb.ToString()
        End Using
    End Function

End Class