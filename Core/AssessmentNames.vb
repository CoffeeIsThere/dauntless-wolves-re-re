Public Class AssessmentNames
    Public Shared Function NextTitle(kind As String, titles As IEnumerable(Of String)) As String
        Dim used As New HashSet(Of String)(titles.Select(Function(title) title.Trim()), StringComparer.OrdinalIgnoreCase)
        Dim number = 1
        While used.Contains(kind & " " & number)
            number += 1
        End While
        Return kind & " " & number
    End Function
End Class
