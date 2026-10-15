Imports MySql.Data.MySqlClient
Imports System.Data
Imports DAUNTLESS_WOLVES_PROF.Models

Public Class AuthService

    Public Function VerifyLogin(login As String, rawPassword As String) As UserModel
        Dim cleanLogin As String = ValidationHelper.CleanInput(login)
        If cleanLogin = "" OrElse String.IsNullOrEmpty(rawPassword) Then Return Nothing

        ' Cast IDs to CHAR to prevent MySQL UNION data type mismatch conflicts
        Dim sql As String =
            "SELECT 'Admin' AS Role, CAST(admin_id AS CHAR) AS UserId, full_name AS FullName, email, password_hash " &
            "FROM admin WHERE email = @Login " &
            "UNION ALL " &
            "SELECT 'Teacher', CAST(teacher_id AS CHAR), full_name, email, password_hash " &
            "FROM teacher WHERE (email = @Login OR teacher_id = @Login) AND status = 'Active' " &
            "UNION ALL " &
            "SELECT 'Student', CAST(student_id AS CHAR), full_name, email, password_hash " &
            "FROM student WHERE (email = @Login OR student_id = @Login) AND status = 'Active'"

        Dim params As MySqlParameter() = {New MySqlParameter("@Login", cleanLogin)}
        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, params)

        For Each row As DataRow In dt.Rows
            If SecurityHelper.VerifyPassword(rawPassword, row("password_hash").ToString()) Then
                Return New UserModel With {
                    .UserId = row("UserId").ToString(),
                    .FullName = row("FullName").ToString(),
                    .Email = row("email").ToString(),
                    .Role = row("Role").ToString()
                }
            End If
        Next

        Return Nothing
    End Function

End Class