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

    Private Shared Function NullD(o As Object) As Double?
        If o Is Nothing OrElse Convert.IsDBNull(o) Then Return Nothing
        Return Convert.ToDouble(o)
    End Function

    ' overrideScore: for "re-check" requests (no points requested), the teacher inputs the corrected score
    Public Function ApproveAndApplyRevision(requestId As Integer, teacherId As String,
                                        Optional overrideScore As Double? = Nothing) As OperationResult
        Try
            Using conn As MySqlConnection = DatabaseHelper.GetConnection()
                Using tx As MySqlTransaction = conn.BeginTransaction()
                    Try
                        Dim status As String = "", reason As String = ""
                        Dim requested As Double? = Nothing, current As Double? = Nothing
                        Dim enrollmentId As Integer, assessmentId As Integer
                        Dim total As Double = 0

                        ' Lock row to read live values
                        Dim sql As String =
                        "SELECT r.status, r.requested_points, r.reason, sg.enrollment_id, sg.assessment_id, sg.score, a.total_score " &
                        "FROM revisionrequest r " &
                        "JOIN studentgrade sg ON r.grade_id = sg.grade_id " &
                        "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                        "WHERE r.request_id = @Id FOR UPDATE"

                        Using cmd As New MySqlCommand(sql, conn, tx)
                            cmd.Parameters.AddWithValue("@Id", requestId)
                            Using r As MySqlDataReader = cmd.ExecuteReader()
                                If Not r.Read() Then
                                    tx.Rollback()
                                    Return OperationResult.Fail("Request not found.")
                                End If
                                status = r.GetValue(0).ToString()
                                requested = NullD(r.GetValue(1))
                                reason = r.GetValue(2).ToString()
                                enrollmentId = Convert.ToInt32(r.GetValue(3))
                                assessmentId = Convert.ToInt32(r.GetValue(4))
                                current = NullD(r.GetValue(5))
                                total = Convert.ToDouble(r.GetValue(6))
                            End Using
                        End Using

                        ' Prevent approving twice
                        If status <> "Pending" AndAlso status <> "Under Review" Then
                            tx.Rollback()
                            Return OperationResult.Fail("This request was already " & status.ToLower() & ".")
                        End If

                        ' Calculate score addition or request override score
                        Dim newScore As Double
                        If overrideScore.HasValue Then
                            newScore = overrideScore.Value
                        ElseIf requested.HasValue AndAlso current.HasValue Then
                            newScore = Math.Min(current.Value + requested.Value, total)
                        Else
                            tx.Rollback()
                            Return OperationResult.Fail("NEEDSCORE")   ' Signal frmprof to ask professor for the score
                        End If

                        ' 1) Record score, recalculate running grade, and log score audit
                        Dim gradeRes As OperationResult = New GradeService().RecordScoreTx(
                        conn, tx, enrollmentId, assessmentId, newScore, teacherId,
                        $"Approved revision request #{requestId}: {reason}")

                        If Not gradeRes.Success Then
                            tx.Rollback()
                            Return gradeRes
                        End If

                        ' 2) Update revision request status
                        Using cmd As New MySqlCommand(
                        "UPDATE revisionrequest SET status = 'Approved', reviewed_at = NOW(), reviewed_by = @T WHERE request_id = @Id", conn, tx)
                            cmd.Parameters.AddWithValue("@T", teacherId)
                            cmd.Parameters.AddWithValue("@Id", requestId)
                            cmd.ExecuteNonQuery()
                        End Using

                        ' 3) Write request approval audit log
                        AuditService.LogActionTx(conn, tx, "Teacher", teacherId, "Updated request status",
                                             "RevisionRequest", requestId.ToString(), status, "Approved", reason)

                        tx.Commit()
                        Return OperationResult.Ok($"Request #{requestId} approved. New score: {newScore:0.##}/{total:0.##}")
                    Catch
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

    Public Function GetRevisionRequestsByStudent(studentId As String) As DataTable
        Dim sql As String = "SELECT " &
                       "    r.request_id, " &
                       "    co.course_code, " &
                       "    co.course_name, " &
                       "    c.section, " &
                       "    IFNULL(t.full_name, 'Unassigned') AS professor_name, " &
                       "    a.title AS assessment_title, " &
                       "    sg.score AS current_score, " &
                       "    a.total_score, " &
                       "    r.requested_points, " &
                       "    r.reason, " &
                       "    r.status, " &
                       "    r.requested_at, " &
                       "    r.reviewed_at " &
                       "FROM revisionrequest r " &
                       "INNER JOIN studentgrade sg ON r.grade_id = sg.grade_id " &
                       "INNER JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                       "INNER JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
                       "INNER JOIN class c ON e.class_id = c.class_id " &
                       "INNER JOIN course co ON c.course_code = co.course_code " &
                       "LEFT JOIN teacher t ON c.teacher_id = t.teacher_id " &
                       "WHERE e.student_id = @StudentId " &
                       "ORDER BY r.requested_at DESC"

        Dim params As MySqlParameter() = {
        New MySqlParameter("@StudentId", studentId)
    }

        Return DatabaseHelper.ExecuteQuery(sql, params)
    End Function


End Class