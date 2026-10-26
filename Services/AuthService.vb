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
            Dim storedHash As String = row("password_hash").ToString()

            If SecurityHelper.VerifyPassword(rawPassword, storedHash) Then
                Dim role As String = row("Role").ToString()
                Dim userId As String = row("UserId").ToString()

                ' Old SHA-256 hashes are quietly upgraded to the salted format after a good login
                If SecurityHelper.NeedsRehash(storedHash) Then
                    UpgradeHash(role, userId, rawPassword)
                End If

                Dim deptOrProg As String = If(Not IsDBNull(row("department_or_program")), row("department_or_program").ToString(), "")

                Return New UserModel With {
                    .UserId = userId,
                    .FullName = row("FullName").ToString(),
                    .Email = row("email").ToString(),
                    .Role = role,
                    .DepartmentOrProgram = deptOrProg
                }
            End If
        Next

        Return Nothing
    End Function

    ' A failed upgrade must never block the login itself
    Private Shared Sub UpgradeHash(role As String, userId As String, rawPassword As String)
        Try
            Dim tableName As String
            Dim idColumn As String

            Select Case role.Trim().ToLower()
                Case "admin"
                    tableName = "admin"
                    idColumn = "admin_id"
                Case "teacher"
                    tableName = "teacher"
                    idColumn = "teacher_id"
                Case "student"
                    tableName = "student"
                    idColumn = "student_id"
                Case Else
                    Return
            End Select

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE " & tableName & " SET password_hash = @Hash WHERE " & idColumn & " = @Id",
                New MySqlParameter("@Hash", SecurityHelper.HashPassword(rawPassword)),
                New MySqlParameter("@Id", userId))
        Catch ex As Exception
            Console.WriteLine("[AUTH] Could not upgrade password hash: " & ex.Message)
        End Try
    End Sub

    ' Change your own password (needs the current one)
    Public Function ChangePassword(role As String, userId As String, currentPassword As String, newPassword As String) As OperationResult
        If Not ValidationHelper.IsStrongPassword(newPassword) Then
            Return OperationResult.Fail("New password needs 8+ characters with an uppercase letter, a lowercase letter, a number and a symbol.")
        End If

        Dim tableName As String
        Dim idColumn As String
        Select Case role.Trim().ToLower()
            Case "admin"
                tableName = "admin"
                idColumn = "admin_id"
            Case "teacher"
                tableName = "teacher"
                idColumn = "teacher_id"
            Case "student"
                tableName = "student"
                idColumn = "student_id"
            Case Else
                Return OperationResult.Fail("Unknown role.")
        End Select

        Try
            Dim stored As Object = DatabaseHelper.ExecuteScalar(
                "SELECT password_hash FROM " & tableName & " WHERE " & idColumn & " = @Id",
                New MySqlParameter("@Id", userId))
            If stored Is Nothing OrElse Convert.IsDBNull(stored) Then Return OperationResult.Fail("Account not found.")

            If Not SecurityHelper.VerifyPassword(currentPassword, stored.ToString()) Then
                Return OperationResult.Fail("Your current password is not correct.")
            End If

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE " & tableName & " SET password_hash = @Hash WHERE " & idColumn & " = @Id",
                New MySqlParameter("@Hash", SecurityHelper.HashPassword(newPassword)),
                New MySqlParameter("@Id", userId))

            AuditService.LogAction(role, userId, "Changed password", role, userId)
            Return OperationResult.Ok("Password changed.")
        Catch ex As Exception
            Return OperationResult.Fail("Could not change the password: " & ex.Message)
        End Try
    End Function

End Class