
Imports MySql.Data.MySqlClient


Public Class frmStudent


    Private ReadOnly _gradeService As New GradeService()
    Private ReadOnly _revisionService As New RevisionRequestService()

    Private ReadOnly pages As New Dictionary(Of Button, Panel)
    Private ReadOnly boldFont As New Font("Segoe UI", 9.5F, FontStyle.Bold)
    Private ReadOnly normalFont As New Font("Segoe UI", 9.5F, FontStyle.Regular)


    Private _currentStudentId As String = ""
    Private refreshingCourses As Boolean
    Private catalogsReady As Boolean
    Private refreshingStudentData As Boolean
    Private ReadOnly gradeRows As New List(Of String())
    Private ReadOnly standingItems As New List(Of (Term As String, Item As ScoreItem))
    Private ReadOnly performanceClassIds As New List(Of Integer)
    Private ReadOnly assessmentMap As New Dictionary(Of Integer, DataRow)()


    Private quizPct, activityPct, examPct, projectPct As Integer
    Private Const OverallLabel As String = "Overall (All Courses)"

    Private Sub frmStudent_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 1. Session Guard Check

        If Not AppSession.IsStudent Then
            MessageBox.Show("Please log in to access this page.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            Dim loginForm As New frmlogin()
            loginForm.Show()
            Me.Close()
            Return
        End If

        _currentStudentId = AppSession.CurrentUser.UserId
        lblstudentname.Text = AppSession.CurrentUser.FullName

        ' My Profile button beside Change Password
        Dim btnMyProfile As New Button With {
            .Name = "btnMyProfile",
            .Text = "My Profile",
            .Size = New Size(105, btnChangePassword.Height),
            .Location = New Point(btnChangePassword.Left - 115,
                                  btnChangePassword.Top),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .BackColor = Color.White,
            .ForeColor = Color.DarkGreen,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }

        btnMyProfile.FlatAppearance.BorderColor = Color.DarkGreen

        AddHandler btnMyProfile.Click, AddressOf ShowMyProfile

        btnChangePassword.Parent.Controls.Add(btnMyProfile)
        btnMyProfile.BringToFront()



        ' 2. Bind Navigation Pages
        pages.Clear()
        pages.Add(btnMyGrades, pnlMyGrades)
        pages.Add(btnGradeHistory, pnlGradeHistory)
        pages.Add(btnRequestRevision, pnlRequestRevision)
        pages.Add(btnMyPerformance, pnlMyPerformance)

        For Each b As Button In pages.Keys
            RemoveHandler b.Click, AddressOf Tab_Click
            AddHandler b.Click, AddressOf Tab_Click
        Next

        ' 3. Initialize Shared Grid Styles
        StyleGrid(dgvgrades)
        StyleGrid(dgvHistory)
        StyleGrid(dgvRequests)

        ' 4. Populate Dynamic Data from MariaDB
        LoadEnrolledCourses()
        LoadHistory()
        LoadRequests()
        LoadPerformance()
        catalogsReady = True

        ShowPage(btnMyGrades) ' Keep this last
    End Sub

    Private Sub frmStudent_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        dgvgrades.ClearSelection()
        dgvHistory.ClearSelection()
        dgvRequests.ClearSelection()
    End Sub

    ' ---------- NAVIGATION & TAB CONTROL ----------

    Private Sub Tab_Click(sender As Object, e As EventArgs)

        If Not AppSession.IsStudent Then Return

        Dim selectedTab As Button = DirectCast(sender, Button)

        ' Reload data only when the student selects a page.
        Try

            LoadEnrolledCourses()
            If selectedTab Is btnMyGrades Then

                LoadGrades()

            ElseIf selectedTab Is btnGradeHistory Then

                LoadHistory()

            ElseIf selectedTab Is btnRequestRevision Then

                LoadRequests()

                ' Refresh assessment choices for the selected class.
                cboClass_SelectedIndexChanged(cboclass, EventArgs.Empty)

            ElseIf selectedTab Is btnMyPerformance Then

                ShowPerformance()

            End If

        Catch ex As Exception

            MessageBox.Show(
            "Could not refresh student data: " & ex.Message,
            "Refresh Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )

        End Try

        ' Switch to the selected page.
        ShowPage(selectedTab)

    End Sub

    Private Sub ShowPage(activeTab As Button)
        For Each kvp In pages
            Dim isActive As Boolean = (kvp.Key Is activeTab)
            kvp.Value.Visible = isActive
            kvp.Key.Font = If(isActive, boldFont, normalFont)
            kvp.Key.BackColor = If(isActive, Color.White, Color.FromArgb(208, 216, 224))
        Next
        pages(activeTab).BringToFront()

        If activeTab Is btnMyPerformance Then UpdateBars()
    End Sub

    Private Sub btnChangePassword_Click(
    sender As Object,
    e As EventArgs
        ) Handles btnChangePassword.Click

        If Not AppSession.IsStudent OrElse
       AppSession.CurrentUser Is Nothing Then

            MessageBox.Show(
            "Please log in as a student.",
            "Access Denied",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )
            Return
        End If

        Using dlg As New Form()

            dlg.Text = "Change Password"
            dlg.Size = New Size(400, 330)
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog
            dlg.MaximizeBox = False
            dlg.MinimizeBox = False

            Dim lblCurrent As New Label With {
            .Text = "Current Password",
            .Location = New Point(25, 20),
            .AutoSize = True
        }

            Dim txtCurrent As New TextBox With {
            .Location = New Point(25, 43),
            .Size = New Size(330, 25),
            .UseSystemPasswordChar = True
        }

            Dim lblNew As New Label With {
            .Text = "New Password",
            .Location = New Point(25, 85),
            .AutoSize = True
        }

            Dim txtNew As New TextBox With {
            .Location = New Point(25, 108),
            .Size = New Size(330, 25),
            .UseSystemPasswordChar = True
        }

            Dim lblConfirm As New Label With {
            .Text = "Confirm New Password",
            .Location = New Point(25, 150),
            .AutoSize = True
        }

            Dim txtConfirm As New TextBox With {
            .Location = New Point(25, 173),
            .Size = New Size(330, 25),
            .UseSystemPasswordChar = True
        }

            Dim btnSave As New Button With {
            .Text = "Change Password",
            .Location = New Point(180, 230),
            .Size = New Size(120, 32)
        }

            Dim btnCancel As New Button With {
            .Text = "Cancel",
            .Location = New Point(305, 230),
            .Size = New Size(65, 32),
            .DialogResult = DialogResult.Cancel
        }

            dlg.Controls.AddRange(
            New Control() {
                lblCurrent, txtCurrent,
                lblNew, txtNew,
                lblConfirm, txtConfirm,
                btnSave, btnCancel
            }
        )

            dlg.AcceptButton = btnSave
            dlg.CancelButton = btnCancel

            AddHandler btnSave.Click,
            Sub()

                If String.IsNullOrWhiteSpace(txtCurrent.Text) Then
                    MessageBox.Show("Enter your current password.")
                    Return
                End If

                If txtNew.Text <> txtConfirm.Text Then
                    MessageBox.Show(
                        "New passwords do not match.",
                        "Password Mismatch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    )
                    Return
                End If

                If txtNew.Text.Length < 4 Then
                    MessageBox.Show(
                        "New password must contain at least 4 characters.",
                        "Invalid Password",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    )
                    Return
                End If

                If txtCurrent.Text = txtNew.Text Then
                    MessageBox.Show(
                        "Choose a different password from your current one.",
                        "Invalid Password",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    )
                    Return
                End If

                If Not AppSession.IsStudent OrElse
                   AppSession.CurrentUser Is Nothing Then
                    MessageBox.Show("Student session is no longer valid.")
                    Return
                End If

                Try
                    btnSave.Enabled = False

                    Dim auth As New AuthService()

                    Dim result As OperationResult =
                        auth.ChangePassword(
                            "Student",
                            AppSession.CurrentUser.UserId,
                            txtCurrent.Text,
                            txtNew.Text
                        )

                    If result.Success Then

                        MessageBox.Show(
                            "Your password was changed successfully!",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        )

                        dlg.DialogResult = DialogResult.OK
                        dlg.Close()

                    Else

                        MessageBox.Show(
                            result.Message,
                            "Password Change Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        )

                    End If

                Catch ex As Exception

                    MessageBox.Show(
                        "Unable to change password: " & ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    )

                Finally
                    btnSave.Enabled = True
                End Try

            End Sub
            dlg.ShowDialog(Me)
        End Using
    End Sub

    Private Sub ShowMyProfile(sender As Object, e As EventArgs)
        If Not AppSession.IsStudent Then Return
        Using profile As New StudentProfileForm()
            profile.ShowDialog(Me)
        End Using
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        AppSession.ClearSession()

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

    ' ---------- DYNAMIC COURSE LOADER ----------

    Private Sub LoadEnrolledCourses()
        UiKit.TryLoadData(Me, "enrolled courses", AddressOf LoadEnrolledCoursesCore)
    End Sub

    Private Sub LoadEnrolledCoursesCore()
        Dim sql As String = "SELECT e.enrollment_id, e.class_id, " &
                           "CONCAT(co.course_code, ' - ', co.course_name, ' (', c.section, ', ', c.semester, ' ', c.academic_year, ')') AS full_title " &
                           "FROM enrollment e " &
                           "JOIN class c ON e.class_id = c.class_id " &
                           "JOIN course co ON c.course_code COLLATE utf8mb4_unicode_ci = co.course_code COLLATE utf8mb4_unicode_ci " &
                           "WHERE e.student_id COLLATE utf8mb4_unicode_ci = @StudentId"

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, New MySqlParameter() {New MySqlParameter("@StudentId", _currentStudentId)})

        Dim gradeEnrollment = If(IsNumeric(cboCourse.SelectedValue), CInt(cboCourse.SelectedValue), 0)
        Dim requestEnrollment = If(IsNumeric(cboclass.SelectedValue), CInt(cboclass.SelectedValue), 0)
        Dim performanceClass = If(cboperfcourse.SelectedIndex > 0 AndAlso cboperfcourse.SelectedIndex < performanceClassIds.Count, performanceClassIds(cboperfcourse.SelectedIndex), 0)
        refreshingCourses = True
        Try
            cboCourse.DataSource = Nothing
            cboclass.DataSource = Nothing
            cboperfcourse.Items.Clear()
            performanceClassIds.Clear()
            cboperfcourse.Items.Add(OverallLabel)
            performanceClassIds.Add(0)
            cboCourse.DisplayMember = "full_title"
            cboCourse.ValueMember = "enrollment_id"
            cboCourse.DataSource = dt.Copy()
            cboclass.DisplayMember = "full_title"
            cboclass.ValueMember = "enrollment_id"
            cboclass.DataSource = dt.Copy()
            If dt.AsEnumerable().Any(Function(row) CInt(row("enrollment_id")) = gradeEnrollment) Then cboCourse.SelectedValue = gradeEnrollment
            cboclass.SelectedIndex = -1
            If dt.AsEnumerable().Any(Function(row) CInt(row("enrollment_id")) = requestEnrollment) Then cboclass.SelectedValue = requestEnrollment
            For Each row As DataRow In dt.Rows
                performanceClassIds.Add(CInt(row("class_id")))
                cboperfcourse.Items.Add(row("full_title").ToString())
            Next
            cboperfcourse.SelectedIndex = Math.Max(0, performanceClassIds.IndexOf(performanceClass))
        Finally
            refreshingCourses = False
        End Try
        LoadGrades()
        Dim newRequestEnrollment = If(IsNumeric(cboclass.SelectedValue), CInt(cboclass.SelectedValue), 0)
        If newRequestEnrollment <> requestEnrollment Then cboClass_SelectedIndexChanged(cboclass, EventArgs.Empty)
        If dt.Rows.Count = 0 Then lblstanding.Text = "Current Standing: No enrolled classes"
    End Sub

    Private Sub RefreshStudentOnActivate(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not catalogsReady OrElse refreshingCourses OrElse Not AppSession.IsStudent Then Return
        RefreshStudentData()
    End Sub

    Private Sub RefreshStudentData()
        If refreshingStudentData Then Return
        refreshingStudentData = True
        Try
            LoadEnrolledCourses()
            If pnlGradeHistory.Visible Then LoadHistory()
            If pnlRequestRevision.Visible Then
                Dim selectedGrade = If(cboAssessment.SelectedIndex >= 0 AndAlso assessmentMap.ContainsKey(cboAssessment.SelectedIndex), CInt(assessmentMap(cboAssessment.SelectedIndex)("grade_id")), 0)
                cboClass_SelectedIndexChanged(cboclass, EventArgs.Empty)
                For Each entry In assessmentMap
                    If CInt(entry.Value("grade_id")) = selectedGrade Then cboAssessment.SelectedIndex = entry.Key
                Next
                LoadRequests()
            End If
            If pnlMyPerformance.Visible Then ShowPerformance()
        Finally
            refreshingStudentData = False
        End Try
    End Sub

    ' ---------- MY GRADES ----------

    Private Sub cboCourse_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboCourse.SelectedIndexChanged
        If refreshingCourses Then Return
        LoadGrades()
    End Sub

    Private Sub LoadGrades()
        UiKit.TryLoadData(Me, "grades", AddressOf LoadGradesCore)
    End Sub

    Private Sub LoadGradesCore()
        If dgvgrades.Columns.Count = 0 Then
            For Each h In {"Assessment", "Score", "Weight", "Running Grade", "Status"}
                dgvgrades.Columns.Add(h, h)
            Next
        End If

        If dgvgrades.Columns.Contains("Weight") Then dgvgrades.Columns("Weight").HeaderText = "Relative Weight"
        gradeRows.Clear()
        standingItems.Clear()

        If cboCourse.SelectedValue IsNot Nothing AndAlso IsNumeric(cboCourse.SelectedValue) Then
            Dim enrollmentId As Integer = Convert.ToInt32(cboCourse.SelectedValue)

            ' Query live assessment grades for this enrollment
            Dim sql As String = "SELECT a.term, a.title, sg.score, a.total_score, a.weight " &
                               "FROM assessment a " &
                               "JOIN enrollment e ON a.class_id = e.class_id " &
                               "LEFT JOIN studentgrade sg ON a.assessment_id = sg.assessment_id AND sg.enrollment_id = e.enrollment_id " &
                               "WHERE e.enrollment_id = @EnrollmentId " &
                               "ORDER BY a.created_at ASC"

            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, New MySqlParameter() {New MySqlParameter("@EnrollmentId", enrollmentId)})

            If dt IsNot Nothing Then
                For Each row As DataRow In dt.Rows
                    Dim term As String = row("term").ToString()
                    standingItems.Add((term, New ScoreItem(If(IsDBNull(row("score")), CType(Nothing, Double?), CDbl(row("score"))), CDbl(row("total_score")), CDbl(row("weight")))))
                    Dim title As String = row("title").ToString()
                    Dim weightStr As String = $"{Convert.ToDouble(row("weight")):0.##}"

                    Dim scoreStr As String = "---"
                    Dim runningGradeStr As String = "Pending"
                    Dim statusStr As String = "Not Yet"

                    If Not IsDBNull(row("score")) AndAlso
                        Not IsDBNull(row("total_score")) Then

                        Dim scoreVal As Double = Convert.ToDouble(row("score"))
                        Dim totalVal As Double = Convert.ToDouble(row("total_score"))

                        ' Always show the recorded score.
                        scoreStr = $"{scoreVal:0.##}/{totalVal:0.##}"

                        ' Only calculate percentages when maximum points are valid.
                        If totalVal > 0 Then

                            Dim pct As Double = GradeCalculator.CalculateScorePercentage(scoreVal, totalVal)

                            runningGradeStr = $"{pct:0.0}"
                            statusStr = If(pct >= 75.0, "Passed", "At Risk")

                        Else

                            runningGradeStr = "N/A"
                            statusStr = "Invalid Maximum"

                        End If
                    End If

                    ' Row structure: {Term, Assessment, Score, Weight, Running Grade, Status}
                    gradeRows.Add({term, title, scoreStr, weightStr, runningGradeStr, statusStr})
                Next
            End If
        End If

        If cboterm.Items.Count = 0 Then
            cboterm.Items.AddRange({"Midterm", "Finals", "Combined"})
            cboterm.SelectedIndex = 2 ' triggers ShowGrades
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
            If term = "Combined" OrElse r(0).Equals(term, StringComparison.OrdinalIgnoreCase) Then
                dgvgrades.Rows.Add(r(1), r(2), r(3), r(4), r(5))
            End If
        Next
        dgvgrades.ClearSelection()
        UpdateStanding(term)
    End Sub

    Private Sub UpdateStanding(term As String)
        Dim running = GradeCalculator.CalculateRunningGrade(standingItems.Where(Function(item) term = "Combined" OrElse item.Term.Equals(term, StringComparison.OrdinalIgnoreCase)).Select(Function(item) item.Item))
        Dim label As String = If(term = "Combined", "Current Standing", term & " Standing")
        lblstanding.Text = If(running.HasValue, $"{label}: {running.GetValueOrDefault():0.##}% " & If(running.GetValueOrDefault() >= GradeCalculator.PassingGrade, "(Passing)", "(At Risk)"), label & ": No grades recorded yet")
    End Sub

    Private Sub dgvGrades_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvgrades.CellFormatting
        If e.ColumnIndex >= 3 AndAlso e.Value IsNot Nothing Then
            Dim t = e.Value.ToString()
            If t = "Passed" OrElse IsNumeric(t) Then
                e.CellStyle.BackColor = Color.FromArgb(153, 190, 165)
            ElseIf t = "Pending" OrElse t = "Not Yet" Then
                e.CellStyle.BackColor = Color.FromArgb(238, 220, 170)
            ElseIf t = "At Risk" OrElse t = "Failing" Then
                e.CellStyle.BackColor = Color.FromArgb(225, 160, 160)
            End If
        End If
    End Sub

    ' ---------- GRADE HISTORY ----------

    Private Sub LoadHistory()
        UiKit.TryLoadData(Me, "grade history", AddressOf LoadHistoryCore)
    End Sub

    Private Sub LoadHistoryCore()
        dgvHistory.Rows.Clear()
        If dgvHistory.Columns.Count = 0 Then
            For Each h In {"Date", "Assessment", "Old Score", "New Score", "Changed by", "Reason"}
                dgvHistory.Columns.Add(h, h)
            Next
        End If

        Dim dt = _gradeService.GetGradeHistoryForStudent(_currentStudentId)

        If dt IsNot Nothing Then
            For Each row As DataRow In dt.Rows
                dgvHistory.Rows.Add(CDate(row("changed_at")).ToString("yyyy-MM-dd HH:mm"), row("assessment"), If(IsDBNull(row("old_value")), "---", row("old_value")), If(IsDBNull(row("new_value")), "---", row("new_value")), row("changed_by"), row("reason"))
            Next
        End If

        dgvHistory.ClearSelection()
    End Sub

    ' ---------- REQUEST REVISION ----------

    Private Sub cboClass_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboclass.SelectedIndexChanged
        UiKit.TryLoadData(Me, "class selections", Sub() cboClass_SelectedIndexChangedCore(sender, e))
    End Sub

    Private Sub cboClass_SelectedIndexChangedCore(sender As Object, e As EventArgs)
        If refreshingCourses Then Return
        cboAssessment.Items.Clear()
        assessmentMap.Clear()
        txtCurrentGrade.Clear()

        If cboclass.SelectedValue IsNot Nothing AndAlso IsNumeric(cboclass.SelectedValue) Then
            Dim enrollmentId As Integer = Convert.ToInt32(cboclass.SelectedValue)

            Dim sql As String = "SELECT sg.grade_id, a.title, sg.score, a.total_score " &
                               "FROM studentgrade sg " &
                               "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                               "WHERE sg.enrollment_id = @EnrollmentId AND sg.score IS NOT NULL"

            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, New MySqlParameter() {New MySqlParameter("@EnrollmentId", enrollmentId)})

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For Each row As DataRow In dt.Rows
                    Dim title As String = row("title").ToString()
                    Dim gradeId As Integer = Convert.ToInt32(row("grade_id"))

                    Dim itemIndex As Integer = cboAssessment.Items.Count

                    ' Include the unique grade ID so identical titles are distinguishable.
                    cboAssessment.Items.Add(title & " (Grade #" & gradeId & ")")

                    ' Store the exact database row for this dropdown position.
                    assessmentMap.Add(itemIndex, row)
                Next
                cboAssessment.Enabled = True
            Else
                cboAssessment.Enabled = False
            End If
        Else
            cboAssessment.Enabled = False
        End If

        LoadRequests()
    End Sub

    Private Sub cboAssessment_SelectedIndexChanged(
        sender As Object,
        e As EventArgs
    ) Handles cboAssessment.SelectedIndexChanged

        txtCurrentGrade.Clear()

        Dim selectedIndex As Integer = cboAssessment.SelectedIndex

        If selectedIndex < 0 OrElse
       Not assessmentMap.ContainsKey(selectedIndex) Then
            Return
        End If

        Dim row As DataRow = assessmentMap(selectedIndex)

        Dim scoreVal As Double = Convert.ToDouble(row("score"))
        Dim totalVal As Double = Convert.ToDouble(row("total_score"))

        txtCurrentGrade.Text = $"{scoreVal:0.##}/{totalVal:0.##}"

    End Sub

    Private requestResponse As TextBox

    Private Sub ConfigureRequestResponse()
        If requestResponse IsNot Nothing Then Return
        Dim panel As New Panel With {.Dock = DockStyle.Bottom, .Height = 145, .Padding = New Padding(8), .BackColor = Color.FromArgb(240, 250, 240)}
        Dim caption As New Label With {.Dock = DockStyle.Top, .Height = 28, .Text = "Reviewer response", .ForeColor = Color.DarkGreen, .Font = New Font("Segoe UI", 10, FontStyle.Bold)}
        requestResponse = New TextBox With {.Dock = DockStyle.Fill, .Multiline = True, .ReadOnly = True, .BorderStyle = BorderStyle.None, .ScrollBars = ScrollBars.Vertical, .BackColor = panel.BackColor, .Font = New Font("Segoe UI", 10)}
        panel.Controls.Add(requestResponse)
        panel.Controls.Add(caption)
        dgvRequests.Parent.Controls.Add(panel)
        dgvRequests.BringToFront()
        dgvRequests.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvRequests.MultiSelect = False
    End Sub

    Private Sub RequestSelectionChanged(sender As Object, e As EventArgs) Handles dgvRequests.SelectionChanged
        If requestResponse Is Nothing Then Return
        If dgvRequests.SelectedRows.Count = 0 Then
            requestResponse.Text = "Select a request to view the reviewer's response."
            Return
        End If
        requestResponse.Text = Convert.ToString(dgvRequests.SelectedRows(0).Tag)
    End Sub

    Private Sub LoadRequests()
        UiKit.TryLoadData(Me, "clarification requests", AddressOf LoadRequestsCore)
    End Sub

    Private Sub LoadRequestsCore()
        ConfigureRequestResponse()
        Dim selectedRequest = If(dgvRequests.CurrentRow Is Nothing, 0, Convert.ToInt32(dgvRequests.CurrentRow.Cells("Assessment").Tag))
        Dim enrollment As Integer? = If(IsNumeric(cboclass.SelectedValue), CType(Convert.ToInt32(cboclass.SelectedValue), Integer?), Nothing)
        Dim dt = _revisionService.GetRevisionRequestsByStudent(_currentStudentId, enrollment)
        dgvRequests.Rows.Clear()
        If dgvRequests.Columns.Count = 0 Then
            For Each h In {"Course", "Assessment", "Requested", "Status"}
                dgvRequests.Columns.Add(h, h)
            Next
        End If

        If dt IsNot Nothing Then
            For Each row As DataRow In dt.Rows
                Dim title As String = row("assessment_title").ToString()
                Dim reqPts As String = If(IsDBNull(row("requested_points")), "Re-check", "+" & row("requested_points").ToString() & " pts")
                Dim status As String = row("status").ToString()

                Dim index = dgvRequests.Rows.Add(row("class_label"), title, reqPts, status)
                dgvRequests.Rows(index).Cells("Assessment").Tag = CInt(row("request_id"))
                Dim response = Convert.ToString(row("reviewer_response"))
                If String.IsNullOrWhiteSpace(response) Then response = If(status = "Approved" OrElse status = "Closed", "No reviewer response was provided.", "Your request is awaiting review.")
                If Not IsDBNull(row("reviewed_at")) Then response = "Reviewed " & CDate(row("reviewed_at")).ToString("MMM d, yyyy HH:mm") & vbCrLf & vbCrLf & response
                dgvRequests.Rows(index).Tag = response
            Next
        End If

        dgvRequests.ClearSelection()
        If dgvRequests.Rows.Count > 0 Then
            Dim selected = dgvRequests.Rows.Cast(Of DataGridViewRow).FirstOrDefault(Function(row) Convert.ToInt32(row.Cells("Assessment").Tag) = selectedRequest)
            If selected Is Nothing Then selected = dgvRequests.Rows(0)
            dgvRequests.CurrentCell = selected.Cells("Assessment")
            selected.Selected = True
        End If
        RequestSelectionChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub btnSubmitRequest_Click(sender As Object, e As EventArgs) Handles btnSubmitRequest.Click
        If cboAssessment.SelectedIndex = -1 OrElse Not assessmentMap.ContainsKey(cboAssessment.SelectedIndex) Then
            MessageBox.Show("Please select an assessment item.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If String.IsNullOrWhiteSpace(txtReason.Text) Then
            MessageBox.Show("Please enter a reason for your request.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtReason.Focus()
            Return
        End If

        ' Create & submit RevisionRequest entity
        Dim selectedRow As DataRow = assessmentMap(cboAssessment.SelectedIndex)
        Dim gradeId As Integer = Convert.ToInt32(selectedRow("grade_id"))

        Dim req As New RevisionRequest With {
            .GradeId = gradeId,
            .Reason = txtReason.Text.Trim()
        }
        Dim result As OperationResult =
            _revisionService.SubmitRequestResult(req, _currentStudentId)

        If result.Success Then

            MessageBox.Show(
                result.Message,
                "Request Submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

            cboAssessment.SelectedIndex = -1
            txtCurrentGrade.Clear()
            txtReason.Clear()

            LoadRequests()

        Else

            MessageBox.Show(
                result.Message,
                "Unable to Submit Request",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

        End If

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
                Case "Closed", "Rejected"
                    e.CellStyle.BackColor = Color.FromArgb(225, 160, 160)
            End Select
        End If
    End Sub

    ' ---------- MY PERFORMANCE ----------

    Private Sub LoadPerformance()
        If cboperfterm.Items.Count = 0 Then
            cboperfterm.Items.AddRange({"Midterm", "Finals", "Combined"})
            cboperfterm.SelectedIndex = 2 ' Combined
        End If

        If cboperfcourse.Items.Count > 0 AndAlso cboperfcourse.SelectedIndex = -1 Then
            cboperfcourse.SelectedIndex = 0 ' Overall, triggers ShowPerformance
        Else
            ShowPerformance()
        End If
    End Sub

    Private Sub cboPerfTerm_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboperfterm.SelectedIndexChanged
        If refreshingCourses Then Return
        ShowPerformance()
    End Sub

    Private Sub cboPerfCourse_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboperfcourse.SelectedIndexChanged
        If refreshingCourses Then Return
        ShowPerformance()
    End Sub

    Private Sub ShowPerformance()
        UiKit.TryLoadData(Me, "performance", AddressOf ShowPerformanceCore)
    End Sub

    Private Sub ShowPerformanceCore()
        If cboperfterm.SelectedIndex = -1 OrElse cboperfcourse.SelectedIndex = -1 Then Return

        Dim termFilter As String = cboperfterm.Text
        Dim courseFilter As String = cboperfcourse.Text

        Dim sql As String =
            "SELECT a.term, a.assessment_type AS type, " &
            "sg.score, a.total_score, a.weight, " &
            "CASE WHEN EXISTS ( " &
            "    SELECT 1 FROM revisionrequest r " &
            "    WHERE r.grade_id = sg.grade_id " &
            "    AND r.status IN ('Pending', 'Under Review') " &
            ") THEN 'Pending' ELSE NULL END AS req_status " &
            "FROM assessment a " &
            "JOIN enrollment e ON a.class_id = e.class_id " &
            "JOIN class c ON e.class_id = c.class_id " &
            "JOIN course co ON c.course_code = co.course_code " &
            "LEFT JOIN studentgrade sg " &
            "ON a.assessment_id = sg.assessment_id " &
            "AND sg.enrollment_id = e.enrollment_id " &
            "WHERE e.student_id = @StudentId"

        Dim params As New List(Of MySqlParameter) From {New MySqlParameter("@StudentId", _currentStudentId)}

        If courseFilter <> OverallLabel Then
            If cboperfcourse.SelectedIndex >= performanceClassIds.Count Then Return
            sql &= " AND c.class_id = @ClassId"
            params.Add(New MySqlParameter("@ClassId", performanceClassIds(cboperfcourse.SelectedIndex)))
        End If

        If termFilter <> "Combined" Then
            sql &= " AND a.term = @Term"
            params.Add(New MySqlParameter("@Term", termFilter))
        End If

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, params.ToArray())

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim performanceItems As New List(Of ScoreItem)
            Dim totalPct As Double = 0
            Dim scoreCount As Integer = 0
            Dim passedCount As Integer = 0
            Dim pendingCount As Integer = 0
            Dim maxPct As Double = 0

            Dim quizScores As New List(Of ScoreItem)()
            Dim actScores As New List(Of ScoreItem)()
            Dim examScores As New List(Of ScoreItem)()
            Dim projScores As New List(Of ScoreItem)()

            For Each row As DataRow In dt.Rows
                performanceItems.Add(New ScoreItem(If(IsDBNull(row("score")), CType(Nothing, Double?), CDbl(row("score"))), CDbl(row("total_score")), CDbl(row("weight"))))
                Dim typeStr As String = row("type").ToString().ToLower()
                Dim reqStatus As String = If(IsDBNull(row("req_status")), "", row("req_status").ToString())

                If reqStatus = "Pending" OrElse reqStatus = "Under Review" Then
                    pendingCount += 1
                End If

                If Not IsDBNull(row("score")) AndAlso Not IsDBNull(row("total_score")) Then
                    Dim scoreVal As Double = Convert.ToDouble(row("score"))
                    Dim totalVal As Double = Convert.ToDouble(row("total_score"))

                    If totalVal > 0 Then
                        Dim pct As Double = GradeCalculator.CalculateScorePercentage(scoreVal, totalVal)
                        totalPct += pct
                        scoreCount += 1

                        If pct >= 75.0 Then passedCount += 1
                        If pct > maxPct Then maxPct = pct

                        Select Case typeStr
                            Case "quiz"
                                quizScores.Add(New ScoreItem(scoreVal, totalVal, CDbl(row("weight"))))
                            Case "activity", "seatwork", "homework"
                                actScores.Add(New ScoreItem(scoreVal, totalVal, CDbl(row("weight"))))
                            Case "exam"
                                examScores.Add(New ScoreItem(scoreVal, totalVal, CDbl(row("weight"))))
                            Case "project"
                                projScores.Add(New ScoreItem(scoreVal, totalVal, CDbl(row("weight"))))
                        End Select
                    End If
                End If
            Next

            Dim avgPct = GradeCalculator.CalculateRunningGrade(performanceItems)
            SetStat(lblAverageValue, If(avgPct.HasValue, avgPct.GetValueOrDefault().ToString("0.##"), "--"), "CURRENT AVERAGE")
            SetStat(lblPassedValue, passedCount.ToString(), "ACTIVITIES PASSED")
            SetStat(lblPendingValue, pendingCount.ToString(), "OPEN REQUESTS")
            SetStat(lblHighestValue, maxPct.ToString("0.0"), "HIGHEST SCORE")

            quizPct = If(quizScores.Count > 0, CInt(GradeCalculator.CalculateRunningGrade(quizScores).GetValueOrDefault()), 0)
            activityPct = If(actScores.Count > 0, CInt(GradeCalculator.CalculateRunningGrade(actScores).GetValueOrDefault()), 0)
            examPct = If(examScores.Count > 0, CInt(GradeCalculator.CalculateRunningGrade(examScores).GetValueOrDefault()), 0)
            projectPct = If(projScores.Count > 0, CInt(GradeCalculator.CalculateRunningGrade(projScores).GetValueOrDefault()), 0)
        Else
            SetStat(lblAverageValue, "--", "CURRENT AVERAGE")
            SetStat(lblPassedValue, "--", "ACTIVITIES PASSED")
            SetStat(lblPendingValue, "--", "OPEN REQUESTS")
            SetStat(lblHighestValue, "--", "HIGHEST SCORE")
            quizPct = 0 : activityPct = 0 : examPct = 0 : projectPct = 0
        End If

        UpdateBars()
    End Sub

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
            anyLabel.Text = value
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
        track.Height = Math.Max(32, lbl.Font.Height + 12)
        tlpbars.MinimumSize = New Size(0, 160)
        ' Keep the number outside the fill so even a small percentage is readable.
        If lbl.Parent IsNot track Then track.Controls.Add(lbl)
        lbl.AutoSize = False
        lbl.Dock = DockStyle.None
        lbl.SetBounds(Math.Max(4, track.ClientSize.Width - 58), 4, 50, track.ClientSize.Height - 8)
        lbl.TextAlign = ContentAlignment.MiddleCenter
        lbl.BackColor = Color.White
        lbl.ForeColor = Color.DarkGreen
        lbl.Text = pct & "%"
        lbl.BringToFront()
        fill.Dock = DockStyle.None
        fill.SetBounds(4, 4, Math.Max(0, CInt(Math.Max(0, track.ClientSize.Width - 68) * pct / 100.0)), track.ClientSize.Height - 8)
        fill.Visible = pct > 0 AndAlso fill.Width > 0
    End Sub

    Private Sub tlpBars_SizeChanged(sender As Object, e As EventArgs) Handles tlpbars.SizeChanged
        UpdateBars()
    End Sub

    ' ---------- DESIGNER EVENT STUBS (Preserved for Event Wiring) ----------

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

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboperfterm.SelectedIndexChanged
    End Sub

    Private Function ClassCode(fullName As String) As String
        If String.IsNullOrWhiteSpace(fullName) Then Return ""
        Return fullName.Split("-"c)(0).Trim()
    End Function

    Private Sub btnRequestRevision_Click(sender As Object, e As EventArgs) Handles btnRequestRevision.Click

    End Sub
End Class