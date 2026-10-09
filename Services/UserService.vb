Imports MySql.Data.MySqlClient
Imports System.Data
Public Class UserService

    Public Function CreateTeacher(rawPassword As String, fullName As String, email As String, adminId As Integer) As Boolean
        Dim hashedPassword As String = SecurityHelper.HashSHA256(rawPassword)
        Dim sql As String = "INSERT INTO Teacher (password_hash, full_name, email)" &
            "VALUES(@Username, @PasswordHash, @Fullname, @Email)"

        Dim params As MySqlParameter() = {
        New MySqlParameter("@PasswordHash", hashedPassword),
        New MySqlParameter("@FullName", ValidationHelper.SanitizeInput(fullName)),
        New MySqlParameter("@Email", ValidationHelper.SanitizeInput(email))
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Admin", adminId, "CREATE_TEACHER", "Teacher", 0, $"Created Teacher: {email}")
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[USER SERVICE ERROR]" & ex.Message)
            Return False
        End Try
    End Function

    Public Function CreateStudent(rawPassword As String, fullname As String, email As String, adminId As Integer) As Boolean
        Dim hashedPassword As String = SecurityHelper.HashSHA256(rawPassword)
        Dim sql As String = "INSERT INTO Student (password_hash, full_name, email)" &
            "VALUES(@Username, @PasswordHash, @Fullname, @Email)"

        Dim params As MySqlParameter() = {
        New MySqlParameter("@PasswordHash", hashedPassword),
        New MySqlParameter("@FullName", ValidationHelper.SanitizeInput(fullname)),
        New MySqlParameter("@Email", ValidationHelper.SanitizeInput(email))
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Admin", adminId, "CREATE_STUDENT", "Student", 0, $"Created student: {email}")
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[USER SERVICE ERROR]" & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetAllTeachers() As DataTable
        Dim sql As String = "SELECT teacher_id, full_name, email, created_at FROM Teacher"
        Return DatabaseHelper.ExecuteQuery(sql)
    End Function

    Public Function GetAllStudents() As DataTable
        Dim sql As String = "SELECT student_id, full_name, email, created_at FROM Student"
        Return DatabaseHelper.ExecuteQuery(sql)
    End Function

    Public Function DeleteUser(role As String, userId As Integer, adminId As Integer) As Boolean
        Dim tableName As String = If(role.Equals("Teacher", StringComparison.OrdinalIgnoreCase), "Teacher", "Student")
        Dim idColumn As String = If(role.Equals("Teacher", StringComparison.OrdinalIgnoreCase), "teacher_id", "student_id")

        Dim sql As String = $"DELETE FROM {tableName} Where {idColumn} = @UserId"
        Dim params As MySqlParameter() = {
        New MySqlParameter("@UserId", userId)
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Admin", adminId, $"DELETE_{role.ToUpper()}", tableName, userId, $"Deleted {role} ID {userId}")
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[USER SERVICE ERROR]" & ex.Message)
            Return False

        End Try
    End Function
End Class