Imports System.Security.Cryptography
Imports System.Text

Public Class SecurityHelper

    Public Shared Function HashPassword(password As String) As String
        Return HashSHA256(password)
    End Function

    Public Shared Function VerifyPassword(password As String, storedHash As String) As Boolean
        If String.IsNullOrEmpty(password) OrElse String.IsNullOrEmpty(storedHash) Then Return False
        Return String.Equals(HashSHA256(password), storedHash, StringComparison.OrdinalIgnoreCase)
    End Function

    Public Shared Function HashSHA256(input As String) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty
        Return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLower()
    End Function

End Class