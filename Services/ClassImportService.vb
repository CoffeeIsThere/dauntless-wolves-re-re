Imports MySql.Data.MySqlClient
Imports System.Globalization
Imports System.Text.RegularExpressions

' A class/roster import is committed as one unit, including the audit trail.
Public Class ClassImportService
    Public Function ImportClass(code As String, name As String, section As String, semester As String, year As String,
                                term As String, rows As List(Of String()), ByRef classId As Integer) As OperationResult
        classId = 0
        Dim importedId As Integer
        If Not AppSession.IsTeacher Then Return OperationResult.Fail("Teacher access is required.")
        code = code.Trim().ToUpperInvariant()
        name = name.Trim()
        section = section.Trim().ToUpperInvariant()
        If code.Length = 0 OrElse code.Length > 20 OrElse name.Length = 0 OrElse name.Length > 100 OrElse section.Length = 0 OrElse section.Length > 20 Then Return OperationResult.Fail("Enter a course code, name, and section (up to 20 characters).")
        If Not {"1st Semester", "2nd Semester"}.Contains(semester) Then Return OperationResult.Fail("Choose 1st Semester or 2nd Semester.")
        If Not Regex.IsMatch(year, "^\d{4}-\d{4}$") OrElse Integer.Parse(year.Substring(5)) <> Integer.Parse(year.Substring(0, 4)) + 1 Then Return OperationResult.Fail("Enter an academic year such as 2026-2027.")
        If Not {"Midterm", "Finals"}.Contains(term) Then Return OperationResult.Fail("Choose Midterm or Finals for imported assessments.")
        Dim titles As New List(Of String)
        Dim maxima As New List(Of Double)
        If rows IsNot Nothing Then
            If rows.Count < 2 OrElse rows(0).Length < 3 OrElse rows(0)(0).Trim().ToLowerInvariant() <> "student_id" OrElse rows(0)(1).Trim().ToLowerInvariant() <> "full_name" Then Return OperationResult.Fail("CSV needs student_id,full_name and assessment columns such as Quiz 1 /20.")
            For Each header In rows(0).Skip(2)
                Dim match = Regex.Match(header.Trim(), "^(.+?)\s*/\s*(\d+(?:\.\d+)?)$")
                Dim maximum As Double
                If Not match.Success OrElse Not Double.TryParse(match.Groups(2).Value, NumberStyles.Float, CultureInfo.InvariantCulture, maximum) OrElse Not ValidationHelper.HasTwoDecimalPlaces(maximum) OrElse maximum <= 0 OrElse maximum > 9999.99 Then Return OperationResult.Fail("Each assessment header needs a maximum: Quiz 1 /20.")
                Dim title = match.Groups(1).Value.Trim()
                If title.Length > 100 OrElse titles.Any(Function(existing) existing.Equals(title, StringComparison.OrdinalIgnoreCase)) Then Return OperationResult.Fail("Assessment titles must be unique and at most 100 characters.")
                titles.Add(title)
                maxima.Add(maximum)
            Next
            Dim ids As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each row In rows.Skip(1)
                If row.Length <> rows(0).Length OrElse String.IsNullOrWhiteSpace(row(1)) OrElse Not ids.Add(row(0).Trim()) Then Return OperationResult.Fail("Each CSV row needs a unique student ID, name, and the same number of columns as the header.")
                For i = 2 To row.Length - 1
                    Dim score As Double
                    If row(i).Trim() <> "" AndAlso (Not Double.TryParse(row(i), NumberStyles.Float, CultureInfo.InvariantCulture, score) OrElse Not ValidationHelper.HasTwoDecimalPlaces(score) OrElse score < 0 OrElse score > maxima(i - 2)) Then Return OperationResult.Fail("Invalid score for student " & row(0) & ", " & titles(i - 2))
                Next
            Next
        End If
        Try
            Using conn = DatabaseHelper.GetConnection()
                Using tx = conn.BeginTransaction()
                    Dim teacher = AppSession.CurrentUser.UserId
                    Using check As New MySqlCommand("SELECT COUNT(*) FROM teacher WHERE teacher_id=@Teacher AND status='Active'", conn, tx)
                        check.Parameters.AddWithValue("@Teacher", teacher)
                        If CInt(check.ExecuteScalar()) <> 1 Then Return OperationResult.Fail("An active professor account is required.")
                    End Using
                    Using cmd As New MySqlCommand("INSERT INTO course(course_code,course_name) VALUES(@Code,@Name) ON DUPLICATE KEY UPDATE course_code=course_code", conn, tx)
                        cmd.Parameters.AddWithValue("@Code", code)
                        cmd.Parameters.AddWithValue("@Name", name)
                        cmd.ExecuteNonQuery()
                    End Using
                    Using cmd As New MySqlCommand("INSERT INTO class(course_code,section,semester,academic_year,teacher_id) VALUES(@Code,@Section,@Semester,@Year,@Teacher)", conn, tx)
                        cmd.Parameters.AddWithValue("@Code", code)
                        cmd.Parameters.AddWithValue("@Section", section)
                        cmd.Parameters.AddWithValue("@Semester", semester)
                        cmd.Parameters.AddWithValue("@Year", year)
                        cmd.Parameters.AddWithValue("@Teacher", teacher)
                        cmd.ExecuteNonQuery()
                        importedId = CInt(cmd.LastInsertedId)
                    End Using
                    Dim assessments As New List(Of Integer)
                    For i = 0 To titles.Count - 1
                        Dim lower = titles(i).ToLowerInvariant()
                        Dim kind = If(lower.Contains("quiz"), "Quiz", If(lower.Contains("exam"), "Exam", If(lower.Contains("project"), "Project", "Activity")))
                        Using cmd As New MySqlCommand("INSERT INTO assessment(class_id,term,title,assessment_type,total_score,weight) VALUES(@Class,@Term,@Title,@Type,@Max,10)", conn, tx)
                            cmd.Parameters.AddWithValue("@Class", importedId)
                            cmd.Parameters.AddWithValue("@Term", term)
                            cmd.Parameters.AddWithValue("@Title", titles(i))
                            cmd.Parameters.AddWithValue("@Type", kind)
                            cmd.Parameters.AddWithValue("@Max", maxima(i))
                            cmd.ExecuteNonQuery()
                            assessments.Add(CInt(cmd.LastInsertedId))
                        End Using
                    Next
                    If rows IsNot Nothing Then
                        Dim grades As New GradeService()
                        For Each row In rows.Skip(1)
                            Using check As New MySqlCommand("SELECT full_name FROM student WHERE student_id=@Id AND status='Active'", conn, tx)
                                check.Parameters.AddWithValue("@Id", row(0).Trim())
                                Dim registeredName = Convert.ToString(check.ExecuteScalar())
                                If registeredName = "" Then Return OperationResult.Fail("Student " & row(0) & " must first be registered and active in Admin. Nothing was imported.")
                                If Not registeredName.Trim().Equals(row(1).Trim(), StringComparison.OrdinalIgnoreCase) Then Return OperationResult.Fail("Name does not match registered student " & row(0) & ". Nothing was imported.")
                            End Using
                            Dim enrollment As Integer
                            Using cmd As New MySqlCommand("INSERT INTO enrollment(student_id,class_id) VALUES(@Student,@Class)", conn, tx)
                                cmd.Parameters.AddWithValue("@Student", row(0).Trim())
                                cmd.Parameters.AddWithValue("@Class", importedId)
                                cmd.ExecuteNonQuery()
                                enrollment = CInt(cmd.LastInsertedId)
                            End Using
                            For i = 0 To assessments.Count - 1
                                Using cmd As New MySqlCommand("INSERT INTO studentgrade(enrollment_id,assessment_id) VALUES(@Enrollment,@Assessment)", conn, tx)
                                    cmd.Parameters.AddWithValue("@Enrollment", enrollment)
                                    cmd.Parameters.AddWithValue("@Assessment", assessments(i))
                                    cmd.ExecuteNonQuery()
                                End Using
                                If row(i + 2).Trim() <> "" Then
                                    Dim result = grades.RecordScoreTx(conn, tx, enrollment, assessments(i), Double.Parse(row(i + 2), CultureInfo.InvariantCulture), teacher, "Class CSV import")
                                    If Not result.Success Then Return result
                                End If
                            Next
                        Next
                    End If
                    AuditService.LogActionTx(conn, tx, "Teacher", teacher, "Created class", "Class", importedId.ToString(), Nothing, Nothing, "Class import: " & code & " - " & section)
                    tx.Commit()
                    classId = importedId
                    Return OperationResult.Ok("Class saved with " & titles.Count & " assessments.")
                End Using
            End Using
        Catch ex As Exception
            classId = 0
            Return OperationResult.Fail("Nothing was imported: " & ex.Message)
        End Try
    End Function
End Class
