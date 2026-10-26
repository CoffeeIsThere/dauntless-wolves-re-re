Imports System.Globalization
Imports MySql.Data.MySqlClient

Public Class frmprof
    Private ReadOnly _exportService As New ExportService()
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

    ' Right Panel Controls Cache
    Private _lblRightStudentName As Label = Nothing
    Private _lblRightStudentId As Label = Nothing
    Private _pnlRightScoresContainer As Control = Nothing
    Private _isRightPanelInitialized As Boolean = False

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
            .ColumnHeadersVisible = False
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
        Dim classId As Integer = SelectedClassId()
        If classId = 0 Then
            Dim emptyDt As New DataTable()
            For Each c In {"Student", "SW1", "HW1", "Quiz1", "Act4", "Act5", "Act6", "Midterms", "Running Grade", "Status"}
                emptyDt.Columns.Add(c)
            Next
            dgvClassGrades.DataSource = emptyDt
            AlignGridToHeader()
            Return
        End If

        Dim dt As DataTable = _gradeService.GetClassGradeSheet(classId, _currentTeacherId)

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            dgvClassGrades.DataSource = dt
        Else
            Dim emptyDt As New DataTable()
            For Each c In {"Student", "SW1", "HW1", "Quiz1", "Act4", "Act5", "Act6", "Midterms", "Running Grade", "Status"}
                emptyDt.Columns.Add(c)
            Next
            dgvClassGrades.DataSource = emptyDt
        End If

        AlignGridToHeader()
    End Sub

    Private Function HeaderCenterX(ctrl As Control) As Integer
        Dim screenPt As Point = ctrl.Parent.PointToScreen(
            New Point(ctrl.Left + ctrl.Width \ 2, 0))
        Return dgvClassGrades.PointToClient(screenPt).X
    End Function

    Private Sub AlignGridToHeader()
        If dgvClassGrades.Columns.Count = 0 Then Return
        dgvClassGrades.ColumnHeadersVisible = True
        dgvClassGrades.ScrollBars = ScrollBars.Both
        For Each col As DataGridViewColumn In dgvClassGrades.Columns
            col.Visible = col.Name <> "EnrollmentId" AndAlso col.Name <> "StudentId"
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            If col.Name = "Student" Then col.MinimumWidth = 180
            If col.Name = "RunningGrade" Then col.HeaderText = "Running Grade"
        Next
        Dim table = TryCast(dgvClassGrades.DataSource, DataTable)
        If table IsNot Nothing Then
            For Each col As DataColumn In table.Columns
                dgvClassGrades.Columns(col.ColumnName).HeaderText = col.Caption
            Next
        End If
        ' The designer's fixed assessment headings do not describe dynamic classes.
        For Each heading As Control In {cboSW1, cboHW1, cboQuiz1, lblMidterms, lblRunningGrade, lblStatus}
            heading.Visible = False
        Next
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

        ElseIf colName = "RunningGrade" Then
            Dim g As Double
            If Double.TryParse(e.Value.ToString(), g) Then
                e.CellStyle.BackColor = If(g < 75, Color.FromArgb(255, 240, 170), Color.FromArgb(190, 235, 200))
                e.CellStyle.SelectionBackColor = e.CellStyle.BackColor
            End If
        End If
    End Sub

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
        Dim dtDb As DataTable = _revisionService.GetPendingRequestsByTeacher(_currentTeacherId)

        Dim dt As New DataTable()
        For Each col In {"RequestId", "EnrollmentId", "AssessmentId", "RequestedPointsVal",
                         "Student", "Assessment", "CurrentGrade", "Requested", "Reason", "Status", "Action"}
            dt.Columns.Add(col)
        Next

        For Each row As DataRow In dtDb.Rows
            Dim reqId As Integer = Convert.ToInt32(row("request_id"))
            Dim enrolId As Integer = Convert.ToInt32(row("enrollment_id"))
            Dim assessId As Integer = Convert.ToInt32(row("assessment_id"))
            Dim studentName As String = row("student_name").ToString()
            Dim assessTitle As String = row("assessment_title").ToString()

            Dim currScore As String = "--"
            If Not IsDBNull(row("current_score")) AndAlso Not IsDBNull(row("total_score")) Then
                Dim cScore As Double = Convert.ToDouble(row("current_score"))
                Dim tScore As Double = Convert.ToDouble(row("total_score"))
                currScore = $"{cScore:0.##}/{tScore:0.##}"
            End If

            Dim reqPtsVal As Double = If(IsDBNull(row("requested_points")), 0, Convert.ToDouble(row("requested_points")))
            Dim reqPtsStr As String = If(IsDBNull(row("requested_points")), "Re-check", "+" & reqPtsVal.ToString("0.##") & "pts")

            Dim reason As String = row("reason").ToString()
            Dim status As String = row("status").ToString()
            Dim action As String = If(status = "Pending" OrElse status = "Under Review", "Approve", "Closed")

            dt.Rows.Add(reqId, enrolId, assessId, reqPtsVal, studentName, assessTitle, currScore, reqPtsStr, reason, status, action)
        Next

        dgvRequests.DataSource = dt
        dgvRequests.ClearSelection()
    End Sub

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

            Dim result As OperationResult = _revisionService.ApproveAndApplyRevision(requestId, _currentTeacherId)

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
        Dim dtDb As DataTable = _gradeService.GetAuditLogsForTeacher(_currentTeacherId)

        Dim dt As New DataTable()
        For Each c In {"Action", "Student", "Item", "DateTime", "ChangedBy"}
            dt.Columns.Add(c)
        Next

        If dtDb IsNot Nothing AndAlso dtDb.Rows.Count > 0 Then
            For Each row As DataRow In dtDb.Rows
                Dim actionName = Convert.ToString(row("Action"))
                Dim studentName = Convert.ToString(row("Student"))
                Dim itemTitle = Convert.ToString(row("Item"))
                Dim dtFormatted = Convert.ToString(row("DateTime"))
                Dim changedBy = Convert.ToString(row("ChangedBy"))

                dt.Rows.Add(actionName, studentName, itemTitle, dtFormatted, changedBy)
            Next
        End If

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

    Private Sub LayoutAnalytics()
        Const margin As Integer = 20
        Const gap As Integer = 20
        Const minCardH As Integer = 400
        Const maxHeightRatio As Double = 0.5

        Dim clientTopOnScreen As Integer = Me.PointToScreen(Point.Empty).Y
        Dim visibleH As Integer = Math.Min(Me.ClientSize.Height,
                               Screen.FromControl(Me).WorkingArea.Bottom - clientTopOnScreen)

        pnlAnalytics.SetBounds(pnlAnalytics.Left, pnlAnalytics.Top,
                               Me.ClientSize.Width - pnlAnalytics.Left,
                               visibleH - pnlAnalytics.Top)

        Dim cardW As Integer = (pnlAnalytics.ClientSize.Width - 2 * margin - gap) \ 2

        Dim availH As Integer = pnlAnalytics.ClientSize.Height - RoundedPanel4.Top - margin
        Dim cardH As Integer = CInt(pnlAnalytics.ClientSize.Height * maxHeightRatio) - RoundedPanel4.Top
        cardH = Math.Min(Math.Max(cardH, minCardH), availH)

        If cardW < 300 OrElse cardH < 200 Then Exit Sub

        RoundedPanel4.SetBounds(margin, RoundedPanel4.Top, cardW, cardH)
        RoundedPanel5.SetBounds(margin + cardW + gap, RoundedPanel4.Top, cardW, cardH)

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
        Dim classId As Integer = SelectedClassId()
        If classId = 0 Then
            ResetAnalyticsUI()
            Return
        End If

        ' 1. Query Class Performance Summary Tiles
        Dim sqlSummary As String = "SELECT " &
                                  "    IFNULL(AVG(running_grade), 0) AS class_avg, " &
                                  "    IFNULL(SUM(CASE WHEN running_grade >= 75.0 THEN 1 ELSE 0 END), 0) AS passing_count, " &
                                  "    IFNULL(SUM(CASE WHEN running_grade < 75.0 AND running_grade IS NOT NULL THEN 1 ELSE 0 END), 0) AS at_risk_count, " &
                                  "    IFNULL(MAX(running_grade), 0) AS top_score " &
                                  "FROM enrollment " &
                                  "WHERE class_id = @ClassId"

        Dim dtSummary As DataTable = DatabaseHelper.ExecuteQuery(sqlSummary, New MySqlParameter() {New MySqlParameter("@ClassId", classId)})

        If dtSummary IsNot Nothing AndAlso dtSummary.Rows.Count > 0 Then
            Dim row As DataRow = dtSummary.Rows(0)
            Dim avgVal As Double = Convert.ToDouble(row("class_avg"))
            Dim passVal As Integer = Convert.ToInt32(row("passing_count"))
            Dim riskVal As Integer = Convert.ToInt32(row("at_risk_count"))
            Dim topVal As Double = Convert.ToDouble(row("top_score"))

            If lblClassAvg IsNot Nothing Then lblClassAvg.Text = If(avgVal > 0, avgVal.ToString("0.0"), "--")
            If lblPassing IsNot Nothing Then lblPassing.Text = passVal.ToString()
            If lblAtRisk IsNot Nothing Then lblAtRisk.Text = riskVal.ToString()
            If lblTopScore IsNot Nothing Then lblTopScore.Text = If(topVal > 0, topVal.ToString("0.0"), "--")
        Else
            ResetAnalyticsUI()
        End If

        ' 2. Query Averages by Assessment Type
        Dim sqlTypes As String = "SELECT " &
                                "    LOWER(a.assessment_type) AS type_name, " &
                                "    IFNULL(AVG((sg.score / a.total_score) * 100.0), 0) AS avg_pct " &
                                "FROM studentgrade sg " &
                                "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                                "WHERE a.class_id = @ClassId AND sg.score IS NOT NULL AND a.total_score > 0 " &
                                "GROUP BY LOWER(a.assessment_type)"

        Dim dtTypes As DataTable = DatabaseHelper.ExecuteQuery(sqlTypes, New MySqlParameter() {New MySqlParameter("@ClassId", classId)})

        Dim quizAvg As Integer = 0
        Dim actAvg As Integer = 0
        Dim examAvg As Integer = 0
        Dim projAvg As Integer = 0

        If dtTypes IsNot Nothing Then
            For Each r As DataRow In dtTypes.Rows
                Dim tName As String = r("type_name").ToString().ToLower()
                Dim pct As Integer = CInt(Math.Round(Convert.ToDouble(r("avg_pct"))))

                If tName.Contains("quiz") Then
                    quizAvg = pct
                ElseIf tName.Contains("act") OrElse tName.Contains("seatwork") OrElse tName.Contains("homework") OrElse tName.Contains("lab") Then
                    actAvg = pct
                ElseIf tName.Contains("exam") OrElse tName.Contains("midterm") OrElse tName.Contains("final") OrElse tName.Contains("test") Then
                    examAvg = pct
                ElseIf tName.Contains("proj") Then
                    projAvg = pct
                End If
            Next
        End If

        SetTypeBars(quizAvg, actAvg, examAvg, projAvg)
    End Sub

    Private Sub ResetAnalyticsUI()
        If lblClassAvg IsNot Nothing Then lblClassAvg.Text = "--"
        If lblPassing IsNot Nothing Then lblPassing.Text = "0"
        If lblAtRisk IsNot Nothing Then lblAtRisk.Text = "0"
        If lblTopScore IsNot Nothing Then lblTopScore.Text = "--"
        SetTypeBars(0, 0, 0, 0)
    End Sub
    Private Sub SetAnalyticsTiles(avgVal As Double, passCount As Integer, riskCount As Integer, topVal As Double)
        If lblClassAvg IsNot Nothing Then lblClassAvg.Text = If(avgVal > 0, avgVal.ToString("0.0"), "--")
        If lblPassing IsNot Nothing Then lblPassing.Text = passCount.ToString()
        If lblAtRisk IsNot Nothing Then lblAtRisk.Text = riskCount.ToString()
        If lblTopScore IsNot Nothing Then lblTopScore.Text = If(topVal > 0, topVal.ToString("0.0"), "--")
    End Sub

    Private Sub SetTypeBars(quizPct As Integer, actPct As Integer, examPct As Integer, projPct As Integer)
        Dim container As Control = If(RoundedPanel5, pnlAnalytics)
        If container Is Nothing Then Return

        Dim targetPcts() As Integer = {quizPct, actPct, examPct, projPct}
        Dim catKeys() As String = {"quiz", "act", "exam", "proj"}
        Dim allControls As List(Of Control) = GetAllNestedControls(container)

        ' 1. Check for CustomProgressBar controls if present
        Dim progressControls = allControls.Where(Function(c) _
            c.GetType().Name.ToLower().Contains("progress") OrElse
            c.GetType().GetProperty("Value") IsNot Nothing
        ).OrderBy(Function(c) c.PointToScreen(Point.Empty).Y).ToList()

        If progressControls.Count >= 4 Then
            For i As Integer = 0 To 3
                Dim ctrl = progressControls(i)
                Dim val = Math.Max(0, Math.Min(100, targetPcts(i)))
                Dim propValue = ctrl.GetType().GetProperty("Value")
                If propValue IsNot Nothing AndAlso propValue.CanWrite Then
                    Try
                        propValue.SetValue(ctrl, Convert.ChangeType(val, propValue.PropertyType), Nothing)
                    Catch
                    End Try
                End If
                ctrl.Text = $"{val}%"
                ctrl.Visible = True
                ctrl.Refresh()
            Next
        End If

        ' 2. Screen Coordinate Row-Matching via PointToScreen
        For i As Integer = 0 To 3
            Dim key As String = catKeys(i)
            Dim val As Integer = targetPcts(i)

            ' Locate Category Title ("Quizzes", "Activities", "Exams", "Projects")
            Dim headerLbl As Label = Nothing
            For Each c As Control In allControls
                If TypeOf c Is Label Then
                    Dim t As String = c.Text.ToLower()
                    If t.Contains(key) AndAlso Not t.Contains("%") AndAlso Not IsNumeric(t.Trim()) Then
                        headerLbl = DirectCast(c, Label)
                        Exit For
                    End If
                End If
            Next

            If headerLbl IsNot Nothing Then
                Dim headerY As Integer = headerLbl.PointToScreen(Point.Empty).Y

                ' Update controls on the exact same row (within 30px screen height)
                For Each c As Control In allControls
                    If c IsNot headerLbl Then
                        Dim cY As Integer = c.PointToScreen(Point.Empty).Y
                        If Math.Abs(cY - headerY) <= 30 Then
                            c.Visible = True

                            ' Update Percentage Text Label
                            If TypeOf c Is Label Then
                                Dim lbl As Label = DirectCast(c, Label)
                                Dim lt As String = lbl.Text.ToLower()
                                If Not lt.Contains(key) OrElse lt.Contains("%") OrElse IsNumeric(lt) OrElse lt = "--" Then
                                    lbl.Text = $"{val}%"
                                End If
                            End If

                            ' Resize Fill Bar Panel
                            If TypeOf c Is Panel AndAlso c.Parent IsNot Nothing AndAlso c.Parent IsNot container Then
                                Dim maxW As Integer = c.Parent.ClientSize.Width - c.Parent.Padding.Horizontal
                                If maxW > 20 Then
                                    c.Width = Math.Max(5, CInt(maxW * val / 100.0))
                                End If
                            End If
                        End If
                    End If
                Next
            End If
        Next
    End Sub

    Private Function GetAllNestedControls(parent As Control) As List(Of Control)
        Dim list As New List(Of Control)()
        If parent Is Nothing Then Return list
        For Each c As Control In parent.Controls
            list.Add(c)
            If c.HasChildren Then list.AddRange(GetAllNestedControls(c))
        Next
        Return list
    End Function





    ' =====================================================
    '  GENERAL & CLASS LOADING
    ' =====================================================

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        AppSession.ClearSession()
        Dim loginForm As New frmlogin()
        loginForm.Show()
        Me.Close()
    End Sub

    Private Sub frmprof_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If isLoading Then Exit Sub
        If dgvClassGrades.Columns.Count > 0 Then AlignGridToHeader()
        LayoutAnalytics()
    End Sub

    Private Sub LoadTeacherInfo()
        lblProfName.Text = AppSession.CurrentUser.FullName

        Dim classes As DataTable = _gradeService.GetClassesByTeacher(_currentTeacherId)
        If classes IsNot Nothing Then
            If Not classes.Columns.Contains("Display") Then
                classes.Columns.Add("Display", GetType(String), "course_code + ' - ' + section")
            End If
            ComboBox1.DataSource = classes
            ComboBox1.DisplayMember = "Display"
            ComboBox1.ValueMember = "class_id"

            ' Force initial load of students & assessments for the first class
            If ComboBox1.Items.Count > 0 Then
                ComboBox1.SelectedIndex = 0
                ComboBox1_SelectedIndexChanged(Nothing, EventArgs.Empty)
            End If
        End If
    End Sub

    Private Function SelectedClassId() As Integer
        Dim drv As DataRowView = TryCast(ComboBox1.SelectedItem, DataRowView)
        If drv Is Nothing Then Return 0
        Return Convert.ToInt32(drv("class_id"))
    End Function

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim classId As Integer = SelectedClassId()
        If classId = 0 Then Exit Sub

        ' 1. Load enrolled students
        Dim students As DataTable = _gradeService.GetEnrollmentsByClass(classId, _currentTeacherId)
        If students IsNot Nothing Then
            If Not students.Columns.Contains("Display") Then
                students.Columns.Add("Display", GetType(String), "student_id + ' - ' + full_name")
            End If
            ComboBox2.DataSource = students
            ComboBox2.DisplayMember = "Display"
            ComboBox2.ValueMember = "enrollment_id"
        Else
            ComboBox2.DataSource = Nothing
        End If

        ' 2. Load assessments
        Dim assessments As DataTable = _gradeService.GetAssessmentsByClass(classId)
        If assessments IsNot Nothing AndAlso assessments.Rows.Count > 0 Then
            ComboBox4.DataSource = assessments
            ComboBox4.DisplayMember = "title"
            ComboBox4.ValueMember = "assessment_id"
        Else
            ComboBox4.DataSource = Nothing
        End If

        ' 3. Refresh right-side student profile card & class analytics
        UpdateSelectedStudentDisplay()
        LoadAnalytics()
    End Sub

    ' =====================================================
    '  RECORD SCORES & RIGHT PANEL SCORE CARD
    ' =====================================================

    Private Sub InitRightPanelReferences()
        If _isRightPanelInitialized Then Return

        For Each ctrl As Control In GetAllControls(pnlRecordScores)
            If TypeOf ctrl Is Label Then
                Dim lbl As Label = DirectCast(ctrl, Label)
                If _lblRightStudentName Is Nothing AndAlso (lbl.Text.Contains("Lalata") OrElse lbl.Name.Equals("lblStudentNameDisplay", StringComparison.OrdinalIgnoreCase)) Then
                    _lblRightStudentName = lbl
                ElseIf _lblRightStudentId Is Nothing AndAlso (lbl.Text.Contains("25-0676") OrElse lbl.Name.Equals("lblStudentIdDisplay", StringComparison.OrdinalIgnoreCase)) Then
                    _lblRightStudentId = lbl
                ElseIf _pnlRightScoresContainer Is Nothing AndAlso (lbl.Text.StartsWith("SW1") OrElse lbl.Text.StartsWith("Quiz") OrElse lbl.Text.StartsWith("18/20")) Then
                    _pnlRightScoresContainer = lbl.Parent
                End If
            End If
        Next

        If _pnlRightScoresContainer Is Nothing Then
            For Each ctrl As Control In pnlRecordScores.Controls
                If ctrl.HasChildren AndAlso ctrl.Width > 200 AndAlso Not ContainsControl(ctrl, ComboBox2) Then
                    Dim innerPanels = ctrl.Controls.OfType(Of Panel)().ToList()
                    If innerPanels.Count > 0 Then
                        _pnlRightScoresContainer = innerPanels(0)
                    Else
                        _pnlRightScoresContainer = ctrl
                    End If
                    Exit For
                End If
            Next
        End If

        _isRightPanelInitialized = True
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        UpdateSelectedStudentDisplay()
    End Sub

    Private Sub UpdateSelectedStudentDisplay()
        If ComboBox2.SelectedItem Is Nothing Then Return

        InitRightPanelReferences()

        Dim studentId As String = ""
        Dim fullName As String = ""
        Dim enrollmentId As Integer = 0

        If TypeOf ComboBox2.SelectedItem Is DataRowView Then
            Dim drv As DataRowView = DirectCast(ComboBox2.SelectedItem, DataRowView)
            studentId = Convert.ToString(drv("student_id"))
            fullName = Convert.ToString(drv("full_name"))
            If drv.Row.Table.Columns.Contains("enrollment_id") AndAlso Not IsDBNull(drv("enrollment_id")) Then
                enrollmentId = Convert.ToInt32(drv("enrollment_id"))
            End If
        Else
            Dim text As String = ComboBox2.SelectedItem.ToString()
            Dim parts As String() = text.Split(New String() {" - "}, StringSplitOptions.None)
            If parts.Length >= 2 Then
                studentId = parts(0).Trim()
                fullName = parts(1).Trim()
            Else
                studentId = text.Trim()
                fullName = text.Trim()
            End If
        End If

        If String.IsNullOrEmpty(studentId) Then Return

        If _lblRightStudentName IsNot Nothing Then _lblRightStudentName.Text = fullName
        If _lblRightStudentId IsNot Nothing Then _lblRightStudentId.Text = studentId

        Dim classId As Integer = SelectedClassId()
        RefreshRightScoresList(studentId, classId, enrollmentId)
    End Sub

    Private Sub RefreshRightScoresList(studentId As String, classId As Integer, enrollmentId As Integer)
        If _pnlRightScoresContainer Is Nothing Then Return

        _pnlRightScoresContainer.Controls.Clear()

        Dim dt As DataTable = Nothing
        If classId > 0 Then
            dt = _gradeService.GetStudentScoresForClass(studentId, classId)
        End If

        If (dt Is Nothing OrElse dt.Rows.Count = 0) AndAlso enrollmentId > 0 Then
            dt = _gradeService.GetGradesByEnrollment(enrollmentId)
        End If

        Dim topOffset As Integer = 10

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            For Each row As DataRow In dt.Rows
                Dim title As String = Convert.ToString(row("title"))
                Dim totalScore As Double = If(dt.Columns.Contains("total_score") AndAlso Not IsDBNull(row("total_score")), Convert.ToDouble(row("total_score")), 100)
                Dim hasScore As Boolean = dt.Columns.Contains("score") AndAlso Not row("score") Is DBNull.Value
                Dim scoreVal As Double = If(hasScore, Convert.ToDouble(row("score")), 0)

                Dim scoreText As String = If(hasScore, $"{scoreVal:0.##}/{totalScore:0.##}", "N/A")

                Dim lblTitle As New Label With {
                    .Text = title,
                    .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                    .ForeColor = Color.FromArgb(40, 110, 50),
                    .Location = New Point(15, topOffset + 3),
                    .AutoSize = True
                }

                Dim lblBadge As New Label With {
                    .Text = scoreText,
                    .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                    .BackColor = If(hasScore, Color.FromArgb(40, 167, 69), Color.FromArgb(70, 150, 80)),
                    .ForeColor = Color.White,
                    .TextAlign = ContentAlignment.MiddleCenter,
                    .Size = New Size(80, 24),
                    .Location = New Point(280, topOffset)
                }

                _pnlRightScoresContainer.Controls.Add(lblTitle)
                _pnlRightScoresContainer.Controls.Add(lblBadge)
                topOffset += 36
            Next
        Else
            Dim lblEmpty As New Label With {
                .Text = "No assessment scores recorded yet.",
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Italic),
                .ForeColor = Color.Gray,
                .Location = New Point(15, 15),
                .AutoSize = True
            }
            _pnlRightScoresContainer.Controls.Add(lblEmpty)
        End If
    End Sub

    Private Function ContainsControl(parent As Control, target As Control) As Boolean
        If parent Is Nothing OrElse target Is Nothing Then Return False
        If parent Is target Then Return True
        For Each c As Control In parent.Controls
            If ContainsControl(c, target) Then Return True
        Next
        Return False
    End Function

    Private Function GetAllControls(parent As Control) As List(Of Control)
        Dim list As New List(Of Control)()
        If parent Is Nothing Then Return list
        For Each c As Control In parent.Controls
            list.Add(c)
            If c.HasChildren Then
                list.AddRange(GetAllControls(c))
            End If
        Next
        Return list
    End Function

    ' Action Buttons
    Private Sub CustomButton4_Click(sender As Object, e As EventArgs) Handles CustomButton4.Click
        frmGradebook.Show()
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

        Dim res As OperationResult = _gradeService.RecordScore(
            Convert.ToInt32(stu("enrollment_id")), Convert.ToInt32(asm("assessment_id")),
            score, _currentTeacherId, "Recorded via Record Scores page")

        If res.Success Then
            MessageBox.Show(res.Message, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
            TextBox1.Clear()
            UpdateSelectedStudentDisplay()
        Else
            MessageBox.Show(res.Message, "Could not save", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub CustomButton6_Click(sender As Object, e As EventArgs) Handles CustomButton6.Click
        If dgvClassGrades.Rows.Count = 0 Then
            LoadClassGrades()
        End If

        If dgvClassGrades.Rows.Count = 0 Then
            MessageBox.Show("No class grade records available to export.", "Export Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Using sfd As New SaveFileDialog()
            sfd.Filter = "CSV Files (*.csv)|*.csv|PDF Files (*.pdf)|*.pdf"
            sfd.FileName = $"Class_Grades_{DateTime.Now:yyyyMMdd}.csv"

            If sfd.ShowDialog() = DialogResult.OK Then
                If sfd.FilterIndex = 1 Then
                    ' CSV Export
                    If _exportService.ExportToCSV(dgvClassGrades, sfd.FileName) Then
                        MessageBox.Show("Class grades exported to CSV successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                Else
                    ' PDF Export
                    Dim printDoc As New Printing.PrintDocument()
                    AddHandler printDoc.PrintPage, Sub(s, args)
                                                       Dim g As Graphics = args.Graphics
                                                       Dim titleFont As New Font("Segoe UI", 14, FontStyle.Bold)
                                                       Dim headerFont As New Font("Segoe UI", 9, FontStyle.Bold)
                                                       Dim bodyFont As New Font("Segoe UI", 9, FontStyle.Regular)

                                                       Dim y As Integer = 40
                                                       g.DrawString("DAUNTLESS WOLVES - CLASS GRADE REPORT", titleFont, Brushes.DarkGreen, 40, y)
                                                       y += 28
                                                       g.DrawString($"Faculty ID: {_currentTeacherId} | Generated: {DateTime.Now:MMM d, yyyy HH:mm}", bodyFont, Brushes.Gray, 40, y)
                                                       y += 30

                                                       ' Headers
                                                       Dim x As Integer = 40
                                                       For Each col As DataGridViewColumn In dgvClassGrades.Columns
                                                           If col.Visible Then
                                                               g.DrawString(col.HeaderText, headerFont, Brushes.DarkGreen, x, y)
                                                               x += 75
                                                           End If
                                                       Next

                                                       y += 20
                                                       g.DrawLine(Pens.DarkGreen, 40, y, Math.Max(x, 650), y)
                                                       y += 10

                                                       ' Data Rows
                                                       For Each row As DataGridViewRow In dgvClassGrades.Rows
                                                           If Not row.IsNewRow Then
                                                               x = 40
                                                               For Each col As DataGridViewColumn In dgvClassGrades.Columns
                                                                   If col.Visible Then
                                                                       Dim cellVal As String = If(row.Cells(col.Index).Value IsNot Nothing, row.Cells(col.Index).Value.ToString(), "")
                                                                       g.DrawString(cellVal, bodyFont, Brushes.Black, x, y)
                                                                       x += 75
                                                                   End If
                                                               Next
                                                               y += 22
                                                           End If
                                                       Next
                                                   End Sub

                    If _exportService.ExportToPDF(printDoc, sfd.FileName) Then
                        MessageBox.Show("Class grades exported to PDF successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End If
            End If
        End Using
    End Sub

    ' Designer stubs
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


End Class