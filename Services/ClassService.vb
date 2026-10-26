Imports MySql.Data.MySqlClient
Imports System.Data

' Everything about classes: courses, class offerings, teacher assignment and enrollment.
Public Class ClassService

    Private Const DuplicateKeyError As Integer = 1062

    ' ---------------------------------------------------------
    '  Dynamic defaults
    ' ---------------------------------------------------------

    ' Semester and academic year that match today's date (Jun-Dec = 1st semester, Jan-May = 2nd semester).
    ' Example: October 2026 gives ("1st Semester", "2026-2027").
    Public Shared Function GetDefaultTerm() As (Semester As String, AcademicYear As String)
        Dim today As Date = Date.Today
        If today.Month >= 6 Then
            Return ("1st Semester", today.Year.ToString() & "-" & (today.Year + 1).ToString())
        End If
        Return ("2nd Semester", (today.Year - 1).ToString() & "-" & today.Year.ToString())
    End Function

    ' ---------------------------------------------------------
    '  Lists for dropdowns
    ' ---------------------------------------------------------

    ' Retrieves all available courses for ComboBox dropdowns.
    Public Function GetAvailableCourses() As DataTable
        Dim sql As String = "SELECT course_code, course_name, CONCAT(course_code, ' - ', course_name) AS display_name FROM course ORDER BY course_code"
        Return DatabaseHelper.ExecuteQuery(sql)
    End Function

    ' Retrieves all active teachers for ComboBox dropdowns.
    Public Function GetActiveTeachers() As DataTable
        Dim sql As String = "SELECT teacher_id, CONCAT(full_name, ' (', teacher_id, ')') AS display_name FROM teacher WHERE status = 'Active' ORDER BY full_name"
        Return DatabaseHelper.ExecuteQuery(sql)
    End Function

    ' Every class with its teacher (admin screens)
    Public Function GetAllClasses() As DataTable
        Dim sql As String =
            "SELECT c.class_id, c.course_code, co.course_name, c.section, c.semester, c.academic_year, c.teacher_id, " &
            "       COALESCE(t.full_name, 'Unassigned') AS teacher_name, " &
            "       CONCAT(c.course_code, ' - Section ', c.section, ' (', c.semester, ' ', c.academic_year, ') - ', COALESCE(t.full_name, 'Unassigned')) AS class_label " &
            "FROM class c " &
            "JOIN course co ON c.course_code = co.course_code " &
            "LEFT JOIN teacher t ON c.teacher_id = t.teacher_id " &
            "ORDER BY c.course_code, c.section"
        Return DatabaseHelper.ExecuteQuery(sql)
    End Function

    ' A teacher's classes. Old aliases (class_code, term, schedule) are kept so older code still works.
    ' Use class_id as the key. Course code alone is NOT unique (CS 103 has sections A and B).
    Public Function GetClassesByTeacher(teacherId As String) As DataTable
        Dim sql As String =
            "SELECT c.class_id, " &
            "       c.course_code AS class_code, c.course_code, " &
            "       co.course_name, c.teacher_id, " &
            "       c.semester AS term, c.semester, c.academic_year, " &
            "       c.section AS schedule, c.section, " &
            "       CONCAT(c.course_code, ' - ', co.course_name, ' (Section ', c.section, ', ', c.semester, ' ', c.academic_year, ')') AS class_label " &
            "FROM class c " &
            "JOIN course co ON c.course_code = co.course_code " &
            "WHERE c.teacher_id = @teacherId " &
            "ORDER BY c.course_code, c.section"
        Return DatabaseHelper.ExecuteQuery(sql, New MySqlParameter("@teacherId", teacherId))
    End Function

    ' Class record details by course code (can return several sections)
    Public Function GetClassByCode(classCode As String) As DataTable
        Dim sql As String =
            "SELECT c.class_id, c.course_code AS class_code, co.course_name, c.teacher_id, " &
            "       c.semester AS term, c.academic_year, c.section AS schedule " &
            "FROM class c JOIN course co ON c.course_code = co.course_code " &
            "WHERE c.course_code = @classCode"
        Return DatabaseHelper.ExecuteQuery(sql, New MySqlParameter("@classCode", classCode))
    End Function

    ' ---------------------------------------------------------
    '  Create a class offering (creates the course too when it is new)
    ' ---------------------------------------------------------

    Public Function CreateClass(courseCode As String, courseName As String, section As String,
                                semester As String, academicYear As String,
                                teacherId As String, adminUserId As String) As OperationResult
        courseCode = ValidationHelper.CleanInput(courseCode).ToUpper()
        courseName = ValidationHelper.CleanInput(courseName)
        section = ValidationHelper.CleanInput(section).ToUpper()
        academicYear = ValidationHelper.CleanInput(academicYear)

        If courseCode = "" OrElse section = "" Then Return OperationResult.Fail("Subject code and section are required.")
        If courseCode.Length > 20 Then Return OperationResult.Fail("Subject code can have at most 20 characters.")
        If section.Length > 20 Then Return OperationResult.Fail("Section can have at most 20 characters.")
        If semester <> "1st Semester" AndAlso semester <> "2nd Semester" Then Return OperationResult.Fail("Semester must be 1st Semester or 2nd Semester.")
        If academicYear.Length <> 9 OrElse academicYear(4) <> "-"c Then Return OperationResult.Fail("Academic year must look like 2026-2027.")
        If String.IsNullOrEmpty(teacherId) Then Return OperationResult.Fail("Please choose a professor.")

        Try
            Using conn As MySqlConnection = DatabaseHelper.GetConnection()
                Using tx As MySqlTransaction = conn.BeginTransaction()

                    ' 1) The teacher must exist and be active
                    Using cmd As New MySqlCommand("SELECT status FROM teacher WHERE teacher_id = @T", conn, tx)
                        cmd.Parameters.AddWithValue("@T", teacherId)
                        Dim st As Object = cmd.ExecuteScalar()
                        If st Is Nothing OrElse Convert.IsDBNull(st) Then Return OperationResult.Fail("That professor does not exist.")
                        If st.ToString() <> "Active" Then Return OperationResult.Fail("That professor is inactive.")
                    End Using

                    ' 2) Make sure the course exists (class.course_code is a foreign key to course)
                    Dim existingName As Object
                    Using cmd As New MySqlCommand("SELECT course_name FROM course WHERE course_code = @C", conn, tx)
                        cmd.Parameters.AddWithValue("@C", courseCode)
                        existingName = cmd.ExecuteScalar()
                    End Using

                    Dim courseIsNew As Boolean = (existingName Is Nothing OrElse Convert.IsDBNull(existingName))
                    If courseIsNew Then
                        If courseName = "" Then Return OperationResult.Fail("This is a new subject code. Please enter the subject name.")
                        If courseName.Length > 100 Then Return OperationResult.Fail("Subject name can have at most 100 characters.")

                        Using cmd As New MySqlCommand("INSERT INTO course (course_code, course_name) VALUES (@C, @N)", conn, tx)
                            cmd.Parameters.AddWithValue("@C", courseCode)
                            cmd.Parameters.AddWithValue("@N", courseName)
                            cmd.ExecuteNonQuery()
                        End Using
                        AuditService.LogActionTx(conn, tx, "Admin", adminUserId, "Created course", "Course", courseCode, Nothing, Nothing, courseName)
                    End If

                    ' 3) Create the class offering (UNIQUE key blocks the same course, section, semester and year twice)
                    Dim newId As Long
                    Dim insertSql As String =
                        "INSERT INTO class (course_code, section, semester, academic_year, teacher_id) " &
                        "VALUES (@C, @S, @Sem, @AY, @T)"
                    Using cmd As New MySqlCommand(insertSql, conn, tx)
                        cmd.Parameters.AddWithValue("@C", courseCode)
                        cmd.Parameters.AddWithValue("@S", section)
                        cmd.Parameters.AddWithValue("@Sem", semester)
                        cmd.Parameters.AddWithValue("@AY", academicYear)
                        cmd.Parameters.AddWithValue("@T", teacherId)
                        cmd.ExecuteNonQuery()
                        newId = cmd.LastInsertedId
                    End Using

                    AuditService.LogActionTx(conn, tx, "Admin", adminUserId, "Created class", "Class", newId.ToString(), Nothing, Nothing,
                                             courseCode & " Sec " & section & " assigned to " & teacherId)

                    tx.Commit()
                    Return OperationResult.Ok("Class created.")
                End Using
            End Using
        Catch ex As MySqlException When ex.Number = DuplicateKeyError
            Return OperationResult.Fail("A class for " & courseCode & " Section " & section & " (" & semester & ", " & academicYear & ") already exists.")
        Catch ex As Exception
            Console.WriteLine("[CLASS SERVICE ERROR] " & ex.Message)
            Return OperationResult.Fail("Could not create the class: " & ex.Message)
        End Try
    End Function

    ' ---------------------------------------------------------
    '  Reassign the teacher of a class
    ' ---------------------------------------------------------

    Public Function AssignTeacher(classId As Integer, newTeacherId As String, adminUserId As String) As OperationResult
        Try
            Using conn As MySqlConnection = DatabaseHelper.GetConnection()
                Using tx As MySqlTransaction = conn.BeginTransaction()
                    Dim oldTeacher As Object
                    Using cmd As New MySqlCommand("SELECT teacher_id FROM class WHERE class_id = @Id FOR UPDATE", conn, tx)
                        cmd.Parameters.AddWithValue("@Id", classId)
                        oldTeacher = cmd.ExecuteScalar()
                    End Using
                    If oldTeacher Is Nothing OrElse Convert.IsDBNull(oldTeacher) Then Return OperationResult.Fail("Class not found.")

                    Using cmd As New MySqlCommand("SELECT status FROM teacher WHERE teacher_id = @T", conn, tx)
                        cmd.Parameters.AddWithValue("@T", newTeacherId)
                        Dim st As Object = cmd.ExecuteScalar()
                        If st Is Nothing OrElse Convert.IsDBNull(st) Then Return OperationResult.Fail("That professor does not exist.")
                        If st.ToString() <> "Active" Then Return OperationResult.Fail("That professor is inactive.")
                    End Using

                    Using cmd As New MySqlCommand("UPDATE class SET teacher_id = @T WHERE class_id = @Id", conn, tx)
                        cmd.Parameters.AddWithValue("@T", newTeacherId)
                        cmd.Parameters.AddWithValue("@Id", classId)
                        cmd.ExecuteNonQuery()
                    End Using

                    AuditService.LogActionTx(conn, tx, "Admin", adminUserId, "Assigned teacher", "Class", classId.ToString(),
                                             oldTeacher.ToString(), newTeacherId, Nothing)
                    tx.Commit()
                    Return OperationResult.Ok("Teacher assigned.")
                End Using
            End Using
        Catch ex As Exception
            Return OperationResult.Fail("Could not assign the teacher: " & ex.Message)
        End Try
    End Function

    ' ---------------------------------------------------------
    '  Enrollment
    ' ---------------------------------------------------------

    ' Active students that are NOT yet in this class (search matches id, name or email). Empty search = everyone.
    Public Function GetEnrollableStudents(classId As Integer, Optional search As String = "") As DataTable
        Dim sql As String =
            "SELECT s.student_id, s.full_name, s.program, s.year_level, s.section, " &
            "       CONCAT(s.student_id, ' - ', s.full_name) AS display_name " &
            "FROM student s " &
            "WHERE s.status = 'Active' " &
            "  AND NOT EXISTS (SELECT 1 FROM enrollment e WHERE e.student_id = s.student_id AND e.class_id = @ClassId)"
        Dim params As New List(Of MySqlParameter) From {New MySqlParameter("@ClassId", classId)}

        If Not String.IsNullOrWhiteSpace(search) Then
            sql &= " AND (LOWER(s.full_name) LIKE @Q OR LOWER(s.student_id) LIKE @Q OR LOWER(s.email) LIKE @Q)"
            params.Add(New MySqlParameter("@Q", "%" & search.Trim().ToLower() & "%"))
        End If

        sql &= " ORDER BY s.full_name"
        Return DatabaseHelper.ExecuteQuery(sql, params.ToArray())
    End Function

    ' Puts a student in a class. actorRole is "Admin" or "Teacher".
    ' A teacher can only enroll into their own classes. A grade row is created for every existing
    ' assessment, so the student immediately sees them as Pending.
    Public Function EnrollStudent(studentId As String, classId As Integer,
                                  actorRole As String, actorId As String) As OperationResult
        Dim isTeacher As Boolean = String.Equals(actorRole, "Teacher", StringComparison.OrdinalIgnoreCase)
        If Not isTeacher AndAlso Not String.Equals(actorRole, "Admin", StringComparison.OrdinalIgnoreCase) Then
            Return OperationResult.Fail("Only an admin or the class teacher can enroll students.")
        End If

        Try
            Using conn As MySqlConnection = DatabaseHelper.GetConnection()
                Using tx As MySqlTransaction = conn.BeginTransaction()

                    ' Class exists, and the teacher owns it
                    Dim owner As Object
                    Using cmd As New MySqlCommand("SELECT teacher_id FROM class WHERE class_id = @Id", conn, tx)
                        cmd.Parameters.AddWithValue("@Id", classId)
                        owner = cmd.ExecuteScalar()
                    End Using
                    If owner Is Nothing OrElse Convert.IsDBNull(owner) Then Return OperationResult.Fail("Class not found.")
                    If isTeacher AndAlso Not String.Equals(owner.ToString(), actorId, StringComparison.OrdinalIgnoreCase) Then
                        Return OperationResult.Fail("You can only enroll students into your own classes.")
                    End If

                    ' Student exists and is active
                    Dim st As Object
                    Using cmd As New MySqlCommand("SELECT status FROM student WHERE student_id = @S", conn, tx)
                        cmd.Parameters.AddWithValue("@S", studentId)
                        st = cmd.ExecuteScalar()
                    End Using
                    If st Is Nothing OrElse Convert.IsDBNull(st) Then Return OperationResult.Fail("That student does not exist.")
                    If st.ToString() <> "Active" Then Return OperationResult.Fail("That student is inactive.")

                    ' Enroll (UNIQUE key blocks a second enrollment)
                    Dim newId As Long
                    Using cmd As New MySqlCommand("INSERT INTO enrollment (student_id, class_id) VALUES (@S, @C)", conn, tx)
                        cmd.Parameters.AddWithValue("@S", studentId)
                        cmd.Parameters.AddWithValue("@C", classId)
                        cmd.ExecuteNonQuery()
                        newId = cmd.LastInsertedId
                    End Using

                    ' One empty (pending) grade row per assessment of this class
                    Using cmd As New MySqlCommand(
                        "INSERT INTO studentgrade (enrollment_id, assessment_id) SELECT @E, assessment_id FROM assessment WHERE class_id = @C", conn, tx)
                        cmd.Parameters.AddWithValue("@E", newId)
                        cmd.Parameters.AddWithValue("@C", classId)
                        cmd.ExecuteNonQuery()
                    End Using

                    AuditService.LogActionTx(conn, tx, actorRole, actorId, "Enrolled student", "Enrollment", newId.ToString(),
                                             Nothing, Nothing, studentId & " into class " & classId.ToString())
                    tx.Commit()
                    Return OperationResult.Ok("Student enrolled.")
                End Using
            End Using
        Catch ex As MySqlException When ex.Number = DuplicateKeyError
            Return OperationResult.Fail("That student is already enrolled in this class.")
        Catch ex As Exception
            Console.WriteLine("[ENROLL ERROR] " & ex.Message)
            Return OperationResult.Fail("Could not enroll the student: " & ex.Message)
        End Try
    End Function

End Class