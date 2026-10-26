
Imports System.Text.RegularExpressions

Public Class ValidationHelper
    Public Shared Function IsRequired(input As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(input)
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
        If String.IsNullOrWhiteSpace(input) Then Return ""
        Return input.Trim()
    End Function

    ' Alias to CleanInput to eliminate duplicate code across services
    Public Shared Function SanitizeInput(input As String) As String
        Return CleanInput(input)
    End Function
    Public Shared Function IsValidScore(score As Double, maxScore As Double) As Boolean
        Return score >= 0.0 AndAlso score <= maxScore
    End Function

    Public Shared Function IsValidWeight(weight As Double) As Boolean
        Return weight >= 0.0 AndAlso weight <= 100.0
    End Function
    Public Shared Function TryAssessmentImportRow(fields As String(), ByRef maximum As Double, ByRef weight As Double) As Boolean
        If fields Is Nothing OrElse fields.Length <> 5 Then Return False
        If Not {"Quiz", "Activity", "Exam", "Project", "Attendance"}.Contains(fields(0).Trim()) Then Return False
        If Not {"Midterm", "Finals"}.Contains(fields(3).Trim()) Then Return False
        Return Double.TryParse(fields(2), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, maximum) AndAlso
            Double.IsFinite(maximum) AndAlso maximum > 0 AndAlso maximum <= 9999.99 AndAlso
            Double.TryParse(fields(4), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, weight) AndAlso
            Double.IsFinite(weight) AndAlso weight > 0 AndAlso weight <= 100
    End Function
End Class
