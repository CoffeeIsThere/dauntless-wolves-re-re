Public Class frmAddClass
    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles txtCode.TextChanged

    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles txtSection.TextChanged

    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles txtName.TextChanged

    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles lblrpof.Click

    End Sub



    ' Values the dashboard reads after the dialog returns OK
    Public Property SubjectCode As String
    Public Property SubjectName As String
    Public Property SectionName As String
    Public Property ProfessorName As String

    ' Called by the Admin form before ShowDialog
    Public Sub LoadProfessors(names As IEnumerable(Of String))
        cboProfessor.Items.Clear()
        For Each n In names
            cboProfessor.Items.Add(n)
        Next
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnsave.Click
        lblerror.Visible = False

        If String.IsNullOrWhiteSpace(txtCode.Text) OrElse
           String.IsNullOrWhiteSpace(txtName.Text) OrElse
           String.IsNullOrWhiteSpace(txtSection.Text) Then
            ShowError("Please fill in the subject code, name and section.")
            Return
        End If

        If cboProfessor.SelectedIndex = -1 Then
            ShowError("Please choose a professor.")
            Return
        End If

        SubjectCode = txtCode.Text.Trim().ToUpper()
        SubjectName = txtName.Text.Trim()
        SectionName = txtSection.Text.Trim().ToUpper()
        ProfessorName = cboProfessor.Text

        ' Later: INSERT the class into MySQL here, and write an audit log row.
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub ShowError(msg As String)
        lblerror.Text = msg
        lblerror.Visible = True
    End Sub

    Private Sub lblerror_Click(sender As Object, e As EventArgs) Handles lblerror.Click

    End Sub
End Class
