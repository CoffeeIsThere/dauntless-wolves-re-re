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
        Panel1.Margin = New Padding(3, 2, 3, 2)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(1264, 26)
        Panel1.TabIndex = 0
        ' 
        ' Panel2
        ' 
        Panel2.Controls.Add(CustomButton1)
        Panel2.Controls.Add(Label1)
        Panel2.Controls.Add(PictureBox1)
        Panel2.Dock = DockStyle.Top
        Panel2.Location = New Point(0, 26)
        Panel2.Margin = New Padding(3, 2, 3, 2)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(1264, 60)
        Panel2.TabIndex = 1
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
        CustomButton1.Location = New Point(1156, 25)
        CustomButton1.Margin = New Padding(3, 2, 3, 2)
        CustomButton1.Name = "CustomButton1"
        CustomButton1.NormalColor = Color.Green
        CustomButton1.Size = New Size(98, 22)
        CustomButton1.TabIndex = 2
        CustomButton1.Text = "Save as Pdf"
        CustomButton1.UseVisualStyleBackColor = False
        ' 
        ' Label1
        ' 
        Label1.Font = New Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.DarkGreen
        Label1.Location = New Point(68, 19)
        Label1.Name = "Label1"
        Label1.Size = New Size(228, 31)
        Label1.TabIndex = 1
        Label1.Text = "Digital Gradebook"
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), Image)
        PictureBox1.Location = New Point(10, 4)
        PictureBox1.Margin = New Padding(3, 2, 3, 2)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(52, 45)
        PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox1.TabIndex = 0
        PictureBox1.TabStop = False
        ' 
        ' PnlGrid
        ' 
        PnlGrid.Controls.Add(dgvGrades)
        PnlGrid.Dock = DockStyle.Fill
        PnlGrid.Location = New Point(0, 86)
        PnlGrid.Margin = New Padding(3, 2, 3, 2)
        PnlGrid.Name = "PnlGrid"
        PnlGrid.Padding = New Padding(35, 15, 35, 8)
        PnlGrid.Size = New Size(1264, 595)
        PnlGrid.TabIndex = 2
        ' 
        ' dgvGrades
        ' 
        dgvGrades.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvGrades.Dock = DockStyle.Fill
        dgvGrades.Location = New Point(35, 15)
        dgvGrades.Margin = New Padding(3, 2, 3, 2)
        dgvGrades.Name = "dgvGrades"
        dgvGrades.RowHeadersWidth = 51
        dgvGrades.Size = New Size(1194, 572)
        dgvGrades.TabIndex = 0
        ' 
        ' frmGradebook
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1264, 681)
        Controls.Add(PnlGrid)
        Controls.Add(Panel2)
        Controls.Add(Panel1)
        Margin = New Padding(3, 2, 3, 2)
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
End Class
