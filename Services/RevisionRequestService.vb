Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Globalization
Imports DAUNTLESS_WOLVES_PROF.Models

' Revision-request life cycle (statuses come from the database ENUM):
'     Pending  ->  Under Review (optional)  ->  Approved   (the new score is applied)
'                                           ->  Closed     (rejected; the reason goes to the audit log)
'
' Every action runs in ONE transaction together with its audit row, checks that the request belongs to the
' right student / teacher, and refuses to touch a request that is already Approved or Closed.
Public Class RevisionRequestService

    Public Const StatusPending As String = "Pending"
    Public Const StatusUnderReview As String = "Under Review"
    Public Const StatusApproved As String = "Approved"
    Public Const StatusClosed As String = "Closed"

    Private ReadOnly _gradeService As New GradeService()

    ' =========================================================
    '  STUDENT SIDE
    ' =========================================================

    Public Function SubmitRequest(request As RevisionRequest, studentId As String) As Boolean
        Return SubmitRequestResult(request, studentId).Success
    End Function

    Public Function SubmitRequestResult(request As RevisionRequest, studentId As String) As OperationResult
        If request Is Nothing Then Return OperationResult.Fail("No request was given.")

        Dim reason As String = ValidationHelper.CleanInput(request.Reason)
        If reason = "" Then Return OperationResult.Fail("Please explain why you are asking for a revision.")
        If reason.Length > 255 Then Return OperationResult.Fail("The reason can have at most 255 characters (now " & reason.Length.ToString() & ").")

        Return InTransaction(Function(c, t) SubmitTx(c, t, request, reason, studentId))
    End Function

    Private Shared Function SubmitTx(conn As MySqlConnection, tx As MySqlTransaction,
                                     request As RevisionRequest, reason As String, studentId As String) As OperationResult
        ' 1) The grade must belong to THIS student and must already have a score
        Dim score As Double? = Nothing
        Dim total As Double = 0
        Dim title As String = ""

        Dim gradeSql As String =
            "SELECT sg.score, a.total_score, a.title " &
            "FROM studentgrade sg " &
            "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
            "WHERE sg.grade_id = @GradeId AND e.student_id = @StudentId"
        Using cmd As New MySqlCommand(gradeSql, conn, tx)
            cmd.Parameters.AddWithValue("@GradeId", request.GradeId)
            cmd.Parameters.AddWithValue("@StudentId", studentId)
            Using r As MySqlDataReader = cmd.ExecuteReader()
                If Not r.Read() Then Return OperationResult.Fail("That grade does not belong to you.")
                score = ToNullableDouble(r.GetValue(0))
                total = Convert.ToDouble(r.GetValue(1))
                title = r.GetValue(2).ToString()
            End Using
        End Using

        If Not score.HasValue Then Return OperationResult.Fail("That assessment has not been graded yet.")

        ' 2) Requested points must make sense (empty = plain re-check)
        If request.RequestedPoints.HasValue Then
            If request.RequestedPoints.Value <= 0 Then
                Return OperationResult.Fail("Requested points must be more than 0, or leave it empty to ask for a re-check.")
            End If
            If score.Value + request.RequestedPoints.Value > total Then
                Return OperationResult.Fail("That would go above the maximum score of " & Fmt(total) & ".")
            End If
        End If

        ' 3) Only one open request per grade
        Using cmd As New MySqlCommand("SELECT COUNT(*) FROM revisionrequest WHERE grade_id = @GradeId AND status IN ('Pending','Under Review')", conn, tx)
            cmd.Parameters.AddWithValue("@GradeId", request.GradeId)
            If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                Return OperationResult.Fail("You already have an open request for this assessment.")
            End If
        End Using

        ' 4) Save it
        Dim newId As Long
        Dim insertSql As String =
            "INSERT INTO revisionrequest (grade_id, requested_points, reason, status) " &
            "VALUES (@GradeId, @RequestedPoints, @Reason, 'Pending')"
        Using cmd As New MySqlCommand(insertSql, conn, tx)
            cmd.Parameters.AddWithValue("@GradeId", request.GradeId)
            cmd.Parameters.AddWithValue("@RequestedPoints", If(request.RequestedPoints.HasValue, CObj(request.RequestedPoints.Value), DBNull.Value))
            cmd.Parameters.AddWithValue("@Reason", reason)
            cmd.ExecuteNonQuery()
            newId = cmd.LastInsertedId
        End Using

        ' 5) Audit row in the same style as the seed data: entity = the REQUEST (never the grade)
        Dim askText As String = If(request.RequestedPoints.HasValue, "+" & Fmt(request.RequestedPoints.Value) & " pts", "re-check")
        AuditService.LogActionTx(conn, tx, "Student", studentId, "Submitted revision request",
                                 "RevisionRequest", newId.ToString(), Nothing, StatusPending, title & ": " & askText)

        Return OperationResult.Ok("Request submitted.")
    End Function

    ' "My Request Status" table.
    ' Columns: request_id, grade_id, assessment_title, current_score, total_score, requested_points, reason,
    '          status, requested_at, reviewed_at
    Public Function GetRevisionRequestsByStudent(studentId As String) As DataTable
        Dim sql As String =
            "SELECT r.request_id, r.grade_id, a.title AS assessment_title, sg.score AS current_score, a.total_score, " &
            "       r.requested_points, r.reason, r.status, r.requested_at, r.reviewed_at, " &
            "       (SELECT al.details FROM auditlog al WHERE al.entity_type = 'RevisionRequest' " &
            "        AND CAST(al.entity_id AS BINARY) = CAST(CAST(r.request_id AS CHAR) AS BINARY) AND CAST(al.new_value AS BINARY) = CAST(r.status AS BINARY) " &
            "        AND CAST(al.teacher_id AS BINARY) = CAST(r.reviewed_by AS BINARY) AND al.action = 'Updated request status' " &
            "        ORDER BY al.log_id DESC LIMIT 1) AS reviewer_response " &
            "FROM revisionrequest r " &
            "JOIN studentgrade sg ON r.grade_id = sg.grade_id " &
            "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
            "WHERE e.student_id = @StudentId " &
            "ORDER BY r.requested_at DESC, r.request_id DESC"
        Return DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@StudentId", studentId)})
    End Function

    ' =========================================================
    '  TEACHER SIDE (lists)
    ' =========================================================

    ' Open requests (Pending and Under Review) of this teacher's classes
    Public Function GetPendingRequestsByTeacher(teacherId As String) As DataTable
        Return GetRequestsByTeacher(teacherId, "Open")
    End Function

    ' statusFilter: "Open" (Pending + Under Review, default), "All", or one exact status such as "Approved".
    ' Columns: request_id, grade_id, enrollment_id, assessment_id, student_id, student_name, assessment_title,
    '          current_score, total_score, requested_points, reason, status, requested_at, reviewed_at
    Public Function GetRequestsByTeacher(teacherId As String, Optional statusFilter As String = "Open") As DataTable
        Dim sql As String =
            "SELECT r.request_id, r.grade_id, sg.enrollment_id, sg.assessment_id, s.student_id, s.full_name AS student_name, " &
            "       c.class_id, CONCAT(c.course_code, ' - ', c.section) AS class_label, a.title AS assessment_title, sg.score AS current_score, a.total_score, " &
            "       r.requested_points, r.reason, r.status, r.requested_at, r.reviewed_at " &
            "FROM revisionrequest r " &
            "JOIN studentgrade sg ON r.grade_id = sg.grade_id " &
            "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
            "JOIN student s ON e.student_id = s.student_id " &
            "JOIN class c ON e.class_id = c.class_id " &
            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
            "WHERE c.teacher_id = @TeacherId"

        Dim params As New List(Of MySqlParameter) From {New MySqlParameter("@TeacherId", teacherId)}

        If String.IsNullOrEmpty(statusFilter) OrElse statusFilter.Equals("Open", StringComparison.OrdinalIgnoreCase) Then
            sql &= " AND r.status IN ('Pending','Under Review')"
        ElseIf Not statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase) Then
            sql &= " AND r.status = @Status"
            params.Add(New MySqlParameter("@Status", statusFilter))
        End If

        sql &= " ORDER BY FIELD(r.status, 'Pending', 'Under Review', 'Approved', 'Closed'), r.requested_at DESC, r.request_id DESC"
        Return DatabaseHelper.ExecuteQuery(sql, params.ToArray())
    End Function

    ' =========================================================
    '  TEACHER SIDE (actions)
    ' =========================================================

    ' Approve and apply the new score.
    '   overrideScore given   -> that exact score is saved (needed for plain re-check requests)
    '   overrideScore Nothing -> current score + requested points, capped at the maximum
    ' When a re-check request needs a typed score the result is Fail with Message = "NEEDSCORE".
    ' Score, request status and audit rows all succeed or all fail together.
    Public Function ApproveAndApplyRevision(requestId As Integer, teacherId As String,
                                            Optional overrideScore As Double? = Nothing, Optional reviewerResponse As String = Nothing) As OperationResult
        If reviewerResponse IsNot Nothing AndAlso reviewerResponse.Length > 350 Then Return OperationResult.Fail("Reviewer response must be at most 350 characters.")
        Return InTransaction(Function(c, t) ApproveTx(c, t, requestId, teacherId, overrideScore, reviewerResponse))
    End Function

    Public Function RejectRequest(requestId As Integer, teacherId As String, Optional reason As String = "") As Boolean
        Return RejectRequestResult(requestId, teacherId, reason).Success
    End Function

    ' "Rejected" is stored as Closed (the database has no Rejected status). The reason is kept in the audit log.
    Public Function RejectRequestResult(requestId As Integer, teacherId As String, Optional reason As String = "") As OperationResult
        Return InTransaction(Function(c, t) ChangeStatusTx(c, t, requestId, teacherId, StatusClosed, reason))
    End Function

    Public Function ReviewRequest(requestId As Integer, status As String, teacherId As String) As Boolean
        Return ReviewRequestResult(requestId, status, teacherId).Success
    End Function

    ' Only "Under Review" and "Closed" go through here. Approving must use ApproveAndApplyRevision so the score is applied.
    Public Function ReviewRequestResult(requestId As Integer, status As String, teacherId As String) As OperationResult
        Dim target As String

        If String.Equals(status, StatusApproved, StringComparison.OrdinalIgnoreCase) Then
            Return OperationResult.Fail("Use ApproveAndApplyRevision to approve, so the new score is applied.")
        ElseIf String.Equals(status, StatusUnderReview, StringComparison.OrdinalIgnoreCase) Then
            target = StatusUnderReview
        ElseIf String.Equals(status, StatusClosed, StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase) Then
            target = StatusClosed
        Else
            Return OperationResult.Fail("Status must be Under Review or Closed.")
        End If

        Return InTransaction(Function(c, t) ChangeStatusTx(c, t, requestId, teacherId, target, Nothing))
    End Function

    ' =========================================================
    '  INTERNALS
    ' =========================================================

    Private Class RequestInfo
        Public GradeId As Integer
        Public Status As String
        Public RequestedPoints As Double?
        Public EnrollmentId As Integer
        Public AssessmentId As Integer
        Public CurrentScore As Double?
        Public TotalScore As Double
        Public OwnerTeacherId As String
    End Class

    ' Loads the request together with who owns its class, and locks the row.
    Private Shared Function LoadRequest(conn As MySqlConnection, tx As MySqlTransaction, requestId As Integer) As RequestInfo
        Dim sql As String =
            "SELECT r.grade_id, r.status, r.requested_points, sg.enrollment_id, sg.assessment_id, " &
            "       sg.score, a.total_score, c.teacher_id " &
            "FROM revisionrequest r " &
            "JOIN studentgrade sg ON r.grade_id = sg.grade_id " &
            "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
            "JOIN class c ON e.class_id = c.class_id " &
            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
            "WHERE r.request_id = @RequestId " &
            "FOR UPDATE"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@RequestId", requestId)
            Using r As MySqlDataReader = cmd.ExecuteReader()
                If Not r.Read() Then Return Nothing

                Dim info As New RequestInfo()
                info.GradeId = Convert.ToInt32(r.GetValue(0))
                info.Status = r.GetValue(1).ToString()
                info.RequestedPoints = ToNullableDouble(r.GetValue(2))
                info.EnrollmentId = Convert.ToInt32(r.GetValue(3))
                info.AssessmentId = Convert.ToInt32(r.GetValue(4))
                info.CurrentScore = ToNullableDouble(r.GetValue(5))
                info.TotalScore = Convert.ToDouble(r.GetValue(6))
                info.OwnerTeacherId = r.GetValue(7).ToString()
                Return info
            End Using
        End Using
    End Function

    ' The request must exist, belong to this teacher's class, and still be open.
    Private Shared Function CheckOpenAndOwned(info As RequestInfo, teacherId As String) As OperationResult
        If info Is Nothing Then
            Return OperationResult.Fail("Request not found.")
        End If
        If Not String.Equals(info.OwnerTeacherId, teacherId, StringComparison.OrdinalIgnoreCase) Then
            Return OperationResult.Fail("You can only review requests for your own classes.")
        End If
        If info.Status <> StatusPending AndAlso info.Status <> StatusUnderReview Then
            Return OperationResult.Fail("This request is already " & info.Status & ".")
        End If
        Return OperationResult.Ok()
    End Function

    Private Function ApproveTx(conn As MySqlConnection, tx As MySqlTransaction,
                               requestId As Integer, teacherId As String,
                               overrideScore As Double?, reviewerResponse As String) As OperationResult
        Dim info As RequestInfo = LoadRequest(conn, tx, requestId)
        Dim check As OperationResult = CheckOpenAndOwned(info, teacherId)
        If Not check.Success Then Return check

        Dim target As Double
        If overrideScore.HasValue Then
            target = overrideScore.Value
        ElseIf info.RequestedPoints.HasValue AndAlso info.CurrentScore.HasValue Then
            target = Math.Min(info.CurrentScore.Value + info.RequestedPoints.Value, info.TotalScore)
        Else
            Return OperationResult.Fail("NEEDSCORE")
        End If

        ' Mark the request approved (only if it is still open)
        Dim rows As Integer
        Dim updateSql As String =
            "UPDATE revisionrequest SET status = 'Approved', reviewed_at = NOW(), reviewed_by = @TeacherId " &
            "WHERE request_id = @RequestId AND status IN ('Pending','Under Review')"
        Using cmd As New MySqlCommand(updateSql, conn, tx)
            cmd.Parameters.AddWithValue("@TeacherId", teacherId)
            cmd.Parameters.AddWithValue("@RequestId", requestId)
            rows = cmd.ExecuteNonQuery()
        End Using
        If rows <= 0 Then Return OperationResult.Fail("The request could not be updated.")

        ' Apply the score through the same code path as normal grading (ownership, range, running grade, audit)
        Dim gradeRes As OperationResult = _gradeService.RecordScoreTx(conn, tx, info.EnrollmentId, info.AssessmentId, target, teacherId,
                                                                      "Approved revision request #" & requestId.ToString())
        If Not gradeRes.Success Then Return gradeRes

        ' Log the decision itself too (the score change was logged by RecordScoreTx)
        AuditService.LogActionTx(conn, tx, "Teacher", teacherId, "Updated request status", "RevisionRequest",
                                 requestId.ToString(), info.Status, StatusApproved, "Score set to " & Fmt(target) & "/" & Fmt(info.TotalScore) & If(String.IsNullOrWhiteSpace(reviewerResponse), "", vbCrLf & reviewerResponse.Trim()))

        Return OperationResult.Ok("Request #" & requestId.ToString() & " approved. Score is now " & Fmt(target) & "/" & Fmt(info.TotalScore) & ".")
    End Function

    ' Under Review or Closed
    Private Shared Function ChangeStatusTx(conn As MySqlConnection, tx As MySqlTransaction,
                                           requestId As Integer, teacherId As String,
                                           newStatus As String, note As String) As OperationResult
        Dim info As RequestInfo = LoadRequest(conn, tx, requestId)
        Dim check As OperationResult = CheckOpenAndOwned(info, teacherId)
        If Not check.Success Then Return check

        If newStatus = StatusUnderReview AndAlso info.Status = StatusUnderReview Then
            Return OperationResult.Fail("This request is already under review.")
        End If

        Dim closing As Boolean = (newStatus = StatusClosed)
        Dim sql As String
        If closing Then
            sql = "UPDATE revisionrequest SET status = @Status, reviewed_at = NOW(), reviewed_by = @TeacherId " &
                  "WHERE request_id = @RequestId AND status IN ('Pending','Under Review')"
        Else
            sql = "UPDATE revisionrequest SET status = @Status " &
                  "WHERE request_id = @RequestId AND status IN ('Pending','Under Review')"
        End If

        Dim rows As Integer
        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@Status", newStatus)
            cmd.Parameters.AddWithValue("@RequestId", requestId)
            If closing Then cmd.Parameters.AddWithValue("@TeacherId", teacherId)
            rows = cmd.ExecuteNonQuery()
        End Using
        If rows <= 0 Then Return OperationResult.Fail("The request could not be updated.")

        AuditService.LogActionTx(conn, tx, "Teacher", teacherId, "Updated request status", "RevisionRequest",
                                 requestId.ToString(), info.Status, newStatus, note)

        Return OperationResult.Ok("Request #" & requestId.ToString() & " is now " & newStatus & ".")
    End Function

    ' Runs one unit of work in a transaction: commit when it succeeds, roll back when it fails or throws.
    Private Shared Function InTransaction(work As Func(Of MySqlConnection, MySqlTransaction, OperationResult)) As OperationResult
        Try
            Using conn As MySqlConnection = DatabaseHelper.GetConnection()
                Using tx As MySqlTransaction = conn.BeginTransaction()
                    Try
                        Dim res As OperationResult = work(conn, tx)
                        If res.Success Then tx.Commit() Else tx.Rollback()
                        Return res
                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine("[REVISION SERVICE ERROR] " & ex.Message)
            Return OperationResult.Fail("Database error: " & ex.Message)
        End Try
    End Function

    Private Shared Function ToNullableDouble(o As Object) As Double?
        If o Is Nothing OrElse Convert.IsDBNull(o) Then Return Nothing
        Return Convert.ToDouble(o)
    End Function

    Private Shared Function Fmt(value As Double) As String
        Return value.ToString("0.##", CultureInfo.InvariantCulture)
    End Function

End Class