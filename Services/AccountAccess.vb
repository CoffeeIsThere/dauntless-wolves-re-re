Imports MySql.Data.MySqlClient

Public Class AccountAccess
    Public Shared Function IsActive(conn As MySqlConnection, tx As MySqlTransaction, role As String, id As String) As Boolean
        Dim table, key As String
        Select Case role
            Case "Teacher" : table = "teacher" : key = "teacher_id"
            Case "Student" : table = "student" : key = "student_id"
            Case "Admin" : table = "admin" : key = "admin_id"
            Case Else : Return False
        End Select
        Using cmd As New MySqlCommand("SELECT " & key & " FROM " & table & " WHERE " & key & "=@Id" & If(role = "Admin", "", " AND status='Active'") & " FOR UPDATE", conn, tx)
            cmd.Parameters.AddWithValue("@Id", id)
            Return cmd.ExecuteScalar() IsNot Nothing
        End Using
    End Function
End Class
