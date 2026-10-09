Imports System.Security.Cryptography
Imports System.Text

Public Class SecurityHelper

    Public Shared Function HashSHA256(input As String) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty

        Using SHA256 As SHA256 = SHA256.Create()
            Dim bytes As Byte() = SHA256.ComputeHash(Encoding.UTF8.GetBytes(input))
            Dim builder As New StringBuilder()

            For Each b As Byte In bytes
                builder.Append(b.ToString("x2"))
            Next

            Return builder.ToString()
        End Using
    End Function

End Class
