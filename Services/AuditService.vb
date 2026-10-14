Imports MySql.Data.MySqlClient
Imports System.Data
Imports DAUNTLESS_WOLVES_PROF.Core

Public Class AuditService

    Public Shared Function LogAction(actorRole As String,
                                     actorUserId As String,
                                     action As String,
                                     entityType As String,
                                     entityId As String,
                                     Optional oldValue As String = Nothing,
                                     Optional newValue As String = Nothing,
                                     Optional details As String = Nothing) As Boolean

        Dim sql As String = "INSERT INTO auditlog (student_id, teacher_id, admin_id, action, entity_type, entity_id, old_value, new_value, details) " &
                            "VALUES (@StudentId, @TeacherId, @AdminId, @Action, @EntityType, @EntityId, @OldVal, @NewVal, @Details)"

        Dim studentIdParam As Object = If(actorRole.Equals("Student", StringComparison.OrdinalIgnoreCase), actorUserId, DBNull.Value)
        Dim teacherIdParam As Object = If(actorRole.Equals("Teacher", StringComparison.OrdinalIgnoreCase), actorUserId, DBNull.Value)
        Dim adminIdParam As Object = DBNull.Value

        If actorRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) Then
            Dim parsedAdminId As UInteger
            If UInteger.TryParse(actorUserId, parsedAdminId) Then
                adminIdParam = parsedAdminId
            End If
        End If

        Dim params As MySqlParameter() = {
            New MySqlParameter("@StudentId", studentIdParam),
            New MySqlParameter("@TeacherId", teacherIdParam),
            New MySqlParameter("@AdminId", adminIdParam),
            New MySqlParameter("@Action", action),
            New MySqlParameter("@EntityType", entityType),
            New MySqlParameter("@EntityId", entityId),
            New MySqlParameter("@OldVal", If(String.IsNullOrEmpty(oldValue), DBNull.Value, oldValue)),
            New MySqlParameter("@NewVal", If(String.IsNullOrEmpty(newValue), DBNull.Value, newValue)),
            New MySqlParameter("@Details", If(String.IsNullOrEmpty(details), DBNull.Value, details))
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            Return rowsAffected > 0
        Catch ex As Exception
            Console.WriteLine("[AUDIT LOG ERROR] " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetLogsByActor(actorRole As String, actorUserId As String) As DataTable
        Dim whereClause As String = ""

        Select Case actorRole
            Case "Student"
                whereClause = "student_id = @ActorId"
            Case "Teacher"
                whereClause = "teacher_id = @ActorId"
            Case "Admin"
                whereClause = "admin_id = @ActorId"
            Case Else
                Return New DataTable()
        End Select

        Dim sql As String = $"SELECT log_id, student_id, teacher_id, admin_id, action, entity_type, entity_id, old_value, new_value, timestamp, details " &
                            $"FROM auditlog WHERE {whereClause} " &
                            $"ORDER BY timestamp DESC"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@ActorId", actorUserId)
        }

        Return DatabaseHelper.ExecuteQuery(sql, params)
    End Function

    Public Function GetGradeHistoryForStudent(studentId As String) As DataTable
        Dim sql As String = "SELECT al.timestamp, al.action, al.old_value, al.new_value, al.details, " &
                            "a.title AS assessment_title, c.section, cr.course_name " &
                            "FROM auditlog al " &
                            "JOIN studentgrade sg ON al.entity_id = CAST(sg.grade_id AS CHAR) " &
                            "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
                            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                            "JOIN class c ON e.class_id = c.class_id " &
                            "JOIN course cr ON c.course_code = cr.course_code " &
                            "WHERE e.student_id = @StudId AND al.entity_type = 'studentgrade' " &
                            "ORDER BY al.timestamp DESC"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@StudId", studentId)
        }

        Return DatabaseHelper.ExecuteQuery(sql, params)
    End Function

End Class