Imports MySql.Data.MySqlClient
Imports System.Data

Public Class RevisionRequestService


    Public Function SubmitRequest(request As RevisionRequest, studentId As Integer) As Boolean
        Dim sql As String = "INSERT INTO RevisionRequest (grade_id, reason, status, requested_at) " &
                           "VALUES (@GradeId, @Reason, 'Pending', NOW())"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@GradeId", request.GradeId),
            New MySqlParameter("@Reason", request.Reason)
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Student", studentId, "SUBMIT_REVISION_REQUEST", "StudentGrade", request.GradeId, request.Reason)
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[REVISION SERVICE ERROR] " & ex.Message)
            Return False
        End Try
    End Function


    Public Function ReviewRequest(requestId As Integer, status As String, teacherId As Integer) As Boolean
        Dim sql As String = "UPDATE RevisionRequest " &
                           "SET status = @Status, reviewed_at = NOW(), reviewed_by = @TeacherId " &
                           "WHERE request_id = @RequestId"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@Status", status),
            New MySqlParameter("@TeacherId", teacherId),
            New MySqlParameter("@RequestId", requestId)
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Teacher", teacherId, $"REVISION_{status.ToUpper()}", "RevisionRequest", requestId, $"Status set to {status}")
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[REVISION SERVICE ERROR] " & ex.Message)
            Return False
        End Try
    End Function


    Public Function GetPendingRequests() As DataTable
        Dim sql As String = "SELECT r.request_id, r.grade_id, a.title, sg.score, a.total_score, r.reason, r.status, r.requested_at " &
                           "FROM RevisionRequest r " &
                           "JOIN StudentGrade sg ON r.grade_id = sg.grade_id " &
                           "JOIN Assessment a ON sg.assessment_id = a.assessment_id " &
                           "WHERE r.status = 'Pending'"
        Return DatabaseHelper.ExecuteQuery(sql)
    End Function

End Class