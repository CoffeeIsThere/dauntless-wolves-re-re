Imports MySql.Data.MySqlClient
Imports System.Data

Public Class AuditService

    ' Standalone: opens its own connection
    Public Shared Function LogAction(actorRole As String, actorUserId As String, action As String,
                                     entityType As String, entityId As String,
                                     Optional oldValue As String = Nothing,
                                     Optional newValue As String = Nothing,
                                     Optional details As String = Nothing) As Boolean
        Try
            Using conn As MySqlConnection = DatabaseHelper.GetConnection()
                Return InsertLog(conn, Nothing, actorRole, actorUserId, action, entityType, entityId, oldValue, newValue, details)
            End Using
        Catch ex As Exception
            Console.WriteLine("[AUDIT LOG ERROR] " & ex.Message)
            Return False
        End Try
    End Function

    ' Inside an existing transaction (so the log rolls back together with the grade change)
    Public Shared Function LogActionTx(conn As MySqlConnection, tx As MySqlTransaction,
                                       actorRole As String, actorUserId As String, action As String,
                                       entityType As String, entityId As String,
                                       Optional oldValue As String = Nothing,
                                       Optional newValue As String = Nothing,
                                       Optional details As String = Nothing) As Boolean
        Return InsertLog(conn, tx, actorRole, actorUserId, action, entityType, entityId, oldValue, newValue, details)
    End Function

    Private Shared Function InsertLog(conn As MySqlConnection, tx As MySqlTransaction,
                                      actorRole As String, actorUserId As String, action As String,
                                      entityType As String, entityId As String,
                                      oldValue As String, newValue As String, details As String) As Boolean
        Dim studentId As Object = DBNull.Value
        Dim teacherId As Object = DBNull.Value
        Dim adminId As Object = DBNull.Value

        Select Case actorRole.Trim().ToLower()
            Case "student"
                studentId = actorUserId
            Case "teacher"
                teacherId = actorUserId
            Case "admin"
                Dim parsed As Integer
                If Not Integer.TryParse(actorUserId, parsed) Then
                    Throw New ArgumentException("Admin id must be numeric.")
                End If
                adminId = parsed
            Case Else
                Throw New ArgumentException("Unknown actor role: " & actorRole)
        End Select

        Dim sql As String =
            "INSERT INTO auditlog (student_id, teacher_id, admin_id, action, entity_type, entity_id, old_value, new_value, details) " &
            "VALUES (@StudentId, @TeacherId, @AdminId, @Action, @EntityType, @EntityId, @OldVal, @NewVal, @Details)"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@StudentId", studentId)
            cmd.Parameters.AddWithValue("@TeacherId", teacherId)
            cmd.Parameters.AddWithValue("@AdminId", adminId)
            cmd.Parameters.AddWithValue("@Action", Clip(action, 100))
            cmd.Parameters.AddWithValue("@EntityType", Clip(entityType, 50))
            cmd.Parameters.AddWithValue("@EntityId", Clip(entityId, 20))
            cmd.Parameters.AddWithValue("@OldVal", OrNull(oldValue, 100))
            cmd.Parameters.AddWithValue("@NewVal", OrNull(newValue, 100))
            cmd.Parameters.AddWithValue("@Details", OrNull(details, 500))
            Return cmd.ExecuteNonQuery() > 0
        End Using
    End Function

    ' Trim to the column length so the insert doesn't fail with "Data too long"
    Private Shared Function Clip(s As String, maxLen As Integer) As String
        If s Is Nothing Then Return ""
        Return If(s.Length > maxLen, s.Substring(0, maxLen), s)
    End Function

    Private Shared Function OrNull(s As String, maxLen As Integer) As Object
        If String.IsNullOrEmpty(s) Then Return DBNull.Value
        Return Clip(s, maxLen)
    End Function

    Public Shared Function GetLogsByActor(actorRole As String, actorUserId As String) As DataTable
        Dim col As String
        Select Case actorRole.Trim().ToLower()
            Case "student" : col = "student_id"
            Case "teacher" : col = "teacher_id"
            Case "admin" : col = "admin_id"
            Case Else : Return New DataTable()
        End Select

        Dim sql As String =
            "SELECT log_id, student_id, teacher_id, admin_id, action, entity_type, entity_id, old_value, new_value, logged_at, details " &
            "FROM auditlog WHERE " & col & " = @ActorId ORDER BY logged_at DESC"

        Return DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@ActorId", actorUserId)})
    End Function

End Class