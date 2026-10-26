Imports MySql.Data.MySqlClient
Imports System.Data

' Accounts: list, create, edit, deactivate / reactivate (never delete).
' The old method names and Boolean return types are KEPT so existing forms still compile.
' Each one calls a matching "...Result" method that returns an OperationResult with a real message
' (for example "That email address is already used by another account.").
' Every change is saved TOGETHER with its audit row in one transaction.
Public Class UserService

    Private Const DuplicateKeyError As Integer = 1062

    ' =========================================================
    '  READ
    ' =========================================================

    Public Function GetAllTeachers(Optional searchQuery As String = "", Optional department As String = "All") As DataTable
        Dim sql As String = "SELECT t.teacher_id, t.full_name, t.email, t.department, t.employment_type, t.status, " &
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

        If Not String.IsNullOrWhiteSpace(yearLevel) AndAlso Not yearLevel.Equals("All", StringComparison.OrdinalIgnoreCase) Then
            Dim yr As Integer
            If Integer.TryParse(yearLevel, yr) Then
                sql &= " AND year_level = @Yr"
                params.Add(New MySqlParameter("@Yr", yr))
            End If
        End If

        If Not String.IsNullOrWhiteSpace(section) AndAlso Not section.Equals("All", StringComparison.OrdinalIgnoreCase) Then
            sql &= " AND section = @Sec"
            params.Add(New MySqlParameter("@Sec", section.Trim()))
        End If

        sql &= " ORDER BY full_name ASC"
        Return DatabaseHelper.ExecuteQuery(sql, params.ToArray())
    End Function

    ' One account's full details (used to fill the Edit form). Returns an empty table when not found.
    Public Function GetStudent(studentId As String) As DataTable
        Return DatabaseHelper.ExecuteQuery(
            "SELECT student_id, full_name, email, program, year_level, section, status FROM student WHERE student_id = @Id",
            New MySqlParameter("@Id", studentId))
    End Function

    Public Function GetTeacher(teacherId As String) As DataTable
        Return DatabaseHelper.ExecuteQuery(
            "SELECT teacher_id, full_name, email, department, employment_type, status FROM teacher WHERE teacher_id = @Id",
            New MySqlParameter("@Id", teacherId))
    End Function

    ' Values for the filter dropdowns, taken from the data so they are never out of date
    Public Function GetDepartments() As List(Of String)
        Return DistinctValues("SELECT DISTINCT department FROM teacher WHERE department <> '' ORDER BY department")
    End Function

    Public Function GetStudentSections() As List(Of String)
        Return DistinctValues("SELECT DISTINCT section FROM student WHERE section <> '' ORDER BY section")
    End Function

    Public Function GetYearLevels() As List(Of String)
        Return DistinctValues("SELECT DISTINCT year_level FROM student ORDER BY year_level")
    End Function

    Public Function GetPrograms() As List(Of String)
        Return DistinctValues("SELECT DISTINCT program FROM student WHERE program <> '' ORDER BY program")
    End Function

    Private Shared Function DistinctValues(sql As String) As List(Of String)
        Dim result As New List(Of String)()
        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql)
        If dt IsNot Nothing Then
            For Each r As DataRow In dt.Rows
                If Not Convert.IsDBNull(r(0)) Then result.Add(r(0).ToString())
            Next
        End If
        Return result
    End Function

    ' =========================================================
    '  CREATE
    ' =========================================================

    Public Function CreateTeacher(teacherId As String, rawPassword As String, fullName As String, email As String, department As String, adminId As String) As Boolean
        Return CreateTeacherResult(teacherId, rawPassword, fullName, email, department, "Full-time", adminId).Success
    End Function

    Public Function CreateTeacherResult(teacherId As String, rawPassword As String, fullName As String, email As String,
                                        department As String, employmentType As String, adminId As String) As OperationResult
        Dim cleanId As String = ValidationHelper.CleanInput(teacherId)
        Dim cleanEmail As String = ValidationHelper.CleanInput(email)
        Dim cleanName As String = ValidationHelper.CleanInput(fullName)
        Dim cleanDept As String = ValidationHelper.CleanInput(department)

        If cleanId = "" OrElse cleanName = "" Then Return OperationResult.Fail("Employee ID and full name are required.")
        If cleanId.Length > 10 Then Return OperationResult.Fail("Employee ID can have at most 10 characters.")
        If cleanDept = "" Then Return OperationResult.Fail("Department is required.")
        If Not ValidationHelper.IsValidEmail(cleanEmail) Then Return OperationResult.Fail("Enter a valid email address.")
        If Not ValidationHelper.IsStrongPassword(rawPassword) Then
            Return OperationResult.Fail("Password needs 8+ characters with an uppercase letter, a lowercase letter, a number and a symbol.")
        End If
        Dim emp As String = If(String.Equals(employmentType, "Part-time", StringComparison.OrdinalIgnoreCase), "Part-time", "Full-time")

        Dim sql As String = "INSERT INTO teacher (teacher_id, full_name, email, password_hash, department, employment_type, status) " &
                            "VALUES (@TeacherId, @FullName, @Email, @PasswordHash, @Department, @Emp, 'Active')"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@TeacherId", cleanId),
            New MySqlParameter("@FullName", cleanName),
            New MySqlParameter("@Email", cleanEmail),
            New MySqlParameter("@PasswordHash", SecurityHelper.HashPassword(rawPassword)),
            New MySqlParameter("@Department", cleanDept),
            New MySqlParameter("@Emp", emp)
        }

        Return WriteWithAudit(sql, params, adminId, "Created account", "Teacher", cleanId, "Teacher " & cleanName & " (" & cleanEmail & ")", "Teacher")
    End Function

    Public Function CreateStudent(studentId As String, rawPassword As String, fullName As String, email As String, program As String, yearLevel As Integer, section As String, adminId As String) As Boolean
        Return CreateStudentResult(studentId, rawPassword, fullName, email, program, yearLevel, section, adminId).Success
    End Function

    Public Function CreateStudentResult(studentId As String, rawPassword As String, fullName As String, email As String,
                                        program As String, yearLevel As Integer, section As String, adminId As String) As OperationResult
        Dim cleanId As String = ValidationHelper.CleanInput(studentId)
        Dim cleanEmail As String = ValidationHelper.CleanInput(email)
        Dim cleanName As String = ValidationHelper.CleanInput(fullName)
        Dim cleanProgram As String = ValidationHelper.CleanInput(program)
        Dim cleanSection As String = ValidationHelper.CleanInput(section)

        If cleanId = "" OrElse cleanName = "" Then Return OperationResult.Fail("Student ID and full name are required.")
        If cleanId.Length > 10 Then Return OperationResult.Fail("Student ID can have at most 10 characters (example 25-00418).")
        If cleanProgram = "" OrElse cleanSection = "" Then Return OperationResult.Fail("Program and section are required.")
        If Not ValidationHelper.IsValidEmail(cleanEmail) Then Return OperationResult.Fail("Enter a valid email address.")
        If Not ValidationHelper.IsStrongPassword(rawPassword) Then
            Return OperationResult.Fail("Password needs 8+ characters with an uppercase letter, a lowercase letter, a number and a symbol.")
        End If
        If yearLevel < 1 OrElse yearLevel > 6 Then Return OperationResult.Fail("Year level must be between 1 and 6.")

        Dim sql As String = "INSERT INTO student (student_id, full_name, email, password_hash, program, year_level, section, status) " &
                            "VALUES (@StudentId, @FullName, @Email, @PasswordHash, @Program, @YearLevel, @Section, 'Active')"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@StudentId", cleanId),
            New MySqlParameter("@FullName", cleanName),
            New MySqlParameter("@Email", cleanEmail),
            New MySqlParameter("@PasswordHash", SecurityHelper.HashPassword(rawPassword)),
            New MySqlParameter("@Program", cleanProgram),
            New MySqlParameter("@YearLevel", yearLevel),
            New MySqlParameter("@Section", cleanSection)
        }

        Return WriteWithAudit(sql, params, adminId, "Created account", "Student", cleanId, "Student " & cleanName & " (" & cleanEmail & ")", "Student")
    End Function

    ' =========================================================
    '  UPDATE
    ' =========================================================

    Public Function UpdateTeacher(teacherId As String, fullName As String, email As String, department As String, adminId As String) As Boolean
        Return UpdateTeacherResult(teacherId, fullName, email, department, "Full-time", adminId).Success
    End Function

    Public Function UpdateTeacherResult(teacherId As String, fullName As String, email As String,
                                        department As String, employmentType As String, adminId As String) As OperationResult
        Dim cleanEmail As String = ValidationHelper.CleanInput(email)
        Dim cleanName As String = ValidationHelper.CleanInput(fullName)
        Dim cleanDept As String = ValidationHelper.CleanInput(department)
        If cleanName = "" Then Return OperationResult.Fail("Full name is required.")
        If cleanDept = "" Then Return OperationResult.Fail("Department is required.")
        If Not ValidationHelper.IsValidEmail(cleanEmail) Then Return OperationResult.Fail("Enter a valid email address.")
        Dim emp As String = If(String.Equals(employmentType, "Part-time", StringComparison.OrdinalIgnoreCase), "Part-time", "Full-time")

        Dim sql As String = "UPDATE teacher SET full_name = @FullName, email = @Email, department = @Department, employment_type = @Emp WHERE teacher_id = @TeacherId"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@FullName", cleanName),
            New MySqlParameter("@Email", cleanEmail),
            New MySqlParameter("@Department", cleanDept),
            New MySqlParameter("@Emp", emp),
            New MySqlParameter("@TeacherId", teacherId)
        }

        Return WriteWithAudit(sql, params, adminId, "Updated account", "Teacher", teacherId, "Updated details of " & teacherId, "Teacher")
    End Function

    Public Function UpdateStudent(studentId As String, fullName As String, email As String, program As String, yearLevel As Integer, section As String, adminId As String) As Boolean
        Return UpdateStudentResult(studentId, fullName, email, program, yearLevel, section, adminId).Success
    End Function

    Public Function UpdateStudentResult(studentId As String, fullName As String, email As String,
                                        program As String, yearLevel As Integer, section As String, adminId As String) As OperationResult
        Dim cleanEmail As String = ValidationHelper.CleanInput(email)
        Dim cleanName As String = ValidationHelper.CleanInput(fullName)
        Dim cleanProgram As String = ValidationHelper.CleanInput(program)
        Dim cleanSection As String = ValidationHelper.CleanInput(section)
        If cleanName = "" Then Return OperationResult.Fail("Full name is required.")
        If cleanProgram = "" OrElse cleanSection = "" Then Return OperationResult.Fail("Program and section are required.")
        If Not ValidationHelper.IsValidEmail(cleanEmail) Then Return OperationResult.Fail("Enter a valid email address.")
        If yearLevel < 1 OrElse yearLevel > 6 Then Return OperationResult.Fail("Year level must be between 1 and 6.")

        Dim sql As String = "UPDATE student SET full_name = @FullName, email = @Email, program = @Program, year_level = @YearLevel, section = @Section WHERE student_id = @StudentId"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@FullName", cleanName),
            New MySqlParameter("@Email", cleanEmail),
            New MySqlParameter("@Program", cleanProgram),
            New MySqlParameter("@YearLevel", yearLevel),
            New MySqlParameter("@Section", cleanSection),
            New MySqlParameter("@StudentId", studentId)
        }

        Return WriteWithAudit(sql, params, adminId, "Updated account", "Student", studentId, "Updated details of " & studentId, "Student")
    End Function

    ' Admin sets a new password for someone who forgot theirs
    Public Function ResetPassword(userRole As String, userId As String, newPassword As String, adminId As String) As OperationResult
        Dim isTeacher As Boolean = String.Equals(userRole, "Teacher", StringComparison.OrdinalIgnoreCase)
        Dim isStudent As Boolean = String.Equals(userRole, "Student", StringComparison.OrdinalIgnoreCase)
        If Not isTeacher AndAlso Not isStudent Then Return OperationResult.Fail("Role must be Teacher or Student.")
        If Not ValidationHelper.IsStrongPassword(newPassword) Then
            Return OperationResult.Fail("Password needs 8+ characters with an uppercase letter, a lowercase letter, a number and a symbol.")
        End If

        Dim tableName As String = If(isTeacher, "teacher", "student")
        Dim idColumn As String = If(isTeacher, "teacher_id", "student_id")
        Dim entity As String = If(isTeacher, "Teacher", "Student")

        Dim sql As String = "UPDATE " & tableName & " SET password_hash = @Hash WHERE " & idColumn & " = @Id"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@Hash", SecurityHelper.HashPassword(newPassword)),
            New MySqlParameter("@Id", userId)
        }
        Return WriteWithAudit(sql, params, adminId, "Reset password", entity, userId, "Password reset for " & userId, entity)
    End Function

    ' =========================================================
    '  DEACTIVATE / REACTIVATE (never delete)
    ' =========================================================

    Public Function SetUserStatus(userRole As String, userId As String, newStatus As String, adminId As String) As Boolean
        Return SetUserStatusResult(userRole, userId, newStatus, adminId).Success
    End Function

    ' Same thing as SetUserStatus; kept because the old code had both names.
    Public Function ToggleUserStatus(role As String, userId As String, newStatus As String, adminId As String) As Boolean
        Return SetUserStatusResult(role, userId, newStatus, adminId).Success
    End Function

    Public Function SetUserStatusResult(userRole As String, userId As String, newStatus As String, adminId As String) As OperationResult
        Dim isTeacher As Boolean = String.Equals(userRole, "Teacher", StringComparison.OrdinalIgnoreCase)
        Dim isStudent As Boolean = String.Equals(userRole, "Student", StringComparison.OrdinalIgnoreCase)
        If Not isTeacher AndAlso Not isStudent Then Return OperationResult.Fail("Role must be Teacher or Student.")

        Dim status As String
        If String.Equals(newStatus, "Active", StringComparison.OrdinalIgnoreCase) Then
            status = "Active"
        ElseIf String.Equals(newStatus, "Inactive", StringComparison.OrdinalIgnoreCase) Then
            status = "Inactive"
        Else
            Return OperationResult.Fail("Status must be Active or Inactive.")
        End If

        Dim tableName As String = If(isTeacher, "teacher", "student")
        Dim idColumn As String = If(isTeacher, "teacher_id", "student_id")
        Dim entity As String = If(isTeacher, "Teacher", "Student")

        Dim sql As String = "UPDATE " & tableName & " SET status = @Status WHERE " & idColumn & " = @UserId"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@Status", status),
            New MySqlParameter("@UserId", userId)
        }

        Return WriteWithAudit(sql, params, adminId, If(status = "Active", "Reactivated account", "Deactivated account"), entity, userId,
                              entity & " " & userId & " is now " & status, entity)
    End Function

    ' =========================================================
    '  SHARED HELPER: one change + its audit row in ONE transaction
    ' =========================================================

    Private Shared Function WriteWithAudit(sql As String, params As MySqlParameter(),
                                           adminId As String, action As String,
                                           entityType As String, entityId As String,
                                           details As String, duplicateLabel As String) As OperationResult
        Try
            Using conn As MySqlConnection = DatabaseHelper.GetConnection()
                Using tx As MySqlTransaction = conn.BeginTransaction()
                    Dim rows As Integer
                    Using cmd As New MySqlCommand(sql, conn, tx)
                        cmd.Parameters.AddRange(params)
                        rows = cmd.ExecuteNonQuery()
                    End Using

                    If rows <= 0 Then
                        tx.Rollback()
                        Return OperationResult.Fail("Nothing was changed (record not found).")
                    End If

                    ' If the admin id is wrong this throws and everything above is rolled back
                    AuditService.LogActionTx(conn, tx, "Admin", adminId, action, entityType, entityId, Nothing, Nothing, details)

                    tx.Commit()
                    Return OperationResult.Ok("Saved.")
                End Using
            End Using
        Catch ex As MySqlException When ex.Number = DuplicateKeyError
            Return OperationResult.Fail(DuplicateMessage(ex, duplicateLabel))
        Catch ex As Exception
            Console.WriteLine("[USER SERVICE ERROR] " & ex.Message)
            Return OperationResult.Fail("Could not save: " & ex.Message)
        End Try
    End Function

    Private Shared Function DuplicateMessage(ex As MySqlException, label As String) As String
        If ex.Message.IndexOf("email", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return "That email address is already used by another account."
        End If
        Return "That " & label & " ID already exists."
    End Function

End Class