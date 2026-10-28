Imports System.Globalization
Imports MySql.Data.MySqlClient

Public Class frmprof
    Private ReadOnly _exportService As New ExportService()
    Private ReadOnly _gradeService As New GradeService()
    Private ReadOnly _revisionService As New RevisionRequestService()
    Private ReadOnly _auditService As New AuditService()

    Private _currentTeacherId As String = "TCH001"
    Private savingScore As Boolean
    Private refreshingClassList As Boolean
    Private refreshingScoreSelectors As Boolean
    Private entryClassId As Integer
    Private entryEnrollmentId As Integer
    Private entryAssessmentId As Integer
    Private ReadOnly scoreFeedback As New Label With {.AutoSize = False, .ForeColor = Color.DarkGreen, .TextAlign = ContentAlignment.TopLeft}


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
        ConfigureProfessorLayout()
        ApplyProfessorTheme()
        AddPasswordButton()
        LoadAnalytics()
    End Sub

    Private Sub AddPasswordButton()
        Dim button = UiKit.MakeSecondaryButton("Change Password", 145)
        button.Name = "btnProfessorChangePassword"
        button.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        button.Height = btnLogout.Height
        button.Location = New Point(btnLogout.Left - button.Width - 10, btnLogout.Top)
        AddHandler button.Click, Sub()
                                     If Not AppSession.IsTeacher Then Return
                                     Using dialog As New ProfessorPasswordForm()
                                         dialog.ShowDialog(Me)
                                     End Using
                                 End Sub
        btnLogout.Parent.Controls.Add(button)
        button.BringToFront()
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
        If Not isLoading Then LoadTeacherInfo()
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
            kvp.Key.BackColor = If(isActive, Color.White, Color.FromArgb(232, 242, 232))
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
        UiKit.TryLoadData(Me, "class grades", AddressOf LoadClassGradesCore)
    End Sub

    Private Sub LoadClassGradesCore()
        Dim selectedClass = TryCast(ComboBox1.SelectedItem, DataRowView)
        Label13.Text = If(selectedClass Is Nothing, "No class selected", selectedClass("course_code").ToString() & " - " & selectedClass("section").ToString() & " (" & selectedClass("semester").ToString() & " " & selectedClass("academic_year").ToString() & ")")
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

        Dim dt As DataTable = _gradeService.GetClassGradeSheet(classId, _currentTeacherId, If(ComboBox3.SelectedItem Is Nothing, "All", ComboBox3.SelectedItem.ToString()))

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

    Private Sub SortClassStudents(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvClassGrades.ColumnHeaderMouseClick
        If e.ColumnIndex < 0 OrElse dgvClassGrades.Columns(e.ColumnIndex).Name <> "Student" Then Return
        Dim table = TryCast(dgvClassGrades.DataSource, DataTable)
        If table Is Nothing OrElse Not table.Columns.Contains("StudentId") Then Return
        dgvClassGrades.DataSource = StudentNameOrder.Sort(table, "Student", "StudentId")
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
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            col.MinimumWidth = 85
            If col.Name = "Student" Then col.MinimumWidth = 180
            If col.Name <> "Student" Then
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            End If
            If col.Name = "RunningGrade" Then col.HeaderText = "Running Grade"
        Next
        Dim table = TryCast(dgvClassGrades.DataSource, DataTable)
        If table IsNot Nothing Then
            For Each col As DataColumn In table.Columns
                dgvClassGrades.Columns(col.ColumnName).HeaderText = col.Caption
            Next
        End If
        If dgvClassGrades.Columns.Contains("Student") Then
            With dgvClassGrades.Columns("Student")
                .SortMode = DataGridViewColumnSortMode.Programmatic
                .HeaderCell.SortGlyphDirection = SortOrder.Ascending
                .ToolTipText = "Click to sort students by surname A–Z."
            End With
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
            btnCol.HeaderText = "Review / Approve"
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
        UiKit.TryLoadData(Me, "clarification requests", AddressOf LoadRequestsCore)
    End Sub

    Private Sub LoadRequestsCore()
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
            Dim action As String = If(status = "Pending" OrElse status = "Under Review", "Review", "Closed")

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

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Exit Sub
        If dgvRequests.Columns(e.ColumnIndex).Name <> "Action" Then Exit Sub

        Dim row As DataGridViewRow = dgvRequests.Rows(e.RowIndex)
        If row.Cells("Action").Value.ToString() <> "Review" Then Exit Sub

        Dim drv = TryCast(row.DataBoundItem, DataRowView)
        If drv Is Nothing Then Return
        ReviewRequest(CInt(drv("RequestId")))
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
        UiKit.TryLoadData(Me, "audit logs", AddressOf LoadAuditLogsCore)
    End Sub

    Private Sub LoadAuditLogsCore()
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
        l.SetBounds(0, 4, tile.Width, 44)
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
        Dim captions = {"Class Average", "Passing Students", "At-Risk Students", "Highest Grade"}
        For i As Integer = 0 To tiles.Length - 1
            tiles(i).Height = 100
            tiles(i).Padding = New Padding(6)
            For Each old As Label In tiles(i).Controls.OfType(Of Label)().ToList()
                If old IsNot valueLabels(i) Then old.Visible = False
            Next
            Dim caption As New Label With {.Text = captions(i), .Dock = DockStyle.Bottom, .Height = 44, .TextAlign = ContentAlignment.MiddleCenter, .ForeColor = Color.DarkGreen, .BackColor = Color.White}
            tiles(i).Controls.Add(caption)
            caption.BringToFront()
        Next
    End Sub

    Private Sub LoadAnalytics()
        UiKit.TryLoadData(Me, "analytics", AddressOf LoadAnalyticsCore)
    End Sub

    Private Sub LoadAnalyticsCore()
        Dim classId As Integer = SelectedClassId()
        If classId = 0 Then
            ResetAnalyticsUI()
            Return
        End If

        Dim summary = _gradeService.GetClassSummary(classId, _currentTeacherId)
        If lblClassAvg IsNot Nothing Then lblClassAvg.Text = If(summary.Average.HasValue, summary.Average.GetValueOrDefault().ToString("0.0"), "--")
        If lblPassing IsNot Nothing Then lblPassing.Text = summary.Passing.ToString()
        If lblAtRisk IsNot Nothing Then lblAtRisk.Text = summary.AtRisk.ToString()
        If lblTopScore IsNot Nothing Then lblTopScore.Text = If(summary.Top.HasValue, summary.Top.GetValueOrDefault().ToString("0.0"), "--")

        ' 2. Query Averages by Assessment Type
        Dim sqlTypes As String = "SELECT " &
                                "    LOWER(a.assessment_type) AS type_name, " &
                                "    IFNULL(SUM(sg.score / a.total_score * a.weight) / NULLIF(SUM(a.weight),0) * 100.0, 0) AS avg_pct " &
                                "FROM studentgrade sg " &
                                "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                                "WHERE a.class_id = @ClassId AND sg.score IS NOT NULL AND a.total_score > 0 AND a.weight > 0 " &
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
        If lblClassAvg IsNot Nothing Then lblClassAvg.Text = avgVal.ToString("0.0")
        If lblPassing IsNot Nothing Then lblPassing.Text = passCount.ToString()
        If lblAtRisk IsNot Nothing Then lblAtRisk.Text = riskCount.ToString()
        If lblTopScore IsNot Nothing Then lblTopScore.Text = topVal.ToString("0.0")
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
        UiKit.TryLoadData(Me, "class choices", AddressOf LoadTeacherInfoCore)
    End Sub

    Private Sub LoadTeacherInfoCore()
        lblProfName.Text = AppSession.CurrentUser.FullName
        Dim previous = SelectedClassId()
        Dim classes = _gradeService.GetClassesByTeacher(_currentTeacherId)
        classes.Columns.Add("Display", GetType(String), "course_code + ' - ' + section + ' (' + semester + ' ' + academic_year + ')'")
        refreshingClassList = True
        Try
            ComboBox1.DisplayMember = "Display"
            ComboBox1.ValueMember = "class_id"
            ComboBox1.DataSource = classes
            If classes.AsEnumerable().Any(Function(row) CInt(row("class_id")) = previous) Then ComboBox1.SelectedValue = previous
        Finally
            refreshingClassList = False
        End Try
        ComboBox1_SelectedIndexChanged(Nothing, EventArgs.Empty)
    End Sub

    Private Function SelectedClassId() As Integer
        Dim drv As DataRowView = TryCast(ComboBox1.SelectedItem, DataRowView)
        If drv Is Nothing Then Return 0
        Return Convert.ToInt32(drv("class_id"))
    End Function

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        UiKit.TryLoadData(Me, "class selections", Sub() ComboBox1_SelectedIndexChangedCore(sender, e))
    End Sub

    Private Sub ComboBox1_SelectedIndexChangedCore(sender As Object, e As EventArgs)
        If refreshingClassList OrElse refreshingScoreSelectors Then Return
        Dim previousStudent = TryCast(ComboBox2.SelectedItem, DataRowView)
        Dim previousAssessment = TryCast(ComboBox4.SelectedItem, DataRowView)
        Dim enrollmentId = If(previousStudent Is Nothing, 0, CInt(previousStudent("enrollment_id")))
        Dim assessmentId = If(previousAssessment Is Nothing, 0, CInt(previousAssessment("assessment_id")))
        Dim classId = SelectedClassId()
        refreshingScoreSelectors = True
        Try
            If classId = 0 Then
                ComboBox2.DataSource = Nothing
                ComboBox4.DataSource = Nothing
            Else
                Dim students = _gradeService.GetEnrollmentsByClass(classId, _currentTeacherId)
                students.Columns.Add("Display", GetType(String), "student_id + ' - ' + full_name")
                ComboBox2.DisplayMember = "Display"
                ComboBox2.ValueMember = "enrollment_id"
                ComboBox2.DataSource = students
                If students.AsEnumerable().Any(Function(row) CInt(row("enrollment_id")) = enrollmentId) Then ComboBox2.SelectedValue = enrollmentId
                Dim assessments = _gradeService.GetAssessmentsByClass(classId, _currentTeacherId)
                ComboBox4.DisplayMember = "title"
                ComboBox4.ValueMember = "assessment_id"
                ComboBox4.DataSource = assessments
                If assessments.AsEnumerable().Any(Function(row) CInt(row("assessment_id")) = assessmentId) Then ComboBox4.SelectedValue = assessmentId
            End If
        Finally
            refreshingScoreSelectors = False
            ResetScoreDraftForContext()
        End Try
        UpdateSelectedStudentDisplay()
        LoadAnalytics()
        If Not isLoading Then LoadClassGrades()
    End Sub

    Private Sub ResetScoreDraftForContext()
        If refreshingClassList OrElse refreshingScoreSelectors Then Return
        Dim student = TryCast(ComboBox2.SelectedItem, DataRowView)
        Dim assessment = TryCast(ComboBox4.SelectedItem, DataRowView)
        Dim enrollment = If(student Is Nothing, 0, CInt(student("enrollment_id")))
        Dim selectedAssessment = If(assessment Is Nothing, 0, CInt(assessment("assessment_id")))
        Dim classId = SelectedClassId()
        If Not savingScore AndAlso (classId <> entryClassId OrElse enrollment <> entryEnrollmentId OrElse selectedAssessment <> entryAssessmentId) Then
            Dim hadDraft = TextBox1.TextLength > 0
            TextBox1.Clear()
            scoreFeedback.Text = If(hadDraft, "Unsaved score cleared because the student, class, or assessment changed.", "")
        End If
        entryClassId = classId
        entryEnrollmentId = enrollment
        entryAssessmentId = selectedAssessment
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
        If refreshingScoreSelectors Then Return
        ResetScoreDraftForContext()
        UpdateSelectedStudentDisplay()
    End Sub

    Private Sub UpdateSelectedStudentDisplay()
        UiKit.TryLoadData(Me, "student scores", AddressOf UpdateSelectedStudentDisplayCore)
    End Sub

    Private Sub UpdateSelectedStudentDisplayCore()
        If ComboBox2.SelectedItem Is Nothing Then
            If _lblRightStudentName IsNot Nothing Then _lblRightStudentName.Text = "No student selected"
            If _lblRightStudentId IsNot Nothing Then _lblRightStudentId.Text = ""
            If _pnlRightScoresContainer IsNot Nothing Then
                _pnlRightScoresContainer.Controls.Clear()
                _pnlRightScoresContainer.Controls.Add(New Label With {.Text = "No enrolled students in this class.", .AutoSize = True, .Location = New Point(15, 15), .ForeColor = Color.DarkGreen})
            End If
            Return
        End If

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

        ClearChildren(_pnlRightScoresContainer)

        Dim dt As DataTable = Nothing
        If classId > 0 Then
            dt = _gradeService.GetStudentScoresForClass(studentId, classId)
        End If

        If (dt Is Nothing OrElse dt.Rows.Count = 0) AndAlso enrollmentId > 0 Then
            dt = _gradeService.GetGradesByEnrollment(enrollmentId)
        End If

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim grid As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 2,
                .BackColor = Color.Transparent
            }
            grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))   ' title
            grid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110))  ' badge, pinned right

            Dim r As Integer = 0
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
                    .Dock = DockStyle.Fill,
                    .Size = New Size(100, 36),
                    .Margin = New Padding(14, 0, 6, 0),
                    .TextAlign = ContentAlignment.MiddleLeft,
                    .AutoEllipsis = True
                }

                Dim lblBadge As New Label With {
                    .Text = scoreText,
                    .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                    .BackColor = If(hasScore, Color.FromArgb(40, 167, 69), Color.FromArgb(70, 150, 80)),
                    .ForeColor = Color.White,
                    .TextAlign = ContentAlignment.MiddleCenter,
                    .Size = New Size(80, 26),
                    .Margin = New Padding(0, 5, 14, 5),
                    .Anchor = AnchorStyles.Right
                }

                grid.RowStyles.Add(New RowStyle(SizeType.AutoSize))
                grid.RowCount = r + 1
                grid.Controls.Add(lblTitle, 0, r)
                grid.Controls.Add(lblBadge, 1, r)
                r += 1
            Next

            _pnlRightScoresContainer.Controls.Add(grid)
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

    ' Fixed header + scrolling host (only the host is cleared on refresh).
    Private Sub BuildRightScoresPanel()
        RoundedPanel12.AutoScroll = False
        RoundedPanel12.Padding = New Padding(3)
        ClearChildren(RoundedPanel12)

        Dim host As New Panel With {
            .Dock = DockStyle.Fill,
            .AutoScroll = True,
            .BackColor = Color.Transparent
        }

        Dim header As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 26,
            .BackColor = Color.Transparent
        }

        Dim lblTitle As New Label With {
            .Text = "Student Scores",
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
            .ForeColor = Color.DarkGreen,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Padding = New Padding(14, 0, 0, 0)
        }

        Dim lblName As New Label With {
            .Dock = DockStyle.Right,
            .Width = 260,
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Regular),
            .ForeColor = Color.FromArgb(40, 110, 50),
            .TextAlign = ContentAlignment.MiddleRight,
            .AutoEllipsis = True,
            .Padding = New Padding(0, 0, 14, 0)
        }

        header.Controls.Add(lblTitle)
        header.Controls.Add(lblName)

        AddHandler header.Paint, Sub(s, e)
                                     Using pen As New Pen(Color.Black, 2)
                                         e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1)
                                     End Using
                                 End Sub
        AddHandler header.Resize, Sub() header.Invalidate()

        RoundedPanel12.Controls.Add(host)
        RoundedPanel12.Controls.Add(header)

        _lblRightStudentName = lblName
        _pnlRightScoresContainer = host
        _isRightPanelInitialized = True
    End Sub

    Private Sub ClearChildren(parent As Control)
        For i As Integer = parent.Controls.Count - 1 To 0 Step -1
            Dim c = parent.Controls(i)
            parent.Controls.RemoveAt(i)
            c.Dispose()
        Next
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
        OpenAssessmentManager(False)
    End Sub

    Private Sub CustomButton5_Click(sender As Object, e As EventArgs) Handles CustomButton5.Click
        SubmitRecordedScore()
    End Sub

    Private Sub ScoreBox_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode <> Keys.Enter OrElse e.Modifiers <> Keys.None Then Return
        e.Handled = True
        e.SuppressKeyPress = True
        ' Ignore a repeated Enter after the previous score has cleared.
        If Not String.IsNullOrWhiteSpace(TextBox1.Text) Then SubmitRecordedScore()
    End Sub

    Private Sub SubmitRecordedScore()
        If savingScore Then Return
        savingScore = True
        CustomButton5.Enabled = False
        Dim scoreSaved As Boolean = False
        Try
            Dim student = TryCast(ComboBox2.SelectedItem, DataRowView)
            Dim assessment = TryCast(ComboBox4.SelectedItem, DataRowView)
            If student Is Nothing OrElse assessment Is Nothing Then
                ShowScoreError("Select a student and an assessment.")
                Return
            End If
            Dim score As Double
            If Not Double.TryParse(TextBox1.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, score) OrElse Not Double.IsFinite(score) Then
                ShowScoreError("Enter a valid numeric score.")
                Return
            End If
            Dim index = ComboBox2.SelectedIndex
            Dim studentName = student("full_name").ToString()
            Dim result = _gradeService.RecordScore(CInt(student("enrollment_id")), CInt(assessment("assessment_id")), score, _currentTeacherId, "Recorded via Record Scores page")
            If Not result.Success Then
                ShowScoreError(result.Message)
                Return
            End If
            scoreSaved = True
            ' Advance only after the transaction succeeds; keep the class and assessment.
            TextBox1.Clear()
            If index < ComboBox2.Items.Count - 1 Then
                ComboBox2.SelectedIndex = index + 1
                scoreFeedback.Text = "Saved for " & studentName & ". Ready for the next student."
            Else
                scoreFeedback.Text = "Saved for " & studentName & ". All students completed for this assessment."
                UpdateSelectedStudentDisplay()
            End If
            scoreFeedback.ForeColor = Color.DarkGreen
        Catch ex As Exception
            If scoreSaved Then
                scoreFeedback.Text = "Score saved, but the student display could not refresh: " & ex.Message
                scoreFeedback.ForeColor = Color.DarkRed
            Else
                ShowScoreError("Could not save the score: " & ex.Message)
            End If
        Finally
            CustomButton5.Enabled = True
            If Not IsDisposed AndAlso Not Disposing Then
                TextBox1.Focus()
                TextBox1.SelectionStart = TextBox1.TextLength
                TextBox1.SelectionLength = 0
            End If
            savingScore = False
        End Try
    End Sub

    Private Sub ScoreAssessmentChanged(sender As Object, e As EventArgs) Handles ComboBox4.SelectedIndexChanged
        ResetScoreDraftForContext()
    End Sub

    Private Sub ShowScoreError(message As String)
        scoreFeedback.Text = message
        scoreFeedback.ForeColor = Color.DarkRed
        MessageBox.Show(Me, message, "Score not saved", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Sub CustomButton6_Click(sender As Object, e As EventArgs) Handles CustomButton6.Click
        OpenAssessmentManager(True)
    End Sub

    Private Sub OpenAssessmentManager(addAssessment As Boolean)
        If addAssessment Then
            If SelectedClassId() = 0 Then
                MessageBox.Show("Select a class first.")
                Return
            End If
            Using assessmentHost As New frmGradebook()
                assessmentHost.ShowAssessmentForClass(SelectedClassId(), Me)
            End Using
            ComboBox1_SelectedIndexChanged(Nothing, EventArgs.Empty)
            Return
        End If
        Dim book = My.Forms.frmGradebook
        book.InitialClassId = SelectedClassId()
        book.ShowInTaskbar = False
        book.Show(Me)
        book.RefreshAssignedClasses()
        book.SelectClass(SelectedClassId())
        book.BringToFront()
    End Sub

    Private Sub ExportAudit(sender As Object, e As EventArgs) Handles CustomButton1.Click
        ExportGrid(dgvAuditLogs, "PROFESSOR AUDIT LOG")
    End Sub

    Private Sub ExportClass(sender As Object, e As EventArgs) Handles CustomButton3.Click
        ExportGrid(dgvClassGrades, "CLASS GRADE REPORT")
    End Sub

    Private Sub ExportGrid(grid As DataGridView, title As String)
        If grid.Rows.Count = 0 Then
            MessageBox.Show("There are no records to export.")
            Return
        End If
        Using dialog As New SaveFileDialog With {.Filter = "PDF files (*.pdf)|*.pdf", .FileName = title.Replace(" ", "_") & ".pdf"}
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Using doc = _exportService.CreateGridPrintDocument(grid, title, "Professor: " & AppSession.CurrentUser.FullName)
                If _exportService.ExportToPDF(doc, dialog.FileName) Then MessageBox.Show("PDF saved successfully.")
            End Using
        End Using
    End Sub

    Private Sub ReviewRequest(requestId As Integer)
        If RevisionReviewDialog.Review(Me, requestId, _currentTeacherId) Then
            LoadRequests()
            LoadClassGrades()
            LoadAuditLogs()
            LoadAnalytics()
            UpdateSelectedStudentDisplay()
        End If
    End Sub

    Public Sub ReloadClasses(classId As Integer)
        LoadTeacherInfo()
        ComboBox1.SelectedValue = classId
    End Sub

    Private Sub ApplyProfessorTheme()
        Text = "The Dauntless Grading System"
        Lbltransparentgradingsytem.Text = "The Dauntless Grading System"
        Lbltransparentgradingsytem.Parent.BackColor = Color.DarkGreen
        Lbltransparentgradingsytem.BackColor = Color.DarkGreen
        Lbltransparentgradingsytem.ForeColor = Color.White
        LblRole.BackColor = Color.DarkGreen
        For Each button As CustomButton In {CustomButton4, CustomButton6}
            button.NormalColor = Color.DarkGreen
            button.HoverColor = Color.FromArgb(0, 125, 45)
            button.ForeColor = Color.White
        Next
        BackColor = Color.White
        pnlTabs.BackColor = Color.FromArgb(240, 248, 240)
        pnl2ndtab.BackColor = pnlTabs.BackColor
        For Each control As Control In {btnLogout, btnRecordScores, btnClassGrades, btnRevisionRequests, btnAnalytics, btnAuditLogs}
            control.ForeColor = Color.DarkGreen
            Dim button = TryCast(control, Button)
            If button IsNot Nothing Then
                button.FlatStyle = FlatStyle.Flat
                button.FlatAppearance.BorderColor = Color.DarkGreen
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 238, 220)
                button.UseVisualStyleBackColor = False
            End If
        Next
    End Sub

    Private Sub ConfigureProfessorLayout()
        ComboBox3.Items.Clear()
        ComboBox3.Items.AddRange({"All", "Midterm", "Finals"})
        ComboBox3.SelectedIndex = 0
        AddHandler CustomButton2.Click, Sub(sender, args) LoadClassGrades()
        AddHandler ComboBox3.SelectedIndexChanged, Sub(sender, args) LoadClassGrades()
        dgvRequests.ReadOnly = True
        dgvRequests.EditMode = DataGridViewEditMode.EditProgrammatically
        dgvRequests.Columns("Action").HeaderText = "Review / Approve"
        AddHandler Me.Activated, Sub(sender, args)
                                     If Not isLoading AndAlso Not savingScore AndAlso AppSession.IsTeacher Then
                                         Try
                                             LoadTeacherInfo()
                                         Catch ex As Exception
                                             Console.WriteLine("[CLASS REFRESH] " & ex.Message)
                                         End Try
                                     End If
                                 End Sub
        ComboBox1.Width = 360
        ComboBox1.DropDownWidth = 500
        Label2.Text = "Student (surname A–Z)"
        Label3.Text = "Assessment"
        Dim inputs() As Control = {Label2, ComboBox2, Label3, ComboBox4, Label4, TextBox1, CustomButton5}
        Dim y = 40
        For Each control In inputs
            control.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            control.SetBounds(14, y, Math.Max(150, control.Parent.ClientSize.Width - 28), If(TypeOf control Is Label, 22, 30))
            y += control.Height + 5
        Next
        scoreFeedback.SetBounds(14, y + 2, Math.Max(150, RoundedPanel11.ClientSize.Width - 28), 48)
        scoreFeedback.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        RoundedPanel11.Controls.Add(scoreFeedback)
        pnlRecordScores.BackColor = Color.White
        pnlRecordScores.AutoScroll = True
        TableLayoutPanel1.Dock = DockStyle.Top
        TableLayoutPanel1.Height = Math.Max(360, y + 20)
        TableLayoutPanel1.RowStyles.Clear()
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        For Each style As RowStyle In TableLayoutPanel1.RowStyles
            style.SizeType = SizeType.Percent
            style.Height = 100
        Next
        RoundedPanel11.Dock = DockStyle.Fill
        RoundedPanel12.Dock = DockStyle.Fill
        BuildRightScoresPanel()
        UpdateSelectedStudentDisplay()
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

    Private Sub Label21_Click(sender As Object, e As EventArgs) Handles LblRole.Click

    End Sub

    Private Sub RoundedPanel12_Paint(sender As Object, e As PaintEventArgs) Handles RoundedPanel12.Paint

    End Sub
End Class
