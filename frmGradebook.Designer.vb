<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmGradebook
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmGradebook))
        Panel1 = New Panel()
        Panel2 = New Panel()
        CustomButton2 = New CustomButton()
        btnAddStudent = New CustomButton()
        btnAddAssessment = New CustomButton()
        cboCourse = New ComboBox()
        btnAddClass = New CustomButton()
        CustomButton1 = New CustomButton()
        Label1 = New Label()
        PictureBox1 = New PictureBox()
        PnlGrid = New Panel()
        dgvGrades = New DataGridView()
        Panel2.SuspendLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        PnlGrid.SuspendLayout()
        CType(dgvGrades, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Panel1
        ' 
        Panel1.BackColor = Color.DarkGreen
        Panel1.Dock = DockStyle.Top
        Panel1.Location = New Point(0, 0)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(1445, 35)
        Panel1.TabIndex = 0
        ' 
        ' Panel2
        ' 
        Panel2.Controls.Add(CustomButton2)
        Panel2.Controls.Add(btnAddStudent)
        Panel2.Controls.Add(btnAddAssessment)
        Panel2.Controls.Add(cboCourse)
        Panel2.Controls.Add(btnAddClass)
        Panel2.Controls.Add(CustomButton1)
        Panel2.Controls.Add(Label1)
        Panel2.Controls.Add(PictureBox1)
        Panel2.Dock = DockStyle.Top
        Panel2.Location = New Point(0, 35)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(1445, 80)
        Panel2.TabIndex = 1
        ' 
        ' CustomButton2
        ' 
        CustomButton2.BackColor = Color.Green
        CustomButton2.BorderColor = Color.Black
        CustomButton2.BorderSize = 2
        CustomButton2.CornerRadius = 10
        CustomButton2.ForeColor = Color.White
        CustomButton2.HoverColor = Color.DarkGreen
        CustomButton2.Location = New Point(871, 33)
        CustomButton2.Name = "CustomButton2"
        CustomButton2.NormalColor = Color.Green
        CustomButton2.Size = New Size(135, 29)
        CustomButton2.TabIndex = 8
        CustomButton2.Text = "Revision Request"
        CustomButton2.UseVisualStyleBackColor = False
        ' 
        ' btnAddStudent
        ' 
        btnAddStudent.BackColor = Color.Green
        btnAddStudent.BorderColor = Color.Black
        btnAddStudent.BorderSize = 2
        btnAddStudent.CornerRadius = 10
        btnAddStudent.ForeColor = Color.White
        btnAddStudent.HoverColor = Color.DarkGreen
        btnAddStudent.Location = New Point(611, 32)
        btnAddStudent.Name = "btnAddStudent"
        btnAddStudent.NormalColor = Color.Green
        btnAddStudent.Size = New Size(112, 29)
        btnAddStudent.TabIndex = 7
        btnAddStudent.Text = "Add Student"
        btnAddStudent.UseVisualStyleBackColor = False
        ' 
        ' btnAddAssessment
        ' 
        btnAddAssessment.BackColor = Color.Green
        btnAddAssessment.BorderColor = Color.Black
        btnAddAssessment.BorderSize = 2
        btnAddAssessment.CornerRadius = 10
        btnAddAssessment.ForeColor = Color.White
        btnAddAssessment.HoverColor = Color.DarkGreen
        btnAddAssessment.Location = New Point(730, 32)
        btnAddAssessment.Name = "btnAddAssessment"
        btnAddAssessment.NormalColor = Color.Green
        btnAddAssessment.Size = New Size(135, 29)
        btnAddAssessment.TabIndex = 6
        btnAddAssessment.Text = "Add Assessment"
        btnAddAssessment.UseVisualStyleBackColor = False
        ' 
        ' cboCourse
        ' 
        cboCourse.DropDownStyle = ComboBoxStyle.DropDownList
        cboCourse.FormattingEnabled = True
        cboCourse.Location = New Point(329, 33)
        cboCourse.Name = "cboCourse"
        cboCourse.Size = New Size(157, 28)
        cboCourse.TabIndex = 5
        ' 
        ' btnAddClass
        ' 
        btnAddClass.BackColor = Color.Green
        btnAddClass.BorderColor = Color.Black
        btnAddClass.BorderSize = 2
        btnAddClass.CornerRadius = 10
        btnAddClass.ForeColor = Color.White
        btnAddClass.HoverColor = Color.DarkGreen
        btnAddClass.Location = New Point(492, 32)
        btnAddClass.Name = "btnAddClass"
        btnAddClass.NormalColor = Color.Green
        btnAddClass.Size = New Size(112, 29)
        btnAddClass.TabIndex = 4
        btnAddClass.Text = "Add Class"
        btnAddClass.UseVisualStyleBackColor = False
        ' 
        ' CustomButton1
        ' 
        CustomButton1.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        CustomButton1.BackColor = Color.Green
        CustomButton1.BorderColor = Color.Black
        CustomButton1.BorderSize = 2
        CustomButton1.CornerRadius = 10
        CustomButton1.ForeColor = Color.White
        CustomButton1.HoverColor = Color.DarkGreen
        CustomButton1.Location = New Point(1330, 25)
        CustomButton1.Name = "CustomButton1"
        CustomButton1.NormalColor = Color.Green
        CustomButton1.Size = New Size(112, 29)
        CustomButton1.TabIndex = 2
        CustomButton1.Text = "Save as Pdf"
        CustomButton1.UseVisualStyleBackColor = False
        ' 
        ' Label1
        ' 
        Label1.Font = New Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.DarkGreen
        Label1.Location = New Point(78, 25)
        Label1.Name = "Label1"
        Label1.Size = New Size(261, 41)
        Label1.TabIndex = 1
        Label1.Text = "Digital Gradebook"
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), Image)
        PictureBox1.Location = New Point(11, 5)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(59, 60)
        PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox1.TabIndex = 0
        PictureBox1.TabStop = False
        ' 
        ' PnlGrid
        ' 
        PnlGrid.Controls.Add(dgvGrades)
        PnlGrid.Dock = DockStyle.Fill
        PnlGrid.Location = New Point(0, 115)
        PnlGrid.Name = "PnlGrid"
        PnlGrid.Padding = New Padding(40, 20, 40, 11)
        PnlGrid.Size = New Size(1445, 793)
        PnlGrid.TabIndex = 2
        ' 
        ' dgvGrades
        ' 
        dgvGrades.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvGrades.Dock = DockStyle.Fill
        dgvGrades.Location = New Point(40, 20)
        dgvGrades.Name = "dgvGrades"
        dgvGrades.RowHeadersWidth = 51
        dgvGrades.Size = New Size(1365, 762)
        dgvGrades.TabIndex = 0
        ' 
        ' frmGradebook
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1445, 908)
        Controls.Add(PnlGrid)
        Controls.Add(Panel2)
        Controls.Add(Panel1)
        Name = "frmGradebook"
        StartPosition = FormStartPosition.CenterScreen
        Text = "frmGradebook"
        Panel2.ResumeLayout(False)
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        PnlGrid.ResumeLayout(False)
        CType(dgvGrades, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Panel1 As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents CustomButton1 As CustomButton
    Friend WithEvents Label1 As Label
    Friend WithEvents PnlGrid As Panel
    Friend WithEvents dgvGrades As DataGridView
    Friend WithEvents btnAddAssessment As CustomButton
    Friend WithEvents cboCourse As ComboBox
    Friend WithEvents btnAddClass As CustomButton
    Friend WithEvents btnAddStudent As CustomButton
    Friend WithEvents CustomButton2 As CustomButton
End Class
