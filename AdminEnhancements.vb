Imports System.Data

Partial Public Class Admin
    Private ReadOnly adminWorkspace As New AdminWorkspaceService()
    Private ReadOnly studentStatus As New ComboBox()
    Private ReadOnly profStatus As New ComboBox()
    Private ReadOnly profEmployment As New ComboBox()
    Private ReadOnly studentSelection As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly profSelection As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly bulkBars As New Dictionary(Of String, FlowLayoutPanel)
    Private ReadOnly bulkLabels As New Dictionary(Of String, Label)
    Private ReadOnly loadRevisions As New Dictionary(Of String, Integer) From {{"Student", 0}, {"Teacher", 0}}
    Private ReadOnly adminState As New Label With {.Dock = DockStyle.Bottom, .Height = 24, .ForeColor = Color.DarkGreen, .BackColor = Color.White, .Text = "Ready", .AutoEllipsis = True, .Padding = New Padding(16, 3, 0, 0)}
    Private renderingAccounts As Boolean
    Private accountBusy As Boolean
    Private restoringFilters As Boolean

    Private Sub SetAdminState(message As String, Optional failed As Boolean = False)
        If IsDisposed OrElse Disposing Then Return
        adminState.Text = message
        adminState.ForeColor = If(failed, Color.Firebrick, Color.DarkGreen)
    End Sub

    Private Shared Function MatchesFilter(combo As ComboBox, value As String) As Boolean
        Return combo.SelectedItem Is Nothing OrElse combo.Text = "All" OrElse combo.Text.Equals(value, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function SelectedAccounts(role As String) As HashSet(Of String)
        Return If(role = "Teacher", profSelection, studentSelection)
    End Function

    Private Sub InitAdminEnhancements()
        Dim footer As New Panel With {.Dock = DockStyle.Bottom, .Height = 26, .BackColor = Color.White}
        Dim retry = UiKit.MakeSecondaryButton("Refresh", 80)
        retry.Dock = DockStyle.Right
        AddHandler retry.Click, Async Sub()
                                    If pnlDashboard.Visible Then
                                        SetAdminState("Refreshing dashboard…")
                                        Await RefreshDashboardUpdatesAsync(True)
                                    Else
                                        Await RefreshAccountsAsync(If(pnlProfessors.Visible, "Teacher", "Student"))
                                    End If
                                End Sub
        adminState.Dock = DockStyle.Fill
        footer.Controls.Add(adminState) : footer.Controls.Add(retry) : adminState.BringToFront()
        Controls.Add(footer) : pnlContent.BringToFront()
        AddFilterHeader(Panel2, False)
        AddFilterHeader(pnlProfTop, True)
        AddBulkBar("Student", pnlstudentcard, dgvstudents)
        AddBulkBar("Teacher", pnlProfCard, dgvProf)
        WireTile(lblStatStudents.Parent, Sub() btnStudents.PerformClick())
        WireTile(lblStatProfs.Parent, Sub() btnProfessors.PerformClick())
        WireTile(lblStatPending.Parent, Sub() OpenQuickAction(quickactions.FormMode.ViewRequests, True))
        WireTile(lblStatClasses.Parent, AddressOf ShowClasses)
        AddHandler dgvupdates.CellClick, AddressOf ShowUpdateDetails
        AddHandler dgvstudents.CellDoubleClick, Sub(sender, args) ShowAccountDetails("Student", dgvstudents, "colStudentId", args)
        AddHandler dgvProf.CellDoubleClick, Sub(sender, args) ShowAccountDetails("Teacher", dgvProf, "colEmpId", args)
        SetAdminState("Double-click an account name for details. Click an update to read its full history.")
    End Sub

    Private Sub AddFilterHeader(host As Control, professor As Boolean)
        Dim flow As New FlowLayoutPanel With {.Dock = DockStyle.Top, .WrapContents = True, .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink, .Padding = New Padding(0, 0, 0, 3)}
        Dim search = If(professor, txtSearchProf, txtStudentSearch)
        search.PlaceholderText = If(professor, "Name, ID, email, department", "Name, ID, email, program")
        Dim fields As New List(Of Control)
        fields.Add(FilterField("Search", search, 200))
        Dim clear = UiKit.MakeSecondaryButton("×", 28)
        clear.Height = 26 : clear.Margin = New Padding(0, 18, 6, 0)
        AddHandler clear.Click, Sub() search.Clear()
        fields.Add(clear)
        If professor Then
            fields.Add(FilterField("Department", cboDeptProf, 100))
        Else
            Label3.Visible = False : Label4.Visible = False
            fields.Add(FilterField("Year level", cboYearLevel, 85))
            fields.Add(FilterField("Section", cboSection, 85))
        End If
        Dim status = If(professor, profStatus, studentStatus)
        status.DropDownStyle = ComboBoxStyle.DropDownList
        status.Items.AddRange({"All", "Active", "Inactive"}) : status.SelectedIndex = 0
        fields.Add(FilterField("Status", status, 100))
        AddHandler status.SelectedIndexChanged, Sub()
                                                    If professor Then ApplyProfFilter() Else ApplyStudentFilter()
                                                End Sub
        If professor Then
            profEmployment.DropDownStyle = ComboBoxStyle.DropDownList
            profEmployment.Items.AddRange({"All", "Full-time", "Part-time"}) : profEmployment.SelectedIndex = 0
            fields.Add(FilterField("Employment", profEmployment, 110))
            AddHandler profEmployment.SelectedIndexChanged, Sub() ApplyProfFilter()
        End If
        Dim refresh = UiKit.MakeSecondaryButton("Refresh", 75)
        refresh.Height = 26 : refresh.Margin = New Padding(3, 18, 3, 0)
        AddHandler refresh.Click, Async Sub() Await RefreshAccountsAsync(If(professor, "Teacher", "Student"))
        fields.Add(refresh)
        If Not professor Then
            btnStudentFilter.Visible = False
            btnStudentReset.Height = 26 : btnStudentReset.Margin = New Padding(3, 18, 3, 0)
            fields.Add(btnStudentReset)
            AddHandler btnStudentReset.Click, Sub() studentStatus.SelectedIndex = 0
        End If
        Dim addButton As Control = If(professor, btnAddProf, btnaddstudent)
        addButton.Margin = New Padding(3, 18, 3, 0) : addButton.Height = 28
        fields.Add(addButton)
        For Each field In fields
            flow.Controls.Add(field)
        Next
        host.Controls.Add(flow)
        If Not professor Then
            lbltotalstudents.Dock = DockStyle.Bottom : lbltotalstudents.Height = 22
        End If
        Dim resize As Action = Sub() host.Height = flow.GetPreferredSize(New Size(Math.Max(200, host.ClientSize.Width), 0)).Height + If(professor, 4, 24)
        AddHandler host.SizeChanged, Sub() resize()
        AddHandler flow.Layout, Sub() resize()
        resize()
    End Sub

    Private Shared Function FilterField(caption As String, input As Control, width As Integer) As Panel
        Dim field As New Panel With {.Width = width, .Height = 48, .Margin = New Padding(0, 0, 6, 0)}
        Dim label As New Label With {.Text = caption, .Dock = DockStyle.Top, .Height = 17, .ForeColor = Color.DarkGreen}
        input.Dock = DockStyle.None
        input.SetBounds(0, 18, width, 26)
        input.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        field.Controls.Add(input) : field.Controls.Add(label)
        Return field
    End Function

    Private Sub AddBulkBar(role As String, host As Control, grid As DataGridView)
        Dim bar As New FlowLayoutPanel With {.Dock = DockStyle.Top, .Height = 38, .Visible = False, .BackColor = UiKit.SoftGreenColor}
        Dim label As New Label With {.Width = 160, .Height = 32, .TextAlign = ContentAlignment.MiddleLeft}
        bar.Controls.Add(label) : bulkBars(role) = bar : bulkLabels(role) = label
        For Each status In {"Active", "Inactive"}
            Dim target = status
            Dim button = UiKit.MakeSecondaryButton(If(target = "Active", "Activate selected", "Deactivate selected"), 145)
            button.Height = 28
            AddHandler button.Click, Async Sub() Await ChangeSelectedAsync(role, target)
            bar.Controls.Add(button)
        Next
        Dim clear = UiKit.MakeSecondaryButton("Clear selection", 120) : clear.Height = 28
        AddHandler clear.Click, Sub()
                                    SelectedAccounts(role).Clear()
                                    If role = "Teacher" Then ShowProfPage() Else ShowStudentPage()
                                End Sub
        bar.Controls.Add(clear)
        host.Controls.Add(bar)
        host.Controls.SetChildIndex(bar, 1)
        grid.BringToFront()
        AddHandler grid.CurrentCellDirtyStateChanged, Sub()
                                                         If grid.IsCurrentCellDirty Then grid.CommitEdit(DataGridViewDataErrorContexts.Commit)
                                                     End Sub
        AddHandler grid.CellValueChanged, Sub(sender, args)
                                              If renderingAccounts OrElse args.RowIndex < 0 OrElse args.ColumnIndex < 0 OrElse grid.Columns(args.ColumnIndex).Name <> "colCheck" Then Return
                                              Dim id = Convert.ToString(grid.Rows(args.RowIndex).Cells(If(role = "Teacher", "colEmpId", "colStudentId")).Value)
                                              If Convert.ToBoolean(grid.Rows(args.RowIndex).Cells("colCheck").Value) Then
                                                  SelectedAccounts(role).Add(id)
                                              Else
                                                  SelectedAccounts(role).Remove(id)
                                              End If
                                              UpdateBulkBar(role)
                                          End Sub
    End Sub

    Private Sub UpdateBulkBar(role As String)
        If Not bulkBars.ContainsKey(role) Then Return
        bulkLabels(role).Text = SelectedAccounts(role).Count & " selected (all pages)"
        bulkBars(role).Visible = SelectedAccounts(role).Count > 0
    End Sub

    Private Async Function ChangeSelectedAsync(role As String, status As String) As Task
        If accountBusy OrElse Not AppSession.IsAdmin Then Return
        Dim ids = SelectedAccounts(role).ToArray()
        If ids.Length = 0 Then Return
        If MessageBox.Show(Me, "Set " & ids.Length & " selected " & If(role = "Teacher", "professor", "student") & " accounts to " & status & "? Selections on other pages and hidden by filters are included.", "Confirm bulk status change", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
        accountBusy = True
        Dim administrator = AppSession.CurrentUser.UserId
        Try
            SetAdminState("Updating " & ids.Length & " accounts…")
            Dim result = Await Task.Run(Function() adminWorkspace.SetStatuses(role, ids, status, administrator))
            If IsDisposed Then Return
            If result.Success Then
                For Each id In ids
                    SelectedAccounts(role).Remove(id)
                Next
                Await RefreshAccountsAsync(role)
                LoadDashboard()
            End If
            SetAdminState(result.Message, Not result.Success)
        Catch ex As Exception
            SetAdminState("Bulk update failed: " & ex.Message, True)
        Finally
            accountBusy = False
        End Try
    End Function

    Private Async Function RefreshAccountsAsync(role As String) As Task
        loadRevisions(role) += 1
        Dim revision = loadRevisions(role)
        SetAdminState("Loading " & If(role = "Teacher", "professors", "students") & "…")
        Try
            Dim data = Await Task.Run(Function() If(role = "Teacher", _userService.GetAllTeachers(), _userService.GetAllStudents()))
            If IsDisposed OrElse Disposing OrElse revision <> loadRevisions(role) Then Return
            If role = "Teacher" Then
                allProfs = data.Rows.Cast(Of DataRow).Select(Function(row) New ProfessorInfo With {.EmployeeID = CStr(row("teacher_id")), .Name = CStr(row("full_name")), .Email = Convert.ToString(row("email")), .Department = Convert.ToString(row("department")), .Classes = CInt(row("class_count")), .EmploymentType = CStr(row("employment_type")), .Status = CStr(row("status"))}).ToList()
                profSelection.IntersectWith(allProfs.Select(Function(p) p.EmployeeID))
                Dim departments = allProfs.Select(Function(p) p.Department).Distinct().OrderBy(Function(value) value).ToArray()
                restoringFilters = True
                UpdateOptions(cboDeptProf, departments)
                restoringFilters = False
                ApplyProfFilter(False)
            Else
                allStudents = data.Rows.Cast(Of DataRow).Select(Function(row) New StudentInfo With {.StudentID = CStr(row("student_id")), .FullName = CStr(row("full_name")), .Email = Convert.ToString(row("email")), .Program = Convert.ToString(row("program")), .YearLevel = CInt(row("year_level")), .Section = Convert.ToString(row("section")), .Status = CStr(row("status"))}).ToList()
                studentSelection.IntersectWith(allStudents.Select(Function(s) s.StudentID))
                restoringFilters = True
                UpdateOptions(cboSection, allStudents.Select(Function(s) s.Section).Distinct().OrderBy(Function(value) value).ToArray())
                restoringFilters = False
                ApplyStudentFilter(False)
            End If
            SetAdminState("Updated " & Date.Now.ToString("HH:mm:ss") & ". Double-click an account name for details.")
        Catch ex As Exception
            restoringFilters = False
            If revision = loadRevisions(role) Then SetAdminState("Could not refresh accounts. Use Refresh to retry: " & ex.Message, True)
        End Try
    End Function

    Private Shared Sub UpdateOptions(combo As ComboBox, values As String())
        Dim desired = New String() {"All"}.Concat(values.Where(Function(value) value <> "All" AndAlso value <> "")).ToArray()
        If combo.Items.Cast(Of String).SequenceEqual(desired) Then Return
        Dim selected = combo.Text
        combo.Items.Clear() : combo.Items.AddRange(desired)
        combo.SelectedItem = If(desired.Contains(selected), selected, "All")
    End Sub

    Private Shared Function CapturePosition(grid As DataGridView, key As String) As (Id As String, Scroll As Integer, Column As Integer)
        Return (If(grid.CurrentRow Is Nothing, "", Convert.ToString(grid.CurrentRow.Cells(key).Value)), grid.FirstDisplayedScrollingRowIndex, If(grid.CurrentCell Is Nothing, 0, grid.CurrentCell.ColumnIndex))
    End Function

    Private Shared Sub RestorePosition(grid As DataGridView, key As String, saved As (Id As String, Scroll As Integer, Column As Integer))
        For Each row As DataGridViewRow In grid.Rows
            If Convert.ToString(row.Cells(key).Value) = saved.Id Then
                grid.CurrentCell = row.Cells(Math.Min(saved.Column, grid.ColumnCount - 1))
                Exit For
            End If
        Next
        If saved.Scroll >= 0 AndAlso grid.Rows.Count > 0 Then grid.FirstDisplayedScrollingRowIndex = Math.Min(saved.Scroll, grid.Rows.Count - 1)
    End Sub

    Private Sub ShowAccountDetails(role As String, grid As DataGridView, key As String, args As DataGridViewCellEventArgs)
        If accountBusy OrElse args.RowIndex < 0 OrElse args.ColumnIndex < 0 Then Return
        If {"colActions", "colType", "colStatus", "colCheck"}.Contains(grid.Columns(args.ColumnIndex).Name) Then Return
        Using dialog As New AdminAccountDetailsForm(role, Convert.ToString(grid.Rows(args.RowIndex).Cells(key).Value), AppSession.CurrentUser.UserId)
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Shared Sub WireTile(control As Control, action As Action)
        control.Cursor = Cursors.Hand
        AddHandler control.Click, Sub() action()
        For Each child As Control In control.Controls
            WireTile(child, action)
        Next
    End Sub

    Private Async Sub ShowClasses()
        Try
            SetAdminState("Loading class directory…")
            Dim data = Await Task.Run(Function() New ClassService().GetAllClasses())
            If IsDisposed Then Return
            Using dialog As New Form()
                UiKit.StyleDialog(dialog, "Active classes", 850, 460)
                Dim view = AdminDialogs.Grid(data)
                dialog.Controls.Add(view) : dialog.Controls.Add(UiKit.MakeHeader("Class directory")) : view.BringToFront()
                dialog.ShowDialog(Me)
            End Using
            SetAdminState("Ready")
        Catch ex As Exception
            SetAdminState("Could not load classes: " & ex.Message, True)
        End Try
    End Sub

    Private Async Sub ShowUpdateDetails(sender As Object, args As DataGridViewCellEventArgs)
        If args.RowIndex < 0 OrElse dgvupdates.Rows(args.RowIndex).Tag Is Nothing Then Return
        Dim id = CInt(dgvupdates.Rows(args.RowIndex).Tag)
        Dim administrator = AppSession.CurrentUser.UserId
        Try
            Dim data = Await Task.Run(Function() adminWorkspace.AuditRows(administrator, logId:=id))
            If Not IsDisposed AndAlso data.Rows.Count > 0 Then AdminDialogs.ShowAudit(Me, data.Rows(0))
        Catch ex As Exception
            SetAdminState("Could not load update: " & ex.Message, True)
        End Try
    End Sub
End Class
