Imports MySql.Data.MySqlClient
Imports System.Data

Public Class AuditService
    Public Shared Function LogAction(actorType As String, actorId As Integer, action As String, entityType As String, entityId As Integer, Optional details As String = "") As Boolean
        Dim sql As String = "INSERT INTO AuditLog (actor_type, actor_id, action, entity_type, entity_id, timestamp, details) " &
            "VALUES (@ActorType, @ActorId, @Action, @EntityType, @EntityId, NOW(), @Details)"

        Dim params As MySqlParameter() = {
        New MySqlParameter("@ActorType", actorType),
        New MySqlParameter("@ActorId", actorId),
        New MySqlParameter("@Action", action),
        New MySqlParameter("@EntityType", entityType),
        New MySqlParameter("@EntityId", entityId),
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

    Public Function GetLogsByActor(actorType As String, actorId As Integer) As DataTable
        Dim sql As String = "SELECT log_id, action, entity_type, entity_id, timestamp, details " &
            "FROM AuditLog WHERE actor_type = @ActorType AND actor_id = @ActorId " &
            "ORDER BY timestamp DESC"

        Dim params As MySqlParameter() = {
        New MySqlParameter("@ActorType", actorType),
        New MySqlParameter("@ActorId", actorId)
        }

        Return DatabaseHelper.ExecuteQuery(sql, params)
    End Function

End Class
