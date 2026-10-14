Imports MySql.Data.MySqlClient
Imports System.Data
Imports DAUNTLESS_WOLVES_PROF.Core
Imports DAUNTLESS_WOLVES_PROF.Models
Imports System.IO

Public Class GradeService

    Public Function CalculateScorePercentage(score As Double, totalScore As Double) As Double
        If totalScore <= 0 Then Return 0.0
        Return Math.Round((score / totalScore) * 100, 2)
    End Function

    Public Function DetermineRemarks(percentage As Double) As String
        Return If(percentage >= 75.0, "Passed", "Needs Improvement")
    End Function


    Public Function RecordScore(enrollmentId As Integer, assessmentId As Integer, newScore As Double, teacherId As String, Optional reason As String = "Grade entry") As Boolean
        Using conn As MySqlConnection = DatabaseHelper.GetConnection()
            Using tx As MySqlTransaction = conn.BeginTransaction()
                Try

                    Dim oldScoreStr As String = "NULL"
                    Dim checkSql As String = "SELECT score FROM studentgrade WHERE enrollment_id = @EnrollmentId AND assessment_id = @AssessmentId"
                    Using cmdCheck As New MySqlCommand(checkSql, conn, tx)
                        cmdCheck.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
                        cmdCheck.Parameters.AddWithValue("@AssessmentId", assessmentId)
                        Dim result = cmdCheck.ExecuteScalar()
                        If result IsNot Nothing AndAlso Not Convert.IsDBNull(result) Then
                            oldScoreStr = Convert.ToDouble(result).ToString("F2")
                        End If
                    End Using


                    Dim upsertSql As String = "INSERT INTO studentgrade (enrollment_id, assessment_id, score, remarks) " &
                                             "VALUES (@EnrollmentId, @AssessmentId, @Score, @Remarks) " &
                                             "ON DUPLICATE KEY UPDATE score = VALUES(score), remarks = VALUES(remarks), updated_at = NOW()"

                    Dim percentage As Double = 0.0

                    Dim totalScoreSql As String = "SELECT total_score FROM assessment WHERE assessment_id = @AssessmentId"
                    Using cmdTotal As New MySqlCommand(totalScoreSql, conn, tx)
                        cmdTotal.Parameters.AddWithValue("@AssessmentId", assessmentId)
                        Dim tot = Convert.ToDouble(cmdTotal.ExecuteScalar())
                        percentage = CalculateScorePercentage(newScore, tot)
                    End Using

                    Using cmdUpsert As New MySqlCommand(upsertSql, conn, tx)
                        cmdUpsert.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
                        cmdUpsert.Parameters.AddWithValue("@AssessmentId", assessmentId)
                        cmdUpsert.Parameters.AddWithValue("@Score", newScore)
                        cmdUpsert.Parameters.AddWithValue("@Remarks", DetermineRemarks(percentage))
                        cmdUpsert.ExecuteNonQuery()
                    End Using


                    Dim runningGrade As Double = CalculateRunningGradeTx(enrollmentId, conn, tx)
                    Dim updateEnrollSql As String = "UPDATE enrollment SET running_grade = @RunningGrade WHERE enrollment_id = @EnrollmentId"
                    Using cmdEnroll As New MySqlCommand(updateEnrollSql, conn, tx)
                        cmdEnroll.Parameters.AddWithValue("@RunningGrade", runningGrade)
                        cmdEnroll.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
                        cmdEnroll.ExecuteNonQuery()
                    End Using


                    Dim gradeId As String = ""
                    Dim gradeIdSql As String = "SELECT grade_id FROM studentgrade WHERE enrollment_id = @EnrollmentId AND assessment_id = @AssessmentId"
                    Using cmdGId As New MySqlCommand(gradeIdSql, conn, tx)
                        cmdGId.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
                        cmdGId.Parameters.AddWithValue("@AssessmentId", assessmentId)
                        gradeId = cmdGId.ExecuteScalar().ToString()
                    End Using


                    AuditService.LogAction(
                        actorRole:="Teacher",
                        actorUserId:=teacherId,
                        action:="RECORD_SCORE",
                        entityType:="studentgrade",
                        entityId:=gradeId,
                        oldValue:=oldScoreStr,
                        newValue:=newScore.ToString("F2"),
                        details:=reason
                    )

                    tx.Commit()
                    Return True
                Catch ex As Exception
                    tx.Rollback()
                    Console.WriteLine("[GRADE RECORD TRANSACTION ERROR] " & ex.Message)
                    Return False
                End Try
            End Using
        End Using
    End Function

    Private Function CalculateRunningGradeTx(enrollmentId As Integer, conn As MySqlConnection, tx As MySqlTransaction) As Double
        Dim sql As String = "SELECT sg.score, a.total_score, a.weight " &
                            "FROM studentgrade sg " &
                            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                            "WHERE sg.enrollment_id = @EnrollmentId AND sg.score IS NOT NULL"

        Dim totalWeightedScore As Double = 0.0
        Dim totalWeightEarned As Double = 0.0

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@EnrollmentId", enrollmentId)
            Using reader As MySqlDataReader = cmd.ExecuteReader()
                While reader.Read()
                    Dim score As Double = reader.GetDouble("score")
                    Dim totalScore As Double = reader.GetDouble("total_score")
                    Dim weight As Double = reader.GetDouble("weight")

                    If totalScore > 0 Then
                        totalWeightedScore += (score / totalScore) * weight
                        totalWeightEarned += weight
                    End If
                End While
            End Using
        End Using

        If totalWeightEarned = 0 Then Return 0.0
        Return Math.Round((totalWeightedScore / totalWeightEarned) * 100, 2)
    End Function

    Public Function GetGradesByEnrollment(enrollmentId As Integer) As DataTable
        Dim sql As String = "SELECT sg.grade_id, a.term, a.title, a.type, sg.score, a.total_score, a.weight, sg.remarks, sg.updated_at " &
                            "FROM studentgrade sg " &
                            "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                            "WHERE sg.enrollment_id = @EnrollmentId " &
                            "ORDER BY a.term DESC, a.created_at ASC"

        Dim params As MySqlParameter() = {
            New MySqlParameter("@EnrollmentId", enrollmentId)
        }
        Return DatabaseHelper.ExecuteQuery(sql, params)
    End Function

    Public Function BulkImportGradesFromCSV(classId As Integer, assessmentId As Integer, filePath As String, teacherId As String) As (SuccessCount As Integer, FailCount As Integer)
        Dim successCount As Integer = 0
        Dim failCount As Integer = 0

        If Not File.Exists(filePath) Then Return (0, 0)

        Dim lines() As String = File.ReadAllLines(filePath)

        ' Skip Row 0 (Header Line)
        For i As Integer = 1 To lines.Length - 1
            Dim line As String = lines(i).Trim()
            If String.IsNullOrWhiteSpace(line) Then Continue For

            Dim parts() As String = line.Split(","c)
            If parts.Length >= 2 Then
                Dim studentId As String = parts(0).Trim().Replace("""", "")
                Dim scoreVal As Double
                Dim remarks As String = If(parts.Length >= 3, parts(2).Trim().Replace("""", ""), "Bulk CSV Upload")

                If Double.TryParse(parts(1).Trim().Replace("""", ""), scoreVal) Then
                    ' 1. Lookup student enrollment ID for this class
                    Dim enrollmentId As Integer = GetEnrollmentIdByStudentAndClass(studentId, classId)

                    If enrollmentId > 0 Then
                        ' 2. Re-use existing RecordScore transaction
                        Dim recorded As Boolean = RecordScore(enrollmentId, assessmentId, scoreVal, teacherId, "Bulk CSV Upload")
                        If recorded Then
                            successCount += 1
                        Else
                            failCount += 1
                        End If
                    Else
                        failCount += 1 ' Student not enrolled in class
                    End If
                Else
                    failCount += 1 ' Non-numeric score value
                End If
            End If
        Next

        Return (successCount, failCount)
    End Function

    Private Function GetEnrollmentIdByStudentAndClass(studentId As String, classId As Integer) As Integer
        Dim sql As String = "SELECT enrollment_id FROM enrollment WHERE student_id = @StudentId AND class_id = @ClassId"
        Dim params As MySqlParameter() = {
            New MySqlParameter("@StudentId", studentId),
            New MySqlParameter("@ClassId", classId)
        }
        Dim result = DatabaseHelper.ExecuteScalar(sql, params)
        If result IsNot Nothing AndAlso Not Convert.IsDBNull(result) Then
            Return Convert.ToInt32(result)
        End If
        Return 0
    End Function

End Class