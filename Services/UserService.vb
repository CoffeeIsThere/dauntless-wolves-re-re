Imports MySql.Data.MySqlClient
Imports System.Data
Imports DAUNTLESS_WOLVES_PROF.Core

Public Class UserService

    Public Function CreateTeacher(teacherId As String, rawPassword As String, fullName As String, email As String, department As String, adminId As String) As Boolean
        Dim hashedPassword As String = SecurityHelper.HashSHA256(rawPassword)
        Dim sql As String = "INSERT INTO teacher (teacher_id, full_name, email, password_hash, department, status) " &
                            "VALUES (@TeacherId, @FullName, @Email, @PasswordHash, @Department, 'Active')"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@TeacherId", ValidationHelper.SanitizeInput(teacherId)),
            New MySqlParameter("@FullName", ValidationHelper.SanitizeInput(fullName)),
            New MySqlParameter("@Email", ValidationHelper.SanitizeInput(email)),
            New MySqlParameter("@PasswordHash", hashedPassword),
            New MySqlParameter("@Department", ValidationHelper.SanitizeInput(department))
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Admin", adminId, "CREATE_TEACHER", "teacher", teacherId, details:=$"Created Teacher: {email}")
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[USER SERVICE ERROR] " & ex.Message)
            Return False
        End Try
    End Function

    Public Function CreateStudent(studentId As String, rawPassword As String, fullName As String, email As String, program As String, yearLevel As Integer, section As String, adminId As String) As Boolean
        Dim hashedPassword As String = SecurityHelper.HashSHA256(rawPassword)
        Dim sql As String = "INSERT INTO student (student_id, full_name, email, password_hash, program, year_level, section, status) " &
                            "VALUES (@StudentId, @FullName, @Email, @PasswordHash, @Program, @YearLevel, @Section, 'Active')"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@StudentId", ValidationHelper.SanitizeInput(studentId)),
            New MySqlParameter("@FullName", ValidationHelper.SanitizeInput(fullName)),
            New MySqlParameter("@Email", ValidationHelper.SanitizeInput(email)),
            New MySqlParameter("@PasswordHash", hashedPassword),
            New MySqlParameter("@Program", ValidationHelper.SanitizeInput(program)),
            New MySqlParameter("@YearLevel", yearLevel),
            New MySqlParameter("@Section", ValidationHelper.SanitizeInput(section))
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Admin", adminId, "CREATE_STUDENT", "student", studentId, details:=$"Created Student: {email}")
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[USER SERVICE ERROR] " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetAllTeachers() As DataTable
        Dim sql As String = "SELECT teacher_id, full_name, email, department, status, created_at FROM teacher ORDER BY full_name ASC"
        Return DatabaseHelper.ExecuteQuery(sql)
    End Function

    Public Function GetAllStudents() As DataTable
        Dim sql As String = "SELECT student_id, full_name, email, program, year_level, section, status, created_at FROM student ORDER BY full_name ASC"
        Return DatabaseHelper.ExecuteQuery(sql)
    End Function

    Public Function ToggleUserStatus(role As String, userId As String, newStatus As String, adminId As String) As Boolean
        Dim tableName As String = If(role.Equals("Teacher", StringComparison.OrdinalIgnoreCase), "teacher", "student")
        Dim idColumn As String = If(role.Equals("Teacher", StringComparison.OrdinalIgnoreCase), "teacher_id", "student_id")

        Dim sql As String = $"UPDATE {tableName} SET status = @Status WHERE {idColumn} = @UserId"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@Status", newStatus),
            New MySqlParameter("@UserId", userId)
        }

        Try
            Dim rowsAffected As Integer = DatabaseHelper.ExecuteNonQuery(sql, params)
            If rowsAffected > 0 Then
                AuditService.LogAction("Admin", adminId, $"SET_STATUS_{newStatus.ToUpper()}", tableName, userId, details:=$"Changed status of {role} {userId} to {newStatus}")
                Return True
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[USER SERVICE ERROR] " & ex.Message)
            Return False
        End Try
    End Function

End Class