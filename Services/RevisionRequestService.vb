Imports MySql.Data.MySqlClient
Imports System.Data

Public Class RevisionRequestService

    Public Function SubmitRequest(request As RevisionRequest, studentId As String) As Boolean
        Dim sql As String = "INSERT INTO revisionrequest (grade_id, requested_points, reason, status, requested_at) " &
                            "VALUES (@GradeId, @RequestedPoints, @Reason, 'Pending', NOW())"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@GradeId", request.GradeId),
            New MySqlParameter("@RequestedPoints", If(request.RequestedPoints.HasValue, request.RequestedPoints.Value, DBNull.Value)),
            New MySqlParameter("@Reason", ValidationHelper.SanitizeInput(request.Reason))
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Student", studentId, "SUBMIT_CLARIFICATION_REQUEST", "studentgrade", request.GradeId.ToString(), details:=request.Reason)
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[REVISION SERVICE ERROR] " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ApproveAndApplyRevision(requestId As Integer, enrollmentId As Integer, assessmentId As Integer, newScore As Double, teacherId As String, reason As String) As OperationResult
        Try
            Using conn As MySqlConnection = DatabaseHelper.GetConnection()
                Using tx As MySqlTransaction = conn.BeginTransaction()
                    Try
                        ' 1. Mark Revision Request as Approved
                        Dim updateReqSql As String = "UPDATE revisionrequest " &
                                                     "SET status = 'Approved', reviewed_at = NOW(), reviewed_by = @TeacherId " &
                                                     "WHERE request_id = @RequestId"
                        Using cmd As New MySqlCommand(updateReqSql, conn, tx)
                            cmd.Parameters.AddWithValue("@TeacherId", teacherId)
                            cmd.Parameters.AddWithValue("@RequestId", requestId)
                            cmd.ExecuteNonQuery()
                        End Using

                        ' 2. Apply Grade Change and Recalculate via GradeService in same transaction
                        Dim gradeSvc As New GradeService()
                        Dim gradeRes As OperationResult = gradeSvc.RecordScoreTx(conn, tx, enrollmentId, assessmentId, newScore, teacherId, $"Approved Revision Request #{requestId}: {reason}")

                        If Not gradeRes.Success Then
                            tx.Rollback()
                            Return gradeRes
                        End If

                        tx.Commit()
                        Return OperationResult.Ok($"Request #{requestId} approved and new score ({newScore}) applied.")
                    Catch ex As Exception
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine("[REVISION APPROVAL ERROR] " & ex.Message)
            Return OperationResult.Fail("Failed to approve revision request: " & ex.Message)
        End Try
    End Function
    Public Function ReviewRequest(requestId As Integer, status As String, teacherId As String) As Boolean
        Dim sql As String = "UPDATE revisionrequest " &
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
                AuditService.LogAction("Teacher", teacherId, $"REVISION_{status.ToUpper()}", "revisionrequest", requestId.ToString(), details:=$"Status set to {status}")
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[REVISION SERVICE ERROR] " & ex.Message)
            Return False
        End Try
    End Function

    Public Function RejectRequest(requestId As Integer, teacherId As String, Optional reason As String = "") As Boolean
        Dim sql As String = "UPDATE revisionrequest " &
                            "SET status = 'Closed', reviewed_at = NOW(), reviewed_by = @TeacherId " &
                            "WHERE request_id = @RequestId"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@TeacherId", teacherId),
            New MySqlParameter("@RequestId", requestId)
        }

        Try
            If DatabaseHelper.ExecuteNonQuery(sql, params) > 0 Then
                AuditService.LogAction("Teacher", teacherId, "REJECT_REVISION_REQUEST", "revisionrequest", requestId.ToString(), details:=reason)
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[REVISION REJECT ERROR] " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetPendingRequestsByTeacher(teacherId As String) As DataTable
        Dim sql As String = "SELECT r.request_id, r.grade_id, sg.enrollment_id, sg.assessment_id, " &
                            "s.full_name AS student_name, a.title AS assessment_title, " &
                            "sg.score AS current_score, a.total_score, r.requested_points, " &
                            "r.reason, r.status, r.requested_at " &
                            "FROM revisionrequest r " &
                            "JOIN studentgrade sg ON r.grade_id = sg.grade_id " &
                            "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
                            "JOIN student s ON e.student_id = s.student_id " &
                            "JOIN class c ON e.class_id = c.class_id " &
                            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                            "WHERE c.teacher_id = @TeacherId AND r.status = 'Pending' " &
                            "ORDER BY r.requested_at DESC"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@TeacherId", teacherId)
        }

        Return DatabaseHelper.ExecuteQuery(sql, params)
    End Function

End Class