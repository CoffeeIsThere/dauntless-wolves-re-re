Imports System.Data
Imports MySql.Data.MySqlClient

' Administrative reads return metadata only, never password hashes.
Public Class AdminWorkspaceService
    Private Shared Sub RequireAdmin(adminId As String)
        Using conn = DatabaseHelper.GetConnection()
            If Not AccountAccess.IsActive(conn, Nothing, "Admin", adminId) Then Throw New UnauthorizedAccessException("Administrator access is required.")
        End Using
    End Sub

    Public Function SetStatuses(role As String, accountIds As IEnumerable(Of String), status As String, adminId As String) As OperationResult
        If Not {"Student", "Teacher"}.Contains(role) OrElse Not {"Active", "Inactive"}.Contains(status) Then Return OperationResult.Fail("Invalid role or status.")
        If accountIds Is Nothing Then Return OperationResult.Fail("Select at least one account.")
        Dim ids = accountIds.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(Function(id) id, StringComparer.OrdinalIgnoreCase).ToList()
        If ids.Count = 0 Then Return OperationResult.Fail("Select at least one account.")
        Dim table = If(role = "Student", "student", "teacher")
        Dim key = If(role = "Student", "student_id", "teacher_id")
        Try
            Using conn = DatabaseHelper.GetConnection()
                Using tx = conn.BeginTransaction()
                    If Not AccountAccess.IsActive(conn, tx, "Admin", adminId) Then Return OperationResult.Fail("Administrator access is required.")
                    Dim changed = 0
                    For Each id In ids
                        Dim before As String
                        Using cmd As New MySqlCommand("SELECT status FROM " & table & " WHERE " & key & "=@Id FOR UPDATE", conn, tx)
                            cmd.Parameters.AddWithValue("@Id", id)
                            Dim value = cmd.ExecuteScalar()
                            If value Is Nothing Then Return OperationResult.Fail("Account " & id & " no longer exists. No statuses changed.")
                            before = CStr(value)
                        End Using
                        If before = status Then Continue For
                        Using cmd As New MySqlCommand("UPDATE " & table & " SET status=@Status WHERE " & key & "=@Id", conn, tx)
                            cmd.Parameters.AddWithValue("@Id", id)
                            cmd.Parameters.AddWithValue("@Status", status)
                            cmd.ExecuteNonQuery()
                        End Using
                        AuditService.LogActionTx(conn, tx, "Admin", adminId, "Bulk account status", role, id, before, status, "Selected-account status change")
                        changed += 1
                    Next
                    tx.Commit()
                    Return OperationResult.Ok(changed & " accounts changed to " & status & ".")
                End Using
            End Using
        Catch ex As Exception
            Return OperationResult.Fail("No statuses changed: " & ex.Message)
        End Try
    End Function

    Public Function DeletionBlockers(role As String, id As String, adminId As String) As DataTable
        RequireAdmin(adminId)
        Dim sql As String
        If role = "Student" Then
            sql = "SELECT 'Enrollments' AS Category, COUNT(*) AS Records FROM enrollment WHERE student_id=@Id UNION ALL SELECT 'Grade records', COUNT(*) FROM studentgrade g JOIN enrollment e ON e.enrollment_id=g.enrollment_id WHERE e.student_id=@Id UNION ALL SELECT 'Clarification requests', COUNT(*) FROM revisionrequest r JOIN studentgrade g ON g.grade_id=r.grade_id JOIN enrollment e ON e.enrollment_id=g.enrollment_id WHERE e.student_id=@Id UNION ALL SELECT 'Audit entries performed by this student',COUNT(*) FROM auditlog WHERE student_id=@Id"
        ElseIf role = "Teacher" Then
            sql = "SELECT 'Assigned classes',COUNT(*) FROM class WHERE teacher_id=@Id UNION ALL SELECT 'Reviewed requests',COUNT(*) FROM revisionrequest WHERE reviewed_by=@Id UNION ALL SELECT 'Audit entries performed by this professor',COUNT(*) FROM auditlog WHERE teacher_id=@Id"
        Else
            Throw New ArgumentException("Unknown role.")
        End If
        ' Column names must be stable for both UNION queries.
        If role = "Teacher" Then sql = sql.Replace("SELECT 'Assigned classes',COUNT(*)", "SELECT 'Assigned classes' AS Category,COUNT(*) AS Records")
        Return DatabaseHelper.ExecuteQuery(sql, New MySqlParameter("@Id", id))
    End Function

    Public Function AccountProfile(role As String, id As String, adminId As String) As DataTable
        RequireAdmin(adminId)
        If role = "Student" Then Return New UserService().GetStudent(id)
        If role = "Teacher" Then Return New UserService().GetTeacher(id)
        Throw New ArgumentException("Unknown role.")
    End Function

    Public Function AccountClasses(role As String, id As String, adminId As String) As DataTable
        RequireAdmin(adminId)
        Dim sql = "SELECT c.course_code AS Course, co.course_name AS Name, c.section AS Section, c.semester AS Semester, c.academic_year AS Year FROM class c JOIN course co ON co.course_code=c.course_code "
        If role = "Student" Then
            sql &= "JOIN enrollment e ON e.class_id=c.class_id WHERE e.student_id=@Id "
        ElseIf role = "Teacher" Then
            sql &= "WHERE c.teacher_id=@Id "
        Else
            Throw New ArgumentException("Unknown role.")
        End If
        Return DatabaseHelper.ExecuteQuery(sql & "ORDER BY c.academic_year DESC,c.course_code,c.section", New MySqlParameter("@Id", id))
    End Function

    Private Shared Function LikeText(value As String) As String
        Return "%" & value.Trim().Replace("!", "!!").Replace("%", "!%").Replace("_", "!_") & "%"
    End Function

    Public Function AuditRows(adminId As String, Optional fromDate As Date? = Nothing, Optional toDate As Date? = Nothing, Optional actor As String = "", Optional action As String = "", Optional search As String = "", Optional logId As Integer? = Nothing, Optional accountRole As String = Nothing, Optional accountId As String = Nothing) As DataTable
        RequireAdmin(adminId)
        If fromDate.HasValue AndAlso toDate.HasValue AndAlso fromDate.Value.Date > toDate.Value.Date Then Throw New ArgumentException("Start date must not follow end date.")
        Dim actorName = "CONVERT(COALESCE(s.full_name,t.full_name,ad.full_name,'System') USING utf8mb4) COLLATE utf8mb4_general_ci"
        Dim actorId = "CONVERT(COALESCE(al.student_id,al.teacher_id,CAST(al.admin_id AS CHAR),'') USING utf8mb4) COLLATE utf8mb4_general_ci"
        Dim sql = "SELECT al.log_id AS `Log ID`, " & actorName & " AS Actor," & actorId & " AS `Actor ID`,al.action AS Action,al.entity_type AS Entity,al.entity_id AS `Entity ID`,al.old_value AS `Before`,al.new_value AS `After`,al.logged_at AS Time,al.details AS Details FROM auditlog al LEFT JOIN student s ON s.student_id=al.student_id LEFT JOIN teacher t ON t.teacher_id=al.teacher_id LEFT JOIN admin ad ON ad.admin_id=al.admin_id WHERE 1=1"
        Dim parameters As New List(Of MySqlParameter)
        If fromDate.HasValue Then
            sql &= " AND al.logged_at>=@From"
            parameters.Add(New MySqlParameter("@From", fromDate.Value.Date))
        End If
        If toDate.HasValue Then
            sql &= " AND al.logged_at<@To"
            parameters.Add(New MySqlParameter("@To", toDate.Value.Date.AddDays(1)))
        End If
        If actor.Trim() <> "" Then
            sql &= " AND (" & actorName & " LIKE @Actor ESCAPE '!' OR " & actorId & " LIKE @Actor ESCAPE '!')"
            parameters.Add(New MySqlParameter("@Actor", LikeText(actor)))
        End If
        If action.Trim() <> "" Then
            sql &= " AND CONVERT(al.action USING utf8mb4) COLLATE utf8mb4_general_ci LIKE @Action ESCAPE '!'"
            parameters.Add(New MySqlParameter("@Action", LikeText(action)))
        End If
        If search.Trim() <> "" Then
            sql &= " AND (CONVERT(al.action USING utf8mb4) COLLATE utf8mb4_general_ci LIKE @Search ESCAPE '!' OR CONVERT(al.details USING utf8mb4) COLLATE utf8mb4_general_ci LIKE @Search ESCAPE '!' OR CONVERT(al.entity_type USING utf8mb4) COLLATE utf8mb4_general_ci LIKE @Search ESCAPE '!' OR CONVERT(al.entity_id USING utf8mb4) COLLATE utf8mb4_general_ci LIKE @Search ESCAPE '!')"
            parameters.Add(New MySqlParameter("@Search", LikeText(search)))
        End If
        If logId.HasValue Then
            sql &= " AND al.log_id=@Log"
            parameters.Add(New MySqlParameter("@Log", logId.Value))
        End If
        If accountRole IsNot Nothing Then
            If Not {"Student", "Teacher"}.Contains(accountRole) Then Throw New ArgumentException("Unknown account role.")
            sql &= " AND (al." & If(accountRole = "Student", "student_id", "teacher_id") & "=@Account OR (al.entity_type=@Role AND al.entity_id=@Account))"
            parameters.Add(New MySqlParameter("@Account", accountId))
            parameters.Add(New MySqlParameter("@Role", accountRole))
        End If
        sql &= " ORDER BY al.logged_at DESC,al.log_id DESC"
        If accountRole IsNot Nothing Then sql &= " LIMIT 25"
        Return DatabaseHelper.ExecuteQuery(sql, parameters.ToArray())
    End Function
End Class
