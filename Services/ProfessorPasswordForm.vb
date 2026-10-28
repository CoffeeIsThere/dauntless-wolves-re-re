Imports System.Drawing
Imports System.Windows.Forms

Public Class ProfessorPasswordForm
    Inherits Form

    Private ReadOnly teacherId As String
    Private ReadOnly current As New TextBox With {.UseSystemPasswordChar = True}
    Private ReadOnly replacement As New TextBox With {.UseSystemPasswordChar = True}
    Private ReadOnly confirmation As New TextBox With {.UseSystemPasswordChar = True}
    Private ReadOnly feedback As New Label With {.AutoSize = True, .ForeColor = Color.Firebrick, .Dock = DockStyle.Fill}
    Private ReadOnly save As Button = UiKit.MakePrimaryButton("Change password", 160)

    Public Sub New()
        If Not AppSession.IsTeacher Then Throw New UnauthorizedAccessException("Professor access is required.")
        teacherId = AppSession.CurrentUser.UserId
        UiKit.StyleDialog(Me, "Change Password", 470, 400)
        Dim body As New TableLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(22, 12, 22, 10), .ColumnCount = 1, .AutoScroll = True}
        body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        For Each field In {("Current password", current), ("New password", replacement), ("Confirm new password", confirmation)}
            body.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            body.Controls.Add(New Label With {.Text = field.Item1, .AutoSize = True, .Margin = New Padding(0, 6, 0, 4)})
            field.Item2.Dock = DockStyle.Top
            body.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            body.Controls.Add(field.Item2)
        Next
        body.Controls.Add(New Label With {.Text = "Use at least 4 characters. Your current password is required.", .AutoSize = True, .ForeColor = Color.DimGray, .Margin = New Padding(0, 8, 0, 6)})
        body.Controls.Add(feedback)
        Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 58, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(14, 8, 14, 8)}
        Dim cancel = UiKit.MakeSecondaryButton("Cancel")
        cancel.DialogResult = DialogResult.Cancel
        footer.Controls.Add(cancel)
        footer.Controls.Add(save)
        Controls.Add(body)
        Controls.Add(footer)
        Controls.Add(UiKit.MakeHeader("Change Password"))
        AcceptButton = save
        CancelButton = cancel
        AddHandler save.Click, AddressOf SavePassword
        AddHandler Shown, Sub() current.Focus()
    End Sub

    Private Sub SavePassword(sender As Object, e As EventArgs)
        feedback.Text = ""
        If Not AppSession.IsTeacher OrElse AppSession.CurrentUser.UserId <> teacherId Then
            feedback.Text = "Your session has changed. Close this window and log in again."
            Return
        End If
        If current.Text.Length = 0 Then
            feedback.Text = "Enter your current password."
            current.Focus()
            Return
        End If
        If String.IsNullOrWhiteSpace(replacement.Text) OrElse replacement.Text.Length < 4 Then
            feedback.Text = "New password must contain at least 4 characters."
            replacement.Focus()
            Return
        End If
        If replacement.Text <> confirmation.Text Then
            feedback.Text = "The new passwords do not match."
            confirmation.Focus()
            Return
        End If
        If replacement.Text = current.Text Then
            feedback.Text = "Choose a different password from your current one."
            replacement.Focus()
            Return
        End If
        save.Enabled = False
        Try
            Dim result = New AuthService().ChangePassword("Teacher", teacherId, current.Text, replacement.Text)
            If Not result.Success Then
                feedback.Text = result.Message
                Return
            End If
            current.Clear()
            replacement.Clear()
            confirmation.Clear()
            MessageBox.Show(Me, "Your password has been changed. Use the new password the next time you log in.", "Password changed", MessageBoxButtons.OK, MessageBoxIcon.Information)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            feedback.Text = "Could not change your password. Check the database connection and try again."
            Diagnostics.Debug.WriteLine(ex.ToString())
        Finally
            save.Enabled = True
        End Try
    End Sub
End Class
