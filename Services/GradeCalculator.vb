' Pure grade math. No database, so it is easy to test.
Public Class GradeCalculator

    ' Passing mark: the ONE place to change it. Forms should use this constant instead of typing 75.
    Public Const PassingGrade As Double = 75.0

    Public Shared Function CalculateScorePercentage(score As Double, totalScore As Double) As Double
        If totalScore <= 0 Then Return 0.0
        Return Math.Round((score / totalScore) * 100, 2, MidpointRounding.AwayFromZero)
    End Function

    ' Kept for older callers. Statuses are worked out when displaying; do not store them in the database.
    Public Shared Function DetermineRemarks(percentage As Double) As String
        Return If(percentage >= PassingGrade, "Passed", "Needs Improvement")
    End Function

    ' Status of ONE assessment: Pending when not graded yet
    Public Shared Function GetScoreStatus(score As Double?, totalScore As Double) As String
        If Not score.HasValue Then Return "Pending"
        Return If(CalculateScorePercentage(score.Value, totalScore) >= PassingGrade, "Passed", "Needs Improvement")
    End Function

    ' Weighted running grade over graded items only:
    '   sum(score / total x weight) / sum(weight of graded items) x 100
    ' Nothing graded yet = Nothing (NULL), never 0.
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
        Return Math.Round(earned / weightUsed * 100, 2, MidpointRounding.AwayFromZero)
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