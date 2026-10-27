Imports System.Data

Public Class AdminDialogs
    Public Shared Sub ShowAudit(owner As Form, row As DataRow)
        Dim text = String.Join(vbCrLf & vbCrLf, row.Table.Columns.Cast(Of DataColumn).Select(Function(column) column.ColumnName & ": " & Convert.ToString(row(column))))
        Using dialog As New Form()
            UiKit.StyleDialog(dialog, "Update details", 620, 460)
            Dim body As New TextBox With {.Multiline = True, .ReadOnly = True, .Dock = DockStyle.Fill, .ScrollBars = ScrollBars.Vertical, .BorderStyle = BorderStyle.None, .BackColor = Color.White, .Text = text}
            Dim content As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(18)}
            content.Controls.Add(body)
            dialog.Controls.Add(content)
            dialog.Controls.Add(UiKit.MakeHeader("Update details"))
            Dim close = UiKit.MakeSecondaryButton("Close")
            close.DialogResult = DialogResult.Cancel
            Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 48, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(8)}
            footer.Controls.Add(close)
            dialog.Controls.Add(footer)
            content.BringToFront()
            dialog.CancelButton = close
            dialog.ShowDialog(owner)
        End Using
    End Sub

    Public Shared Function Grid(data As DataTable) As DataGridView
        Dim view As New DataGridView With {.DataSource = data, .Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False, .RowHeadersVisible = False, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells, .BackgroundColor = Color.White, .BorderStyle = BorderStyle.None, .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .MultiSelect = False, .EnableHeadersVisualStyles = False}
        view.ColumnHeadersDefaultCellStyle.BackColor = UiKit.SoftGreenColor
        view.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkGreen
        Return view
    End Function
End Class

Public Class AdminAccountDetailsForm
    Inherits Form
    Private ReadOnly service As New AdminWorkspaceService()
    Private ReadOnly role, id, adminId As String
    Private ReadOnly tabs As New TabControl With {.Dock = DockStyle.Fill}
    Private ReadOnly state As New Label With {.Dock = DockStyle.Bottom, .Height = 26, .Text = "Loading account details…", .ForeColor = Color.DarkGreen}

    Public Sub New(accountRole As String, accountId As String, administrator As String)
        role = accountRole : id = accountId : adminId = administrator
        UiKit.StyleDialog(Me, "Account details", 850, 570)
        Controls.Add(tabs)
        Controls.Add(UiKit.MakeHeader(If(role = "Teacher", "Professor", "Student") & " account — " & id))
        Controls.Add(state)
        Dim close = UiKit.MakeSecondaryButton("Close")
        close.DialogResult = DialogResult.Cancel
        Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 48, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(8)}
        footer.Controls.Add(close)
        Controls.Add(footer) : tabs.BringToFront() : CancelButton = close
        AddHandler Shown, AddressOf LoadDetails
    End Sub

    Private Async Sub LoadDetails(sender As Object, e As EventArgs)
        Try
            Dim result = Await Task.Run(Function() (Profile:=service.AccountProfile(role, id, adminId), Classes:=service.AccountClasses(role, id, adminId), Activity:=service.AuditRows(adminId, accountRole:=role, accountId:=id)))
            If IsDisposed Then Return
            Dim profile As New TabPage("Profile") With {.BackColor = Color.White, .Padding = New Padding(18)}
            Dim text = If(result.Profile.Rows.Count = 0, "Account no longer exists.", String.Join(vbCrLf & vbCrLf, result.Profile.Columns.Cast(Of DataColumn).Select(Function(column) column.ColumnName.Replace("_", " ") & ": " & Convert.ToString(result.Profile.Rows(0)(column)))))
            profile.Controls.Add(New TextBox With {.Text = text, .Multiline = True, .ReadOnly = True, .Dock = DockStyle.Fill, .BorderStyle = BorderStyle.None, .BackColor = Color.White, .ScrollBars = ScrollBars.Vertical})
            tabs.TabPages.Add(profile)
            Dim classes As New TabPage(If(role = "Student", "Enrolled classes", "Assigned classes"))
            classes.Controls.Add(AdminDialogs.Grid(result.Classes)) : tabs.TabPages.Add(classes)
            Dim activity As New TabPage("Recent activity")
            Dim view = AdminDialogs.Grid(result.Activity)
            AddHandler view.CellDoubleClick, Sub(s, args)
                                                If args.RowIndex >= 0 Then AdminDialogs.ShowAudit(Me, DirectCast(view.Rows(args.RowIndex).DataBoundItem, DataRowView).Row)
                                            End Sub
            activity.Controls.Add(view) : tabs.TabPages.Add(activity)
            state.Text = "Recent activity shows the latest 25 matching entries. Double-click an entry for full details."
        Catch ex As Exception
            If Not IsDisposed Then state.Text = "Could not load details: " & ex.Message
        End Try
    End Sub
End Class
