

Public Class frmAddClass

    Private ReadOnly _classService As New ClassService()
    Private ReadOnly semesterPicker As New ComboBox With {.Name = "cboOfferingSemester", .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly academicYearInput As New TextBox With {.Name = "txtOfferingYear", .MaxLength = 9}

    ' Values the dashboard reads after the dialog returns OK
    Public Property SubjectCode As String
    Public Property SubjectName As String
    Public Property SectionName As String
    Public Property ProfessorName As String

    Private Sub frmAddClass_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not AppSession.IsAdmin Then
            MessageBox.Show(Me, "Please sign in as an administrator.")
            Close()
            Return
        End If
        ConfigureOfferingFields()
        LoadProfessorsFromDb()
    End Sub

    Private Sub ConfigureOfferingFields()
        Dim defaults = ClassService.GetDefaultTerm()
        semesterPicker.Items.AddRange({"1st Semester", "2nd Semester"})
        semesterPicker.SelectedItem = defaults.Semester
        academicYearInput.Text = defaults.AcademicYear
        Dim fields As New TableLayoutPanel With {.Location = New Point(79, 345), .Size = New Size(343, 63), .ColumnCount = 2, .RowCount = 2}
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 54))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 46))
        fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 24))
        fields.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        fields.Controls.Add(New Label With {.Text = "Semester", .ForeColor = Color.DarkGreen, .Dock = DockStyle.Fill}, 0, 0)
        fields.Controls.Add(New Label With {.Text = "Academic year", .ForeColor = Color.DarkGreen, .Dock = DockStyle.Fill}, 1, 0)
        semesterPicker.Dock = DockStyle.Top : academicYearInput.Dock = DockStyle.Top
        fields.Controls.Add(semesterPicker, 0, 1) : fields.Controls.Add(academicYearInput, 1, 1)
        Controls.Add(fields)
        btnsave.Top = 426 : btncancel.Top = 426
        ClientSize = New Size(ClientSize.Width, 490)
        Text = "The Dauntless Grading System — Add Class"
    End Sub

    ''' <summary>
    ''' Loads active professors directly from MySQL into cboProfessor
    ''' </summary>
    Public Sub LoadProfessorsFromDb()
        btnsave.Enabled = False
        UiKit.TryLoadData(Me, "professor choices", Sub()
            Dim dtTeachers As DataTable = _classService.GetActiveTeachers()
            cboProfessor.DataSource = dtTeachers
            cboProfessor.DisplayMember = "display_name"
            cboProfessor.ValueMember = "teacher_id"
            cboProfessor.SelectedIndex = -1
            btnsave.Enabled = dtTeachers.Rows.Count > 0
            lblrpof.Text = If(dtTeachers.Rows.Count = 0, "No active professors", "Professor")
        End Sub)
    End Sub

    ' Kept for compatibility if called manually from Admin dashboard
    Public Sub LoadProfessors(names As IEnumerable(Of String))
        cboProfessor.DataSource = Nothing
        cboProfessor.Items.Clear()
        For Each n In names
            cboProfessor.Items.Add(n)
        Next
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnsave.Click
        lblerror.Visible = False
        If Not AppSession.IsAdmin Then
            ShowError("Administrator access is required.")
            Return
        End If
        If Not ValidationHelper.IsValidAcademicYear(academicYearInput.Text.Trim()) Then
            ShowError("Enter consecutive academic years, such as 2026-2027.")
            academicYearInput.Focus()
            Return
        End If

        ' Validation check
        If String.IsNullOrWhiteSpace(txtCode.Text) OrElse
           String.IsNullOrWhiteSpace(txtName.Text) OrElse
           String.IsNullOrWhiteSpace(txtSection.Text) Then
            ShowError("Please fill in the subject code, name and section.")
            Return
        End If

        If cboProfessor.SelectedIndex = -1 OrElse cboProfessor.SelectedValue Is Nothing Then
            ShowError("Please choose a professor.")
            Return
        End If

        Try
            ' Extract user inputs
            SubjectCode = txtCode.Text.Trim().ToUpper()
            SubjectName = txtName.Text.Trim()
            SectionName = txtSection.Text.Trim().ToUpper()

            Dim teacherId As String = cboProfessor.SelectedValue.ToString()
            ProfessorName = cboProfessor.Text

            ' Use the authenticated administrator for the audit trail.
            Dim currentAdminId As String = AppSession.CurrentUser.UserId

            ' Save offering to MySQL via ClassService & log audit event
            Dim result As OperationResult = _classService.CreateClass(
    courseCode:=SubjectCode,
    courseName:=SubjectName,
    section:=SectionName,
    semester:=semesterPicker.Text,
    academicYear:=academicYearInput.Text.Trim(),
    teacherId:=teacherId,
    adminUserId:=currentAdminId
)

            If result.Success Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                ShowError(result.Message)
            End If

        Catch ex As Exception
            ' Displays MySQL duplicates or connection errors directly on form
            ShowError(ex.Message)
        End Try
    End Sub


    Private Sub ShowError(msg As String)

        ' Hide the small error label
        lblerror.Visible = False

        ' Display the full error message
        MessageBox.Show(
        msg,
        "Add Class/Subject",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning
    )

    End Sub


    ' Designer Event Handlers
    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles txtCode.TextChanged
    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles txtSection.TextChanged
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles txtName.TextChanged
    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles lblrpof.Click
    End Sub

    Private Sub lblerror_Click(sender As Object, e As EventArgs) Handles lblerror.Click
    End Sub

End Class