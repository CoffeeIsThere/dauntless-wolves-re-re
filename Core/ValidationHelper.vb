
Imports System.Text.RegularExpressions

Public Class ValidationHelper
    Public Shared Function IsRequired(value As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(value)
    End Function

    Public Shared Function IsValidEmail(email As String) As Boolean
        If String.IsNullOrWhiteSpace(email) Then Return False
        Dim pattern As String = "^[^@\s]+@[^@\s]+\.[^@\s]+$"
        Return Regex.IsMatch(email.Trim(), pattern, RegexOptions.IgnoreCase)
    End Function

    Public Shared Function IsStrongPassword(password As String) As Boolean
        If String.IsNullOrEmpty(password) OrElse password.Length < 8 Then Return False
        Dim hasUpper As Boolean = Regex.IsMatch(password, "[A-Z]")
        Dim hasLower As Boolean = Regex.IsMatch(password, "[a-z]")
        Dim hasNumber As Boolean = Regex.IsMatch(password, "[0-9]")
        Dim hasSpecial As Boolean = Regex.IsMatch(password, "[!@#$%^&*(),.?:{}""'|<>]")
        Return hasUpper AndAlso hasLower AndAlso hasNumber AndAlso hasSpecial
    End Function

    Public Shared Function CleanInput(input As String) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty
        Return input.Trim()


    End Function

    Public Shared Function SanitizeInput(Input As String) As String
        If String.IsNullOrWhiteSpace(Input) Then
            Return String.Empty
        End If
        Return Input.Trim()
    End Function

    Public Shared Function IsValidScore(score As Double, maxScore As Double) As Boolean
        Return score >= 0.0 AndAlso score <= maxScore
    End Function

    Public Shared Function IsValidWeight(weight As Double) As Boolean
        Return weight >= 0.0 AndAlso weight <= 100.0
    End Function
End Class
