Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

' Add or edit a student or a teacher. Built in code, so there is no designer file to add.
'   Using dlg As New frmUserEditor(frmUserEditor.UserKind.Student)            ' add
'   Using dlg As New frmUserEditor(frmUserEditor.UserKind.Teacher, "P003")    ' edit
'       If dlg.ShowDialog(Me) = DialogResult.OK Then ...reload the list...
Public Class frmUserEditor
    Inherits Form

    Public Enum UserKind
        Student
        Teacher
    End Enum

    Private ReadOnly _kind As UserKind
    Private ReadOnly _editId As String
    Private ReadOnly _isEdit As Boolean
    Private ReadOnly _service As New UserService()

    Private ReadOnly _table As New TableLayoutPanel()
    Private ReadOnly txtId As New TextBox()
    Private ReadOnly txtName As New TextBox()
    Private ReadOnly txtEmail As New TextBox()
    Private ReadOnly txtPassword As New TextBox()
    Private ReadOnly cboGroup As New ComboBox()      ' Student: program. Teacher: department.
    Private ReadOnly numYear As New NumericUpDown()  ' Student only
    Private ReadOnly cboSection As New ComboBox()    ' Student only
    Private ReadOnly cboEmployment As New ComboBox() ' Teacher only
    Private ReadOnly lblError As New Label()
    Private ReadOnly btnSave As Button = UiKit.MakePrimaryButton("Save")
    Private ReadOnly btnCancel As Button = UiKit.MakeSecondaryButton("Cancel")
    Private ReadOnly btnReset As Button = UiKit.MakeSecondaryButton("Reset password", 130)

    Public Sub New(kind As UserKind, Optional editId As String = Nothing)
        _kind = kind
        _editId = editId
        _isEdit = Not String.IsNullOrEmpty(editId)

        Dim noun As String = If(kind = UserKind.Student, "Student", "Professor")
        UiKit.StyleDialog(Me, If(_isEdit, "Edit " & noun, "Add " & noun), 500, 500)

        BuildLayout(noun)
        FillChoices()
        If _isEdit Then LoadExisting()

        AddHandler btnSave.Click, AddressOf btnSave_Click
        AddHandler btnReset.Click, AddressOf btnReset_Click
        Me.AcceptButton = btnSave
        Me.CancelButton = btnCancel
        btnCancel.DialogResult = DialogResult.Cancel
    End Sub

    Private Sub BuildLayout(noun As String)
        Dim header As Label = UiKit.MakeHeader(If(_isEdit, "Edit " & noun, "Add new " & noun.ToLower()))

        Dim body As New Panel()
        body.Dock = DockStyle.Fill
        body.Padding = New Padding(22, 14, 22, 0)

        _table.ColumnCount = 2
        _table.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130))
        _table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        _table.Dock = DockStyle.Top
        _table.AutoSize = True

        AddRow(If(_kind = UserKind.Student, "Student ID", "Employee ID"), txtId)
        AddRow("Full name", txtName)
        AddRow("Email", txtEmail)
        If Not _isEdit Then
            txtPassword.UseSystemPasswordChar = True

            If _kind = UserKind.Student Then
                txtPassword.PlaceholderText =
            "Leave blank to use Student ID"
            End If

            AddRow("Password", txtPassword)
        End If


        If _kind = UserKind.Student Then
            cboGroup.DropDownStyle = ComboBoxStyle.DropDown
            cboGroup.AccessibleDescription = "Select an existing program or type a new program."
            AddRow("Program", cboGroup)
            numYear.Minimum = 1
            numYear.Maximum = 6
            numYear.Value = 1
            AddRow("Year level", numYear)
            cboSection.DropDownStyle = ComboBoxStyle.DropDown
            AddRow("Section", cboSection)
        Else
            cboGroup.DropDownStyle = ComboBoxStyle.DropDown
            cboGroup.AccessibleDescription = "Select a department or type another department."
            AddRow("Department", cboGroup)
            cboEmployment.DropDownStyle = ComboBoxStyle.DropDownList
            cboEmployment.Items.AddRange(New Object() {"Full-time", "Part-time"})
            cboEmployment.SelectedIndex = 0
            AddRow("Employment", cboEmployment)
        End If

        Dim groupHint As New Label With {
            .Text = "Select a suggestion or type another " & If(_kind = UserKind.Student, "program.", "department."),
            .AutoSize = True,
            .ForeColor = Color.DimGray,
            .Margin = New Padding(0, 4, 0, 6)
        }
        _table.RowCount += 1
        _table.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        _table.Controls.Add(groupHint, 0, _table.RowCount - 1)
        _table.SetColumnSpan(groupHint, 2)

        lblError.ForeColor = Color.Firebrick
        lblError.AutoSize = False
        lblError.Dock = DockStyle.Bottom
        lblError.Height = 44
        lblError.Visible = False

        body.Controls.Add(lblError)
        body.Controls.Add(_table)

        Dim bottom As New Panel()
        bottom.Dock = DockStyle.Bottom
        bottom.Height = 58
        btnSave.Location = New Point(500 - 22 - 110 - 10 - 110, 12)
        btnCancel.Location = New Point(500 - 22 - 110, 12)
        btnReset.Location = New Point(22, 12)
        btnReset.Visible = _isEdit
        bottom.Controls.AddRange(New Control() {btnSave, btnCancel, btnReset})

        Me.Controls.Add(body)
        Me.Controls.Add(bottom)
        Me.Controls.Add(header)
    End Sub

    Private Sub AddRow(labelText As String, ctrl As Control)
        Dim lbl As New Label()
        lbl.Text = labelText
        lbl.AutoSize = True
        lbl.Anchor = AnchorStyles.Left
        lbl.Margin = New Padding(0, 10, 8, 0)

        ctrl.Dock = DockStyle.Fill
        ctrl.Margin = New Padding(0, 6, 0, 6)

        _table.RowCount += 1
        _table.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        _table.Controls.Add(lbl, 0, _table.RowCount - 1)
        _table.Controls.Add(ctrl, 1, _table.RowCount - 1)
    End Sub

    ' Suggest standard departments as well as saved values; allow new typed values.
    Private Sub FillChoices()
        If _kind = UserKind.Student Then
            For Each p In _service.GetPrograms()
                cboGroup.Items.Add(p)
            Next
            For Each s In _service.GetStudentSections()
                cboSection.Items.Add(s)
            Next
        Else
            Dim departments = New String() {"CCS", "CAS", "COA", "COE", "COED", "COHM"}.
                Concat(_service.GetDepartments()).
                Where(Function(value) Not String.IsNullOrWhiteSpace(value)).
                Select(Function(value) value.Trim()).
                Distinct(StringComparer.OrdinalIgnoreCase).
                OrderBy(Function(value) value, StringComparer.OrdinalIgnoreCase)
            For Each d In departments
                cboGroup.Items.Add(d)
            Next
        End If
        If Not _isEdit Then cboGroup.Text = ""
    End Sub

    Private Sub LoadExisting()
        txtId.Text = _editId
        txtId.ReadOnly = True
        txtId.BackColor = Color.FromArgb(240, 240, 240)

        If _kind = UserKind.Student Then
            Dim dt As DataTable = _service.GetStudent(_editId)
            If dt.Rows.Count = 0 Then Return
            Dim r As DataRow = dt.Rows(0)
            txtName.Text = r("full_name").ToString()
            txtEmail.Text = r("email").ToString()
            cboGroup.Text = r("program").ToString()
            numYear.Value = Math.Max(1, Math.Min(6, Convert.ToInt32(r("year_level"))))
            cboSection.Text = r("section").ToString()
        Else
            Dim dt As DataTable = _service.GetTeacher(_editId)
            If dt.Rows.Count = 0 Then Return
            Dim r As DataRow = dt.Rows(0)
            txtName.Text = r("full_name").ToString()
            txtEmail.Text = r("email").ToString()
            cboGroup.Text = r("department").ToString()
            cboEmployment.SelectedItem = r("employment_type").ToString()
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs)
        lblError.Visible = False
        Dim adminId As String = AppSession.CurrentUser.UserId
        Dim res As OperationResult

        If _kind = UserKind.Student Then
            If _isEdit Then
                res = _service.UpdateStudentResult(_editId, txtName.Text, txtEmail.Text, cboGroup.Text, CInt(numYear.Value), cboSection.Text, adminId)
            Else
                res = _service.CreateStudentResult(txtId.Text, txtPassword.Text, txtName.Text, txtEmail.Text, cboGroup.Text, CInt(numYear.Value), cboSection.Text, adminId)
            End If
        Else
            Dim emp As String = If(cboEmployment.SelectedItem Is Nothing, "Full-time", cboEmployment.SelectedItem.ToString())
            If _isEdit Then
                res = _service.UpdateTeacherResult(_editId, txtName.Text, txtEmail.Text, cboGroup.Text, emp, adminId)
            Else
                res = _service.CreateTeacherResult(txtId.Text, txtPassword.Text, txtName.Text, txtEmail.Text, cboGroup.Text, emp, adminId)
            End If
        End If

        If res.Success Then
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Else
            lblError.Text = res.Message
            lblError.Visible = True
        End If
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs)
        Dim pw As String = UiKit.PromptPassword(
            Me,
            "Reset password",
            "Enter a new password for " & _editId &
            " (minimum 4 characters):"
        )
        If pw Is Nothing Then Return

        Dim role As String = If(_kind = UserKind.Student, "Student", "Teacher")
        Dim res As OperationResult = _service.ResetPassword(role, _editId, pw, AppSession.CurrentUser.UserId)
        MessageBox.Show(If(res.Success, "Password was reset.", res.Message), "Reset password",
                        MessageBoxButtons.OK, If(res.Success, MessageBoxIcon.Information, MessageBoxIcon.Warning))
    End Sub

End Class
