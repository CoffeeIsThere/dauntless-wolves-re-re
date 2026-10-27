Imports MySql.Data.MySqlClient

Public Class RosterImportService
    Public Function ImportStudents(classId As Integer, teacher As String, rows As List(Of String())) As OperationResult
        If rows Is Nothing OrElse rows.Count = 0 Then Return OperationResult.Fail("CSV contains no students.")
        Dim ids As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each row In rows
            If row.Length <> 2 OrElse String.IsNullOrWhiteSpace(row(0)) OrElse String.IsNullOrWhiteSpace(row(1)) OrElse Not ids.Add(row(0).Trim()) Then Return OperationResult.Fail("Use unique student_id,full_name rows.")
        Next
        Try
            Using conn = DatabaseHelper.GetConnection()
                Using tx = conn.BeginTransaction()
                    If Not AccountAccess.IsActive(conn, tx, "Teacher", teacher) Then Return OperationResult.Fail("Your professor account is inactive.")
                    Using cmd As New MySqlCommand("SELECT teacher_id FROM class WHERE class_id=@Class FOR UPDATE", conn, tx)
                        cmd.Parameters.AddWithValue("@Class", classId)
                        If Convert.ToString(cmd.ExecuteScalar()) <> teacher Then Return OperationResult.Fail("You can only enroll into your own class.")
                    End Using
                    Dim added = 0
                    For Each row In rows
                        Using cmd As New MySqlCommand("SELECT full_name FROM student WHERE student_id=@Id AND status='Active' FOR UPDATE", conn, tx)
                            cmd.Parameters.AddWithValue("@Id", row(0).Trim())
                            If Not Convert.ToString(cmd.ExecuteScalar()).Equals(row(1).Trim(), StringComparison.OrdinalIgnoreCase) Then Return OperationResult.Fail("Student ID/name mismatch or inactive account: " & row(0) & ". Nothing imported.")
                        End Using
                        Using cmd As New MySqlCommand("SELECT enrollment_id FROM enrollment WHERE class_id=@Class AND student_id=@Id", conn, tx)
                            cmd.Parameters.AddWithValue("@Class", classId)
                            cmd.Parameters.AddWithValue("@Id", row(0).Trim())
                            If cmd.ExecuteScalar() IsNot Nothing Then Continue For
                        End Using
                        Dim enrollment As Long
                        Using cmd As New MySqlCommand("INSERT INTO enrollment(class_id,student_id) VALUES(@Class,@Id)", conn, tx)
                            cmd.Parameters.AddWithValue("@Class", classId)
                            cmd.Parameters.AddWithValue("@Id", row(0).Trim())
                            cmd.ExecuteNonQuery()
                            enrollment = cmd.LastInsertedId
                        End Using
                        Using cmd As New MySqlCommand("INSERT INTO studentgrade(enrollment_id,assessment_id) SELECT @Enrollment,assessment_id FROM assessment WHERE class_id=@Class", conn, tx)
                            cmd.Parameters.AddWithValue("@Enrollment", enrollment)
                            cmd.Parameters.AddWithValue("@Class", classId)
                            cmd.ExecuteNonQuery()
                        End Using
                        AuditService.LogActionTx(conn, tx, "Teacher", teacher, "Enrolled student", "Enrollment", enrollment.ToString(), Nothing, Nothing, "Roster CSV import: " & row(0))
                        added += 1
                    Next
                    tx.Commit()
                    Return OperationResult.Ok("Imported " & added & " students; existing enrollments were kept.")
                End Using
            End Using
        Catch ex As Exception
            Return OperationResult.Fail("Nothing imported: " & ex.Message)
        End Try
    End Function
End Class
