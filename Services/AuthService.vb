Imports MySql.Data.MySqlClient
Imports System.Data
Imports DAUNTLESS_WOLVES_PROF.Models

Public Class AuthService

    Public Function VerifyLogin(login As String, rawPassword As String) As UserModel
        Dim cleanLogin As String = ValidationHelper.CleanInput(login)
        If cleanLogin = "" OrElse String.IsNullOrEmpty(rawPassword) Then Return Nothing

        ' Cast IDs to CHAR and fetch department/program under 'department_or_program'
        Dim sql As String =
            "SELECT 'Admin' AS Role, CAST(admin_id AS CHAR) AS UserId, full_name AS FullName, email, password_hash, '' AS department_or_program " &
            "FROM admin WHERE email = @Login " &
            "UNION ALL " &
            "SELECT 'Teacher', CAST(teacher_id AS CHAR), full_name, email, password_hash, department " &
            "FROM teacher WHERE (email = @Login OR teacher_id = @Login) AND status = 'Active' " &
            "UNION ALL " &
            "SELECT 'Student', CAST(student_id AS CHAR), full_name, email, password_hash, program " &
            "FROM student WHERE (email = @Login OR student_id = @Login) AND status = 'Active'"

        Dim params As MySqlParameter() = {New MySqlParameter("@Login", cleanLogin)}
        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, params)

        For Each row As DataRow In dt.Rows
            If SecurityHelper.VerifyPassword(rawPassword, row("password_hash").ToString()) Then
                Dim deptOrProg As String = If(Not IsDBNull(row("department_or_program")), row("department_or_program").ToString(), "")

                Return New UserModel With {
                    .UserId = row("UserId").ToString(),
                    .FullName = row("FullName").ToString(),
                    .Email = row("email").ToString(),
                    .Role = row("Role").ToString(),
                    .DepartmentOrProgram = deptOrProg
                }
            End If
        Next

        Return Nothing
    End Function

End Class