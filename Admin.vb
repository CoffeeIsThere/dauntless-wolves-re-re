Imports System.Drawing.Drawing2D
Imports System.Drawing.Printing

Public Class Admin

    ' Instantiate backend service
    Private ReadOnly _userService As New UserService()
    ' Keeps track of the selected Updates filter
    Private _selectedUpdatesFilter As String = "All Updates"

    ' Dropdown menu for the Updates filter
    Private ReadOnly _updatesFilterMenu As New ContextMenuStrip()

    Private tabButtons As Button()

    Private Sub Admin_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' Check if user is logged in and is an Admin
        If AppSession.CurrentUser Is Nothing OrElse Not AppSession.IsAdmin Then
            MessageBox.Show("Please log in to access this page.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            Dim loginForm As New frmlogin()
            loginForm.Show()
            Me.Close()
            Return
        End If

        ' Displays actual full name from session
        lblAdminName.Text = AppSession.CurrentUser.FullName

        tabButtons = {btnDashboard, btnStudents, btnProfessors}
        ShowPanel(pnlDashboard, btnDashboard)
        InitProfessorPage()
        InitStudentPage()
        InitDashboardPage()
    End Sub

    Private Sub ShowPanel(p As Panel, activeBtn As Button)
        pnlDashboard.Visible = False
        pnlStudents.Visible = False
        pnlProfessors.Visible = False

        p.Visible = True
        p.BringToFront()

        For Each b As Button In tabButtons
            Dim isActive As Boolean = (b Is activeBtn)
            ' Fix: Update NormalColor on CustomButtons so hover doesn't wipe out active color
            If TypeOf b Is CustomButton Then
                DirectCast(b, CustomButton).NormalColor = If(isActive, Color.White, Color.FromArgb(200, 215, 205))
            Else
                b.BackColor = If(isActive, Color.White, Color.FromArgb(200, 215, 205))
            End If
            b.Font = New Font(b.Font, If(isActive, FontStyle.Bold, FontStyle.Regular))
        Next
    End Sub

    Private Sub btnDashboard_Click(sender As Object, e As EventArgs) Handles btnDashboard.Click
        ShowPanel(pnlDashboard, btnDashboard)
    End Sub

    Private Sub btnStudents_Click(sender As Object, e As EventArgs) Handles btnStudents.Click
        ShowPanel(pnlStudents, btnStudents)
    End Sub

    Private Sub btnProfessors_Click(sender As Object, e As EventArgs) Handles btnProfessors.Click
        ShowPanel(pnlProfessors, btnProfessors)
    End Sub

    ' ===== PROFESSOR PAGE =====
    Private allProfs As New List(Of ProfessorInfo)
    Private filteredProfs As New List(Of ProfessorInfo)
    Private profPage As Integer = 1
    Private Const ProfPageSize As Integer = 11
    Private ReadOnly pillFont As New Font("Segoe UI", 8.5F, FontStyle.Bold)

    Private Sub InitProfessorPage()
        SetupProfGrid()

        cboDeptProf.Items.Clear()
        cboDeptProf.Items.AddRange({"All", "CCS", "CAS", "CHTM", "CHS", "CED", "COE", "COA", "CON"})
        cboDeptProf.SelectedIndex = 0

        LoadProfsFromDb()
        ApplyProfFilter()
    End Sub

    Private Sub SetupProfGrid()
        Dim headerColor As Color = Color.FromArgb(224, 247, 250)

        With dgvProf
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .RowHeadersVisible = False
            .BackgroundColor = Color.White
            .BorderStyle = BorderStyle.None
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            .GridColor = Color.Gainsboro
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            .RowTemplate.Height = 36

            .EnableHeadersVisualStyles = False
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 32
            .ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            With .ColumnHeadersDefaultCellStyle
                .BackColor = headerColor
                .ForeColor = Color.FromArgb(0, 80, 0)
                .Font = New Font("Segoe UI", 9, FontStyle.Bold)
                .SelectionBackColor = headerColor
                .SelectionForeColor = Color.FromArgb(0, 80, 0)
            End With

            With .DefaultCellStyle
                .BackColor = Color.White
                .ForeColor = Color.Black
                .Font = New Font("Segoe UI", 9, FontStyle.Bold)
                .SelectionBackColor = Color.FromArgb(235, 250, 240)
                .SelectionForeColor = Color.Black
            End With

            .Columns.Clear()

            Dim chk As New DataGridViewCheckBoxColumn()
            chk.Name = "colCheck" : chk.HeaderText = ""
            .Columns.Add(chk)

            .Columns.Add("colName", "Name")
            .Columns.Add("colEmpId", "Employee ID")
            .Columns.Add("colDept", "Department")
            .Columns.Add("colClasses", "Assigned Class")
            .Columns.Add("colType", "Type")
            .Columns.Add("colStatus", "Status")
            .Columns.Add("colActions", "Actions")

            ' Fixed-width columns
            For Each item In {("colCheck", 45), ("colType", 110), ("colStatus", 110), ("colActions", 130)}
                With .Columns(item.Item1)
                    .AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                    .Width = item.Item2
                    .Resizable = DataGridViewTriState.False
                End With
            Next

            ' Stretching columns
            .Columns("colName").FillWeight = 30
            .Columns("colEmpId").FillWeight = 18
            .Columns("colDept").FillWeight = 18
            .Columns("colClasses").FillWeight = 22
            For Each n In {"colName", "colEmpId", "colDept", "colClasses"}
                .Columns(n).MinimumWidth = 110
            Next

            .Columns("colType").HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            .Columns("colStatus").HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            .Columns("colActions").HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter

            For Each c As DataGridViewColumn In .Columns
                If c.Name <> "colCheck" Then c.ReadOnly = True
                c.SortMode = DataGridViewColumnSortMode.NotSortable
            Next
        End With
    End Sub

    Private Sub LoadProfsFromDb()
        allProfs.Clear()
        Dim dt As DataTable = _userService.GetAllTeachers()

        For Each row As DataRow In dt.Rows
            Dim classCount As Integer = 0
            If dt.Columns.Contains("class_count") AndAlso Not IsDBNull(row("class_count")) Then
                classCount = Convert.ToInt32(row("class_count"))
            End If

            allProfs.Add(New ProfessorInfo With {
                .Name = row("full_name").ToString(),
                .EmployeeID = row("teacher_id").ToString(),
                .Department = row("department").ToString(),
                .Classes = classCount,
                .EmploymentType = If(dt.Columns.Contains("employment_type"), row("employment_type").ToString(), "Full-time"),
                .Status = row("status").ToString()
            })
        Next

        ApplyProfFilter()
    End Sub

    Private Sub ApplyProfFilter()
        Dim q As String = txtSearchProf.Text.Trim().ToLower()
        Dim dept As String = If(cboDeptProf.SelectedItem Is Nothing, "All", cboDeptProf.SelectedItem.ToString())

        filteredProfs = allProfs.Where(Function(p) _
            (q = "" OrElse p.Name.ToLower().Contains(q) OrElse p.EmployeeID.ToLower().Contains(q)) AndAlso
            (dept = "All" OrElse p.Department = dept)).ToList()

        profPage = 1
        ShowProfPage()
    End Sub

    Private Sub ShowProfPage()
        dgvProf.Rows.Clear()
        For Each p In filteredProfs.Skip((profPage - 1) * ProfPageSize).Take(ProfPageSize)
            dgvProf.Rows.Add(False, p.Name, p.EmployeeID, p.Department, p.Classes & " classes", p.EmploymentType, p.Status, "")
        Next
        lblTotalProf.Text = "Total Professors: " & filteredProfs.Count
        dgvProf.ClearSelection()
        BuildProfPager()
    End Sub

    Private Sub BuildProfPager()
        flowProfPager.SuspendLayout()
        flowProfPager.Controls.Clear()

        Const MinButtons As Integer = 5
        Dim totalPages As Integer = Math.Max(1, CInt(Math.Ceiling(filteredProfs.Count / ProfPageSize)))

        Dim startPage As Integer = Math.Max(1, profPage - 2)
        If totalPages > MinButtons Then
            startPage = Math.Min(startPage, totalPages - MinButtons + 1)
        Else
            startPage = 1
        End If

        ' Previous
        flowProfPager.Controls.Add(MakePagerButton("<", Math.Max(1, profPage - 1), False, profPage > 1))

        ' Number buttons
        For i As Integer = 0 To MinButtons - 1
            Dim pageNo As Integer = startPage + i
            Dim exists As Boolean = pageNo <= totalPages
            flowProfPager.Controls.Add(MakePagerButton(pageNo.ToString(), pageNo, pageNo = profPage, exists))
        Next

        ' Next
        flowProfPager.Controls.Add(MakePagerButton(">", Math.Min(totalPages, profPage + 1), False, profPage < totalPages))

        flowProfPager.AutoSize = False
        flowProfPager.Width = flowProfPager.Controls.Count * 32 + 8

        flowProfPager.ResumeLayout()
    End Sub

    Private Function MakePagerButton(text As String, page As Integer, active As Boolean, enabled As Boolean) As Button
        Dim b As New Button()
        b.Text = text
        b.Size = New Size(28, 28)
        b.Margin = New Padding(2)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderColor = If(enabled, Color.FromArgb(0, 100, 0), Color.Silver)
        b.BackColor = If(active, Color.FromArgb(0, 100, 0), Color.White)
        b.ForeColor = If(active, Color.White, If(enabled, Color.Black, Color.Silver))
        b.Enabled = enabled

        If enabled Then
            AddHandler b.Click, Sub()
                                    profPage = page
                                    ShowProfPage()
                                End Sub
        End If
        Return b
    End Function

    Private Sub txtSearchProf_TextChanged(sender As Object, e As EventArgs) Handles txtSearchProf.TextChanged
        ApplyProfFilter()
    End Sub

    Private Sub cboDeptProf_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboDeptProf.SelectedIndexChanged
        If allProfs.Count > 0 Then ApplyProfFilter()
    End Sub

    ' ---------- drawing helpers ----------
    Private Function RoundedRect(r As Rectangle, radius As Integer) As GraphicsPath
        Dim gp As New GraphicsPath()
        Dim d As Integer = radius * 2
        gp.AddArc(r.X, r.Y, d, d, 180, 90)
        gp.AddArc(r.Right - d, r.Y, d, d, 270, 90)
        gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
        gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
        gp.CloseFigure()
        Return gp
    End Function

    Private Sub DrawPill(g As Graphics, r As Rectangle, back As Color, fore As Color, text As String)
        Using path = RoundedRect(r, 8), br As New SolidBrush(back), tb As New SolidBrush(fore)
            g.FillPath(br, path)
            Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
            g.DrawString(text, pillFont, tb, r, sf)
        End Using
    End Sub

    Private Function ActionRects(cell As Rectangle) As Rectangle()
        Const w As Integer = 52, h As Integer = 24, gap As Integer = 6
        Dim startX As Integer = cell.X + (cell.Width - (w * 2 + gap)) \ 2
        Dim y As Integer = cell.Y + (cell.Height - h) \ 2
        Return {New Rectangle(startX, y, w, h), New Rectangle(startX + w + gap, y, w, h)}
    End Function

    Private Sub dgvProf_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles dgvProf.CellPainting
        If e.RowIndex < 0 Then Return
        Dim col As String = dgvProf.Columns(e.ColumnIndex).Name
        If col <> "colStatus" AndAlso col <> "colActions" AndAlso col <> "colType" Then Return

        e.PaintBackground(e.ClipBounds, False)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        If col = "colType" Then
            Dim t As String = Convert.ToString(e.Value)
            Dim r As New Rectangle(e.CellBounds.X + (e.CellBounds.Width - 90) \ 2,
                                   e.CellBounds.Y + (e.CellBounds.Height - 24) \ 2, 90, 24)
            If t = "Part-time" Then
                DrawPill(e.Graphics, r, Color.FromArgb(255, 232, 190), Color.FromArgb(150, 90, 0), "Part-time")
            Else
                DrawPill(e.Graphics, r, Color.FromArgb(205, 230, 250), Color.FromArgb(0, 70, 140), "Full-time")
            End If

        ElseIf col = "colStatus" Then
            Dim status As String = Convert.ToString(e.Value)
            Dim r As New Rectangle(e.CellBounds.X + (e.CellBounds.Width - 80) \ 2,
                                   e.CellBounds.Y + (e.CellBounds.Height - 24) \ 2, 80, 24)
            If status = "Active" Then
                DrawPill(e.Graphics, r, Color.FromArgb(198, 240, 214), Color.FromArgb(0, 100, 0), "Active")
            Else
                DrawPill(e.Graphics, r, Color.FromArgb(250, 190, 190), Color.FromArgb(180, 0, 0), "Inactive")
            End If

        Else
            Dim rects = ActionRects(e.CellBounds)
            DrawPill(e.Graphics, rects(0), Color.FromArgb(198, 240, 214), Color.FromArgb(0, 100, 0), "Edit")
            DrawPill(e.Graphics, rects(1), Color.FromArgb(250, 190, 190), Color.FromArgb(180, 0, 0), "Delete")
        End If

        e.Handled = True
    End Sub

    Private Sub dgvProf_CellMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvProf.CellMouseClick
        If e.RowIndex < 0 Then Return
        If dgvProf.Columns(e.ColumnIndex).Name <> "colActions" Then Return

        Dim size = dgvProf.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, False).Size
        Dim rects = ActionRects(New Rectangle(0, 0, size.Width, size.Height))
        Dim empId As String = dgvProf.Rows(e.RowIndex).Cells("colEmpId").Value.ToString()

        If rects(0).Contains(e.Location) Then
            MessageBox.Show("Edit " & empId)
        ElseIf rects(1).Contains(e.Location) Then
            If MessageBox.Show("Deactivate " & empId & "? They will no longer be able to log in.", "Confirm",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                If _userService.SetUserStatus("Teacher", empId, "Inactive", AppSession.CurrentUser.UserId) Then
                    LoadProfsFromDb()
                End If
            End If
        End If
    End Sub

    Private Sub btnAddProf_Click(sender As Object, e As EventArgs) Handles btnAddProf.Click
        MessageBox.Show("Open Add Professor form here")
    End Sub

    Private Sub pnlStudents_Paint(sender As Object, e As PaintEventArgs) Handles pnlStudents.Paint
    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click
    End Sub

    ' ===== STUDENT PAGE =====
    Private allStudents As New List(Of StudentInfo)
    Private filteredStudents As New List(Of StudentInfo)
    Private studentPage As Integer = 1
    Private Const StudentPageSize As Integer = 10

    Private Sub InitStudentPage()
        SetupStudentGrid()
        LoadStudentsFromDb()

        cboYearLevel.DropDownStyle = ComboBoxStyle.DropDownList
        cboSection.DropDownStyle = ComboBoxStyle.DropDownList

        cboYearLevel.Items.Clear()
        cboYearLevel.Items.AddRange({"All", "1", "2", "3", "4"})
        cboYearLevel.SelectedIndex = 0

        cboSection.Items.Clear()
        cboSection.Items.Add("All")
        For Each s In allStudents.Select(Function(x) x.Section).Distinct().OrderBy(Function(x) x)
            cboSection.Items.Add(s)
        Next
        cboSection.SelectedIndex = 0

        ApplyStudentFilter()
    End Sub

    Private Sub SetupStudentGrid()
        Dim headerColor As Color = Color.FromArgb(224, 247, 250)

        With dgvstudents
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .RowHeadersVisible = False
            .BackgroundColor = Color.White
            .BorderStyle = BorderStyle.None
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            .GridColor = Color.Gainsboro
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            .RowTemplate.Height = 36

            .EnableHeadersVisualStyles = False
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 32
            .ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            With .ColumnHeadersDefaultCellStyle
                .BackColor = headerColor
                .ForeColor = Color.FromArgb(0, 80, 0)
                .Font = New Font("Segoe UI", 9, FontStyle.Bold)
                .SelectionBackColor = headerColor
                .SelectionForeColor = Color.FromArgb(0, 80, 0)
            End With

            With .DefaultCellStyle
                .BackColor = Color.White
                .ForeColor = Color.Black
                .Font = New Font("Segoe UI", 9, FontStyle.Bold)
                .SelectionBackColor = Color.FromArgb(235, 250, 240)
                .SelectionForeColor = Color.Black
            End With

            .Columns.Clear()

            Dim chk As New DataGridViewCheckBoxColumn()
            chk.Name = "colCheck" : chk.HeaderText = ""
            .Columns.Add(chk)

            .Columns.Add("colStudentId", "Student ID")
            .Columns.Add("colFullName", "Full Name")
            .Columns.Add("colYear", "Year Level")
            .Columns.Add("colSection", "Section")
            .Columns.Add("colProgram", "Program")
            .Columns.Add("colStatus", "Status")
            .Columns.Add("colActions", "Actions")

            ' Fixed-width columns
            For Each item In {("colCheck", 45), ("colYear", 90), ("colSection", 100),
                              ("colStatus", 110), ("colActions", 130)}
                With .Columns(item.Item1)
                    .AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                    .Width = item.Item2
                    .Resizable = DataGridViewTriState.False
                End With
            Next

            ' Stretching columns
            .Columns("colStudentId").FillWeight = 20
            .Columns("colFullName").FillWeight = 40
            .Columns("colProgram").FillWeight = 40
            For Each n In {"colStudentId", "colFullName", "colProgram"}
                .Columns(n).MinimumWidth = 110
            Next

            For Each n In {"colYear", "colSection", "colStatus", "colActions"}
                .Columns(n).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            Next
            .Columns("colYear").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .Columns("colSection").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

            For Each c As DataGridViewColumn In .Columns
                If c.Name <> "colCheck" Then c.ReadOnly = True
                c.SortMode = DataGridViewColumnSortMode.NotSortable
            Next
        End With
    End Sub

    Private Sub LoadStudentsFromDb()
        allStudents.Clear()
        Dim dt As DataTable = _userService.GetAllStudents()

        For Each row As DataRow In dt.Rows
            allStudents.Add(New StudentInfo With {
                .StudentID = row("student_id").ToString(),
                .FullName = row("full_name").ToString(),
                .YearLevel = Convert.ToInt32(row("year_level")),
                .Section = row("section").ToString(),
                .Program = row("program").ToString(),
                .Status = row("status").ToString()
            })
        Next

        ApplyStudentFilter()
    End Sub

    Private Sub ApplyStudentFilter()
        If cboYearLevel.SelectedItem Is Nothing OrElse cboSection.SelectedItem Is Nothing Then Return

        Dim q As String = txtStudentSearch.Text.Trim().ToLower()
        Dim yr As String = cboYearLevel.SelectedItem.ToString()
        Dim sec As String = cboSection.SelectedItem.ToString()

        filteredStudents = allStudents.Where(Function(s) _
            (q = "" OrElse s.FullName.ToLower().Contains(q) OrElse s.StudentID.ToLower().Contains(q)) AndAlso
            (yr = "All" OrElse s.YearLevel.ToString() = yr) AndAlso
            (sec = "All" OrElse s.Section = sec)).ToList()

        studentPage = 1
        ShowStudentPage()
    End Sub

    Private Sub ShowStudentPage()
        dgvstudents.Rows.Clear()
        For Each s In filteredStudents.Skip((studentPage - 1) * StudentPageSize).Take(StudentPageSize)
            dgvstudents.Rows.Add(False, s.StudentID, s.FullName, s.YearLevel,
                                 s.Section, s.Program, s.Status, "")
        Next
        lbltotalstudents.Text = "Total Students: " & filteredStudents.Count
        dgvstudents.ClearSelection()
        BuildStudentPager()
    End Sub

    Private Sub BuildStudentPager()
        flowstudentpager.SuspendLayout()
        flowstudentpager.Controls.Clear()

        Const MinButtons As Integer = 5
        Dim totalPages As Integer = Math.Max(1, CInt(Math.Ceiling(filteredStudents.Count / StudentPageSize)))

        Dim startPage As Integer = Math.Max(1, studentPage - 2)
        If totalPages > MinButtons Then
            startPage = Math.Min(startPage, totalPages - MinButtons + 1)
        Else
            startPage = 1
        End If

        flowstudentpager.Controls.Add(MakeStudentPagerButton("<", Math.Max(1, studentPage - 1), False, studentPage > 1))
        For i As Integer = 0 To MinButtons - 1
            Dim pageNo As Integer = startPage + i
            flowstudentpager.Controls.Add(MakeStudentPagerButton(pageNo.ToString(), pageNo, pageNo = studentPage, pageNo <= totalPages))
        Next
        flowstudentpager.Controls.Add(MakeStudentPagerButton(">", Math.Min(totalPages, studentPage + 1), False, studentPage < totalPages))

        flowstudentpager.AutoSize = False
        flowstudentpager.Width = flowstudentpager.Controls.Count * 32 + 8
        flowstudentpager.ResumeLayout()
    End Sub

    Private Function MakeStudentPagerButton(text As String, page As Integer, active As Boolean, enabled As Boolean) As Button
        Dim b As New Button()
        b.Text = text
        b.Size = New Size(28, 28)
        b.Margin = New Padding(2)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderColor = If(enabled, Color.FromArgb(0, 100, 0), Color.Silver)
        b.BackColor = If(active, Color.FromArgb(0, 100, 0), Color.White)
        b.ForeColor = If(active, Color.White, If(enabled, Color.Black, Color.Silver))
        b.Enabled = enabled
        If enabled Then
            AddHandler b.Click, Sub()
                                    studentPage = page
                                    ShowStudentPage()
                                End Sub
        End If
        Return b
    End Function

    Private Sub txtStudentSearch_TextChanged(sender As Object, e As EventArgs) Handles txtStudentSearch.TextChanged
        ApplyStudentFilter()
    End Sub

    Private Sub cboYearLevel_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboYearLevel.SelectedIndexChanged
        If allStudents.Count > 0 Then ApplyStudentFilter()
    End Sub

    Private Sub cboSection_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSection.SelectedIndexChanged
        If allStudents.Count > 0 Then ApplyStudentFilter()
    End Sub

    Private Sub btnStudentFilter_Click(sender As Object, e As EventArgs) Handles btnStudentFilter.Click
        ApplyStudentFilter()
    End Sub

    Private Sub btnStudentReset_Click(sender As Object, e As EventArgs) Handles btnStudentReset.Click
        txtStudentSearch.Clear()
        cboYearLevel.SelectedIndex = 0
        cboSection.SelectedIndex = 0
        ApplyStudentFilter()
    End Sub

    Private Sub btnAddStudent_Click(sender As Object, e As EventArgs) Handles btnaddstudent.Click
        MessageBox.Show("Open Add Student form here")
    End Sub

    Private Sub dgvStudents_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles dgvstudents.CellPainting
        If e.RowIndex < 0 Then Return
        Dim col As String = dgvstudents.Columns(e.ColumnIndex).Name
        If col <> "colStatus" AndAlso col <> "colActions" Then Return

        e.PaintBackground(e.ClipBounds, False)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        If col = "colStatus" Then
            Dim status As String = Convert.ToString(e.Value)
            Dim r As New Rectangle(e.CellBounds.X + (e.CellBounds.Width - 80) \ 2,
                                   e.CellBounds.Y + (e.CellBounds.Height - 24) \ 2, 80, 24)
            If status = "Active" Then
                DrawPill(e.Graphics, r, Color.FromArgb(198, 240, 214), Color.FromArgb(0, 100, 0), "Active")
            Else
                DrawPill(e.Graphics, r, Color.FromArgb(250, 190, 190), Color.FromArgb(180, 0, 0), "Inactive")
            End If
        Else
            Dim rects = ActionRects(e.CellBounds)
            DrawPill(e.Graphics, rects(0), Color.FromArgb(198, 240, 214), Color.FromArgb(0, 100, 0), "Edit")
            DrawPill(e.Graphics, rects(1), Color.FromArgb(250, 190, 190), Color.FromArgb(180, 0, 0), "Delete")
        End If

        e.Handled = True
    End Sub

    Private Sub dgvStudents_CellMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvstudents.CellMouseClick
        If e.RowIndex < 0 Then Return
        If dgvstudents.Columns(e.ColumnIndex).Name <> "colActions" Then Return

        Dim size = dgvstudents.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, False).Size
        Dim rects = ActionRects(New Rectangle(0, 0, size.Width, size.Height))
        Dim id As String = dgvstudents.Rows(e.RowIndex).Cells("colStudentId").Value.ToString()

        If rects(0).Contains(e.Location) Then
            MessageBox.Show("Edit " & id)
        ElseIf rects(1).Contains(e.Location) Then
            If MessageBox.Show("Deactivate " & id & "? They will no longer be able to log in.", "Confirm",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                If _userService.SetUserStatus("Student", id, "Inactive", AppSession.CurrentUser.UserId) Then
                    LoadStudentsFromDb()
                    ApplyStudentFilter()
                End If
            End If
        End If
    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles lbltotalstudents.Click
    End Sub

    Private Sub pnlDashboard_Paint(sender As Object, e As PaintEventArgs) Handles pnlDashboard.Paint
    End Sub

    Private Sub Label7_Click_1(sender As Object, e As EventArgs) Handles Label7.Click
    End Sub

    Private Sub Label12_Click(sender As Object, e As EventArgs) Handles lblStatClasses.Click
    End Sub

    Private Sub Label11_Click(sender As Object, e As EventArgs) Handles Label11.Click
    End Sub

    Private Sub PictureBox4_Click(sender As Object, e As EventArgs) Handles PictureBox4.Click
    End Sub

    ' ===== DASHBOARD PAGE =====

    Private Sub InitDashboardPage()
        SetupUpdatesGrid()
        LoadDashboard()
    End Sub

    Private Sub SetupUpdatesGrid()
        dgvupdates.Dock = DockStyle.None
        dgvupdates.Location = New Point(13, Panel1.Bottom + 5)
        dgvupdates.Size = New Size(
    pnlupdatesheader.ClientSize.Width - 26,
    pnlupdatesheader.ClientSize.Height - dgvupdates.Top - 6
)
        dgvupdates.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or
                    AnchorStyles.Left Or AnchorStyles.Right
        With dgvupdates
            .ScrollBars = ScrollBars.Vertical
            .MultiSelect = False
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .AllowUserToOrderColumns = False
            .AllowUserToResizeColumns = False
            .Columns.Clear()
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .ReadOnly = True
            .RowHeadersVisible = False
            .ColumnHeadersVisible = False
            .BorderStyle = BorderStyle.None
            .BackgroundColor = Color.White
            .CellBorderStyle = DataGridViewCellBorderStyle.None
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            .RowTemplate.Height = 26
            .DefaultCellStyle.Font = New Font("Segoe UI", 9)
            .DefaultCellStyle.ForeColor = Color.FromArgb(0, 100, 50)
            .DefaultCellStyle.BackColor = Color.White
            .DefaultCellStyle.SelectionBackColor = Color.White
            .DefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 100, 50)
            .Columns.Add("colUpdate", "Update")
            .Columns.Add("colBy", "By")
            .Columns("colUpdate").FillWeight = 70
            .Columns("colBy").FillWeight = 30
        End With
    End Sub

    Private Sub LoadDashboard()
        lblStatStudents.Text = allStudents.Count.ToString("N0")
        lblStatProfs.Text = allProfs.Count.ToString("N0")

        Dim classCountObj As Object = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM class")
        lblStatClasses.Text = If(classCountObj IsNot Nothing AndAlso Not IsDBNull(classCountObj), Convert.ToInt32(classCountObj).ToString("N0"), "0")

        Dim pendingObj As Object = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM revisionrequest WHERE status IN ('Pending','Under Review')")
        lblStatPending.Text = If(pendingObj IsNot Nothing AndAlso Not IsDBNull(pendingObj), Convert.ToInt32(pendingObj).ToString("N0"), "0")

        LoadDashboardUpdates()
    End Sub

    Private Sub LoadDashboardUpdates()

        dgvupdates.Rows.Clear()

        Dim sql As String =
        "SELECT action, details, logged_at " &
        "FROM auditlog WHERE 1=1"

        ' Apply the selected filter
        Select Case _selectedUpdatesFilter

            Case "Student Accounts"
                sql &= " AND LOWER(entity_type) = 'student'"

            Case "Professor Accounts"
                sql &= " AND LOWER(entity_type) = 'teacher'"

            Case "Class/Subjects"
                sql &= " AND LOWER(entity_type) IN " &
                   "('class', 'course', 'enrollment')"

            Case "Grade Changes"
                sql &= " AND LOWER(entity_type) = 'studentgrade' " &
                   "AND UPPER(action) NOT LIKE '%REQUEST%' " &
                   "AND UPPER(action) NOT LIKE '%CLARIFICATION%'"

            Case "Revision Requests"
                sql &= " AND (LOWER(entity_type) = 'revisionrequest' " &
                   "OR UPPER(action) LIKE '%REQUEST%' " &
                   "OR UPPER(action) LIKE '%CLARIFICATION%')"

            Case "All Updates"
                ' No additional filter

        End Select

        ' Retrieve the six most recent matching records
        sql &= " ORDER BY logged_at DESC, log_id DESC LIMIT 6"

        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql)



            If dt IsNot Nothing Then
                For Each r As DataRow In dt.Rows

                    Dim updateText As String = r("action").ToString()
                    Dim details As String = r("details").ToString()

                    If Not String.IsNullOrWhiteSpace(details) Then
                        updateText &= " - " & details
                    End If

                    Dim updateDate As String =
                    CDate(r("logged_at")).ToString("MMM d, HH:mm")

                    dgvupdates.Rows.Add(updateText, updateDate)

                Next
            End If

            If dgvupdates.Rows.Count = 0 Then
                dgvupdates.Rows.Add("No matching updates found.", "")
            End If

            ' Clear selection after loading updates
            dgvupdates.ClearSelection()

            ' Reset scrolling after the UI finishes updating



        Catch ex As Exception
            MessageBox.Show(
            "Unable to load updates: " & ex.Message,
            "Updates Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        )
        End Try

    End Sub

    Private Sub btnUpdatesFilter_Click(
    sender As Object,
    e As EventArgs
) Handles btnupdatesfilter.Click

        ' Clear previous menu items
        _updatesFilterMenu.Items.Clear()

        Dim filters As String() = {
        "All Updates",
        "Student Accounts",
        "Professor Accounts",
        "Class/Subjects",
        "Grade Changes",
        "Revision Requests"
    }

        For Each filterName As String In filters

            Dim menuItem As New ToolStripMenuItem(filterName)

            ' Show a checkmark beside the selected filter
            menuItem.Checked = (filterName = _selectedUpdatesFilter)

            AddHandler menuItem.Click, AddressOf UpdatesFilterItem_Click

            _updatesFilterMenu.Items.Add(menuItem)

        Next

        ' Display the dropdown below the Filter button
        _updatesFilterMenu.Show(
        btnupdatesfilter,
        New Point(0, btnupdatesfilter.Height)
    )

    End Sub


    Private Sub UpdatesFilterItem_Click(
    sender As Object,
    e As EventArgs
)

        Dim selectedItem As ToolStripMenuItem =
        DirectCast(sender, ToolStripMenuItem)

        ' Store the selected filter
        _selectedUpdatesFilter = selectedItem.Text

        ' Reload the filtered records from MySQL
        LoadDashboardUpdates()

    End Sub

    Private Sub btnAddClass_Click(sender As Object, e As EventArgs) Handles btnAddClass.Click

        Using dlg As New frmAddClass()

            If dlg.ShowDialog(Me) = DialogResult.OK Then

                ' Refresh dashboard after adding a class
                LoadDashboard()

                MessageBox.Show(
                "Class/Subject added successfully!",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

            End If

        End Using

    End Sub

    Private ReadOnly quickForm As New quickactions()

    Private Sub OpenQuickAction(mode As quickactions.FormMode)
        quickForm.ShowMode(mode)
        quickForm.ShowDialog(Me)
    End Sub

    Private Sub btnEnterGrades_Click(sender As Object, e As EventArgs) Handles btnEnterGrades.Click
        OpenQuickAction(quickactions.FormMode.EnterGrades)
    End Sub

    Private Sub btnViewRequests_Click(sender As Object, e As EventArgs) Handles btnViewRequests.Click
        OpenQuickAction(quickactions.FormMode.ViewRequests)
    End Sub

    Private Sub btnViewLogs_Click(sender As Object, e As EventArgs) Handles btnViewLogs.Click
        OpenQuickAction(quickactions.FormMode.ViewAuditLogs)
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        If MessageBox.Show("Are you sure you want to log out?", "Confirm Logout",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then

            ' 1. Clear session
            AppSession.ClearSession()

            ' 2. Show login form
            Dim loginForm As New frmlogin()
            loginForm.Show()

            ' 3. Close Admin dashboard
            Me.Close()
        End If
    End Sub

    ' ===== EXPORT HELPER METHODS FOR ADMIN =====

    Private ReadOnly _exportService As New ExportService()

    ''' <summary>
    ''' Exports the currently visible Students table to a CSV file.
    ''' </summary>
    Public Sub ExportStudentsCSV()
        If dgvstudents.Rows.Count = 0 Then
            MessageBox.Show("There are no student records to export.", "Export Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Using sfd As New SaveFileDialog()
            sfd.Filter = "CSV Files (*.csv)|*.csv"
            sfd.FileName = $"Students_Export_{DateTime.Now:yyyyMMdd}.csv"

            If sfd.ShowDialog() = DialogResult.OK Then
                If _exportService.ExportToCSV(dgvstudents, sfd.FileName) Then
                    MessageBox.Show("Student list successfully exported to CSV!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Exports the currently visible Students table to a PDF report.
    ''' </summary>
    Public Sub ExportStudentsPDF()
        If dgvstudents.Rows.Count = 0 Then
            MessageBox.Show("There are no student records to export.", "Export Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Using sfd As New SaveFileDialog()
            sfd.Filter = "PDF Files (*.pdf)|*.pdf"
            sfd.FileName = $"Students_Report_{DateTime.Now:yyyyMMdd}.pdf"

            If sfd.ShowDialog() = DialogResult.OK Then
                Dim printDoc As New PrintDocument()
                AddHandler printDoc.PrintPage, Sub(s, args)
                                                   Dim g As Graphics = args.Graphics
                                                   Dim titleFont As New Font("Segoe UI", 14, FontStyle.Bold)
                                                   Dim bodyFont As New Font("Segoe UI", 9, FontStyle.Regular)
                                                   Dim startY As Integer = 50

                                                   g.DrawString("DAUNTLESS WOLVES - STUDENT DIRECTORY", titleFont, Brushes.DarkGreen, 50, startY)
                                                   startY += 40

                                                   g.DrawString("Student ID", bodyFont, Brushes.Black, 50, startY)
                                                   g.DrawString("Full Name", bodyFont, Brushes.Black, 180, startY)
                                                   g.DrawString("Year/Sec", bodyFont, Brushes.Black, 400, startY)
                                                   g.DrawString("Status", bodyFont, Brushes.Black, 520, startY)
                                                   startY += 25
                                                   g.DrawLine(Pens.Gray, 50, startY, 650, startY)
                                                   startY += 10

                                                   For Each row As DataGridViewRow In dgvstudents.Rows
                                                       If Not row.IsNewRow Then
                                                           Dim sid As String = Convert.ToString(row.Cells("colStudentId").Value)
                                                           Dim name As String = Convert.ToString(row.Cells("colFullName").Value)
                                                           Dim yrSec As String = $"{row.Cells("colYear").Value} - {row.Cells("colSection").Value}"
                                                           Dim st As String = Convert.ToString(row.Cells("colStatus").Value)

                                                           g.DrawString(sid, bodyFont, Brushes.Black, 50, startY)
                                                           g.DrawString(name, bodyFont, Brushes.Black, 180, startY)
                                                           g.DrawString(yrSec, bodyFont, Brushes.Black, 400, startY)
                                                           g.DrawString(st, bodyFont, Brushes.Black, 520, startY)
                                                           startY += 22
                                                       End If
                                                   Next
                                               End Sub

                If _exportService.ExportToPDF(printDoc, sfd.FileName) Then
                    MessageBox.Show("Student PDF report generated successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If
        End Using
    End Sub

    ' ===== LOGOUT HANDLER =====



End Class