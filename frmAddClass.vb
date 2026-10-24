

Public Class frmAddClass

    Private ReadOnly _classService As New ClassService()

    ' Values the dashboard reads after the dialog returns OK
    Public Property SubjectCode As String
    Public Property SubjectName As String
    Public Property SectionName As String
    Public Property ProfessorName As String

    Private Sub frmAddClass_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadProfessorsFromDb()
    End Sub

    ''' <summary>
    ''' Loads active professors directly from MySQL into cboProfessor
    ''' </summary>
    Public Sub LoadProfessorsFromDb()
        Try
            Dim dtTeachers As DataTable = _classService.GetActiveTeachers()
            cboProfessor.DataSource = dtTeachers
            cboProfessor.DisplayMember = "display_name"
            cboProfessor.ValueMember = "teacher_id"
            cboProfessor.SelectedIndex = -1
        Catch ex As Exception
            ' Fallback if database load fails
        End Try
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

            ' Get logged-in Admin ID (defaults to "1" if session is empty)
            Dim currentAdminId As String = If(AppSession.CurrentUser IsNot Nothing, AppSession.CurrentUser.UserId, "1")

            ' Save offering to MySQL via ClassService & log audit event
            Dim success As Boolean = _classService.CreateClass(
                courseCode:=SubjectCode,
                section:=SectionName,
                semester:="1st Semester",
                academicYear:="2026-2027",
                teacherId:=teacherId,
                adminUserId:=currentAdminId
            )

            If success Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If

        Catch ex As Exception
            ' Displays MySQL duplicates or connection errors directly on form
            ShowError(ex.Message)
        End Try
    End Sub

    Private Sub ShowError(msg As String)
        lblerror.Text = msg
        lblerror.Visible = True
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