Imports MySql.Data.MySqlClient
Imports System.Data
Public Class GradeService

    Public Function CalculateScorePercentage(score As Double, totalScore As Double) As String
        If totalScore <= 0 Then Return 0.0
        Return Math.Round((score / totalScore) * 100, 2)
    End Function

    Public Function DetermineRemarks(percentage As Double) As String
        Return If(percentage >= 75.0, "Passed", "Needs Improvement")
    End Function

    Public Function RecordGrade(grade As StudentGrade, teacherId As Integer) As Boolean
        Dim sql As String = "INSERT INTO StudentGrade (enrollment_id, assessment_id, score, remarks, created_at) " &
            "VALUES (@EnrollmentId, @AssessmentId, @Score, @Remarks, NOW())"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@EnrollmentId", grade.EnrollmentId),
            New MySqlParameter("@AssessmentId", grade.AssessmentId),
            New MySqlParameter("@Score", grade.Score),
            New MySqlParameter("@Remarks", grade.Remarks)
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Teacher", teacherId, "RECORD_GRADE", "STUDENT_GRADE", grade.EnrollmentId, $"Score: {grade.Score}")
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[GRADE SERVICE ERROR] " & ex.Message)
            Return False


        End Try
    End Function

    Public Function GetGradesByEnrollment(enrollmentId As Integer) As DataTable
        Dim sql As String = "SELECT sg.grade_id, a.title, a.type, sg.score, a.total_score, sg.remarks, sg.created_at " &
            "FROM StudentGrade sg " &
            "JOIN Assessment a ON sg.assessment_id = a.assessment_id " &
            "WHERE sg.enrollment_id = @EnrollmentId"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@EnrollmentId", enrollmentId)
            }
        Return DatabaseHelper.ExecuteQuery(sql, params)
    End Function

End Class
