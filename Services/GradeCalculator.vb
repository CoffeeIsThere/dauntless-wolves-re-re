' Pure grade math. No database, so it is easy to test.
Public Class GradeCalculator

    ' Passing mark. Change it here if your school uses a different one.
    Public Const PassingGrade As Double = 75.0

    Public Shared Function CalculateScorePercentage(score As Double, totalScore As Double) As Double
        If totalScore <= 0 Then Return 0.0
        Return Math.Round((score / totalScore) * 100, 2)
    End Function

    Public Shared Function DetermineRemarks(percentage As Double) As String
        Return If(percentage >= PassingGrade, "Passed", "Needs Improvement")
    End Function

    ' Weighted running grade over graded items only. Nothing graded yet = Nothing.
    Public Shared Function CalculateRunningGrade(items As IEnumerable(Of ScoreItem)) As Double?
        Dim earned As Double = 0.0
        Dim weightUsed As Double = 0.0

        For Each it As ScoreItem In items
            If it.Score.HasValue AndAlso it.TotalScore > 0 AndAlso it.Weight > 0 Then
                earned += (it.Score.Value / it.TotalScore) * it.Weight
                weightUsed += it.Weight
            End If
        Next

        If weightUsed = 0 Then Return Nothing
        Return Math.Round(earned / weightUsed * 100, 2)
    End Function

    Public Shared Function GetStatus(runningGrade As Double?) As String
        If Not runningGrade.HasValue Then Return "Not Yet"
        Return If(runningGrade.Value >= PassingGrade, "Passed", "At risk")
    End Function

End Class

' One graded (or not yet graded) item, used by the calculator
Public Class ScoreItem
    Public ReadOnly Property Score As Double?
    Public ReadOnly Property TotalScore As Double
    Public ReadOnly Property Weight As Double

    Public Sub New(score As Double?, totalScore As Double, weight As Double)
        Me.Score = score
        Me.TotalScore = totalScore
        Me.Weight = weight
    End Sub
End Class