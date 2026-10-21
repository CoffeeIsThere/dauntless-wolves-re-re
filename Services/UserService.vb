Imports MySql.Data.MySqlClient
Imports System.Data


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

    Public Function GetAllTeachers(Optional searchQuery As String = "", Optional department As String = "All") As DataTable
        Dim sql As String =
        "SELECT t.teacher_id, t.full_name, t.email, t.department, t.employment_type, t.status, " &
        "(SELECT COUNT(*) FROM class c WHERE c.teacher_id = t.teacher_id) AS class_count " &
        "FROM teacher t WHERE 1=1"
        Dim params As New List(Of MySqlParameter)()

        If Not String.IsNullOrWhiteSpace(searchQuery) Then
            sql &= " AND (LOWER(t.full_name) LIKE @Search OR LOWER(t.teacher_id) LIKE @Search OR LOWER(t.email) LIKE @Search)"
            params.Add(New MySqlParameter("@Search", "%" & searchQuery.Trim().ToLower() & "%"))
        End If

        If Not String.IsNullOrWhiteSpace(department) AndAlso Not department.Equals("All", StringComparison.OrdinalIgnoreCase) Then
            sql &= " AND t.department = @Dept"
            params.Add(New MySqlParameter("@Dept", department.Trim()))
        End If

        sql &= " ORDER BY t.full_name ASC"
        Return DatabaseHelper.ExecuteQuery(sql, params.ToArray())
    End Function

    Public Function GetAllStudents(Optional searchQuery As String = "", Optional yearLevel As String = "All", Optional section As String = "All") As DataTable
        Dim sql As String = "SELECT student_id, full_name, email, program, year_level, section, status FROM student WHERE 1=1"
        Dim params As New List(Of MySqlParameter)()

        If Not String.IsNullOrWhiteSpace(searchQuery) Then
            sql &= " AND (LOWER(full_name) LIKE @Search OR LOWER(student_id) LIKE @Search OR LOWER(email) LIKE @Search)"
            params.Add(New MySqlParameter("@Search", "%" & searchQuery.Trim().ToLower() & "%"))
        End If

        Dim yr As Integer
        If Not String.IsNullOrWhiteSpace(yearLevel) AndAlso Not yearLevel.Equals("All", StringComparison.OrdinalIgnoreCase) _
       AndAlso Integer.TryParse(yearLevel, yr) Then
            sql &= " AND year_level = @Yr"
            params.Add(New MySqlParameter("@Yr", yr))
        End If

        If Not String.IsNullOrWhiteSpace(section) AndAlso Not section.Equals("All", StringComparison.OrdinalIgnoreCase) Then
            sql &= " AND section = @Sec"
            params.Add(New MySqlParameter("@Sec", section.Trim()))
        End If

        sql &= " ORDER BY full_name ASC"
        Return DatabaseHelper.ExecuteQuery(sql, params.ToArray())
    End Function

    Public Function UpdateTeacher(teacherId As String, fullName As String, email As String, department As String, adminId As String) As Boolean
        Dim sql As String = "UPDATE teacher SET full_name = @FullName, email = @Email, department = @Department WHERE teacher_id = @TeacherId"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@FullName", ValidationHelper.SanitizeInput(fullName)),
            New MySqlParameter("@Email", ValidationHelper.SanitizeInput(email)),
            New MySqlParameter("@Department", ValidationHelper.SanitizeInput(department)),
            New MySqlParameter("@TeacherId", teacherId)
        }

        If DatabaseHelper.ExecuteNonQuery(sql, params) > 0 Then
            AuditService.LogAction("Admin", adminId, "UPDATE_TEACHER", "teacher", teacherId, details:=$"Updated details for {teacherId}")
            Return True
        End If
        Return False
    End Function

    Public Function UpdateStudent(studentId As String, fullName As String, email As String, program As String, yearLevel As Integer, section As String, adminId As String) As Boolean
        Dim sql As String = "UPDATE student SET full_name = @FullName, email = @Email, program = @Program, year_level = @YearLevel, section = @Section WHERE student_id = @StudentId"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@FullName", ValidationHelper.SanitizeInput(fullName)),
            New MySqlParameter("@Email", ValidationHelper.SanitizeInput(email)),
            New MySqlParameter("@Program", ValidationHelper.SanitizeInput(program)),
            New MySqlParameter("@YearLevel", yearLevel),
            New MySqlParameter("@Section", ValidationHelper.SanitizeInput(section)),
            New MySqlParameter("@StudentId", studentId)
        }

        If DatabaseHelper.ExecuteNonQuery(sql, params) > 0 Then
            AuditService.LogAction("Admin", adminId, "UPDATE_STUDENT", "student", studentId, details:=$"Updated details for {studentId}")
            Return True
        End If
        Return False
    End Function

    Public Function SetUserStatus(userRole As String, userId As String, newStatus As String, adminId As String) As Boolean
        Dim tableName As String = If(userRole.Equals("Teacher", StringComparison.OrdinalIgnoreCase), "teacher", "student")
        Dim idColumn As String = If(userRole.Equals("Teacher", StringComparison.OrdinalIgnoreCase), "teacher_id", "student_id")

        Dim sql As String = $"UPDATE {tableName} SET status = @Status WHERE {idColumn} = @UserId"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@Status", newStatus),
            New MySqlParameter("@UserId", userId)
        }

        If DatabaseHelper.ExecuteNonQuery(sql, params) > 0 Then
            AuditService.LogAction("Admin", adminId, $"SET_{userRole.ToUpper()}_STATUS", tableName, userId, details:=$"Status set to {newStatus}")
            Return True
        End If
        Return False
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