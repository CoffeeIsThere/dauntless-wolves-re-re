<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAddClass
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Panel1 = New Panel()
        Label1 = New Label()
        Label2 = New Label()
        Panel2 = New Panel()
        Panel4 = New Panel()
        Panel3 = New Panel()
        txtCode = New TextBox()
        txtName = New TextBox()
        txtSection = New TextBox()
        cboProfessor = New ComboBox()
        lblCode = New Label()
        lblSection = New Label()
        lblrpof = New Label()
        lblName = New Label()
        btnsave = New Button()
        btncancel = New Button()
        lblerror = New Label()
        Panel1.SuspendLayout()
        Panel2.SuspendLayout()
        SuspendLayout()
        ' 
        ' Panel1
        ' 
        Panel1.BackColor = Color.DarkGreen
        Panel1.Controls.Add(Label1)
        Panel1.Dock = DockStyle.Top
        Panel1.Location = New Point(0, 0)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(444, 32)
        Panel1.TabIndex = 0
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(11, 8)
        Label1.Name = "Label1"
        Label1.Size = New Size(223, 17)
        Label1.TabIndex = 0
        Label1.Text = "Dauntless Wolves' Grading System"
        ' 
        ' Label2
        ' 
        Label2.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label2.ForeColor = Color.DarkGreen
        Label2.Location = New Point(3, 4)
        Label2.Name = "Label2"
        Label2.Size = New Size(122, 27)
        Label2.TabIndex = 0
        Label2.Text = "Quick Actions"
        Label2.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Panel2
        ' 
        Panel2.BackColor = Color.Honeydew
        Panel2.Controls.Add(Panel4)
        Panel2.Controls.Add(Label2)
        Panel2.Dock = DockStyle.Top
        Panel2.Location = New Point(0, 32)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(444, 34)
        Panel2.TabIndex = 3
        ' 
        ' Panel4
        ' 
        Panel4.Location = New Point(0, 34)
        Panel4.Name = "Panel4"
        Panel4.Size = New Size(446, 334)
        Panel4.TabIndex = 0
        ' 
        ' Panel3
        ' 
        Panel3.BackColor = Color.DarkGreen
        Panel3.Dock = DockStyle.Bottom
        Panel3.Location = New Point(0, 399)
        Panel3.Name = "Panel3"
        Panel3.Size = New Size(444, 32)
        Panel3.TabIndex = 5
        ' 
        ' txtCode
        ' 
        txtCode.Location = New Point(82, 119)
        txtCode.Name = "txtCode"
        txtCode.Size = New Size(274, 23)
        txtCode.TabIndex = 6
        ' 
        ' txtName
        ' 
        txtName.Location = New Point(82, 179)
        txtName.Name = "txtName"
        txtName.Size = New Size(275, 23)
        txtName.TabIndex = 7
        ' 
        ' txtSection
        ' 
        txtSection.Location = New Point(81, 233)
        txtSection.Name = "txtSection"
        txtSection.Size = New Size(274, 23)
        txtSection.TabIndex = 8
        ' 
        ' cboProfessor
        ' 
        cboProfessor.DropDownStyle = ComboBoxStyle.DropDownList
        cboProfessor.FormattingEnabled = True
        cboProfessor.Location = New Point(81, 297)
        cboProfessor.Name = "cboProfessor"
        cboProfessor.Size = New Size(275, 23)
        cboProfessor.TabIndex = 9
        ' 
        ' lblCode
        ' 
        lblCode.AutoSize = True
        lblCode.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblCode.ForeColor = Color.DarkGreen
        lblCode.Location = New Point(82, 100)
        lblCode.Name = "lblCode"
        lblCode.Size = New Size(77, 15)
        lblCode.TabIndex = 10
        lblCode.Text = "Subject Code"
        ' 
        ' lblSection
        ' 
        lblSection.AutoSize = True
        lblSection.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblSection.ForeColor = Color.DarkGreen
        lblSection.Location = New Point(82, 217)
        lblSection.Name = "lblSection"
        lblSection.Size = New Size(47, 15)
        lblSection.TabIndex = 11
        lblSection.Text = "Section"
        ' 
        ' lblrpof
        ' 
        lblrpof.AutoSize = True
        lblrpof.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblrpof.ForeColor = Color.DarkGreen
        lblrpof.Location = New Point(79, 280)
        lblrpof.Name = "lblrpof"
        lblrpof.Size = New Size(77, 15)
        lblrpof.TabIndex = 12
        lblrpof.Text = "Professor"
        ' 
        ' lblName
        ' 
        lblName.AutoSize = True
        lblName.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblName.ForeColor = Color.DarkGreen
        lblName.Location = New Point(81, 161)
        lblName.Name = "lblName"
        lblName.Size = New Size(82, 15)
        lblName.TabIndex = 13
        lblName.Text = "Subject Name"
        ' 
        ' btnsave
        ' 
        btnsave.BackColor = Color.DarkGreen
        btnsave.DialogResult = DialogResult.None
        btnsave.FlatStyle = FlatStyle.Flat
        btnsave.ForeColor = Color.White
        btnsave.Location = New Point(258, 357)
        btnsave.Name = "btnsave"
        btnsave.Size = New Size(75, 23)
        btnsave.TabIndex = 14
        btnsave.Text = "Save"
        btnsave.UseVisualStyleBackColor = False
        ' 
        ' btncancel
        ' 
        btncancel.DialogResult = DialogResult.Cancel
        btncancel.FlatStyle = FlatStyle.Flat
        btncancel.Location = New Point(347, 358)
        btncancel.Name = "btncancel"
        btncancel.Size = New Size(75, 23)
        btncancel.TabIndex = 15
        btncancel.Text = "Cancel"
        btncancel.UseVisualStyleBackColor = True
        ' 
        ' lblerror
        ' 
        lblerror.BackColor = Color.WhiteSmoke
        lblerror.ForeColor = Color.Red
        lblerror.Location = New Point(215, 360)
        lblerror.Name = "lblerror"
        lblerror.Size = New Size(34, 23)
        lblerror.TabIndex = 16
        lblerror.Text = "Error"
        lblerror.Visible = False
        ' 
        ' frmAddClass
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.WhiteSmoke
        ClientSize = New Size(444, 431)
        Controls.Add(lblerror)
        Controls.Add(btncancel)
        Controls.Add(btnsave)
        Controls.Add(lblName)
        Controls.Add(lblrpof)
        Controls.Add(lblSection)
        Controls.Add(lblCode)
        Controls.Add(cboProfessor)
        Controls.Add(txtSection)
        Controls.Add(txtName)
        Controls.Add(txtCode)
        Controls.Add(Panel3)
        Controls.Add(Panel2)
        Controls.Add(Panel1)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "frmAddClass"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Add Class/ Subject"
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        Panel2.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Panel1 As Panel
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Panel2 As Panel
    Friend WithEvents Panel3 As Panel
    Friend WithEvents Panel4 As Panel
    Friend WithEvents txtCode As TextBox
    Friend WithEvents txtName As TextBox
    Friend WithEvents txtSection As TextBox
    Friend WithEvents cboProfessor As ComboBox
    Friend WithEvents lblCode As Label
    Friend WithEvents lblSection As Label
    Friend WithEvents lblrpof As Label
    Friend WithEvents lblName As Label
    Friend WithEvents btnsave As Button
    Friend WithEvents btncancel As Button
    Friend WithEvents lblerror As Label
End Class
