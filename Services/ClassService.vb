Imports MySql.Data.MySqlClient
Imports System.Data

Public Class ClassService

    ''' <summary>
    ''' Retrieves all available courses for ComboBox dropdowns.
    ''' </summary>
    Public Function GetAvailableCourses() As DataTable
        Dim sql As String = "SELECT course_code, CONCAT(course_code, ' - ', course_name) AS display_name FROM course ORDER BY course_code"
        Return DatabaseHelper.ExecuteQuery(sql, Nothing)
    End Function

    ''' <summary>
    ''' Retrieves all active teachers for ComboBox dropdowns.
    ''' </summary>
    Public Function GetActiveTeachers() As DataTable
        Dim sql As String = "SELECT teacher_id, CONCAT(full_name, ' (', teacher_id, ')') AS display_name FROM teacher WHERE status = 'Active' ORDER BY full_name"
        Return DatabaseHelper.ExecuteQuery(sql, Nothing)
    End Function

    ''' <summary>
    ''' Inserts a new class offering into the database and logs the audit event.
    ''' </summary>
    Public Function CreateClass(courseCode As String, section As String, semester As String, academicYear As String, teacherId As String, adminUserId As String) As Boolean
        ' 1. Check for duplicate class offering
        Dim checkSql As String = "SELECT COUNT(*) FROM class WHERE course_code = @CourseCode AND section = @Section AND semester = @Semester AND academic_year = @AY"
        Dim checkParams As MySqlParameter() = {
            New MySqlParameter("@CourseCode", courseCode),
            New MySqlParameter("@Section", section),
            New MySqlParameter("@Semester", semester),
            New MySqlParameter("@AY", academicYear)
        }

        Dim count As Integer = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, checkParams))
        If count > 0 Then
            Throw New InvalidOperationException($"A class for {courseCode} Section {section} ({semester}, {academicYear}) already exists.")
        End If

        ' 2. Insert new class
        Dim insertSql As String = "INSERT INTO class (course_code, section, semester, academic_year, teacher_id) " &
                                  "VALUES (@CourseCode, @Section, @Semester, @AY, @TeacherId); " &
                                  "SELECT LAST_INSERT_ID();"

        Dim insertParams As MySqlParameter() = {
            New MySqlParameter("@CourseCode", courseCode),
            New MySqlParameter("@Section", section),
            New MySqlParameter("@Semester", semester),
            New MySqlParameter("@AY", academicYear),
            New MySqlParameter("@TeacherId", teacherId)
        }

        Dim newClassId As Object = DatabaseHelper.ExecuteScalar(insertSql, insertParams)

        If newClassId IsNot Nothing AndAlso Convert.ToInt32(newClassId) > 0 Then
            ' 3. Log audit event
            AuditService.LogAction("Admin", adminUserId, "Created class", "Class", newClassId.ToString(), Nothing, Nothing, $"{courseCode} Sec {section} assigned to {teacherId}")
            Return True
        End If

        Return False
    End Function
    ''' </summary>
    Public Function GetClassesByTeacher(teacherId As String) As DataTable
        Dim sql As String = "SELECT c.class_id, " &
                           "c.course_code AS class_code, " &
                           "co.course_name, " &
                           "c.teacher_id, " &
                           "c.semester AS term, " &
                           "c.academic_year, " &
                           "c.section AS schedule " &
                           "FROM class c " &
                           "JOIN course co ON c.course_code = co.course_code " &
                           "WHERE c.teacher_id = @teacherId " &
                           "ORDER BY c.course_code"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@teacherId", teacherId)
        }
        Return DatabaseHelper.ExecuteQuery(sql, params)
    End Function

    ''' <summary>
    ''' Retrieves class record details by course/class code.
    ''' </summary>
    Public Function GetClassByCode(classCode As String) As DataTable
        Dim sql As String = "SELECT c.class_id, " &
                           "c.course_code AS class_code, " &
                           "co.course_name, " &
                           "c.teacher_id, " &
                           "c.semester AS term, " &
                           "c.academic_year, " &
                           "c.section AS schedule " &
                           "FROM class c " &
                           "JOIN course co ON c.course_code = co.course_code " &
                           "WHERE c.course_code = @classCode"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@classCode", classCode)
        }
        Return DatabaseHelper.ExecuteQuery(sql, params)
    End Function

End Class