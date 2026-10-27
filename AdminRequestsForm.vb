Imports System.ComponentModel
Imports System.Data

' A read-only overview with full decision history; teachers retain approval authority.
Public Class AdminRequestsForm
    Inherits Form

    Private ReadOnly administrator As String
    Private ReadOnly service As New AdminWorkspaceService()
    Private ReadOnly view As New DataGridView With {.Name = "dgvAdminRequests", .Dock = DockStyle.Fill, .AutoGenerateColumns = False, .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False, .RowHeadersVisible = False, .MultiSelect = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .BackgroundColor = Color.White, .BorderStyle = BorderStyle.None, .EnableHeadersVisualStyles = False}
    Private ReadOnly search As New TextBox With {.Width = 195, .MaxLength = 100, .PlaceholderText = "Student name or ID"}
    Private ReadOnly statuses As New ComboBox With {.Width = 135, .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly courses As New ComboBox With {.Width = 280, .DropDownWidth = 680, .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly fromDate As New DateTimePicker With {.Width = 140, .Format = DateTimePickerFormat.Short, .ShowCheckBox = True, .Checked = False}
    Private ReadOnly toDate As New DateTimePicker With {.Width = 140, .Format = DateTimePickerFormat.Short, .ShowCheckBox = True, .Checked = False}
    Private ReadOnly state As New Label With {.Dock = DockStyle.Fill, .AutoEllipsis = True, .TextAlign = ContentAlignment.MiddleLeft, .ForeColor = Color.DarkGreen}
    Private ReadOnly retry As Button = UiKit.MakeSecondaryButton("Retry", 80)
    Private ReadOnly csv As Button = UiKit.MakeSecondaryButton("Export CSV", 110)
    Private ReadOnly pdf As Button = UiKit.MakePrimaryButton("Save as PDF", 110)
    Private ReadOnly detailsTitle As New Label With {.Dock = DockStyle.Top, .Height = 28, .Font = New Font("Segoe UI Semibold", 11), .ForeColor = Color.DarkGreen, .Text = "Select a request"}
    Private ReadOnly metadata As TextBox = DetailText()
    Private ReadOnly reason As TextBox = DetailText()
    Private ReadOnly response As TextBox = DetailText()
    Private ReadOnly refreshTimer As New System.Windows.Forms.Timer With {.Interval = 5000}
    Private applied As RequestFilter
    Private revision As Integer
    Private loading As Boolean
    Private manualLoadFailed As Boolean
    Private rebindingRows As Boolean
    Private displayedRequestId As Integer
    Private exportBusy As Boolean
    Private loadedSuccessfully As Boolean
    Private initializing As Boolean = True

    Private Class RequestFilter
        Public Search, Status, Course As String
        Public ClassId As Integer
        Public FromDate, ThroughDate As Date?
        Public Function Description() As String
            Return "Student: " & If(Search = "", "All", Search) & "; Status: " & Status & "; Course: " & Course & "; From: " & If(FromDate.HasValue, FromDate.Value.ToShortDateString(), "Any") & "; Through: " & If(ThroughDate.HasValue, ThroughDate.Value.ToShortDateString(), "Any")
        End Function
    End Class

    Public Sub New(adminId As String, Optional onlyOpen As Boolean = False)
        administrator = adminId
        UiKit.StyleDialog(Me, "The Dauntless Grading System — Clarification Requests", 1140, 690)
        FormBorderStyle = FormBorderStyle.Sizable : MaximizeBox = True
        MinimumSize = New Size(960, 620)
        statuses.Items.AddRange({"All", "Open", "Pending", "Under Review", "Approved", "Closed"})
        statuses.SelectedItem = If(onlyOpen, "Open", "All")
        BindCourses(New DataTable())
        ConfigureGrid()
        Dim filters As New FlowLayoutPanel With {.Dock = DockStyle.Top, .WrapContents = True, .Padding = New Padding(12, 6, 12, 6), .Height = 110}
        filters.Controls.AddRange({Field("Student", search), Field("Status", statuses), Field("Course / section", courses), Field("Requested from", fromDate), Field("Requested through", toDate)})
        filters.SetFlowBreak(filters.Controls(filters.Controls.Count - 1), True)
        Dim apply = UiKit.MakePrimaryButton("Apply filters", 110)
        Dim reset = UiKit.MakeSecondaryButton("Reset", 80)
        Dim refresh = UiKit.MakeSecondaryButton("Refresh", 90)
        filters.Controls.AddRange({apply, reset, refresh})
        AddHandler filters.Layout, Sub() filters.Height = filters.GetPreferredSize(New Size(Math.Max(300, filters.ClientSize.Width), 0)).Height
        AddHandler filters.SizeChanged, Sub() filters.PerformLayout()
        AddHandler apply.Click, Async Sub() Await ReloadAsync()
        AddHandler refresh.Click, Async Sub() Await ReloadAsync()
        AddHandler retry.Click, Async Sub() Await ReloadAsync()
        AddHandler reset.Click, Async Sub()
                                    initializing = True
                                    search.Clear() : statuses.SelectedItem = "All" : courses.SelectedValue = 0
                                    fromDate.Checked = False : toDate.Checked = False
                                    initializing = False
                                    Await ReloadAsync()
                                End Sub
        AcceptButton = apply
        AddHandler search.TextChanged, AddressOf FiltersChanged
        AddHandler statuses.SelectedIndexChanged, AddressOf FiltersChanged
        AddHandler courses.SelectedIndexChanged, AddressOf FiltersChanged
        AddHandler fromDate.ValueChanged, AddressOf FiltersChanged
        AddHandler toDate.ValueChanged, AddressOf FiltersChanged

        Dim body As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 2, .Padding = New Padding(12, 0, 12, 0)}
        body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        body.RowStyles.Add(New RowStyle(SizeType.Percent, 100)) : body.RowStyles.Add(New RowStyle(SizeType.Absolute, 200))
        body.Controls.Add(view, 0, 0) : body.Controls.Add(CreateDetails(), 0, 1)
        Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 48, .Padding = New Padding(8), .FlowDirection = FlowDirection.RightToLeft}
        Dim close = UiKit.MakeSecondaryButton("Close", 90)
        close.DialogResult = DialogResult.Cancel : CancelButton = close
        footer.Controls.AddRange({close, pdf, csv})
        Dim feedback As New Panel With {.Dock = DockStyle.Bottom, .Height = 38, .Padding = New Padding(14, 2, 14, 2)}
        retry.Dock = DockStyle.Right : retry.Visible = False
        feedback.Controls.Add(state) : feedback.Controls.Add(retry) : state.BringToFront()
        Controls.Add(body) : Controls.Add(feedback) : Controls.Add(footer) : Controls.Add(filters) : Controls.Add(UiKit.MakeHeader("Clarification Requests")) : body.BringToFront()
        AddHandler csv.Click, Sub() ExportRows(False)
        AddHandler pdf.Click, Sub() ExportRows(True)
        AddHandler view.SelectionChanged, Sub()
                                              If Not rebindingRows Then ShowDetails()
                                          End Sub
        AddHandler refreshTimer.Tick, Async Sub()
                                          If Visible AndAlso Not loading AndAlso Not exportBusy Then Await ReloadAsync(True)
                                      End Sub
        AddHandler Shown, Async Sub()
                              Dim available = Screen.FromControl(Me).WorkingArea
                              MinimumSize = New Size(Math.Min(960, available.Width), Math.Min(620, available.Height))
                              Size = New Size(Math.Min(Width, available.Width), Math.Min(Height, available.Height))
                              initializing = False
                              refreshTimer.Start()
                              Await ReloadAsync()
                          End Sub
        AddHandler Activated, Async Sub()
                                  If applied IsNot Nothing AndAlso Not loading AndAlso Not exportBusy Then Await ReloadAsync(True)
                              End Sub
        UpdateExportButtons()
    End Sub

    Private Sub ConfigureGrid()
        view.ColumnHeadersDefaultCellStyle.BackColor = UiKit.SoftGreenColor
        view.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkGreen
        view.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI Semibold", 9)
        view.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True
        view.ColumnHeadersHeight = 44
        view.RowTemplate.Height = 40
        view.GridColor = Color.FromArgb(220, 230, 222)
        view.DefaultCellStyle.SelectionBackColor = Color.FromArgb(213, 235, 219)
        view.DefaultCellStyle.SelectionForeColor = Color.Black
        For Each column In {("request_id", "Request #", 65), ("student_name", "Student", 160), ("student_id", "Student ID", 95), ("course", "Course / section", 175), ("assessment", "Assessment", 130), ("current_score", "Current score", 90), ("requested_points", "Requested adjustment", 100), ("status", "Status", 105), ("requested_at", "Requested on", 140)}
            view.Columns.Add(New DataGridViewTextBoxColumn With {.Name = column.Item1, .DataPropertyName = column.Item1, .HeaderText = column.Item2, .Width = column.Item3, .MinimumWidth = 60, .SortMode = DataGridViewColumnSortMode.Automatic})
        Next
        view.Columns("student_name").DefaultCellStyle.WrapMode = DataGridViewTriState.True
        view.Columns("assessment").DefaultCellStyle.WrapMode = DataGridViewTriState.True
        view.Columns("requested_at").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        view.Columns("requested_at").MinimumWidth = 140
        view.Columns("requested_at").DefaultCellStyle.Format = "MMM dd, yyyy HH:mm"
        AddHandler view.CellFormatting, Sub(sender, args)
                                            If args.RowIndex < 0 OrElse args.ColumnIndex < 0 Then Return
                                            Dim record = TryCast(view.Rows(args.RowIndex).DataBoundItem, DataRowView)
                                            If record Is Nothing Then Return
                                            Select Case view.Columns(args.ColumnIndex).Name
                                                Case "current_score"
                                                    args.Value = ScoreText(record.Row) : args.FormattingApplied = True
                                                Case "requested_points"
                                                    args.Value = Adjustment(record.Row) : args.FormattingApplied = True
                                                Case "status"
                                                    args.CellStyle.BackColor = StatusColor(Convert.ToString(record("status")))
                                                    args.CellStyle.ForeColor = Color.DarkGreen
                                                    args.CellStyle.SelectionBackColor = args.CellStyle.BackColor
                                                    args.CellStyle.SelectionForeColor = Color.Black
                                            End Select
                                        End Sub
    End Sub

    Private Shared Function StatusColor(status As String) As Color
        Select Case status
            Case "Pending" : Return Color.FromArgb(255, 246, 200)
            Case "Under Review" : Return Color.FromArgb(220, 235, 250)
            Case "Approved" : Return Color.FromArgb(200, 237, 212)
            Case "Closed" : Return Color.FromArgb(245, 220, 220)
            Case Else : Return Color.White
        End Select
    End Function

    Private Shared Function Field(caption As String, input As Control) As Control
        Dim container As New Panel With {.Width = input.Width, .Height = 48, .Margin = New Padding(3, 0, 6, 3)}
        input.Dock = DockStyle.Bottom
        container.Controls.Add(input)
        container.Controls.Add(New Label With {.Dock = DockStyle.Top, .Height = 19, .Text = caption, .ForeColor = Color.DarkGreen})
        Return container
    End Function

    Private Shared Function DetailText() As TextBox
        Return New TextBox With {.Dock = DockStyle.Fill, .Multiline = True, .ReadOnly = True, .ScrollBars = ScrollBars.Vertical, .BorderStyle = BorderStyle.None, .BackColor = UiKit.SoftGreenColor, .Font = New Font("Segoe UI", 9)}
    End Function

    Private Function CreateDetails() As Control
        Dim panel As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(8), .BackColor = UiKit.SoftGreenColor}
        Dim notes As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 1}
        notes.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50)) : notes.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        For Each entry In New (String, TextBox)() {("Student's reason", reason), ("Professor's response", response)}
            Dim section As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(4)}
            section.Controls.Add(entry.Item2)
            section.Controls.Add(New Label With {.Text = entry.Item1, .Dock = DockStyle.Top, .Height = 22, .ForeColor = Color.DarkGreen, .Font = New Font("Segoe UI Semibold", 9)})
            entry.Item2.BringToFront()
            notes.Controls.Add(section)
        Next
        metadata.Dock = DockStyle.Top : metadata.Height = 68
        panel.Controls.Add(notes) : panel.Controls.Add(metadata) : panel.Controls.Add(detailsTitle) : notes.BringToFront()
        Return panel
    End Function

    Private Sub ShowDetails()
        Dim record = If(view.CurrentRow Is Nothing, Nothing, TryCast(view.CurrentRow.DataBoundItem, DataRowView))
        If record Is Nothing Then
            displayedRequestId = 0
            detailsTitle.Text = "Select a request" : metadata.Clear() : reason.Clear() : response.Clear()
            Return
        End If
        Dim row = record.Row
        Dim selectedChanged = displayedRequestId <> CInt(row("request_id"))
        displayedRequestId = CInt(row("request_id"))
        detailsTitle.Text = "Request #" & row("request_id").ToString() & " — " & row("status").ToString()
        Dim details = row("student_name").ToString() & " (" & row("student_id").ToString() & ") • " & row("course").ToString() & vbCrLf &
            row("assessment").ToString() & " • Current score: " & ScoreText(row) & " • Requested: " & Adjustment(row) & vbCrLf &
            "Requested: " & DateText(row("requested_at")) & " • Assigned professor: " & OrText(row("assigned_professor"), "Unassigned") & vbCrLf &
            "Reviewed: " & DateText(row("reviewed_at")) & " • Reviewed by: " & OrText(row("reviewed_by"), "Not recorded")
        SetDetailText(metadata, details, selectedChanged)
        SetDetailText(reason, Convert.ToString(row("reason")), selectedChanged)
        SetDetailText(response, OrText(row("response"), If({"Approved", "Closed"}.Contains(CStr(row("status"))), "No reviewer response was recorded.", "No response yet; this request is still open.")), selectedChanged)
    End Sub

    Private Shared Sub SetDetailText(box As TextBox, text As String, reset As Boolean)
        ' Leave unchanged native text alone so its selection and vertical scroll stay where the reader put them.
        If box.Text = text AndAlso Not reset Then Return
        box.Text = text
        box.Select(0, 0)
        box.ScrollToCaret()
    End Sub

    Private Shared Function ScoreText(row As DataRow) As String
        If IsDBNull(row("current_score")) Then Return "Not graded"
        Return CDbl(row("current_score")).ToString("0.##") & "/" & CDbl(row("maximum_score")).ToString("0.##")
    End Function
    Private Shared Function Adjustment(row As DataRow) As String
        If IsDBNull(row("requested_points")) Then Return "Re-check"
        Dim points = CDbl(row("requested_points"))
        Return If(points >= 0, "+", "") & points.ToString("0.##") & " pts"
    End Function
    Private Shared Function OrText(value As Object, fallback As String) As String
        Return If(String.IsNullOrWhiteSpace(Convert.ToString(value)), fallback, Convert.ToString(value))
    End Function
    Private Shared Function DateText(value As Object) As String
        If IsDBNull(value) Then Return "Not recorded"
        Return Convert.ToDateTime(value).ToString("MMM dd, yyyy HH:mm")
    End Function

    Private Sub BindCourses(data As DataTable)
        Dim selectedId = If(courses.SelectedValue Is Nothing OrElse Not IsNumeric(courses.SelectedValue), 0, CInt(courses.SelectedValue))
        Dim choices As New DataTable()
        choices.Columns.Add("class_id", GetType(Integer)) : choices.Columns.Add("class_label", GetType(String))
        choices.Rows.Add(0, "All courses")
        For Each row As DataRow In data.Rows
            choices.Rows.Add(CInt(row("class_id")), row("class_label").ToString())
        Next
        If SameRows(TryCast(courses.DataSource, DataTable), choices) Then Return
        courses.DisplayMember = "class_label" : courses.ValueMember = "class_id" : courses.DataSource = choices
        courses.SelectedValue = If(choices.Rows.Cast(Of DataRow).Any(Function(row) CInt(row("class_id")) = selectedId), selectedId, 0)
    End Sub

    Private Function CurrentFilter() As RequestFilter
        Return New RequestFilter With {.Search = search.Text.Trim(), .Status = statuses.Text, .ClassId = If(IsNumeric(courses.SelectedValue), CInt(courses.SelectedValue), 0), .Course = courses.Text, .FromDate = If(fromDate.Checked, CType(fromDate.Value.Date, Date?), Nothing), .ThroughDate = If(toDate.Checked, CType(toDate.Value.Date, Date?), Nothing)}
    End Function

    Private Sub FiltersChanged(sender As Object, args As EventArgs)
        If initializing OrElse applied Is Nothing OrElse manualLoadFailed Then Return
        If CurrentFilter().Description() <> applied.Description() Then state.Text = "Filters changed — choose Apply filters. Exports use the displayed results."
    End Sub

    Private Async Function ReloadAsync(Optional automatic As Boolean = False) As Task
        If exportBusy OrElse IsDisposed OrElse (automatic AndAlso (loading OrElse applied Is Nothing OrElse manualLoadFailed)) Then Return
        Dim filter = If(automatic, applied, CurrentFilter())
        revision += 1
        Dim current = revision
        loading = True : loadedSuccessfully = False
        state.Text = "Loading clarification requests…" : state.ForeColor = Color.DarkGreen : retry.Visible = False
        UpdateExportButtons()
        Try
            Dim snapshot = Await Task.Run(Function() (Rows:=service.RequestRows(administrator, filter.Search, filter.Status, filter.ClassId, filter.FromDate, filter.ThroughDate), Courses:=service.RequestCourses(administrator)))
            If IsDisposed OrElse Disposing OrElse current <> revision Then Return
            Dim changed = Not SameRows(TryCast(view.DataSource, DataTable), snapshot.Rows)
            If changed Then
                rebindingRows = True
                Try
                    Dim selectedId = If(view.CurrentRow Is Nothing, 0, CInt(DirectCast(view.CurrentRow.DataBoundItem, DataRowView)("request_id")))
                    Dim scroll = view.FirstDisplayedScrollingRowIndex
                    Dim sortColumn = If(view.SortedColumn Is Nothing, "", view.SortedColumn.Name)
                    Dim sortDirection = If(view.SortOrder = SortOrder.Descending, ListSortDirection.Descending, ListSortDirection.Ascending)
                    view.DataSource = snapshot.Rows
                    If sortColumn <> "" Then view.Sort(view.Columns(sortColumn), sortDirection)
                    Dim selected = view.Rows.Cast(Of DataGridViewRow).FirstOrDefault(Function(row) CInt(DirectCast(row.DataBoundItem, DataRowView)("request_id")) = selectedId)
                    If selected Is Nothing AndAlso view.Rows.Count > 0 Then selected = view.Rows(0)
                    If selected IsNot Nothing Then
                        view.CurrentCell = selected.Cells(0) : selected.Selected = True
                    End If
                    If scroll >= 0 AndAlso view.RowCount > 0 Then view.FirstDisplayedScrollingRowIndex = Math.Min(scroll, view.RowCount - 1)
                Finally
                    rebindingRows = False
                End Try
            End If
            initializing = True
            BindCourses(snapshot.Courses)
            initializing = False
            applied = filter : loadedSuccessfully = True : manualLoadFailed = False
            state.Text = snapshot.Rows.Rows.Count & " request(s) • Updated " & Date.Now.ToString("HH:mm:ss") & " • Auto-refresh every 5 seconds."
            FiltersChanged(Me, EventArgs.Empty)
            If changed Then ShowDetails()
        Catch ex As Exception
            If Not IsDisposed AndAlso current = revision Then
                loadedSuccessfully = False
                If Not automatic Then manualLoadFailed = True
                state.Text = "Could not load requests: " & ex.Message
                state.ForeColor = Color.DarkRed : retry.Visible = True
            End If
        Finally
            If current = revision Then
                loading = False
                initializing = False
            End If
            UpdateExportButtons()
        End Try
    End Function

    Private Shared Function SameRows(left As DataTable, right As DataTable) As Boolean
        If left Is Nothing OrElse left.Rows.Count <> right.Rows.Count OrElse left.Columns.Count <> right.Columns.Count Then Return False
        For index = 0 To left.Columns.Count - 1
            If left.Columns(index).ColumnName <> right.Columns(index).ColumnName Then Return False
        Next
        For index = 0 To left.Rows.Count - 1
            If Not left.Rows(index).ItemArray.SequenceEqual(right.Rows(index).ItemArray) Then Return False
        Next
        Return True
    End Function

    Private Sub UpdateExportButtons()
        If IsDisposed Then Return
        csv.Enabled = loadedSuccessfully AndAlso Not loading AndAlso Not exportBusy AndAlso view.RowCount > 0
        pdf.Enabled = csv.Enabled
    End Sub

    ' Export every full field in displayed sort order, including the details not squeezed into the grid.
    Private Function ExportSnapshot() As DataTable
        Dim result = DirectCast(view.DataSource, DataTable).Clone()
        For Each item As DataGridViewRow In view.Rows
            result.ImportRow(DirectCast(item.DataBoundItem, DataRowView).Row)
        Next
        result.Columns.Remove("class_id")
        Dim names As New Dictionary(Of String, String) From {{"request_id", "Request ID"}, {"student_id", "Student ID"}, {"student_name", "Student"}, {"course", "Course / section"}, {"assessment", "Assessment"}, {"current_score", "Current score"}, {"maximum_score", "Maximum score"}, {"requested_points", "Requested adjustment"}, {"status", "Status"}, {"requested_at", "Requested on"}, {"reviewed_at", "Reviewed on"}, {"assigned_professor", "Assigned professor"}, {"reviewed_by", "Reviewed by"}, {"reason", "Student reason"}, {"response", "Professor response"}}
        ' Make a plain re-check explicit in exports while retaining all other field values.
        Dim pointsIndex = result.Columns("requested_points").Ordinal
        result.Columns.Add("Adjustment text", GetType(String))
        For Each row As DataRow In result.Rows
            row("Adjustment text") = Adjustment(row)
        Next
        result.Columns.Remove("requested_points") : result.Columns("Adjustment text").SetOrdinal(pointsIndex)
        names.Remove("requested_points") : names.Add("Adjustment text", "Requested adjustment")
        For Each mapping In names
            result.Columns(mapping.Key).ColumnName = mapping.Value
        Next
        Return result
    End Function

    Private Sub ExportRows(asPdf As Boolean)
        If Not csv.Enabled OrElse exportBusy Then Return
        Dim snapshot = ExportSnapshot()
        Dim description = applied.Description()
        exportBusy = True : UpdateExportButtons()
        Try
            Using picker As New SaveFileDialog With {.Filter = If(asPdf, "PDF files|*.pdf", "CSV files|*.csv"), .FileName = "Clarification-requests-" & Date.Now.ToString("yyyyMMdd-HHmmss") & If(asPdf, ".pdf", ".csv")}
                If picker.ShowDialog(Me) <> DialogResult.OK Then Return
                Dim exporter As New ExportService()
                Dim success As Boolean
                If asPdf Then
                    Using document = exporter.CreateTextReportDocument("CLARIFICATION REQUESTS", description, snapshot)
                        success = exporter.ExportToPDF(document, picker.FileName)
                    End Using
                Else
                    success = exporter.ExportTableToCSV(snapshot, picker.FileName)
                End If
                If success Then state.Text = "Saved " & snapshot.Rows.Count & " request(s): " & picker.FileName
            End Using
        Catch ex As Exception
            state.Text = "Export failed: " & ex.Message : state.ForeColor = Color.DarkRed
        Finally
            exportBusy = False : UpdateExportButtons()
        End Try
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            refreshTimer.Stop() : refreshTimer.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
