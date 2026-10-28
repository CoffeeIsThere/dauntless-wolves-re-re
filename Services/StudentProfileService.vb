Imports System.Data
Imports MySql.Data.MySqlClient

' Read-only snapshot, always scoped to the authenticated student.
Public Class StudentProfileService
    Public Function LoadCurrent() As StudentProfileSnapshot
        If Not AppSession.IsStudent Then Throw New UnauthorizedAccessException("Student access is required.")
        Dim studentId = AppSession.CurrentUser.UserId
        Using conn = DatabaseHelper.GetConnection()
            Using tx = conn.BeginTransaction(IsolationLevel.RepeatableRead)
                Dim profile = ReadTable(conn, tx, "SELECT student_id,full_name,email,program,year_level,section,status FROM student WHERE student_id=@Student", studentId)
                If profile.Rows.Count <> 1 Then Throw New InvalidOperationException("Student profile not found.")
                Dim grades = ReadTable(conn, tx,
                    "SELECT e.enrollment_id,c.course_code,co.course_name,c.section,c.semester,c.academic_year,sg.score,a.total_score,a.weight " &
                    "FROM enrollment e JOIN class c ON c.class_id=e.class_id JOIN course co ON co.course_code=c.course_code " &
                    "LEFT JOIN assessment a ON a.class_id=c.class_id LEFT JOIN studentgrade sg ON sg.enrollment_id=e.enrollment_id AND sg.assessment_id=a.assessment_id " &
                    "WHERE e.student_id=@Student ORDER BY c.course_code,c.academic_year,c.semester,c.section", studentId)
                Dim requests = ReadTable(conn, tx,
                    "SELECT COUNT(*) AS open_count FROM revisionrequest r JOIN studentgrade sg ON sg.grade_id=r.grade_id " &
                    "JOIN enrollment e ON e.enrollment_id=sg.enrollment_id WHERE e.student_id=@Student AND r.status IN ('Pending','Under Review')", studentId)
                Dim classes As New DataTable()
                classes.Columns.Add("Class", GetType(String))
                classes.Columns.Add("Running Grade (%)", GetType(Double))
                classes.Columns.Add("Standing", GetType(String))
                For Each group In grades.AsEnumerable().GroupBy(Function(row) CInt(row("enrollment_id")))
                    Dim first = group.First()
                    Dim items = group.Where(Function(row) Not IsDBNull(row("total_score")) AndAlso Not IsDBNull(row("weight"))).
                        Select(Function(row) New ScoreItem(If(IsDBNull(row("score")), CType(Nothing, Double?), CDbl(row("score"))), CDbl(row("total_score")), CDbl(row("weight"))))
                    Dim grade = GradeCalculator.CalculateRunningGrade(items)
                    Dim label = first("course_code").ToString() & " — " & first("course_name").ToString() & " | " & first("section").ToString() & " | " & first("semester").ToString() & " | " & first("academic_year").ToString()
                    classes.Rows.Add(label, If(grade.HasValue, CType(grade.GetValueOrDefault(), Object), DBNull.Value), GradeCalculator.GetStatus(grade))
                Next
                tx.Commit()
                Return New StudentProfileSnapshot With {.Profile = profile.Rows(0), .Classes = classes, .OpenRequests = CInt(requests.Rows(0)("open_count"))}
            End Using
        End Using
    End Function

    Private Shared Function ReadTable(conn As MySqlConnection, tx As MySqlTransaction, sql As String, id As String) As DataTable
        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@Student", id)
            Using adapter As New MySqlDataAdapter(cmd)
                Dim table As New DataTable()
                adapter.Fill(table)
                Return table
            End Using
        End Using
    End Function
End Class

Public Class StudentProfileSnapshot
    Public Property Profile As DataRow
    Public Property Classes As DataTable
    Public Property OpenRequests As Integer
End Class
