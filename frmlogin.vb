Imports DAUNTLESS_WOLVES_PROF.Core
Imports DAUNTLESS_WOLVES_PROF.Models


Public Class frmlogin

    Private ReadOnly authService As New AuthService()

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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
        Dim username As String = boxuser.Text.Trim()
        Dim password As String = TextBox1.Text.Trim()

        ' 1. Input Validation
        If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
            MessageBox.Show("Please enter both your Username/ID and Password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            ' 2. Authenticate user against database via UserService
            Dim user As UserModel = authService.VerifyLogin(username, password)

            If user IsNot Nothing Then
                ' 3. Save user state in AppSession
                AppSession.StartSession(user)

                ' 4. Route to designated form based on database user role
                Me.Hide()

                Select Case user.Role.Trim().ToLower()
                    Case "admin"
                        Dim adminForm As New Admin()
                        adminForm.Show()

                    Case "teacher", "professor", "prof"
                        Dim profForm As New frmprof()
                        profForm.Show()

                    Case "student"
                        Dim studentForm As New frmstudent()
                        studentForm.Show()

                    Case Else
                        MessageBox.Show("Unrecognized user role assigned.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        AppSession.ClearSession()
                        Me.Show()
                End Select
            Else
                MessageBox.Show("Invalid Username/ID or Password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            MessageBox.Show($"Database connection error: {ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class