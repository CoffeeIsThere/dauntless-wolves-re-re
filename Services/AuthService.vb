Imports MySql.Data.MySqlClient
Imports System.Data
Imports DAUNTLESS_WOLVES_PROF.Core
Imports DAUNTLESS_WOLVES_PROF.Models

Public Class AuthService

    Public Function VerifyLogin(inputIdOrEmail As String, rawPassword As String) As UserModel
        ' CONVERT(... USING utf8mb4) unifies character sets across latin1 and utf8 tables
        Dim sql As String = "
            SELECT 'student' AS role, 
                   CONVERT(student_id USING utf8mb4) AS user_id, 
                   CONVERT(full_name USING utf8mb4) AS full_name, 
                   CONVERT(email USING utf8mb4) AS email, 
                   CONVERT(password_hash USING utf8mb4) AS password_hash 
            FROM student 
            WHERE email = @Input OR student_id = @Input

            UNION ALL

            SELECT 'teacher' AS role, 
                   CONVERT(teacher_id USING utf8mb4) AS user_id, 
                   CONVERT(full_name USING utf8mb4) AS full_name, 
                   CONVERT(email USING utf8mb4) AS email, 
                   CONVERT(password_hash USING utf8mb4) AS password_hash 
            FROM teacher 
            WHERE email = @Input OR teacher_id = @Input

            UNION ALL

            SELECT 'admin' AS role, 
                   CONVERT(admin_id USING utf8mb4) AS user_id, 
                   CONVERT(full_name USING utf8mb4) AS full_name, 
                   CONVERT(email USING utf8mb4) AS email, 
                   CONVERT(password_hash USING utf8mb4) AS password_hash 
            FROM admin 
            WHERE email = @Input OR admin_id = @Input"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@Input", inputIdOrEmail)
        }

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, params)

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim row As DataRow = dt.Rows(0)
            Dim storedHash As String = row("password_hash").ToString()

            ' Verify password hash
            If PasswordHasher.VerifyPassword(rawPassword, storedHash) Then
                Return New UserModel() With {
                    .UserId = row("user_id").ToString(),
                    .FullName = row("full_name").ToString(),
                    .Email = row("email").ToString(),
                    .Role = row("role").ToString()
                }
            End If
        End If

        Return Nothing
    End Function

End Class