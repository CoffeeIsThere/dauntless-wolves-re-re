Imports System.Data
Imports System.Drawing.Printing

Public Class AdminAuditForm
    Inherits Form
    Private ReadOnly administrator As String
    Private ReadOnly service As New AdminWorkspaceService()
    Private ReadOnly view As DataGridView = AdminDialogs.Grid(New DataTable())
    Private ReadOnly query As New TextBox With {.Width = 185, .PlaceholderText = "Search details, entity or ID"}
    Private ReadOnly actor As New TextBox With {.Width = 145, .PlaceholderText = "Actor name or ID"}
    Private ReadOnly action As New TextBox With {.Width = 140, .PlaceholderText = "Action"}
    Private ReadOnly fromDate As New DateTimePicker With {.Width = 132, .Format = DateTimePickerFormat.Short, .ShowCheckBox = True, .Checked = False}
    Private ReadOnly toDate As New DateTimePicker With {.Width = 132, .Format = DateTimePickerFormat.Short, .ShowCheckBox = True, .Checked = False}
    Private ReadOnly state As New Label With {.Dock = DockStyle.Bottom, .Height = 28, .ForeColor = Color.DarkGreen, .AutoEllipsis = True}
    Private ReadOnly csv As Button = UiKit.MakeSecondaryButton("Export CSV")
    Private ReadOnly pdf As Button = UiKit.MakePrimaryButton("Save as PDF")
    Private revision As Integer
    Private exportBusy As Boolean
    Private loadedDescription As String = ""

    Public Sub New(adminId As String)
        administrator = adminId
        UiKit.StyleDialog(Me, "The Dauntless Grading System — Audit logs", 1040, 600)
        MinimumSize = New Size(800, 460) : FormBorderStyle = FormBorderStyle.Sizable
        Dim filters As New FlowLayoutPanel With {.Dock = DockStyle.Top, .Height = 105, .Padding = New Padding(8), .WrapContents = True}
        filters.Controls.AddRange({Field("Search", query), Field("Actor", actor), Field("Action", action), Field("From (optional)", fromDate), Field("Through (optional)", toDate)})
        filters.SetFlowBreak(filters.Controls(filters.Controls.Count - 1), True)
        Dim apply = UiKit.MakePrimaryButton("Apply", 75)
        Dim reset = UiKit.MakeSecondaryButton("Reset", 75)
        Dim refresh = UiKit.MakeSecondaryButton("Refresh", 80)
        filters.Controls.AddRange({apply, reset, refresh, csv, pdf})
        AddHandler filters.SizeChanged, Sub() filters.Height = filters.GetPreferredSize(New Size(filters.Width, 0)).Height
        AddHandler apply.Click, Async Sub() Await ReloadAsync()
        AddHandler refresh.Click, Async Sub() Await ReloadAsync()
        AddHandler reset.Click, Async Sub()
                                    query.Clear() : actor.Clear() : action.Clear() : fromDate.Checked = False : toDate.Checked = False
                                    Await ReloadAsync()
                                End Sub
        AddHandler csv.Click, Sub() ExportRows(False)
        AddHandler pdf.Click, Sub() ExportRows(True)
        AddHandler view.CellDoubleClick, Sub(sender, args)
                                             If args.RowIndex >= 0 Then AdminDialogs.ShowAudit(Me, DirectCast(view.Rows(args.RowIndex).DataBoundItem, DataRowView).Row)
                                         End Sub
        Controls.Add(view) : Controls.Add(state) : Controls.Add(filters) : Controls.Add(UiKit.MakeHeader("Audit logs")) : view.BringToFront()
        AcceptButton = apply
        AddHandler Shown, Async Sub() Await ReloadAsync()
    End Sub

    Private Shared Function Field(caption As String, input As Control) As Control
        Dim panel As New Panel With {.Width = input.Width, .Height = 48}
        panel.Controls.Add(New Label With {.Text = caption, .Dock = DockStyle.Top, .Height = 18, .ForeColor = Color.DarkGreen})
        input.Dock = DockStyle.Bottom : panel.Controls.Add(input)
        Return panel
    End Function

    Private Async Function ReloadAsync() As Task
        revision += 1
        Dim current = revision
        Dim start As Date? = If(fromDate.Checked, CType(fromDate.Value.Date, Date?), Nothing)
        Dim finish As Date? = If(toDate.Checked, CType(toDate.Value.Date, Date?), Nothing)
        Dim actorText = actor.Text.Trim(), actionText = action.Text.Trim(), searchText = query.Text.Trim()
        Dim description = "From: " & If(start.HasValue, start.Value.ToShortDateString(), "any") & "; Through: " & If(finish.HasValue, finish.Value.ToShortDateString(), "any") & "; Actor: " & actorText & "; Action: " & actionText & "; Search: " & searchText
        state.Text = "Loading audit records…" : csv.Enabled = False : pdf.Enabled = False
        Try
            Dim rows = Await Task.Run(Function() service.AuditRows(administrator, start, finish, actorText, actionText, searchText))
            If IsDisposed OrElse current <> revision Then Return
            Dim selectedId = If(view.CurrentRow Is Nothing, -1, Convert.ToInt32(view.CurrentRow.Cells("Log ID").Value))
            Dim scroll = view.FirstDisplayedScrollingRowIndex
            view.DataSource = rows
            For Each row As DataGridViewRow In view.Rows
                If CInt(row.Cells("Log ID").Value) = selectedId Then view.CurrentCell = row.Cells(0)
            Next
            If scroll >= 0 AndAlso view.Rows.Count > 0 Then view.FirstDisplayedScrollingRowIndex = Math.Min(scroll, view.Rows.Count - 1)
            loadedDescription = description
            state.Text = rows.Rows.Count & " records. Double-click an entry for full details. Exports use the displayed results."
            csv.Enabled = rows.Rows.Count > 0 AndAlso Not exportBusy : pdf.Enabled = csv.Enabled
        Catch ex As Exception
            If Not IsDisposed AndAlso current = revision Then
                state.Text = "Could not refresh: " & ex.Message & " Use Apply or Refresh to retry."
                ' Old results stay visible, but export remains disabled until the filter succeeds.
            End If
        End Try
    End Function

    Private Sub ExportRows(asPdf As Boolean)
        If exportBusy OrElse Not csv.Enabled OrElse view.RowCount = 0 Then Return
        Using picker As New SaveFileDialog With {.Filter = If(asPdf, "PDF files|*.pdf", "CSV files|*.csv"), .FileName = "Audit-log-" & Date.Now.ToString("yyyyMMdd-HHmmss") & If(asPdf, ".pdf", ".csv")}
            If picker.ShowDialog(Me) <> DialogResult.OK Then Return
            exportBusy = True : csv.Enabled = False : pdf.Enabled = False
            Try
                Dim exporter As New ExportService()
                Dim success As Boolean
                If asPdf Then
                    Dim rows = DirectCast(view.DataSource, DataTable).Clone()
                    ' The order exactly follows the displayed grid, including a user-selected sort.
                    For Each row As DataGridViewRow In view.Rows
                        rows.ImportRow(DirectCast(row.DataBoundItem, DataRowView).Row)
                    Next
                    Using document = CreateAuditDocument(rows, loadedDescription)
                        success = exporter.ExportToPDF(document, picker.FileName)
                    End Using
                Else
                    success = exporter.ExportToCSV(view, picker.FileName)
                End If
                If success Then state.Text = "Export saved: " & picker.FileName
            Catch ex As Exception
                state.Text = "Export failed: " & ex.Message
            Finally
                exportBusy = False
                csv.Enabled = view.Rows.Count > 0 : pdf.Enabled = csv.Enabled
            End Try
        End Using
    End Sub

    ' Print full values in wrapped blocks, continuing even one very long record across pages.
    Private Shared Function CreateAuditDocument(rows As DataTable, filters As String) As PrintDocument
        Dim document As New PrintDocument()
        Dim content = "AUDIT LOGS" & vbCrLf & filters & vbCrLf & vbCrLf & String.Join(vbCrLf & vbCrLf & "────────────────────────" & vbCrLf, rows.Rows.Cast(Of DataRow).Select(Function(row) String.Join(vbCrLf, rows.Columns.Cast(Of DataColumn).Select(Function(column) column.ColumnName & ": " & Convert.ToString(row(column))))))
        Dim offset = 0
        AddHandler document.BeginPrint, Sub() offset = 0
        AddHandler document.PrintPage, Sub(sender, args)
                                           Using font As New Font("Segoe UI", 10), format As New StringFormat(StringFormat.GenericTypographic)
                                               format.FormatFlags = StringFormatFlags.LineLimit
                                               Dim characters, lines As Integer
                                               args.Graphics.MeasureString(content.Substring(offset), font, args.MarginBounds.Size, format, characters, lines)
                                               If characters <= 0 Then Throw New InvalidOperationException("The printable area is too small.")
                                               args.Graphics.DrawString(content.Substring(offset, characters), font, Brushes.DarkGreen, args.MarginBounds, format)
                                               offset += characters
                                               args.HasMorePages = offset < content.Length
                                           End Using
                                       End Sub
        Return document
    End Function
End Class
