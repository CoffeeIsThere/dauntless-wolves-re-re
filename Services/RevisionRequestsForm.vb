Public Class RevisionRequestsForm
    Inherits Form

    Private ReadOnly service As New RevisionRequestService()
    Private ReadOnly teacherId As String
    Private ReadOnly classId As Integer
    Private ReadOnly grid As New DataGridView With {.Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False, .AutoGenerateColumns = False, .RowHeadersVisible = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .MultiSelect = False, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, .BackgroundColor = Color.White, .BorderStyle = BorderStyle.None}
    Private ReadOnly statuses As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 150}
    Private ReadOnly allClasses As New CheckBox With {.Text = "All my classes", .AutoSize = True}
    Private ReadOnly search As New TextBox With {.Width = 210, .PlaceholderText = "Search student or assessment"}
    Private ReadOnly reason As New TextBox With {.Dock = DockStyle.Fill, .Multiline = True, .ReadOnly = True, .ScrollBars = ScrollBars.Vertical, .BackColor = Color.FromArgb(240, 248, 240), .BorderStyle = BorderStyle.None}
    Private ReadOnly review As New Button With {.Text = "Review selected", .AutoSize = True, .BackColor = Color.DarkGreen, .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .Padding = New Padding(8, 3, 8, 3)}
    Private data As DataTable

    Public Sub New(teacher As String, selectedClass As Integer)
        teacherId = teacher
        classId = selectedClass
        Text = "The Dauntless Grading System — Revision Requests"
        Size = New Size(1000, 640)
        MinimumSize = New Size(800, 540)
        StartPosition = FormStartPosition.CenterParent
        ShowInTaskbar = False
        BackColor = Color.White
        Font = New Font("Segoe UI", 10)
        Dim layout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 5, .Padding = New Padding(18)}
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        For Each height As Single In {38, 44}
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, height))
        Next
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 90))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44))
        Dim title As New Label With {.Text = "Student clarification requests", .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 14, FontStyle.Bold), .ForeColor = Color.DarkGreen}
        Dim filters As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .WrapContents = False}
        statuses.Items.AddRange({"Open", "Pending", "Under Review", "Approved", "Closed", "All"})
        statuses.SelectedIndex = 0
        allClasses.Checked = classId = 0
        allClasses.Enabled = classId <> 0
        Dim refresh As New Button With {.Text = "Refresh", .AutoSize = True}
        filters.Controls.AddRange({statuses, allClasses, search, refresh})
        For Each column In {("student_name", "Student"), ("class_label", "Class"), ("assessment_title", "Assessment"), ("current_score", "Score"), ("requested_points", "+ Points"), ("status", "Status")}
            grid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = column.Item1, .HeaderText = column.Item2, .Name = column.Item1, .MinimumWidth = 80})
        Next
        grid.RowTemplate.Height = 36
        grid.ColumnHeadersHeight = 40
        AddHandler grid.CellFormatting, Sub(sender, args)
                                            If args.RowIndex < 0 OrElse args.ColumnIndex < 0 Then Return
                                            Dim columnName = grid.Columns(args.ColumnIndex).Name
                                            If columnName = "requested_points" AndAlso (args.Value Is Nothing OrElse IsDBNull(args.Value)) Then
                                                args.Value = "Re-check"
                                                args.FormattingApplied = True
                                            ElseIf columnName = "current_score" AndAlso (args.Value Is Nothing OrElse IsDBNull(args.Value)) Then
                                                args.Value = "Not graded"
                                                args.FormattingApplied = True
                                            ElseIf columnName = "status" Then
                                                Dim status = Convert.ToString(args.Value)
                                                args.CellStyle.BackColor = If(status = "Approved", Color.FromArgb(220, 240, 220), If(status = "Closed", Color.FromArgb(248, 226, 226), Color.FromArgb(255, 249, 220)))
                                                args.CellStyle.ForeColor = Color.DarkGreen
                                            End If
                                        End Sub
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 242, 232)
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkGreen
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 238, 220)
        grid.DefaultCellStyle.SelectionForeColor = Color.DarkGreen
        Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
        Dim close As New Button With {.Text = "Close", .AutoSize = True, .DialogResult = DialogResult.Cancel}
        footer.Controls.AddRange({close, review})
        layout.Controls.Add(title, 0, 0)
        layout.Controls.Add(filters, 0, 1)
        layout.Controls.Add(grid, 0, 2)
        layout.Controls.Add(reason, 0, 3)
        layout.Controls.Add(footer, 0, 4)
        Controls.Add(layout)
        CancelButton = close
        AddHandler Shown, Sub(sender, args) ReloadRequests()
        AddHandler refresh.Click, Sub(sender, args) ReloadRequests()
        AddHandler statuses.SelectedIndexChanged, Sub(sender, args) ReloadRequests()
        AddHandler allClasses.CheckedChanged, Sub(sender, args) ApplyFilters()
        AddHandler search.TextChanged, Sub(sender, args) ApplyFilters()
        AddHandler grid.SelectionChanged, Sub(sender, args) ShowSelection()
        AddHandler review.Click, Sub(sender, args) ReviewSelection()
        AddHandler grid.CellDoubleClick, Sub(sender, args)
                                             If args.RowIndex >= 0 Then ReviewSelection()
                                         End Sub
    End Sub

    Private Sub ReloadRequests()
        Try
            data = service.GetRequestsByTeacher(teacherId, statuses.Text)
            ApplyFilters()
        Catch ex As Exception
            MessageBox.Show(Me, "Could not load requests: " & ex.Message)
        End Try
    End Sub

    Private Sub ApplyFilters()
        If data Is Nothing Then Return
        Dim filtered = data.Clone()
        For Each row As DataRow In data.Rows
            If Not allClasses.Checked AndAlso CInt(row("class_id")) <> classId Then Continue For
            If search.Text.Trim() <> "" AndAlso Not (row("student_name").ToString() & " " & row("assessment_title").ToString()).Contains(search.Text.Trim(), StringComparison.OrdinalIgnoreCase) Then Continue For
            filtered.ImportRow(row)
        Next
        grid.DataSource = filtered
        ShowSelection()
    End Sub

    Private Sub ShowSelection()
        Dim row = If(grid.CurrentRow Is Nothing, Nothing, TryCast(grid.CurrentRow.DataBoundItem, DataRowView))
        review.Enabled = row IsNot Nothing AndAlso {"Pending", "Under Review"}.Contains(row("status").ToString())
        reason.Text = If(row Is Nothing, "No matching requests. Adjust the filters or refresh.", "Student's reason:" & vbCrLf & row("reason").ToString())
    End Sub

    Private Sub ReviewSelection()
        If Not review.Enabled OrElse grid.CurrentRow Is Nothing Then Return
        Dim row = DirectCast(grid.CurrentRow.DataBoundItem, DataRowView)
        If RevisionReviewDialog.Review(Me, CInt(row("request_id")), teacherId) Then ReloadRequests()
    End Sub
End Class
