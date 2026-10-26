<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmlogin
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer.
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Panel1 = New Panel()
        Label1 = New Label()
        Panel2 = New Panel()
        cardlogin = New CustomPanel()
        Button1 = New Button()
        boxRole = New CustomPanel()
        RadioButton2 = New RadioButton()
        RadioButton1 = New RadioButton()
        Label3 = New Label()
        boxPass = New CustomPanel()
        TextBox1 = New TextBox()
        Label2 = New Label()
        CustomPanel1 = New CustomPanel()
        boxuser = New TextBox()
        pblogo = New PictureBox()
        Panel1.SuspendLayout()
        cardlogin.SuspendLayout()
        boxRole.SuspendLayout()
        boxPass.SuspendLayout()
        CustomPanel1.SuspendLayout()
        CType(pblogo, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Panel1
        ' 
        Panel1.BackColor = Color.DarkGreen
        Panel1.Controls.Add(Label1)
        Panel1.Dock = DockStyle.Top
        Panel1.Location = New Point(0, 0)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(1264, 29)
        Panel1.TabIndex = 0
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(8, 7)
        Label1.Name = "Label1"
        Label1.Size = New Size(200, 15)
        Label1.TabIndex = 0
        Label1.Text = "Dauntless Wolves' Grading System"
        ' 
        ' Panel2
        ' 
        Panel2.BackColor = Color.DarkGreen
        Panel2.Dock = DockStyle.Bottom
        Panel2.Location = New Point(0, 652)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(1264, 29)
        Panel2.TabIndex = 1
        ' 
        ' cardlogin
        ' 
        cardlogin.Anchor = AnchorStyles.None
        cardlogin.BackColor = Color.WhiteSmoke
        cardlogin.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        cardlogin.ClipCorners = True
        cardlogin.Controls.Add(Button1)
        cardlogin.Controls.Add(boxRole)
        cardlogin.Controls.Add(Label3)
        cardlogin.Controls.Add(boxPass)
        cardlogin.Controls.Add(Label2)
        cardlogin.Controls.Add(CustomPanel1)
        cardlogin.Controls.Add(pblogo)
        cardlogin.Location = New Point(428, 71)
        cardlogin.Name = "cardlogin"
        cardlogin.Size = New Size(410, 520)
        cardlogin.TabIndex = 0
        ' 
        ' Button1
        ' 
        Button1.BackColor = Color.DarkGreen
        Button1.FlatStyle = FlatStyle.Flat
        Button1.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Button1.ForeColor = Color.White
        Button1.Location = New Point(50, 419)
        Button1.Name = "Button1"
        Button1.Size = New Size(307, 46)
        Button1.TabIndex = 6
        Button1.Text = "Log in"
        Button1.UseVisualStyleBackColor = False
        ' 
        ' boxRole
        ' 
        boxRole.BackColor = Color.MintCream
        boxRole.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        boxRole.BorderThickness = 1
        boxRole.Controls.Add(RadioButton2)
        boxRole.Controls.Add(RadioButton1)
        boxRole.CornerRadius = 8
        boxRole.Location = New Point(49, 361)
        boxRole.Name = "boxRole"
        boxRole.Size = New Size(307, 50)
        boxRole.TabIndex = 2
        ' 
        ' RadioButton2
        ' 
        RadioButton2.AutoSize = True
        RadioButton2.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        RadioButton2.ForeColor = Color.DarkGreen
        RadioButton2.Location = New Point(179, 15)
        RadioButton2.Name = "RadioButton2"
        RadioButton2.Size = New Size(73, 21)
        RadioButton2.TabIndex = 7
        RadioButton2.Text = "Teacher"
        RadioButton2.UseVisualStyleBackColor = True
        ' 
        ' RadioButton1
        ' 
        RadioButton1.AutoSize = True
        RadioButton1.Checked = True
        RadioButton1.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        RadioButton1.ForeColor = Color.DarkGreen
        RadioButton1.Location = New Point(45, 15)
        RadioButton1.Name = "RadioButton1"
        RadioButton1.Size = New Size(74, 21)
        RadioButton1.TabIndex = 6
        RadioButton1.TabStop = True
        RadioButton1.Text = "Student"
        RadioButton1.UseVisualStyleBackColor = True
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(49, 274)
        Label3.Name = "Label3"
        Label3.Size = New Size(57, 15)
        Label3.TabIndex = 5
        Label3.Text = "Password"
        ' 
        ' boxPass
        ' 
        boxPass.BackColor = Color.White
        boxPass.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        boxPass.BorderThickness = 1
        boxPass.Controls.Add(TextBox1)
        boxPass.CornerRadius = 8
        boxPass.Location = New Point(49, 293)
        boxPass.Name = "boxPass"
        boxPass.Size = New Size(305, 54)
        boxPass.TabIndex = 4
        ' 
        ' TextBox1
        ' 
        TextBox1.BackColor = Color.White
        TextBox1.BorderStyle = BorderStyle.None
        TextBox1.Font = New Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        TextBox1.Location = New Point(1, 17)
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(302, 22)
        TextBox1.TabIndex = 1
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(50, 186)
        Label2.Name = "Label2"
        Label2.Size = New Size(182, 15)
        Label2.TabIndex = 3
        Label2.Text = "Username/StudentID/ProfessorID"
        ' 
        ' CustomPanel1
        ' 
        CustomPanel1.BackColor = Color.White
        CustomPanel1.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        CustomPanel1.BorderThickness = 1
        CustomPanel1.Controls.Add(boxuser)
        CustomPanel1.CornerRadius = 8
        CustomPanel1.Location = New Point(50, 205)
        CustomPanel1.Name = "CustomPanel1"
        CustomPanel1.Size = New Size(305, 54)
        CustomPanel1.TabIndex = 2
        ' 
        ' boxuser
        ' 
        boxuser.BackColor = Color.White
        boxuser.BorderStyle = BorderStyle.None
        boxuser.Font = New Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        boxuser.Location = New Point(1, 16)
        boxuser.Name = "boxuser"
        boxuser.Size = New Size(302, 22)
        boxuser.TabIndex = 1
        ' 
        ' pblogo
        ' 
        pblogo.Anchor = AnchorStyles.Top
        pblogo.Image = My.Resources.Resources._5037365_orig
        pblogo.Location = New Point(150, 38)
        pblogo.Name = "pblogo"
        pblogo.Size = New Size(108, 102)
        pblogo.SizeMode = PictureBoxSizeMode.StretchImage
        pblogo.TabIndex = 0
        pblogo.TabStop = False
        ' 
        ' frmlogin
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackgroundImage = My.Resources.Resources.images__1_1
        BackgroundImageLayout = ImageLayout.Stretch
        ClientSize = New Size(1264, 681)
        Controls.Add(cardlogin)
        Controls.Add(Panel2)
        Controls.Add(Panel1)
        Name = "frmlogin"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Form1"
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        cardlogin.ResumeLayout(False)
        cardlogin.PerformLayout()
        boxRole.ResumeLayout(False)
        boxRole.PerformLayout()
        boxPass.ResumeLayout(False)
        boxPass.PerformLayout()
        CustomPanel1.ResumeLayout(False)
        CustomPanel1.PerformLayout()
        CType(pblogo, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Panel1 As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents cardlogin As CustomPanel
    Friend WithEvents pblogo As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents boxuser As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents CustomPanel1 As CustomPanel
    Friend WithEvents Label3 As Label
    Friend WithEvents boxPass As CustomPanel
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents boxRole As CustomPanel
    Friend WithEvents RadioButton2 As RadioButton
    Friend WithEvents RadioButton1 As RadioButton
    Friend WithEvents Button1 As Button
End Class