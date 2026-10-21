
Public Class frmStudent

    Private ReadOnly pages As New Dictionary(Of Button, Panel)
    Private ReadOnly boldFont As New Font("Segoe UI", 9.5F, FontStyle.Bold)
    Private ReadOnly normalFont As New Font("Segoe UI", 9.5F, FontStyle.Regular)

    ' Sample data: assessment -> current score. Replace with a MySQL query later.
    Private ReadOnly assessmentScores As New Dictionary(Of String, String)

    ' My Performance bar values (0-100)
    Private quizPct, activityPct, examPct, projectPct As Integer
    ' "course|term" -> avg, passed, pending, highest, quiz%, activity%, exam%, project%
    Private ReadOnly perfData As New Dictionary(Of String, Double())
    ' term, assessment, score, weight, running grade, status
    Private Const OverallLabel As String = "Overall (All Courses)"
    Private ReadOnly gradeRows As New List(Of String())
    Private Sub frmStudent_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If AppSession.CurrentUser Is Nothing Then
            MessageBox.Show("Please log in to access this page.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            Dim loginForm As New frmlogin()
            loginForm.Show()
            Me.Close()
            Return
        End If

        pages.Add(btnMyGrades, pnlMyGrades)
        pages.Add(btnGradeHistory, pnlGradeHistory)
        pages.Add(btnRequestRevision, pnlRequestRevision)
        pages.Add(btnMyPerformance, pnlMyPerformance)

        For Each b As Button In pages.Keys
            AddHandler b.Click, AddressOf Tab_Click
        Next

        ' My Grades
        cboCourse.Items.Clear()
        cboCourse.Items.Add("CS 103 - Object Oriented Programming")
        cboCourse.SelectedIndex = 0

        StyleGrid(dgvgrades)
        LoadGrades()

        ' Grade History
        StyleGrid(dgvHistory)
        LoadHistory()

        ' Request Revision
        StyleGrid(dgvRequests)
        LoadRequests()
        LoadAssessments()

        ' My Performance
        LoadPerformance()

        ShowPage(btnMyGrades)   ' keep this last
    End Sub

    ' class -> (assessment -> current score). Replace with a MySQL query later.
    Private ReadOnly classData As New Dictionary(Of String, Dictionary(Of String, String))

    ' Grids select their first cell when first displayed, so clear it here
    Private Sub frmStudent_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        dgvgrades.ClearSelection()
        dgvHistory.ClearSelection()
        dgvRequests.ClearSelection()
    End Sub

    Private Sub Tab_Click(sender As Object, e As EventArgs)
        ShowPage(DirectCast(sender, Button))
    End Sub

    Private Sub ShowPage(activeTab As Button)
        For Each kvp In pages
            Dim isActive As Boolean = (kvp.Key Is activeTab)
            kvp.Value.Visible = isActive
            kvp.Key.Font = If(isActive, boldFont, normalFont)
            kvp.Key.BackColor = If(isActive, Color.White, Color.FromArgb(208, 216, 224))
        Next
        pages(activeTab).BringToFront()

        ' Measure the bars once the Performance page is actually visible
        If activeTab Is btnMyPerformance Then UpdateBars()
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        AppSession.ClearSession()

        ' 2. Re-open login form and close current dashboard
        Dim loginForm As New frmlogin()
        loginForm.Show()
        Me.Close()
    End Sub

    ' ---------- SHARED GRID STYLE ----------

    Private Sub StyleGrid(grid As DataGridView)
        With grid
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .ReadOnly = True
            .RowHeadersVisible = False
            .BorderStyle = BorderStyle.None
            .BackgroundColor = Color.White
            .EnableHeadersVisualStyles = False
            .ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(208, 216, 224)
            .ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(0, 85, 0)
            .ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            .ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.ForeColor = Color.FromArgb(0, 85, 0)
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 224)
            .DefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 85, 0)
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            .RowTemplate.Height = 40
            .ColumnHeadersHeight = 40
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .AllowUserToResizeRows = False
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            .GridColor = Color.FromArgb(85, 85, 85)
        End With
    End Sub

    ' ---------- MY GRADES ----------

    Private Sub LoadGrades()
        If dgvgrades.Columns.Count = 0 Then
            For Each h In {"Assessment", "Score", "Weight", "Running Grade", "Status"}
                dgvgrades.Columns.Add(h, h)
            Next
        End If

        ' Sample data. Replace with MySQL later (each row needs its term).
        gradeRows.Clear()
        gradeRows.Add({"Midterm", "Quiz 1", "18/20", "10%", "90.0", "Passed"})
        gradeRows.Add({"Midterm", "Activity 2", "45/50", "15%", "90.0", "Passed"})
        gradeRows.Add({"Midterm", "Midterm Exam", "82/100", "25%", "82", "Passed"})
        gradeRows.Add({"Final", "Project Draft", "82/100", "20%", "85.6", "Passed"})
        gradeRows.Add({"Final", "Final Exam", "---", "30%", "Pending", "Not Yet"})

        If cboterm.Items.Count = 0 Then
            cboterm.Items.AddRange({"Midterm", "Final", "Combined"})
            cboterm.SelectedIndex = 2          ' triggers ShowGrades
        Else
            ShowGrades()
        End If
    End Sub

    Private Sub cboTerm_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboterm.SelectedIndexChanged
        If dgvgrades.Columns.Count > 0 Then ShowGrades()
    End Sub

    Private Sub ShowGrades()
        Dim term As String = If(cboterm.SelectedIndex = -1, "Combined", cboterm.Text)

        dgvgrades.Rows.Clear()
        For Each r In gradeRows
            If term = "Combined" OrElse r(0) = term Then
                dgvgrades.Rows.Add(r(1), r(2), r(3), r(4), r(5))
            End If
        Next
        dgvgrades.ClearSelection()
        UpdateStanding(term)
    End Sub

    ' Weighted average of graded items in the visible rows
    Private Sub UpdateStanding(term As String)
        Dim total As Double = 0
        Dim weights As Double = 0

        For Each row As DataGridViewRow In dgvgrades.Rows
            Dim parts = Convert.ToString(row.Cells(1).Value).Split("/"c)
            If parts.Length <> 2 Then Continue For   ' skips ungraded rows like "---"

            Dim score, maxScore, weight As Double
            If Double.TryParse(parts(0), score) AndAlso
           Double.TryParse(parts(1), maxScore) AndAlso maxScore > 0 AndAlso
           Double.TryParse(Convert.ToString(row.Cells(2).Value).TrimEnd("%"c), weight) Then
                total += score / maxScore * 100 * weight
                weights += weight
            End If
        Next

        Dim label As String = If(term = "Combined", "Current Standing", term & " Standing")
        If weights > 0 Then
            Dim pct As Double = Math.Round(total / weights, 1)
            ' 75 is an assumed passing mark. Use your school's.
            lblstanding.Text = label & ": " & pct & "% " & If(pct >= 75, "(Passing)", "(Failing)")
        Else
            lblstanding.Text = label & ": no grades yet"
        End If
    End Sub

    Private Sub dgvGrades_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvgrades.CellFormatting
        If e.ColumnIndex >= 3 AndAlso e.Value IsNot Nothing Then
            Dim t = e.Value.ToString()
            If t = "Passed" OrElse IsNumeric(t) Then
                e.CellStyle.BackColor = Color.FromArgb(153, 190, 165)
            ElseIf t = "Pending" OrElse t = "Not Yet" Then
                e.CellStyle.BackColor = Color.FromArgb(238, 220, 170)
            End If
        End If
    End Sub

    ' ---------- GRADE HISTORY ----------

    Private Sub LoadHistory()
        dgvHistory.Rows.Clear()
        If dgvHistory.Columns.Count = 0 Then
            For Each h In {"Date", "Assessment", "Old Score", "New Score", "Changed by", "Reason"}
                dgvHistory.Columns.Add(h, h)
            Next
        End If
        ' Sample data. Replace with MySQL later.
        dgvHistory.Rows.Add("2026-08-18", "Quiz 1", "16/20", "18/20", "Prof. Cruz", "Correction of misread answer")
        dgvHistory.Rows.Add("2026-08-15", "Activity 2", "43/50", "45/50", "Prof. Cruz", "Late submission accepted")
        dgvHistory.Rows.Add("2026-08-20", "Midterm Exam", "78/100", "82/100", "Prof. Cruz", "Recheck of Items")
        dgvHistory.Rows.Add("2026-08-10", "Project Draft", "---", "82/100", "Prof. Cruz", "Initial Entry")

        dgvHistory.ClearSelection()
    End Sub

    ' ---------- REQUEST REVISION ----------

    Private Sub LoadAssessments()
        classData.Clear()
        classData.Add("CS 103 - Object Oriented Programming", New Dictionary(Of String, String) From {
        {"Quiz 1", "18/20"}, {"Activity 2", "45/50"},
        {"Midterm Exam", "82/100"}, {"Project Draft", "82/100"}})
        classData.Add("CS 101 - Intro to Computing", New Dictionary(Of String, String) From {
        {"Quiz 1", "9/10"}, {"Seatwork 1", "18/20"}})

        cboclass.Items.Clear()
        For Each className In classData.Keys
            cboclass.Items.Add(className)
        Next
        cboclass.SelectedIndex = -1

        cboAssessment.Items.Clear()
        cboAssessment.Enabled = False
        txtCurrentGrade.Clear()
    End Sub

    Private Sub cboAssessment_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAssessment.SelectedIndexChanged
        Dim score As String = ""
        If cboAssessment.SelectedIndex <> -1 AndAlso
           assessmentScores.TryGetValue(cboAssessment.Text, score) Then
            txtCurrentGrade.Text = score
        Else
            txtCurrentGrade.Clear()
        End If
    End Sub

    Private Sub LoadRequests()
        dgvRequests.Rows.Clear()
        If dgvRequests.Columns.Count = 0 Then
            For Each h In {"Assessment", "Requested", "Status"}
                dgvRequests.Columns.Add(h, h)
            Next
        End If
        ' Sample data. Replace with MySQL later.
        dgvRequests.Rows.Add("Quiz 1", "+2 pts", "Pending")
        dgvRequests.Rows.Add("Activity 2", "+5 pts", "Approved")
        dgvRequests.Rows.Add("Midterm Exam", "Re-check", "Under Review")

        dgvRequests.ClearSelection()
    End Sub

    Private Sub btnSubmitRequest_Click(sender As Object, e As EventArgs) Handles btnSubmitRequest.Click
        If cboAssessment.SelectedIndex = -1 Then
            MessageBox.Show("Please select an assessment.", "Missing info",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If String.IsNullOrWhiteSpace(txtReason.Text) Then
            MessageBox.Show("Please enter a reason for your request.", "Missing info",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtReason.Focus()
            Return
        End If

        ' Block duplicate open requests for the same assessment
        For Each row As DataGridViewRow In dgvRequests.Rows
            Dim status = row.Cells(2).Value.ToString()
            If row.Cells(0).Value.ToString() = cboAssessment.Text AndAlso
               (status = "Pending" OrElse status = "Under Review") Then
                MessageBox.Show("You already have an open request for this assessment.",
                                "Duplicate request", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
        Next

        ' Later: INSERT into the requests table here, then call LoadRequests()
        dgvRequests.Rows.Add(cboAssessment.Text, "Re-check", "Pending")
        dgvRequests.ClearSelection()

        cboAssessment.SelectedIndex = -1
        txtReason.Clear()
        MessageBox.Show("Request submitted!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub dgvRequests_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvRequests.CellFormatting
        If e.ColumnIndex = 2 AndAlso e.Value IsNot Nothing Then
            Select Case e.Value.ToString()
                Case "Pending"
                    e.CellStyle.BackColor = Color.FromArgb(238, 220, 170)
                Case "Approved"
                    e.CellStyle.BackColor = Color.FromArgb(153, 190, 165)
                Case "Under Review"
                    e.CellStyle.BackColor = Color.FromArgb(155, 200, 225)
                Case "Rejected"
                    e.CellStyle.BackColor = Color.FromArgb(225, 160, 160)
            End Select
        End If
    End Sub

    ' ---------- MY PERFORMANCE ----------

    Private Sub LoadPerformance()
        ' Sample data. Replace with values computed from MySQL later.
        perfData.Clear()
        perfData.Add("CS 103 - Object Oriented Programming|Midterm", {86.0, 7, 0, 90.0, 90, 90, 82, 0})
        perfData.Add("CS 103 - Object Oriented Programming|Final", {82.5, 5, 2, 85.6, 0, 0, 0, 86})
        perfData.Add("CS 103 - Object Oriented Programming|Combined", {84.2, 12, 2, 88.5, 90, 90, 83, 86})
        perfData.Add("CS 101 - Intro to Computing|Midterm", {91.0, 4, 0, 95.0, 90, 0, 92, 0})
        perfData.Add("CS 101 - Intro to Computing|Final", {88.0, 3, 1, 92.0, 0, 90, 0, 88})
        perfData.Add("CS 101 - Intro to Computing|Combined", {89.5, 7, 1, 95.0, 90, 90, 92, 88})

        cboperfterm.Items.Clear()
        cboperfterm.Items.AddRange({"Midterm", "Final", "Combined"})

        cboperfcourse.Items.Clear()
        cboperfcourse.Items.Add(OverallLabel)
        cboperfcourse.Items.AddRange({"CS 103 - Object Oriented Programming", "CS 101 - Intro to Computing"})

        cboperfterm.SelectedIndex = 2       ' Combined
        cboperfcourse.SelectedIndex = 0     ' Overall, triggers ShowPerformance
    End Sub

    Private Sub cboPerfTerm_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboperfterm.SelectedIndexChanged
        ShowPerformance()
    End Sub

    Private Sub cboPerfCourse_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboperfcourse.SelectedIndexChanged
        ShowPerformance()
    End Sub

    Private Sub ShowPerformance()
        If cboperfterm.SelectedIndex = -1 OrElse cboperfcourse.SelectedIndex = -1 Then Return

        Dim v As Double() = Nothing
        If cboperfcourse.Text = OverallLabel Then
            v = CombineCourses(cboperfterm.Text)
        Else
            perfData.TryGetValue(cboperfcourse.Text & "|" & cboperfterm.Text, v)
        End If

        If v IsNot Nothing Then
            SetStat(lblAverageValue, v(0).ToString("0.0"), "CURRENT AVERAGE")
            SetStat(lblPassedValue, CInt(v(1)).ToString(), "ACTIVITIES PASSED")
            SetStat(lblPendingValue, CInt(v(2)).ToString(), "PENDING")
            SetStat(lblHighestValue, v(3).ToString("0.0"), "HIGHEST SCORE")
            quizPct = CInt(v(4))
            activityPct = CInt(v(5))
            examPct = CInt(v(6))
            projectPct = CInt(v(7))
        Else
            SetStat(lblAverageValue, "--", "CURRENT AVERAGE")
            SetStat(lblPassedValue, "--", "ACTIVITIES PASSED")
            SetStat(lblPendingValue, "--", "PENDING")
            SetStat(lblHighestValue, "--", "HIGHEST SCORE")
            quizPct = 0 : activityPct = 0 : examPct = 0 : projectPct = 0
        End If
        UpdateBars()
    End Sub

    ' Combines all courses for one term into the same 8 values
    Private Function CombineCourses(term As String) As Double()
        Dim rows = perfData.Where(Function(kv) kv.Key.EndsWith("|" & term)).
                        Select(Function(kv) kv.Value).ToList()
        If rows.Count = 0 Then Return Nothing

        Dim result(7) As Double
        result(0) = rows.Average(Function(r) r(0))   ' average of the course averages
        result(1) = rows.Sum(Function(r) r(1))       ' passed: total
        result(2) = rows.Sum(Function(r) r(2))       ' pending: total
        result(3) = rows.Max(Function(r) r(3))       ' highest score overall

        ' Category bars: average only the courses that have grades in that category
        For i As Integer = 4 To 7
            Dim idx As Integer = i
            Dim withData = rows.Where(Function(r) r(idx) > 0).ToList()
            result(i) = If(withData.Count = 0, 0, withData.Average(Function(r) r(idx)))
        Next
        Return result
    End Function

    ' Each stat card has two labels. This finds the pair inside the card of
    ' "anyLabel", puts the number on the one with the bigger font and the
    ' caption on the other, so it works whichever label is named what.
    Private Sub SetStat(anyLabel As Label, value As String, caption As String)
        Dim other As Label = Nothing
        If anyLabel.Parent IsNot Nothing Then
            For Each c As Control In anyLabel.Parent.Controls
                If TypeOf c Is Label AndAlso c IsNot anyLabel Then
                    other = DirectCast(c, Label)
                    Exit For
                End If
            Next
        End If

        If other Is Nothing Then
            anyLabel.Text = value   ' card only has one label
            Return
        End If

        Dim bigLabel As Label = If(anyLabel.Font.Size >= other.Font.Size, anyLabel, other)
        Dim smallLabel As Label = If(bigLabel Is anyLabel, other, anyLabel)

        bigLabel.Text = value
        smallLabel.Text = caption
    End Sub

    Private Sub UpdateBars()
        SetBar(trackQuiz, fillQuiz, lblQuizPct, quizPct)
        SetBar(trackActivity, fillactivity, lblactivitypct, activityPct)
        SetBar(trackExam, fillexam, lblexampct, examPct)
        SetBar(trackProject, fillproject, lblprojectpct, projectPct)
    End Sub

    Private Sub SetBar(track As Panel, fill As Panel, lbl As Label, pct As Integer)
        pct = Math.Max(0, Math.Min(100, pct))
        Dim usable As Integer = track.ClientSize.Width - track.Padding.Horizontal
        fill.Visible = (pct > 0)
        fill.Width = Math.Max(fill.Height, CInt(usable * pct / 100))
        lbl.Text = pct & "%"
    End Sub

    Private Sub tlpBars_SizeChanged(sender As Object, e As EventArgs) Handles tlpbars.SizeChanged
        UpdateBars()
    End Sub

    Private Sub lblHighestCaption_Click(sender As Object, e As EventArgs) Handles lblHighestCaption.Click

    End Sub

    Private Sub lblactivitypct_Click(sender As Object, e As EventArgs) Handles lblactivitypct.Click

    End Sub

    Private Sub lblPassedValue_Click(sender As Object, e As EventArgs) Handles lblPassedValue.Click

    End Sub

    Private Sub lblHighestValue_Click(sender As Object, e As EventArgs) Handles lblHighestValue.Click

    End Sub

    Private Sub lblAverageValue_Click(sender As Object, e As EventArgs) Handles lblAverageValue.Click

    End Sub

    Private Sub lblrole_Click(sender As Object, e As EventArgs) Handles lblrole.Click

    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click

    End Sub

    Private Sub RoundedPanel1_Paint(sender As Object, e As PaintEventArgs) Handles RoundedPanel1.Paint

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub cboCourse_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboCourse.SelectedIndexChanged

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub

    Private Sub lblstanding_Click(sender As Object, e As EventArgs) Handles lblstanding.Click

    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click

    End Sub

    Private Sub PictureBox5_Click(sender As Object, e As EventArgs) Handles PictureBox5.Click

    End Sub

    Private Sub Label9_Click(sender As Object, e As EventArgs) Handles Label9.Click

    End Sub

    Private Sub PictureBox6_Click(sender As Object, e As EventArgs) Handles PictureBox6.Click

    End Sub

    Private Sub Label10_Click(sender As Object, e As EventArgs) Handles Label10.Click

    End Sub

    Private Sub lblPendingCaption_Click(sender As Object, e As EventArgs) Handles lblPendingCaption.Click

    End Sub

    Private Sub lblPendingValue_Click(sender As Object, e As EventArgs) Handles lblPendingValue.Click

    End Sub

    Private Sub lblPassedCaption_Click(sender As Object, e As EventArgs) Handles lblPassedCaption.Click

    End Sub

    Private Sub pnlheader_Paint(sender As Object, e As PaintEventArgs) Handles pnlheader.Paint

    End Sub

    Private Sub cboClass_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboclass.SelectedIndexChanged
        cboAssessment.Items.Clear()
        txtCurrentGrade.Clear()

        Dim items As Dictionary(Of String, String) = Nothing
        If cboclass.SelectedIndex <> -1 AndAlso classData.TryGetValue(cboclass.Text, items) Then
            For Each a In items.Keys
                cboAssessment.Items.Add(a)
            Next
            cboAssessment.Enabled = True
        Else
            cboAssessment.Enabled = False
        End If

        ' Show only requests for the chosen class (all rows when none is chosen)
        Dim code As String = If(cboclass.SelectedIndex = -1, "", ClassCode(cboclass.Text))
        dgvRequests.CurrentCell = Nothing
        For Each row As DataGridViewRow In dgvRequests.Rows
            row.Visible = (code = "" OrElse Convert.ToString(row.Cells(0).Value) = code)
        Next
    End Sub

    ' "CS 103 - Object Oriented Programming" -> "CS 103"
    Private Function ClassCode(fullName As String) As String
        Return fullName.Split("-"c)(0).Trim()
    End Function

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboperfterm.SelectedIndexChanged

    End Sub
End Class