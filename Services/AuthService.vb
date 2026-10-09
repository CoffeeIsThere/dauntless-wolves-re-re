Imports MySql.Data.MySqlClient
Imports System.Data
Imports DAUNTLESS_WOLVES_PROF.Models

Public Class AuthService
    Public Function VerifyLogin(email As String, rawPassword As String) As UserModel
        Dim cleanEmail As String = ValidationHelper.CleanInput(email)
        Dim hashedPassword As String = SecurityHelper.HashSHA256(rawPassword)

        Dim sql As String = "SELECT 'Admin' AS Role, admin_id AS UserId, full_name AS FullName, email, password_hash " &
            "FROM Admin Where email = @Email " &
            "UNION ALL " &
            "SELECT 'Teacher' AS Role, teacher_id AS UserId, full_name AS FullName, email, password_hash " &
            "FROM Teacher Where email = @Email " &
            "UNION ALL " &
            "SELECT 'Student' AS Role, student_id AS UserId, full_name AS FullName, email, password_hash " &
            "FROM Student Where email = @Email "

        Dim params As MySqlParameter() = {
            New MySqlParameter("@Email", cleanEmail)
        }

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, params)

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim row As DataRow = dt.Rows(0)
            Dim dbPasswordHash As String = row("password_hash").ToString()

            If String.Equals(hashedPassword, dbPasswordHash, StringComparison.OrdinalIgnoreCase) Then
                Return New UserModel With {
                    .UserId = Convert.ToInt32(row("UserId")),
                    .FullName = row("FullName").ToString(),
                    .Role = row("Role").ToString()
                    }
            End If
        End If
        Return Nothing
    End Function


End Class
