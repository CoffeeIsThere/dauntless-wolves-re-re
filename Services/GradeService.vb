Imports System.Data
Imports System.Globalization
Imports System.IO
Imports MySql.Data.MySqlClient

Public Class GradeService


    '  RECORD SCORE


    Public Function RecordScore(enrollmentId As Integer, assessmentId As Integer,
                                newScore As Double, teacherId As String,
                                Optional reason As String = Nothing) As OperationResult
        Try
            Using conn As MySqlConnection = DatabaseHelper.GetConnection()
                Using tx As MySqlTransaction = conn.BeginTransaction()
                    Try
                        Dim res As OperationResult = RecordScoreTx(conn, tx, enrollmentId, assessmentId, newScore, teacherId, reason)
                        If res.Success Then tx.Commit() Else tx.Rollback()
                        Return res
                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine("[RECORD SCORE ERROR] " & ex.Message)
            Return OperationResult.Fail("Could not save the score: " & ex.Message)
        End Try
    End Function


    Public Function RecordScoreTx(conn As MySqlConnection, tx As MySqlTransaction,
                                  enrollmentId As Integer, assessmentId As Integer,
                                  newScore As Double, teacherId As String,
                                  Optional reason As String = Nothing) As OperationResult


        Dim totalScore As Double = 0
        Dim title As String = ""
        Dim ownerId As String = ""
        Dim sameClass As Boolean = False

        Dim infoSql As String =
            "SELECT a.total_score, a.title, c.teacher_id, (a.class_id = e.class_id) AS same_class " &
            "FROM enrollment e " &
            "JOIN class c ON e.class_id = c.class_id " &
            "JOIN assessment a ON a.assessment_id = @AssessmentId " &
            "WHERE e.enrollment_id = @EnrollmentId"

        Using cmd As New MySqlCommand(infoSql, conn, tx)
            cmd.Parameters.AddWithValue("@AssessmentId", assessmentId)
            cmd.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
            Using r As MySqlDataReader = cmd.ExecuteReader()
                If Not r.Read() Then Return OperationResult.Fail("Enrollment or assessment not found.")
                totalScore = Convert.ToDouble(r.GetValue(0))
                title = r.GetValue(1).ToString()
                ownerId = r.GetValue(2).ToString()
                sameClass = (Convert.ToInt32(r.GetValue(3)) = 1)
            End Using
        End Using

        If Not String.Equals(ownerId, teacherId, StringComparison.OrdinalIgnoreCase) Then
            Return OperationResult.Fail("You can only record scores for your own classes.")
        End If
        If Not sameClass Then
            Return OperationResult.Fail("That assessment belongs to a different class.")
        End If
        If Not ValidationHelper.IsValidScore(newScore, totalScore) Then
            Return OperationResult.Fail("Score must be between 0 and " & Fmt(totalScore) & ".")
        End If


        Dim oldScore As Double? = Nothing
        Dim lockSql As String = "SELECT score FROM studentgrade WHERE enrollment_id = @EnrollmentId AND assessment_id = @AssessmentId FOR UPDATE"
        Using cmd As New MySqlCommand(lockSql, conn, tx)
            cmd.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
            cmd.Parameters.AddWithValue("@AssessmentId", assessmentId)
            oldScore = ToNullableDouble(cmd.ExecuteScalar())
        End Using

        If oldScore.HasValue AndAlso oldScore.Value = newScore Then
            Return OperationResult.Ok("Score unchanged.")
        End If


        Dim upsertSql As String =
            "INSERT INTO studentgrade (enrollment_id, assessment_id, score, remarks) " &
            "VALUES (@EnrollmentId, @AssessmentId, @Score, @Remarks) " &
            "ON DUPLICATE KEY UPDATE score = VALUES(score), remarks = VALUES(remarks)"
        Using cmd As New MySqlCommand(upsertSql, conn, tx)
            cmd.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
            cmd.Parameters.AddWithValue("@AssessmentId", assessmentId)
            cmd.Parameters.AddWithValue("@Score", newScore)
            cmd.Parameters.AddWithValue("@Remarks", GradeCalculator.DetermineRemarks(GradeCalculator.CalculateScorePercentage(newScore, totalScore)))
            cmd.ExecuteNonQuery()
        End Using

        RefreshRunningGrade(conn, tx, enrollmentId)

        Dim gradeId As String
        Using cmd As New MySqlCommand("SELECT grade_id FROM studentgrade WHERE enrollment_id = @EnrollmentId AND assessment_id = @AssessmentId", conn, tx)
            cmd.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
            cmd.Parameters.AddWithValue("@AssessmentId", assessmentId)
            gradeId = cmd.ExecuteScalar().ToString()
        End Using

        Dim oldText As String = Nothing
        If oldScore.HasValue Then oldText = Fmt(oldScore.Value) & "/" & Fmt(totalScore)

        AuditService.LogActionTx(conn, tx, "Teacher", teacherId,
                                 If(oldScore.HasValue, "Updated score", "Recorded score"),
                                 "StudentGrade", gradeId,
                                 oldText, Fmt(newScore) & "/" & Fmt(totalScore), reason)

        Return OperationResult.Ok("Saved " & title & ": " & Fmt(newScore) & "/" & Fmt(totalScore))
    End Function

    Private Shared Function RefreshRunningGrade(conn As MySqlConnection, tx As MySqlTransaction, enrollmentId As Integer) As Double?
        Dim items As New List(Of ScoreItem)()
        Dim sql As String =
            "SELECT sg.score, a.total_score, a.weight " &
            "FROM studentgrade sg JOIN assessment a ON sg.assessment_id = a.assessment_id " &
            "WHERE sg.enrollment_id = @EnrollmentId"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
            Using r As MySqlDataReader = cmd.ExecuteReader()
                While r.Read()
                    items.Add(New ScoreItem(ToNullableDouble(r.GetValue(0)),
                                            Convert.ToDouble(r.GetValue(1)),
                                            Convert.ToDouble(r.GetValue(2))))
                End While
            End Using
        End Using

        Dim running As Double? = GradeCalculator.CalculateRunningGrade(items)

        Using cmd As New MySqlCommand("UPDATE enrollment SET running_grade = @Running WHERE enrollment_id = @EnrollmentId", conn, tx)
            If running.HasValue Then
                cmd.Parameters.AddWithValue("@Running", running.Value)
            Else
                cmd.Parameters.AddWithValue("@Running", DBNull.Value)
            End If
            cmd.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
            cmd.ExecuteNonQuery()
        End Using

        Return running
    End Function

    Public Function RecalculateAllRunningGrades() As Integer
        Using conn As MySqlConnection = DatabaseHelper.GetConnection()
            Using tx As MySqlTransaction = conn.BeginTransaction()
                Dim ids As New List(Of Integer)()
                Using cmd As New MySqlCommand("SELECT enrollment_id FROM enrollment", conn, tx)
                    Using r As MySqlDataReader = cmd.ExecuteReader()
                        While r.Read()
                            ids.Add(Convert.ToInt32(r.GetValue(0)))
                        End While
                    End Using
                End Using

                For Each id As Integer In ids
                    RefreshRunningGrade(conn, tx, id)
                Next

                tx.Commit()
                Return ids.Count
            End Using
        End Using
    End Function

    '  READ QUERIES

    Public Function GetClassesByTeacher(teacherId As String) As DataTable
        Dim sql As String =
            "SELECT c.class_id, c.course_code, co.course_name, c.section, c.semester, c.academic_year, " &
            "       COUNT(e.enrollment_id) AS student_count " &
            "FROM class c " &
            "JOIN course co ON c.course_code = co.course_code " &
            "LEFT JOIN enrollment e ON e.class_id = c.class_id " &
            "WHERE c.teacher_id = @TeacherId " &
            "GROUP BY c.class_id, c.course_code, co.course_name, c.section, c.semester, c.academic_year " &
            "ORDER BY c.course_code, c.section"
        Return DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@TeacherId", teacherId)})
    End Function


    Public Function GetEnrollmentsByClass(classId As Integer, teacherId As String) As DataTable
        Dim sql As String =
            "SELECT e.enrollment_id, s.student_id, s.full_name, e.running_grade " &
            "FROM enrollment e " &
            "JOIN student s ON e.student_id = s.student_id " &
            "JOIN class c ON e.class_id = c.class_id " &
            "WHERE e.class_id = @ClassId AND c.teacher_id = @TeacherId " &
            "ORDER BY s.full_name"
        Return DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@ClassId", classId), New MySqlParameter("@TeacherId", teacherId)})
    End Function

    Public Function GetEnrollmentsByStudent(studentId As String) As DataTable
        Dim sql As String =
            "SELECT e.enrollment_id, c.class_id, c.course_code, co.course_name, c.section, e.running_grade " &
            "FROM enrollment e " &
            "JOIN class c ON e.class_id = c.class_id " &
            "JOIN course co ON c.course_code = co.course_code " &
            "WHERE e.student_id = @StudentId " &
            "ORDER BY c.course_code"
        Return DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@StudentId", studentId)})
    End Function

    Public Function GetGradesForStudent(enrollmentId As Integer, studentId As String) As DataTable
        Dim sql As String =
            "SELECT sg.grade_id, a.term, a.title, a.type, sg.score, a.total_score, a.weight, sg.remarks, sg.updated_at " &
            "FROM studentgrade sg " &
            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
            "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
            "WHERE sg.enrollment_id = @EnrollmentId AND e.student_id = @StudentId " &
            "ORDER BY a.term, a.created_at, a.assessment_id"
        Return DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@EnrollmentId", enrollmentId), New MySqlParameter("@StudentId", studentId)})
    End Function

    Public Function GetGradeHistoryForStudent(studentId As String) As DataTable
        Dim sql As String =
            "SELECT al.timestamp AS changed_at, a.title AS assessment, al.old_value, al.new_value, " &
            "       COALESCE(t.full_name, al.teacher_id) AS changed_by, al.details AS reason " &
            "FROM auditlog al " &
            "JOIN studentgrade sg ON sg.grade_id = CAST(al.entity_id AS UNSIGNED) " &
            "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
            "LEFT JOIN teacher t ON al.teacher_id = t.teacher_id " &
            "WHERE al.entity_type = 'StudentGrade' AND e.student_id = @StudentId " &
            "ORDER BY al.timestamp DESC, al.log_id DESC"
        Return DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@StudentId", studentId)})
    End Function


    Public Function GetClassGradeSheet(classId As Integer, teacherId As String,
                                       Optional term As String = "All") As DataTable
        If term = "Midterms" Then term = "Midterm"
        Dim filterTerm As Boolean = Not String.IsNullOrEmpty(term) AndAlso
                                    Not term.Equals("All", StringComparison.OrdinalIgnoreCase)

        Dim aSql As String =
            "SELECT a.assessment_id, a.title FROM assessment a " &
            "JOIN class c ON a.class_id = c.class_id " &
            "WHERE a.class_id = @ClassId AND c.teacher_id = @TeacherId"
        Dim aParams As New List(Of MySqlParameter) From {
            New MySqlParameter("@ClassId", classId),
            New MySqlParameter("@TeacherId", teacherId)}
        If filterTerm Then
            aSql &= " AND a.term = @Term"
            aParams.Add(New MySqlParameter("@Term", term))
        End If
        aSql &= " ORDER BY a.term, a.created_at, a.assessment_id"
        Dim assessments As DataTable = DatabaseHelper.ExecuteQuery(aSql, aParams.ToArray())

        Dim students As DataTable = GetEnrollmentsByClass(classId, teacherId)

        Dim gradesSql As String =
            "SELECT sg.enrollment_id, sg.assessment_id, sg.score, a.total_score " &
            "FROM studentgrade sg " &
            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
            "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
            "JOIN class c ON e.class_id = c.class_id " &
            "WHERE e.class_id = @ClassId AND c.teacher_id = @TeacherId"
        Dim grades As DataTable = DatabaseHelper.ExecuteQuery(gradesSql,
            {New MySqlParameter("@ClassId", classId), New MySqlParameter("@TeacherId", teacherId)})

        Dim cells As New Dictionary(Of String, String)()
        For Each g As DataRow In grades.Rows
            Dim key As String = g("enrollment_id").ToString() & ":" & g("assessment_id").ToString()
            If Convert.IsDBNull(g("score")) Then
                cells(key) = "-"
            Else
                cells(key) = Fmt(Convert.ToDouble(g("score"))) & "/" & Fmt(Convert.ToDouble(g("total_score")))
            End If
        Next

        Dim sheet As New DataTable()
        sheet.Columns.Add("EnrollmentId", GetType(Integer))
        sheet.Columns.Add("StudentId", GetType(String))
        sheet.Columns.Add("Student", GetType(String))
        For Each a As DataRow In assessments.Rows
            Dim col As DataColumn = sheet.Columns.Add("A" & a("assessment_id").ToString(), GetType(String))
            col.Caption = a("title").ToString()
        Next
        sheet.Columns.Add("RunningGrade", GetType(Double))
        sheet.Columns.Add("Status", GetType(String))

        For Each s As DataRow In students.Rows
            Dim row As DataRow = sheet.NewRow()
            row("EnrollmentId") = Convert.ToInt32(s("enrollment_id"))
            row("StudentId") = s("student_id").ToString()
            row("Student") = s("full_name").ToString()

            For Each a As DataRow In assessments.Rows
                Dim cell As String = Nothing
                If Not cells.TryGetValue(s("enrollment_id").ToString() & ":" & a("assessment_id").ToString(), cell) Then
                    cell = "-"
                End If
                row("A" & a("assessment_id").ToString()) = cell
            Next

            Dim rg As Double? = ToNullableDouble(s("running_grade"))
            If rg.HasValue Then
                row("RunningGrade") = rg.Value
            Else
                row("RunningGrade") = DBNull.Value
            End If
            row("Status") = GradeCalculator.GetStatus(rg)

            sheet.Rows.Add(row)
        Next

        Return sheet
    End Function


    '  CSV IMPORT


    Public Function BulkImportGradesFromCSV(classId As Integer, assessmentId As Integer,
                                            filePath As String, teacherId As String) As (SuccessCount As Integer, FailCount As Integer)
        Dim successCount As Integer = 0
        Dim failCount As Integer = 0

        If Not File.Exists(filePath) Then Return (0, 0)

        Dim lines() As String = File.ReadAllLines(filePath)

        For i As Integer = 1 To lines.Length - 1
            Dim line As String = lines(i).Trim()
            If String.IsNullOrWhiteSpace(line) Then Continue For

            Dim parts() As String = line.Split(","c)
            If parts.Length < 2 Then
                failCount += 1
                Continue For
            End If

            Dim studentId As String = parts(0).Trim().Replace("""", "")
            Dim scoreVal As Double
            If Not Double.TryParse(parts(1).Trim().Replace("""", ""), NumberStyles.Float, CultureInfo.InvariantCulture, scoreVal) Then
                failCount += 1
                Continue For
            End If

            Dim enrollmentId As Integer = GetEnrollmentIdByStudentAndClass(studentId, classId)
            If enrollmentId <= 0 Then
                failCount += 1
                Continue For
            End If

            If RecordScore(enrollmentId, assessmentId, scoreVal, teacherId, "Bulk CSV Upload").Success Then
                successCount += 1
            Else
                failCount += 1
            End If
        Next

        Return (successCount, failCount)
    End Function

    Private Function GetEnrollmentIdByStudentAndClass(studentId As String, classId As Integer) As Integer
        Dim sql As String = "SELECT enrollment_id FROM enrollment WHERE student_id = @StudentId AND class_id = @ClassId"
        Dim result As Object = DatabaseHelper.ExecuteScalar(sql,
            {New MySqlParameter("@StudentId", studentId), New MySqlParameter("@ClassId", classId)})
        If result IsNot Nothing AndAlso Not Convert.IsDBNull(result) Then
            Return Convert.ToInt32(result)
        End If
        Return 0
    End Function


    Private Shared Function ToNullableDouble(o As Object) As Double?
        If o Is Nothing OrElse Convert.IsDBNull(o) Then Return Nothing
        Return Convert.ToDouble(o)
    End Function

    Private Shared Function Fmt(value As Double) As String
        Return value.ToString("0.##", CultureInfo.InvariantCulture)
    End Function

End Class