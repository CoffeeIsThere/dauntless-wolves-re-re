
Imports DAUNTLESS_WOLVES_PROF.Models



Public Class frmlogin

    Private ReadOnly authService As New AuthService()

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AcceptButton = Button1
        ' Mask password characters in TextBox1
        TextBox1.UseSystemPasswordChar = True

        ' Apply background opacity effect
        If Me.BackgroundImage IsNot Nothing Then
            Dim original As Image = Me.BackgroundImage
            Me.BackgroundImage = FadeImage(original, 0.5F)
        End If
    End Sub

    ' Opacity helper
    Private Function FadeImage(src As Image, opacity As Single) As Image
        Dim bmp As New Bitmap(src.Width, src.Height)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.White)
            Dim cm As New Imaging.ColorMatrix() With {.Matrix33 = opacity}
            Using ia As New Imaging.ImageAttributes()
                ia.SetColorMatrix(cm)
                g.DrawImage(src, New Rectangle(0, 0, bmp.Width, bmp.Height),
                        0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia)
            End Using
        End Using
        Return bmp
    End Function

    ' LOG IN BUTTON CLICK (Button1)
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim username As String = ValidationHelper.SanitizeInput(boxuser.Text)
        Dim password As String = TextBox1.Text

        If Not ValidationHelper.IsRequired(username) OrElse Not ValidationHelper.IsRequired(password) Then
            MessageBox.Show("Please enter both your Username/ID and Password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            boxuser.Focus()
            Return
        End If

        Try
            ' 2. Authenticate user against database via UserService
            Dim user As UserModel = authService.VerifyLogin(username, password, If(RadioButton1.Checked, "Student", "Teacher"))

            If user IsNot Nothing Then
                Dim role As String = user.Role.Trim().ToLower()
                Dim picked As String = If(RadioButton1.Checked, "student", "teacher")

                ' Admins have no radio button, so let them through with either selected
                If role <> "admin" AndAlso role <> picked Then
                    MessageBox.Show("This account is not a " & picked & " account. Please select the correct role.",
                                "Wrong role", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    TextBox1.Clear()
                    Return
                End If

                ' 3. Save user state in AppSession
                AppSession.StartSession(user)

                ' 4. Route to designated form based on database user role
                Select Case user.Role.Trim().ToLower()
                    Case "admin"
                        Dim adminForm As New Admin()
                        adminForm.Show()
                        Me.Close()

                    Case "teacher", "professor", "prof"
                        Dim profForm As New frmprof()
                        profForm.Show()
                        Me.Close()

                    Case "student"
                        Dim studentForm As New frmstudent()
                        studentForm.Show()
                        Me.Close()

                    Case Else
                        MessageBox.Show("Unrecognized user role assigned.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        AppSession.ClearSession()
                End Select
            Else
                MessageBox.Show("Invalid Username/ID or Password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
                TextBox1.Clear()
                TextBox1.Focus()
            End If

        Catch ex As Exception
            MessageBox.Show($"Database connection error: {ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class