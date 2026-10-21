Imports System.Globalization
Public Class frmprof

    Private ReadOnly _gradeService As New GradeService()
    Private ReadOnly _revisionService As New RevisionRequestService()
    Private ReadOnly _auditService As New AuditService()

    Private _currentTeacherId As String = "TCH001"

    ' Maps each tab button to the panel it should show
    Private ReadOnly pages As New Dictionary(Of Button, Panel)

    Private ReadOnly boldFont As New Font("Segoe UI", 9.5F, FontStyle.Bold)
    Private ReadOnly normalFont As New Font("Segoe UI", 9.5F, FontStyle.Regular)
    Private isLoading As Boolean = True

    ' Analytics tile value labels (created in code)
    Private lblClassAvg As Label
    Private lblPassing As Label
    Private lblAtRisk As Label
    Private lblTopScore As Label

    Private Sub frmprof_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If AppSession.CurrentUser Is Nothing Then
            MessageBox.Show("Please log in to access this page.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            Dim loginForm As New frmlogin()
            loginForm.Show()
            Me.Close()
            Return
        End If

        If AppSession.CurrentUser IsNot Nothing Then
            _currentTeacherId = AppSession.CurrentUser.UserId
        End If

        ' Connect buttons to their corresponding panels
        pages.Add(btnRecordScores, pnlRecordScores)
        pages.Add(btnClassGrades, pnlClassGrades)
        pages.Add(btnRevisionRequests, pnlClarificationRequests)
        pages.Add(btnAnalytics, pnlAnalytics)
        pages.Add(btnAuditLogs, pnlAuditLogs)

        ' Add the same Click event to all buttons
        For Each b As Button In pages.Keys
            AddHandler b.Click, AddressOf Tab_Click
        Next

        ' Show Record Scores by default
        ShowPage(btnRecordScores)

        ' Select the first item in all ComboBoxes (including inside panels)
        ' Replace SetAllComboBoxes(Me) with this call:
        LoadTeacherInfo()

        isLoading = False
        ConfigureGrid()
        ConfigureRequestsGrid()
        ConfigureAuditGrid()
        BuildAnalyticsLabels()
    End Sub

    ' Helper function: loops through all controls, including nested ones
    Private Sub SetAllComboBoxes(parent As Control)
        For Each ctrl As Control In parent.Controls
            If TypeOf ctrl Is ComboBox Then
                Dim cb As ComboBox = DirectCast(ctrl, ComboBox)
                If cb.Items.Count > 0 Then
                    cb.SelectedIndex = 0
                End If
            End If

            ' If the control contains children (Panel, GroupBox, TabPage, etc.), check inside too
            If ctrl.HasChildren Then
                SetAllComboBoxes(ctrl)
            End If
        Next
    End Sub

    Private Sub Tab_Click(sender As Object, e As EventArgs)
        Dim clickedButton As Button = DirectCast(sender, Button)
        ShowPage(clickedButton)
    End Sub

    Private Sub ShowPage(activeTab As Button)
        For Each kvp In pages
            Dim isActive As Boolean = (kvp.Key Is activeTab)

            ' Show/hide the corresponding panel
            kvp.Value.Visible = isActive

            ' Change button font
            kvp.Key.Font = If(isActive, boldFont, normalFont)

            ' Change button background
            kvp.Key.BackColor = If(isActive, Color.White, Color.FromArgb(208, 216, 224))
        Next

        ' Put the selected panel in front
        pages(activeTab).BringToFront()

        ' Load data for tabs that have data
        If Not isLoading Then
            If activeTab Is btnClassGrades Then LoadClassGrades()
            If activeTab Is btnRevisionRequests Then LoadRequests()
            If activeTab Is btnAuditLogs Then LoadAuditLogs()
            If activeTab Is btnAnalytics Then
                LayoutAnalytics()
                LoadAnalytics()
            End If
        End If
    End Sub

    ' =====================================================
    '  CLASS GRADES
    ' =====================================================

    Private Sub ConfigureGrid()
        With dgvClassGrades
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .AllowUserToResizeColumns = False
            .ReadOnly = True
            .RowHeadersVisible = False
            .ColumnHeadersVisible = False          ' the green bar is the header
            .BackgroundColor = Color.White
            .BorderStyle = BorderStyle.None
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .RowTemplate.Height = 55
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            .DefaultCellStyle.SelectionBackColor = Color.White
            .DefaultCellStyle.SelectionForeColor = Color.Black
            .ScrollBars = ScrollBars.Vertical
        End With
    End Sub

    Private Sub LoadClassGrades()
        Dim dt As New DataTable()
        For Each c In {"Student", "SW1", "HW1", "Quiz1", "Act4", "Act5", "Act6",
                       "Midterms", "Running Grade", "Status"}
            dt.Columns.Add(c)
        Next

        dt.Rows.Add("Oropilla, Charles", "20/20", "45/50", "96/100", "–", "–", "–", "95/100", "94.2", "Passed")
        dt.Rows.Add("Puyat, Ron Jacob", "19/20", "47/50", "94/100", "–", "–", "–", "92/100", "91.8", "Passed")
        dt.Rows.Add("Lalata, Carl Jade", "18/20", "46/50", "90/100", "–", "–", "–", "90/100", "88.5", "Passed")
        dt.Rows.Add("Mariano, Geoff Andrei", "17/20", "44/50", "88/100", "–", "–", "–", "88/100", "86.0", "Passed")
        dt.Rows.Add("Condes, Hope Gian", "18/20", "45/50", "88/100", "–", "–", "–", "88/100", "84.2", "Passed")
        dt.Rows.Add("Dela Cruz, Juan", "12/20", "32/50", "70/100", "–", "–", "–", "70/100", "68.5", "At risk")

        dgvClassGrades.DataSource = dt
        AlignGridToHeader()
    End Sub

    Private Function HeaderCenterX(ctrl As Control) As Integer
        Dim screenPt As Point = ctrl.Parent.PointToScreen(
            New Point(ctrl.Left + ctrl.Width \ 2, 0))
        Return dgvClassGrades.PointToClient(screenPt).X
    End Function

    Private Sub AlignGridToHeader()
        If dgvClassGrades.Columns.Count = 0 Then Exit Sub

        Dim cSW1 As Integer = HeaderCenterX(cboSW1)
        Dim cHW1 As Integer = HeaderCenterX(cboHW1)
        Dim cQuiz As Integer = HeaderCenterX(cboQuiz1)
        Dim cMid As Integer = HeaderCenterX(lblMidterms)
        Dim cRun As Integer = HeaderCenterX(lblRunningGrade)
        Dim cStat As Integer = HeaderCenterX(lblStatus)
        Dim gap As Double = (cMid - cQuiz) / 4

        Dim names() As String = {"Student", "SW1", "HW1", "Quiz1", "Act4", "Act5", "Act6",
                                 "Midterms", "Running Grade", "Status"}
        Dim centers() As Double = {0, cSW1, cHW1, cQuiz,
                                   cQuiz + gap, cQuiz + 2 * gap, cQuiz + 3 * gap,
                                   cMid, cRun, cStat}

        Dim startX As Double = 0
        For i As Integer = 0 To names.Length - 2          ' every column except Status
            Dim endX As Double
            If i = 0 Then
                endX = cSW1 - 55                          ' Student column width
            Else
                endX = (centers(i) + centers(i + 1)) / 2
            End If

            With dgvClassGrades.Columns(names(i))
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                .Width = CInt(Math.Max(endX - startX, 40))
                .DefaultCellStyle.Alignment = If(i = 0,
                    DataGridViewContentAlignment.MiddleLeft,
                    DataGridViewContentAlignment.MiddleCenter)
            End With
            startX = endX
        Next

        ' Status takes whatever space is left, so no horizontal scrollbar
        With dgvClassGrades.Columns("Status")
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End With
    End Sub

    Private Sub dgvClassGrades_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) _
        Handles dgvClassGrades.CellFormatting

        If e.Value Is Nothing OrElse e.ColumnIndex < 0 Then Exit Sub
        Dim colName As String = dgvClassGrades.Columns(e.ColumnIndex).Name

        If colName = "Status" Then
            If e.Value.ToString() = "At risk" Then
                e.CellStyle.BackColor = Color.FromArgb(248, 200, 200)
                e.CellStyle.ForeColor = Color.DarkRed
            Else
                e.CellStyle.BackColor = Color.FromArgb(190, 235, 200)
                e.CellStyle.ForeColor = Color.DarkGreen
            End If
            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor

        ElseIf colName = "Running Grade" Then
            Dim g As Double
            If Double.TryParse(e.Value.ToString(), g) Then
                e.CellStyle.BackColor = If(g < 75, Color.FromArgb(255, 240, 170), Color.FromArgb(190, 235, 200))
                e.CellStyle.SelectionBackColor = e.CellStyle.BackColor
            End If
        End If
    End Sub

    ' Clears the default selection every time data is bound
    Private Sub dgvClassGrades_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) _
        Handles dgvClassGrades.DataBindingComplete
        dgvClassGrades.ClearSelection()
        dgvClassGrades.CurrentCell = Nothing
    End Sub

    ' =====================================================
    '  REVISION REQUESTS
    ' =====================================================

    Private Sub ConfigureRequestsGrid()
        With dgvRequests
            .AutoGenerateColumns = False
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .RowHeadersVisible = False
            .BackgroundColor = Color.White
            .BorderStyle = BorderStyle.None
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            .GridColor = Color.Gray
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .RowTemplate.Height = 45
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            .DefaultCellStyle.ForeColor = Color.FromArgb(0, 100, 0)
            .DefaultCellStyle.SelectionBackColor = Color.White
            .DefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 100, 0)

            ' header (replaces your old label row)
            .EnableHeadersVisualStyles = False
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 45
            .ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            With .ColumnHeadersDefaultCellStyle
                .BackColor = Color.White
                .ForeColor = Color.FromArgb(0, 100, 0)
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
                .Alignment = DataGridViewContentAlignment.MiddleLeft
                .SelectionBackColor = Color.White
                .SelectionForeColor = Color.FromArgb(0, 100, 0)
            End With

            .Columns.Clear()
            AddReqColumn("Student", "Student", 22)
            AddReqColumn("Assessment", "Assessment", 13)
            AddReqColumn("CurrentGrade", "Current Grade", 12)
            AddReqColumn("Requested", "Requested", 11)
            AddReqColumn("Reason", "Reason", 18)
            AddReqColumn("Status", "Status", 12)

            Dim btnCol As New DataGridViewButtonColumn()
            btnCol.Name = "Action"
            btnCol.DataPropertyName = "Action"
            btnCol.HeaderText = "Action"
            btnCol.FillWeight = 12
            btnCol.FlatStyle = FlatStyle.Flat
            btnCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .Columns.Add(btnCol)

            .Columns("Status").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End With
    End Sub

    Private Sub AddReqColumn(name As String, header As String, weight As Single)
        Dim c As New DataGridViewTextBoxColumn()
        c.Name = name
        c.DataPropertyName = name
        c.HeaderText = header
        c.FillWeight = weight
        dgvRequests.Columns.Add(c)
    End Sub

    Private Sub LoadRequests()
        ' 1. Fetch live pending requests from MySQL
        Dim dtDb As DataTable = _revisionService.GetPendingRequestsByTeacher(_currentTeacherId)

        ' 2. Build the DataTable structure expected by dgvRequests
        Dim dt As New DataTable()
        For Each col In {"RequestId", "EnrollmentId", "AssessmentId", "RequestedPointsVal",
                 "Student", "Assessment", "CurrentGrade", "Requested", "Reason", "Status", "Action"}
            dt.Columns.Add(col)
        Next

        ' 3. Populate rows from database query results
        For Each row As DataRow In dtDb.Rows
            Dim reqId As Integer = Convert.ToInt32(row("request_id"))
            Dim enrolId As Integer = Convert.ToInt32(row("enrollment_id"))
            Dim assessId As Integer = Convert.ToInt32(row("assessment_id"))
            Dim studentName As String = row("student_name").ToString()
            Dim assessTitle As String = row("assessment_title").ToString()

            ' Format Current Score nicely (e.g. 9/20 instead of 9.00/20.00)
            Dim currScore As String = "--"
            If Not IsDBNull(row("current_score")) AndAlso Not IsDBNull(row("total_score")) Then
                Dim cScore As Double = Convert.ToDouble(row("current_score"))
                Dim tScore As Double = Convert.ToDouble(row("total_score"))
                currScore = $"{cScore:0.##}/{tScore:0.##}"
            End If

            ' Show "Re-check" if requested_points is NULL, otherwise "+Xpts"
            Dim reqPtsVal As Double = If(IsDBNull(row("requested_points")), 0, Convert.ToDouble(row("requested_points")))
            Dim reqPtsStr As String = If(IsDBNull(row("requested_points")), "Re-check", "+" & reqPtsVal.ToString("0.##") & "pts")

            Dim reason As String = row("reason").ToString()
            Dim status As String = row("status").ToString()
            Dim action As String = If(status.Equals("Pending", StringComparison.OrdinalIgnoreCase), "Approve", "Closed")

            dt.Rows.Add(reqId, enrolId, assessId, reqPtsVal, studentName, assessTitle, currScore, reqPtsStr, reason, status, action)
        Next

        dgvRequests.DataSource = dt
        dgvRequests.ClearSelection()
    End Sub

    ' Pending = yellow, Approved = green, Approve button = gray, Closed = dark gray
    Private Sub dgvRequests_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) _
        Handles dgvRequests.CellFormatting

        If e.Value Is Nothing OrElse e.ColumnIndex < 0 Then Exit Sub
        Dim colName As String = dgvRequests.Columns(e.ColumnIndex).Name

        If colName = "Status" Then
            If e.Value.ToString() = "Approved" Then
                e.CellStyle.BackColor = Color.FromArgb(0, 128, 0)
                e.CellStyle.ForeColor = Color.White
            Else
                e.CellStyle.BackColor = Color.FromArgb(255, 248, 200)
                e.CellStyle.ForeColor = Color.FromArgb(90, 80, 0)
            End If
            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor

        ElseIf colName = "Action" Then
            If e.Value.ToString() = "Closed" Then
                e.CellStyle.BackColor = Color.FromArgb(120, 120, 120)
                e.CellStyle.ForeColor = Color.White
            Else
                e.CellStyle.BackColor = Color.FromArgb(225, 225, 225)
                e.CellStyle.ForeColor = Color.Black
            End If
            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor
        End If
    End Sub

    ' Approve button click
    Private Sub dgvRequests_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) _
    Handles dgvRequests.CellContentClick

        If e.RowIndex < 0 Then Exit Sub
        If dgvRequests.Columns(e.ColumnIndex).Name <> "Action" Then Exit Sub

        Dim row As DataGridViewRow = dgvRequests.Rows(e.RowIndex)
        If row.Cells("Action").Value.ToString() <> "Approve" Then Exit Sub

        If MessageBox.Show("Approve this revision request and update the student's grade?", "Confirm Revision Approval",
                   MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then

            Dim drv As DataRowView = DirectCast(row.DataBoundItem, DataRowView)
            If drv Is Nothing Then Exit Sub

            Dim requestId As Integer = Convert.ToInt32(drv("RequestId"))

            ' Call updated Fix 5 method using requestId and teacherId
            Dim result As OperationResult = _revisionService.ApproveAndApplyRevision(requestId, _currentTeacherId)

            ' If "re-check" request (no points specified), prompt professor for corrected score
            If Not result.Success AndAlso result.Message = "NEEDSCORE" Then
                Dim txt As String = InputBox("Enter the corrected score for this assessment:", "Re-check")
                Dim fixedScore As Double
                If txt = "" OrElse Not Double.TryParse(txt, Globalization.NumberStyles.Float,
                                                   Globalization.CultureInfo.InvariantCulture, fixedScore) Then Exit Sub
                result = _revisionService.ApproveAndApplyRevision(requestId, _currentTeacherId, fixedScore)
            End If

            If result.Success Then
                MessageBox.Show(result.Message, "Request Approved", MessageBoxButtons.OK, MessageBoxIcon.Information)
                LoadRequests()
                LoadClassGrades()
            Else
                MessageBox.Show(result.Message, "Approval Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End If
    End Sub

    ' =====================================================
    '  AUDIT LOGS
    ' =====================================================

    Private Sub ConfigureAuditGrid()
        With dgvAuditLogs
            .AutoGenerateColumns = False
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .ReadOnly = True
            .RowHeadersVisible = False
            .BackgroundColor = Color.White
            .BorderStyle = BorderStyle.None
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            .GridColor = Color.Gray
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .RowTemplate.Height = 45
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            .DefaultCellStyle.ForeColor = Color.FromArgb(0, 100, 0)
            .DefaultCellStyle.SelectionBackColor = Color.White
            .DefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 100, 0)

            .EnableHeadersVisualStyles = False
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 40
            .ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            With .ColumnHeadersDefaultCellStyle
                .BackColor = Color.White
                .ForeColor = Color.FromArgb(0, 100, 0)
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
                .Alignment = DataGridViewContentAlignment.MiddleLeft
                .SelectionBackColor = Color.White
                .SelectionForeColor = Color.FromArgb(0, 100, 0)
            End With

            .Columns.Clear()
            AddAuditColumn("Action", "Action", 18)
            AddAuditColumn("Student", "Student", 22)
            AddAuditColumn("Item", "Item", 16)
            AddAuditColumn("DateTime", "Date & Time", 20)
            AddAuditColumn("ChangedBy", "Changed By", 20)
        End With
    End Sub

    Private Sub AddAuditColumn(name As String, header As String, weight As Single)
        Dim c As New DataGridViewTextBoxColumn()
        c.Name = name
        c.DataPropertyName = name
        c.HeaderText = header
        c.FillWeight = weight
        dgvAuditLogs.Columns.Add(c)
    End Sub

    Private Sub LoadAuditLogs()
        Dim dt As New DataTable()
        For Each c In {"Action", "Student", "Item", "DateTime", "ChangedBy"}
            dt.Columns.Add(c)
        Next

        ' newest first
        dt.Rows.Add("Grades Updated", "Condes, Hope Gian", "Midterms", "2026-08-24 09:14", "Prof. Jhun")
        dt.Rows.Add("Revision Approved", "Mariano, Geoff Andrei", "Activity#2", "2026-08-23 15:40", "Prof. Jhun")
        dt.Rows.Add("Grades Updated", "Lalata, Carl Jade", "Quiz#1", "2026-08-22 11:05", "Prof. Jhun")
        dt.Rows.Add("Score Recorded", "Dela Cruz, Juan", "SW1", "2026-08-21 10:30", "Prof. Jhun")

        dgvAuditLogs.DataSource = dt
        dgvAuditLogs.ClearSelection()
    End Sub

    Private Sub dgvAuditLogs_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) _
        Handles dgvAuditLogs.DataBindingComplete
        dgvAuditLogs.ClearSelection()
        dgvAuditLogs.CurrentCell = Nothing
    End Sub

    ' =====================================================
    '  ANALYTICS
    ' =====================================================

    ' Sizes the two cards to fill the page and spreads the four tiles in one row
    Private Sub LayoutAnalytics()
        Const margin As Integer = 20
        Const gap As Integer = 20
        Const minCardH As Integer = 400        ' never shorter than this (normal window size)
        Const maxHeightRatio As Double = 0.5   ' 0.5 = cards end at the middle of the page

        ' visible height = bottom of the screen's working area minus where the form's client area starts
        Dim clientTopOnScreen As Integer = Me.PointToScreen(Point.Empty).Y
        Dim visibleH As Integer = Math.Min(Me.ClientSize.Height,
            Screen.FromControl(Me).WorkingArea.Bottom - clientTopOnScreen)

        pnlAnalytics.SetBounds(pnlAnalytics.Left, pnlAnalytics.Top,
                               Me.ClientSize.Width - pnlAnalytics.Left,
                               visibleH - pnlAnalytics.Top)

        Dim cardW As Integer = (pnlAnalytics.ClientSize.Width - 2 * margin - gap) \ 2

        ' height: stop at the middle of the page, but never below minCardH
        ' and never taller than the space that is actually available
        Dim availH As Integer = pnlAnalytics.ClientSize.Height - RoundedPanel4.Top - margin
        Dim cardH As Integer = CInt(pnlAnalytics.ClientSize.Height * maxHeightRatio) - RoundedPanel4.Top
        cardH = Math.Min(Math.Max(cardH, minCardH), availH)

        If cardW < 300 OrElse cardH < 200 Then Exit Sub

        RoundedPanel4.SetBounds(margin, RoundedPanel4.Top, cardW, cardH)
        RoundedPanel5.SetBounds(margin + cardW + gap, RoundedPanel4.Top, cardW, cardH)

        ' four tiles in one row inside the left card
        Const tMargin As Integer = 12
        Const tGap As Integer = 12
        Dim tiles() As Control = {RoundedPanel6, RoundedPanel7, RoundedPanel8, RoundedPanel9}
        Dim tileW As Integer = (RoundedPanel4.ClientSize.Width - 2 * tMargin - 3 * tGap) \ 4
        If tileW < 80 Then Exit Sub

        Dim tileTop As Integer = RoundedPanel6.Top
        Dim tileH As Integer = RoundedPanel6.Height
        For i As Integer = 0 To tiles.Length - 1
            tiles(i).SetBounds(tMargin + i * (tileW + tGap), tileTop, tileW, tileH)
        Next
    End Sub

    ' Creates one big number label inside a tile
    Private Function AddValueLabel(tile As Control, textColor As Color) As Label
        Dim l As New Label()
        l.AutoSize = False
        l.BackColor = Color.Transparent
        l.ForeColor = textColor
        l.Font = New Font("Segoe UI", 20.0F, FontStyle.Bold)
        l.TextAlign = ContentAlignment.MiddleCenter
        l.SetBounds(0, 10, tile.Width, 55)
        l.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        tile.Controls.Add(l)
        l.BringToFront()
        Return l
    End Function

    Private Sub BuildAnalyticsLabels()
        Dim green As Color = Color.FromArgb(0, 100, 0)
        lblClassAvg = AddValueLabel(RoundedPanel6, green)
        lblPassing = AddValueLabel(RoundedPanel7, green)
        lblAtRisk = AddValueLabel(RoundedPanel8, Color.DarkRed)
        lblTopScore = AddValueLabel(RoundedPanel9, green)

        ' make each tile's caption (CLASS AVERAGE, PASSING, ...) fit and stay centered
        Dim tiles() As Control = {RoundedPanel6, RoundedPanel7, RoundedPanel8, RoundedPanel9}
        Dim valueLabels() As Label = {lblClassAvg, lblPassing, lblAtRisk, lblTopScore}
        For i As Integer = 0 To tiles.Length - 1
            For Each c As Control In tiles(i).Controls
                If TypeOf c Is Label AndAlso c IsNot valueLabels(i) Then
                    Dim cap As Label = DirectCast(c, Label)
                    cap.AutoSize = False
                    cap.Dock = DockStyle.Bottom
                    cap.Height = 30
                    cap.TextAlign = ContentAlignment.MiddleCenter
                    cap.BackColor = Color.Transparent
                End If
            Next
        Next
    End Sub

    Private Sub LoadAnalytics()
        ' sample data for now (matches the six students in Class Grades)
        lblClassAvg.Text = "85.5"
        lblPassing.Text = "5"
        lblAtRisk.Text = "1"
        lblTopScore.Text = "94.2"
    End Sub

    ' =====================================================
    '  GENERAL
    ' =====================================================

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        AppSession.ClearSession()

        ' 2. Re-open login form and close current dashboard
        Dim loginForm As New frmlogin()
        loginForm.Show()
        Me.Close()
    End Sub

    Private Sub frmprof_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If isLoading Then Exit Sub
        If dgvClassGrades.Columns.Count > 0 Then AlignGridToHeader()
        LayoutAnalytics()
    End Sub

    ' =====================================================
    '  Designer-generated stubs (safe to leave as they are)
    ' =====================================================

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim classId As Integer = SelectedClassId()
        If classId = 0 Then Exit Sub

        ' Load students enrolled in the selected class
        Dim students As DataTable = _gradeService.GetEnrollmentsByClass(classId, _currentTeacherId)
        students.Columns.Add("Display", GetType(String), "student_id + ' - ' + full_name")
        ComboBox2.DataSource = students
        ComboBox2.DisplayMember = "Display"
        ComboBox2.ValueMember = "enrollment_id"

        ' Load assessments created for the selected class
        Dim assessments As DataTable = _gradeService.GetAssessmentsByClass(classId, _currentTeacherId)
        assessments.Columns.Add("Display", GetType(String), "term + ' - ' + title")
        ComboBox4.DataSource = assessments
        ComboBox4.DisplayMember = "Display"
        ComboBox4.ValueMember = "assessment_id"

        TextBox1.Clear()
        If Not isLoading Then LoadClassGrades()
    End Sub

    Private Sub LoadTeacherInfo()
        lblProfName.Text = AppSession.CurrentUser.FullName

        Dim classes As DataTable = _gradeService.GetClassesByTeacher(_currentTeacherId)
        classes.Columns.Add("Display", GetType(String), "course_code + ' - ' + section")
        ComboBox1.DataSource = classes
        ComboBox1.DisplayMember = "Display"
        ComboBox1.ValueMember = "class_id"
    End Sub

    Private Function SelectedClassId() As Integer
        Dim drv As DataRowView = TryCast(ComboBox1.SelectedItem, DataRowView)
        If drv Is Nothing Then Return 0
        Return Convert.ToInt32(drv("class_id"))
    End Function

    Private Sub lblEntryTitle_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub cboStudent_SelectedIndexChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label18_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label26_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label47_Click(sender As Object, e As EventArgs) Handles Label47.Click

    End Sub

    Private Sub Label55_Click(sender As Object, e As EventArgs) Handles Label55.Click

    End Sub

    Private Sub AssesmentTypetxt_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Cmbo_BoxSW_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSW1.SelectedIndexChanged

    End Sub

    Private Sub Label16_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub RoundedPanel1_Paint(sender As Object, e As PaintEventArgs) Handles RoundedPanel1.Paint

    End Sub

    Private Sub Label19_Click(sender As Object, e As EventArgs) Handles Label19.Click

    End Sub

    Private Sub CustomButton4_Click(sender As Object, e As EventArgs) Handles CustomButton4.Click
        frmGradebook.Show()
    End Sub

    Private Sub CustomButton6_Click(sender As Object, e As EventArgs) Handles CustomButton6.Click
    End Sub

    Private Sub CustomButton5_Click(sender As Object, e As EventArgs) Handles CustomButton5.Click
        Dim stu As DataRowView = TryCast(ComboBox2.SelectedItem, DataRowView)
        Dim asm As DataRowView = TryCast(ComboBox4.SelectedItem, DataRowView)
        If stu Is Nothing OrElse asm Is Nothing Then
            MessageBox.Show("Select a student and an assessment.", "Missing info", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim score As Double
        If Not Double.TryParse(TextBox1.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, score) Then
            MessageBox.Show("Enter a valid numeric score.", "Invalid score", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            Return
        End If

        ' RecordScore saves the score, recalculates running grade, and logs audit in ONE transaction
        Dim res As OperationResult = _gradeService.RecordScore(
            Convert.ToInt32(stu("enrollment_id")), Convert.ToInt32(asm("assessment_id")),
            score, _currentTeacherId, "Recorded via Record Scores page")

        If res.Success Then
            MessageBox.Show(res.Message, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
            TextBox1.Clear()
        Else
            MessageBox.Show(res.Message, "Could not save", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub
End Class