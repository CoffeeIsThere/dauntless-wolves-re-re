<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Admin
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Admin))
        pnlTitle = New Panel()
        Label1 = New Label()
        pnlUser = New Panel()
        btnLogout = New CustomButton()
        PictureBox1 = New PictureBox()
        lblRole = New PillLabel()
        lblAdminName = New Label()
        pnlTabs = New Panel()
        Panel3 = New Panel()
        btnProfessors = New CustomButton()
        btnStudents = New CustomButton()
        btnDashboard = New CustomButton()
        pnlFooter = New Panel()
        pnlContent = New Panel()
        pnlProfessors = New Panel()
        pnlProfCard = New Panel()
        dgvProf = New DataGridView()
        pnlProfPager = New Panel()
        flowProfPager = New FlowLayoutPanel()
        lblTotalProf = New Label()
        pnlProfTop = New Panel()
        btnAddProf = New CustomButton()
        cboDeptProf = New ComboBox()
        txtSearchProf = New TextBox()
        CustomPanel7 = New CustomPanel()
        pnlDashboard = New Panel()
        cardDashboard = New CustomPanel()
        tlpDashBottom = New TableLayoutPanel()
        pnlupdatesheader = New CustomPanel()
        Panel1 = New Panel()
        PictureBox3 = New PictureBox()
        btnupdatesfilter = New Button()
        Label10 = New Label()
        dgvupdates = New DataGridView()
        CustomPanel6 = New CustomPanel()
        tlpActions = New TableLayoutPanel()
        btnAddClass = New CustomButton()
        btnEnterGrades = New CustomButton()
        btnViewRequests = New CustomButton()
        btnViewLogs = New CustomButton()
        pnlactionsheader = New Panel()
        PictureBox4 = New PictureBox()
        Label11 = New Label()
        tlpdashstats = New TableLayoutPanel()
        CustomPanel5 = New CustomPanel()
        lblStatPending = New Label()
        Label9 = New Label()
        CustomPanel4 = New CustomPanel()
        lblStatClasses = New Label()
        Label8 = New Label()
        CustomPanel3 = New CustomPanel()
        lblStatProfs = New Label()
        Label7 = New Label()
        CustomPanel2 = New CustomPanel()
        lblStatStudents = New Label()
        Label6 = New Label()
        pnlDashHeader = New Panel()
        CustomPanel1 = New CustomPanel()
        Label5 = New Label()
        Label2 = New Label()
        PictureBox2 = New PictureBox()
        pnlStudents = New Panel()
        pnlstudentcard = New CustomPanel()
        dgvstudents = New DataGridView()
        Panel2 = New Panel()
        lbltotalstudents = New Label()
        btnaddstudent = New CustomButton()
        btnStudentReset = New CustomButton()
        btnStudentFilter = New CustomButton()
        Label4 = New Label()
        Label3 = New Label()
        cboSection = New ComboBox()
        cboYearLevel = New ComboBox()
        txtStudentSearch = New TextBox()
        flowstudentpager = New FlowLayoutPanel()
        pnlTitle.SuspendLayout()
        pnlUser.SuspendLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        pnlTabs.SuspendLayout()
        pnlContent.SuspendLayout()
        pnlProfessors.SuspendLayout()
        pnlProfCard.SuspendLayout()
        CType(dgvProf, ComponentModel.ISupportInitialize).BeginInit()
        pnlProfPager.SuspendLayout()
        pnlProfTop.SuspendLayout()
        pnlDashboard.SuspendLayout()
        cardDashboard.SuspendLayout()
        tlpDashBottom.SuspendLayout()
        pnlupdatesheader.SuspendLayout()
        Panel1.SuspendLayout()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvupdates, ComponentModel.ISupportInitialize).BeginInit()
        CustomPanel6.SuspendLayout()
        tlpActions.SuspendLayout()
        pnlactionsheader.SuspendLayout()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        tlpdashstats.SuspendLayout()
        CustomPanel5.SuspendLayout()
        CustomPanel4.SuspendLayout()
        CustomPanel3.SuspendLayout()
        CustomPanel2.SuspendLayout()
        pnlDashHeader.SuspendLayout()
        CustomPanel1.SuspendLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        pnlStudents.SuspendLayout()
        pnlstudentcard.SuspendLayout()
        CType(dgvstudents, ComponentModel.ISupportInitialize).BeginInit()
        Panel2.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlTitle
        ' 
        pnlTitle.BackColor = Color.DarkGreen
        pnlTitle.Controls.Add(Label1)
        pnlTitle.Dock = DockStyle.Top
        pnlTitle.Location = New Point(0, 0)
        pnlTitle.Name = "pnlTitle"
        pnlTitle.Size = New Size(1370, 29)
        pnlTitle.TabIndex = 0
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.ForeColor = Color.White
        Label1.Location = New Point(8, 4)
        Label1.Name = "Label1"
        Label1.Size = New Size(323, 20)
        Label1.TabIndex = 0
        Label1.Text = "Transparent Grading System - Dauntless Wolves"
        ' 
        ' pnlUser
        ' 
        pnlUser.BackColor = Color.White
        pnlUser.Controls.Add(btnLogout)
        pnlUser.Controls.Add(PictureBox1)
        pnlUser.Controls.Add(lblRole)
        pnlUser.Controls.Add(lblAdminName)
        pnlUser.Dock = DockStyle.Top
        pnlUser.Location = New Point(0, 29)
        pnlUser.Name = "pnlUser"
        pnlUser.Size = New Size(1370, 40)
        pnlUser.TabIndex = 1
        ' 
        ' btnLogout
        ' 
        btnLogout.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnLogout.BackColor = Color.White
        btnLogout.BorderColor = Color.Black
        btnLogout.BorderSize = 2
        btnLogout.CornerRadius = 10
        btnLogout.HoverColor = Color.Red
        btnLogout.Location = New Point(1265, 8)
        btnLogout.Name = "btnLogout"
        btnLogout.NormalColor = Color.White
        btnLogout.Size = New Size(94, 29)
        btnLogout.TabIndex = 3
        btnLogout.Text = "Log out"
        btnLogout.UseVisualStyleBackColor = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), Image)
        PictureBox1.Location = New Point(27, 3)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(35, 35)
        PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox1.TabIndex = 2
        PictureBox1.TabStop = False
        ' 
        ' lblRole
        ' 
        lblRole.BackColor = Color.DarkGreen
        lblRole.CornerRadius = 20
        lblRole.ForeColor = Color.White
        lblRole.Location = New Point(238, 11)
        lblRole.Name = "lblRole"
        lblRole.Size = New Size(91, 25)
        lblRole.TabIndex = 1
        lblRole.Text = "Admin"
        lblRole.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblAdminName
        ' 
        lblAdminName.AutoSize = True
        lblAdminName.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lblAdminName.ForeColor = Color.Black
        lblAdminName.Location = New Point(69, 11)
        lblAdminName.Name = "lblAdminName"
        lblAdminName.Size = New Size(143, 23)
        lblAdminName.TabIndex = 0
        lblAdminName.Text = "Oropilla, Charles"
        ' 
        ' pnlTabs
        ' 
        pnlTabs.Controls.Add(Panel3)
        pnlTabs.Controls.Add(btnProfessors)
        pnlTabs.Controls.Add(btnStudents)
        pnlTabs.Controls.Add(btnDashboard)
        pnlTabs.Dock = DockStyle.Top
        pnlTabs.Location = New Point(0, 69)
        pnlTabs.Name = "pnlTabs"
        pnlTabs.Size = New Size(1370, 45)
        pnlTabs.TabIndex = 2
        ' 
        ' Panel3
        ' 
        Panel3.BackColor = Color.DimGray
        Panel3.Dock = DockStyle.Bottom
        Panel3.Location = New Point(0, 41)
        Panel3.Margin = New Padding(3, 4, 3, 4)
        Panel3.Name = "Panel3"
        Panel3.Size = New Size(1370, 4)
        Panel3.TabIndex = 1
        ' 
        ' btnProfessors
        ' 
        btnProfessors.BackColor = Color.Gray
        btnProfessors.BorderColor = Color.Black
        btnProfessors.BorderSize = 2
        btnProfessors.CornerRadius = 10
        btnProfessors.ForeColor = Color.DarkGreen
        btnProfessors.HoverColor = Color.LightGray
        btnProfessors.Location = New Point(338, 11)
        btnProfessors.Name = "btnProfessors"
        btnProfessors.NormalColor = Color.Gray
        btnProfessors.Size = New Size(150, 32)
        btnProfessors.TabIndex = 2
        btnProfessors.Text = "Manage Professors"
        btnProfessors.UseVisualStyleBackColor = False
        ' 
        ' btnStudents
        ' 
        btnStudents.BackColor = Color.Gray
        btnStudents.BorderColor = Color.Black
        btnStudents.BorderSize = 2
        btnStudents.CornerRadius = 10
        btnStudents.ForeColor = Color.DarkGreen
        btnStudents.HoverColor = Color.LightGray
        btnStudents.Location = New Point(184, 11)
        btnStudents.Name = "btnStudents"
        btnStudents.NormalColor = Color.Gray
        btnStudents.Size = New Size(150, 32)
        btnStudents.TabIndex = 1
        btnStudents.Text = "Manage Students"
        btnStudents.UseVisualStyleBackColor = False
        ' 
        ' btnDashboard
        ' 
        btnDashboard.BackColor = Color.Gray
        btnDashboard.BorderColor = Color.Black
        btnDashboard.BorderSize = 2
        btnDashboard.CornerRadius = 10
        btnDashboard.ForeColor = Color.DarkGreen
        btnDashboard.HoverColor = Color.LightGray
        btnDashboard.Location = New Point(27, 11)
        btnDashboard.Name = "btnDashboard"
        btnDashboard.NormalColor = Color.Gray
        btnDashboard.Size = New Size(150, 32)
        btnDashboard.TabIndex = 0
        btnDashboard.Text = "Dashboard"
        btnDashboard.UseVisualStyleBackColor = False
        ' 
        ' pnlFooter
        ' 
        pnlFooter.BackColor = Color.DarkGreen
        pnlFooter.Dock = DockStyle.Bottom
        pnlFooter.Location = New Point(0, 724)
        pnlFooter.Name = "pnlFooter"
        pnlFooter.Size = New Size(1370, 25)
        pnlFooter.TabIndex = 3
        ' 
        ' pnlContent
        ' 
        pnlContent.Controls.Add(pnlProfessors)
        pnlContent.Controls.Add(pnlDashboard)
        pnlContent.Controls.Add(pnlStudents)
        pnlContent.Dock = DockStyle.Fill
        pnlContent.Location = New Point(0, 114)
        pnlContent.Name = "pnlContent"
        pnlContent.Size = New Size(1370, 610)
        pnlContent.TabIndex = 4
        ' 
        ' pnlProfessors
        ' 
        pnlProfessors.BackColor = Color.White
        pnlProfessors.Controls.Add(pnlProfCard)
        pnlProfessors.Controls.Add(CustomPanel7)
        pnlProfessors.Dock = DockStyle.Fill
        pnlProfessors.Location = New Point(0, 0)
        pnlProfessors.Name = "pnlProfessors"
        pnlProfessors.Padding = New Padding(23, 27, 23, 17)
        pnlProfessors.Size = New Size(1370, 610)
        pnlProfessors.TabIndex = 2
        ' 
        ' pnlProfCard
        ' 
        pnlProfCard.BackColor = Color.White
        pnlProfCard.Controls.Add(dgvProf)
        pnlProfCard.Controls.Add(pnlProfPager)
        pnlProfCard.Controls.Add(lblTotalProf)
        pnlProfCard.Controls.Add(pnlProfTop)
        pnlProfCard.Dock = DockStyle.Fill
        pnlProfCard.ForeColor = SystemColors.ControlText
        pnlProfCard.Location = New Point(23, 27)
        pnlProfCard.Name = "pnlProfCard"
        pnlProfCard.Padding = New Padding(21, 20, 21, 20)
        pnlProfCard.Size = New Size(1324, 566)
        pnlProfCard.TabIndex = 0
        ' 
        ' dgvProf
        ' 
        dgvProf.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvProf.Dock = DockStyle.Fill
        dgvProf.Location = New Point(21, 91)
        dgvProf.Name = "dgvProf"
        dgvProf.RowHeadersWidth = 51
        dgvProf.Size = New Size(1282, 415)
        dgvProf.TabIndex = 3
        ' 
        ' pnlProfPager
        ' 
        pnlProfPager.Controls.Add(flowProfPager)
        pnlProfPager.Dock = DockStyle.Bottom
        pnlProfPager.Location = New Point(21, 506)
        pnlProfPager.Name = "pnlProfPager"
        pnlProfPager.Size = New Size(1282, 40)
        pnlProfPager.TabIndex = 2
        ' 
        ' flowProfPager
        ' 
        flowProfPager.Dock = DockStyle.Right
        flowProfPager.Location = New Point(1153, 0)
        flowProfPager.Name = "flowProfPager"
        flowProfPager.Size = New Size(129, 40)
        flowProfPager.TabIndex = 0
        flowProfPager.WrapContents = False
        ' 
        ' lblTotalProf
        ' 
        lblTotalProf.AutoSize = True
        lblTotalProf.Dock = DockStyle.Top
        lblTotalProf.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblTotalProf.ForeColor = Color.DarkGreen
        lblTotalProf.Location = New Point(21, 71)
        lblTotalProf.Name = "lblTotalProf"
        lblTotalProf.Size = New Size(148, 20)
        lblTotalProf.TabIndex = 1
        lblTotalProf.Text = "Total Professors: 67"
        ' 
        ' pnlProfTop
        ' 
        pnlProfTop.Controls.Add(btnAddProf)
        pnlProfTop.Controls.Add(cboDeptProf)
        pnlProfTop.Controls.Add(txtSearchProf)
        pnlProfTop.Dock = DockStyle.Top
        pnlProfTop.Location = New Point(21, 20)
        pnlProfTop.Name = "pnlProfTop"
        pnlProfTop.Size = New Size(1282, 51)
        pnlProfTop.TabIndex = 0
        ' 
        ' btnAddProf
        ' 
        btnAddProf.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnAddProf.BackColor = Color.DarkGreen
        btnAddProf.BorderColor = Color.Black
        btnAddProf.BorderSize = 2
        btnAddProf.CornerRadius = 10
        btnAddProf.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnAddProf.ForeColor = Color.White
        btnAddProf.HoverColor = Color.LightGray
        btnAddProf.Location = New Point(1153, 16)
        btnAddProf.Name = "btnAddProf"
        btnAddProf.NormalColor = Color.DarkGreen
        btnAddProf.Size = New Size(117, 29)
        btnAddProf.TabIndex = 2
        btnAddProf.Text = "Add Professor"
        btnAddProf.UseVisualStyleBackColor = False
        ' 
        ' cboDeptProf
        ' 
        cboDeptProf.DropDownStyle = ComboBoxStyle.DropDownList
        cboDeptProf.FormattingEnabled = True
        cboDeptProf.Location = New Point(288, 8)
        cboDeptProf.Name = "cboDeptProf"
        cboDeptProf.Size = New Size(220, 28)
        cboDeptProf.TabIndex = 1
        ' 
        ' txtSearchProf
        ' 
        txtSearchProf.Location = New Point(0, 8)
        txtSearchProf.Name = "txtSearchProf"
        txtSearchProf.Size = New Size(260, 27)
        txtSearchProf.TabIndex = 0
        ' 
        ' CustomPanel7
        ' 
        CustomPanel7.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        CustomPanel7.BackColor = Color.White
        CustomPanel7.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        CustomPanel7.Location = New Point(9, 21)
        CustomPanel7.Margin = New Padding(3, 4, 3, 4)
        CustomPanel7.Name = "CustomPanel7"
        CustomPanel7.Size = New Size(1544, 785)
        CustomPanel7.TabIndex = 1
        ' 
        ' pnlDashboard
        ' 
        pnlDashboard.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        pnlDashboard.BackColor = Color.WhiteSmoke
        pnlDashboard.Controls.Add(cardDashboard)
        pnlDashboard.Location = New Point(0, 0)
        pnlDashboard.Name = "pnlDashboard"
        pnlDashboard.Padding = New Padding(46, 53, 46, 40)
        pnlDashboard.Size = New Size(1370, 610)
        pnlDashboard.TabIndex = 0
        ' 
        ' cardDashboard
        ' 
        cardDashboard.BackColor = Color.White
        cardDashboard.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        cardDashboard.Controls.Add(tlpDashBottom)
        cardDashboard.Controls.Add(tlpdashstats)
        cardDashboard.Controls.Add(pnlDashHeader)
        cardDashboard.Dock = DockStyle.Fill
        cardDashboard.Location = New Point(46, 53)
        cardDashboard.Margin = New Padding(3, 4, 3, 4)
        cardDashboard.Name = "cardDashboard"
        cardDashboard.Padding = New Padding(11, 13, 11, 13)
        cardDashboard.Size = New Size(1278, 517)
        cardDashboard.TabIndex = 0
        ' 
        ' tlpDashBottom
        ' 
        tlpDashBottom.ColumnCount = 2
        tlpDashBottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpDashBottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpDashBottom.Controls.Add(pnlupdatesheader, 0, 0)
        tlpDashBottom.Controls.Add(CustomPanel6, 1, 0)
        tlpDashBottom.Dock = DockStyle.Fill
        tlpDashBottom.Location = New Point(11, 248)
        tlpDashBottom.Margin = New Padding(3, 4, 3, 4)
        tlpDashBottom.Name = "tlpDashBottom"
        tlpDashBottom.RowCount = 1
        tlpDashBottom.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpDashBottom.RowStyles.Add(New RowStyle(SizeType.Absolute, 255F))
        tlpDashBottom.Size = New Size(1256, 256)
        tlpDashBottom.TabIndex = 2
        ' 
        ' pnlupdatesheader
        ' 
        pnlupdatesheader.BackColor = Color.White
        pnlupdatesheader.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        pnlupdatesheader.Controls.Add(Panel1)
        pnlupdatesheader.Controls.Add(dgvupdates)
        pnlupdatesheader.Dock = DockStyle.Fill
        pnlupdatesheader.Location = New Point(11, 13)
        pnlupdatesheader.Margin = New Padding(11, 13, 11, 13)
        pnlupdatesheader.Name = "pnlupdatesheader"
        pnlupdatesheader.Padding = New Padding(15, 7, 15, 27)
        pnlupdatesheader.Size = New Size(606, 230)
        pnlupdatesheader.TabIndex = 0
        ' 
        ' Panel1
        ' 
        Panel1.Controls.Add(PictureBox3)
        Panel1.Controls.Add(btnupdatesfilter)
        Panel1.Controls.Add(Label10)
        Panel1.Dock = DockStyle.Top
        Panel1.Location = New Point(15, 7)
        Panel1.Margin = New Padding(3, 4, 3, 4)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(576, 64)
        Panel1.TabIndex = 3
        ' 
        ' PictureBox3
        ' 
        PictureBox3.Image = CType(resources.GetObject("PictureBox3.Image"), Image)
        PictureBox3.Location = New Point(2, 8)
        PictureBox3.Margin = New Padding(3, 4, 3, 4)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New Size(62, 47)
        PictureBox3.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox3.TabIndex = 2
        PictureBox3.TabStop = False
        ' 
        ' btnupdatesfilter
        ' 
        btnupdatesfilter.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnupdatesfilter.BackColor = Color.Honeydew
        btnupdatesfilter.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnupdatesfilter.ForeColor = Color.DarkGreen
        btnupdatesfilter.Location = New Point(486, 16)
        btnupdatesfilter.Margin = New Padding(3, 4, 3, 4)
        btnupdatesfilter.Name = "btnupdatesfilter"
        btnupdatesfilter.Size = New Size(86, 31)
        btnupdatesfilter.TabIndex = 1
        btnupdatesfilter.Text = "Filter"
        btnupdatesfilter.UseVisualStyleBackColor = False
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.Font = New Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label10.ForeColor = Color.DarkGreen
        Label10.Location = New Point(64, 16)
        Label10.Name = "Label10"
        Label10.Size = New Size(107, 32)
        Label10.TabIndex = 1
        Label10.Text = "Updates"
        ' 
        ' dgvupdates
        ' 
        dgvupdates.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvupdates.Dock = DockStyle.Fill
        dgvupdates.Location = New Point(15, 7)
        dgvupdates.Margin = New Padding(3, 4, 3, 4)
        dgvupdates.Name = "dgvupdates"
        dgvupdates.RowHeadersWidth = 51
        dgvupdates.Size = New Size(576, 196)
        dgvupdates.TabIndex = 2
        ' 
        ' CustomPanel6
        ' 
        CustomPanel6.BackColor = Color.White
        CustomPanel6.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        CustomPanel6.Controls.Add(tlpActions)
        CustomPanel6.Controls.Add(pnlactionsheader)
        CustomPanel6.Dock = DockStyle.Fill
        CustomPanel6.Location = New Point(639, 13)
        CustomPanel6.Margin = New Padding(11, 13, 11, 13)
        CustomPanel6.Name = "CustomPanel6"
        CustomPanel6.Padding = New Padding(11, 7, 11, 20)
        CustomPanel6.Size = New Size(606, 230)
        CustomPanel6.TabIndex = 1
        ' 
        ' tlpActions
        ' 
        tlpActions.ColumnCount = 2
        tlpActions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpActions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpActions.Controls.Add(btnAddClass, 0, 0)
        tlpActions.Controls.Add(btnEnterGrades, 1, 0)
        tlpActions.Controls.Add(btnViewRequests, 0, 1)
        tlpActions.Controls.Add(btnViewLogs, 1, 1)
        tlpActions.Dock = DockStyle.Fill
        tlpActions.Location = New Point(11, 71)
        tlpActions.Margin = New Padding(3, 4, 3, 4)
        tlpActions.Name = "tlpActions"
        tlpActions.Padding = New Padding(11, 13, 11, 13)
        tlpActions.RowCount = 2
        tlpActions.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        tlpActions.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        tlpActions.Size = New Size(584, 139)
        tlpActions.TabIndex = 5
        ' 
        ' btnAddClass
        ' 
        btnAddClass.BackColor = Color.AliceBlue
        btnAddClass.BorderColor = Color.DarkGreen
        btnAddClass.BorderSize = 5
        btnAddClass.CornerRadius = 10
        btnAddClass.Dock = DockStyle.Fill
        btnAddClass.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold)
        btnAddClass.ForeColor = Color.DarkGreen
        btnAddClass.HoverColor = Color.LightGray
        btnAddClass.Location = New Point(14, 17)
        btnAddClass.Margin = New Padding(3, 4, 3, 4)
        btnAddClass.Name = "btnAddClass"
        btnAddClass.NormalColor = Color.AliceBlue
        btnAddClass.Size = New Size(275, 48)
        btnAddClass.TabIndex = 6
        btnAddClass.Text = "Add Class/Subject"
        btnAddClass.UseVisualStyleBackColor = False
        ' 
        ' btnEnterGrades
        ' 
        btnEnterGrades.BackColor = Color.AliceBlue
        btnEnterGrades.BorderColor = Color.DarkGreen
        btnEnterGrades.BorderSize = 5
        btnEnterGrades.CornerRadius = 10
        btnEnterGrades.Dock = DockStyle.Fill
        btnEnterGrades.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnEnterGrades.ForeColor = Color.DarkGreen
        btnEnterGrades.HoverColor = Color.LightGray
        btnEnterGrades.Location = New Point(295, 17)
        btnEnterGrades.Margin = New Padding(3, 4, 3, 4)
        btnEnterGrades.Name = "btnEnterGrades"
        btnEnterGrades.NormalColor = Color.AliceBlue
        btnEnterGrades.Size = New Size(275, 48)
        btnEnterGrades.TabIndex = 7
        btnEnterGrades.Text = "Enter Grades"
        btnEnterGrades.UseVisualStyleBackColor = False
        ' 
        ' btnViewRequests
        ' 
        btnViewRequests.BackColor = Color.AliceBlue
        btnViewRequests.BorderColor = Color.DarkGreen
        btnViewRequests.BorderSize = 5
        btnViewRequests.CornerRadius = 10
        btnViewRequests.Dock = DockStyle.Fill
        btnViewRequests.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold)
        btnViewRequests.ForeColor = Color.DarkGreen
        btnViewRequests.HoverColor = Color.LightGray
        btnViewRequests.Location = New Point(14, 73)
        btnViewRequests.Margin = New Padding(3, 4, 3, 4)
        btnViewRequests.Name = "btnViewRequests"
        btnViewRequests.NormalColor = Color.AliceBlue
        btnViewRequests.RightToLeft = RightToLeft.Yes
        btnViewRequests.Size = New Size(275, 49)
        btnViewRequests.TabIndex = 8
        btnViewRequests.Text = "View Requests"
        btnViewRequests.UseVisualStyleBackColor = False
        ' 
        ' btnViewLogs
        ' 
        btnViewLogs.BackColor = Color.AliceBlue
        btnViewLogs.BorderColor = Color.DarkGreen
        btnViewLogs.BorderSize = 5
        btnViewLogs.CornerRadius = 10
        btnViewLogs.Dock = DockStyle.Fill
        btnViewLogs.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold)
        btnViewLogs.ForeColor = Color.DarkGreen
        btnViewLogs.HoverColor = Color.LightGray
        btnViewLogs.Image = My.Resources.Resources.image_removebg_preview__2_
        btnViewLogs.ImageAlign = ContentAlignment.MiddleLeft
        btnViewLogs.Location = New Point(295, 73)
        btnViewLogs.Margin = New Padding(3, 4, 3, 4)
        btnViewLogs.Name = "btnViewLogs"
        btnViewLogs.NormalColor = Color.AliceBlue
        btnViewLogs.Size = New Size(275, 49)
        btnViewLogs.TabIndex = 9
        btnViewLogs.Text = "View Audit Logs"
        btnViewLogs.TextImageRelation = TextImageRelation.ImageBeforeText
        btnViewLogs.UseVisualStyleBackColor = False
        ' 
        ' pnlactionsheader
        ' 
        pnlactionsheader.Controls.Add(PictureBox4)
        pnlactionsheader.Controls.Add(Label11)
        pnlactionsheader.Dock = DockStyle.Top
        pnlactionsheader.Location = New Point(11, 7)
        pnlactionsheader.Margin = New Padding(3, 4, 3, 4)
        pnlactionsheader.Name = "pnlactionsheader"
        pnlactionsheader.Size = New Size(584, 64)
        pnlactionsheader.TabIndex = 4
        ' 
        ' PictureBox4
        ' 
        PictureBox4.Image = CType(resources.GetObject("PictureBox4.Image"), Image)
        PictureBox4.Location = New Point(9, 8)
        PictureBox4.Margin = New Padding(3, 4, 3, 4)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New Size(51, 47)
        PictureBox4.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox4.TabIndex = 3
        PictureBox4.TabStop = False
        ' 
        ' Label11
        ' 
        Label11.AutoSize = True
        Label11.Font = New Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label11.ForeColor = Color.DarkGreen
        Label11.Location = New Point(64, 15)
        Label11.Name = "Label11"
        Label11.Size = New Size(172, 32)
        Label11.TabIndex = 2
        Label11.Text = "Quick Actions"
        ' 
        ' tlpdashstats
        ' 
        tlpdashstats.ColumnCount = 4
        tlpdashstats.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25F))
        tlpdashstats.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25F))
        tlpdashstats.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25F))
        tlpdashstats.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25F))
        tlpdashstats.Controls.Add(CustomPanel5, 3, 0)
        tlpdashstats.Controls.Add(CustomPanel4, 2, 0)
        tlpdashstats.Controls.Add(CustomPanel3, 1, 0)
        tlpdashstats.Controls.Add(CustomPanel2, 0, 0)
        tlpdashstats.Dock = DockStyle.Top
        tlpdashstats.Location = New Point(11, 88)
        tlpdashstats.Margin = New Padding(3, 4, 3, 4)
        tlpdashstats.Name = "tlpdashstats"
        tlpdashstats.RowCount = 1
        tlpdashstats.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpdashstats.Size = New Size(1256, 160)
        tlpdashstats.TabIndex = 1
        ' 
        ' CustomPanel5
        ' 
        CustomPanel5.BackColor = Color.AliceBlue
        CustomPanel5.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        CustomPanel5.Controls.Add(lblStatPending)
        CustomPanel5.Controls.Add(Label9)
        CustomPanel5.Dock = DockStyle.Fill
        CustomPanel5.Location = New Point(965, 13)
        CustomPanel5.Margin = New Padding(23, 13, 23, 13)
        CustomPanel5.Name = "CustomPanel5"
        CustomPanel5.Padding = New Padding(11, 13, 11, 13)
        CustomPanel5.Size = New Size(268, 134)
        CustomPanel5.TabIndex = 0
        ' 
        ' lblStatPending
        ' 
        lblStatPending.Dock = DockStyle.Top
        lblStatPending.FlatStyle = FlatStyle.Flat
        lblStatPending.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblStatPending.ForeColor = Color.DarkGreen
        lblStatPending.Location = New Point(11, 13)
        lblStatPending.Name = "lblStatPending"
        lblStatPending.Size = New Size(246, 72)
        lblStatPending.TabIndex = 5
        lblStatPending.Text = "Total Students"
        lblStatPending.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.FlatStyle = FlatStyle.Flat
        Label9.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label9.ForeColor = Color.DarkGreen
        Label9.Location = New Point(61, 93)
        Label9.Name = "Label9"
        Label9.Size = New Size(179, 28)
        Label9.TabIndex = 3
        Label9.Text = "Pending Requests"
        Label9.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' CustomPanel4
        ' 
        CustomPanel4.BackColor = Color.AliceBlue
        CustomPanel4.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        CustomPanel4.Controls.Add(lblStatClasses)
        CustomPanel4.Controls.Add(Label8)
        CustomPanel4.Dock = DockStyle.Fill
        CustomPanel4.Location = New Point(651, 13)
        CustomPanel4.Margin = New Padding(23, 13, 23, 13)
        CustomPanel4.Name = "CustomPanel4"
        CustomPanel4.Padding = New Padding(11, 13, 11, 13)
        CustomPanel4.Size = New Size(268, 134)
        CustomPanel4.TabIndex = 0
        ' 
        ' lblStatClasses
        ' 
        lblStatClasses.Dock = DockStyle.Top
        lblStatClasses.FlatStyle = FlatStyle.Flat
        lblStatClasses.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblStatClasses.ForeColor = Color.DarkGreen
        lblStatClasses.Location = New Point(11, 13)
        lblStatClasses.Name = "lblStatClasses"
        lblStatClasses.Size = New Size(246, 81)
        lblStatClasses.TabIndex = 4
        lblStatClasses.Text = "Total Students"
        lblStatClasses.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.FlatStyle = FlatStyle.Flat
        Label8.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label8.ForeColor = Color.DarkGreen
        Label8.Location = New Point(79, 93)
        Label8.Name = "Label8"
        Label8.Size = New Size(145, 28)
        Label8.TabIndex = 3
        Label8.Text = "Active Classes"
        Label8.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' CustomPanel3
        ' 
        CustomPanel3.BackColor = Color.AliceBlue
        CustomPanel3.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        CustomPanel3.Controls.Add(lblStatProfs)
        CustomPanel3.Controls.Add(Label7)
        CustomPanel3.Dock = DockStyle.Fill
        CustomPanel3.Location = New Point(337, 13)
        CustomPanel3.Margin = New Padding(23, 13, 23, 13)
        CustomPanel3.Name = "CustomPanel3"
        CustomPanel3.Padding = New Padding(11, 13, 11, 13)
        CustomPanel3.Size = New Size(268, 134)
        CustomPanel3.TabIndex = 0
        ' 
        ' lblStatProfs
        ' 
        lblStatProfs.Dock = DockStyle.Top
        lblStatProfs.FlatStyle = FlatStyle.Flat
        lblStatProfs.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblStatProfs.ForeColor = Color.DarkGreen
        lblStatProfs.Location = New Point(11, 13)
        lblStatProfs.Name = "lblStatProfs"
        lblStatProfs.Size = New Size(246, 79)
        lblStatProfs.TabIndex = 4
        lblStatProfs.Text = "Total Students"
        lblStatProfs.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.FlatStyle = FlatStyle.Flat
        Label7.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label7.ForeColor = Color.DarkGreen
        Label7.Location = New Point(90, 95)
        Label7.Name = "Label7"
        Label7.Size = New Size(110, 28)
        Label7.TabIndex = 3
        Label7.Text = "Professors"
        Label7.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' CustomPanel2
        ' 
        CustomPanel2.BackColor = Color.AliceBlue
        CustomPanel2.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        CustomPanel2.Controls.Add(lblStatStudents)
        CustomPanel2.Controls.Add(Label6)
        CustomPanel2.Dock = DockStyle.Fill
        CustomPanel2.Location = New Point(23, 13)
        CustomPanel2.Margin = New Padding(23, 13, 23, 13)
        CustomPanel2.Name = "CustomPanel2"
        CustomPanel2.Padding = New Padding(11, 13, 11, 13)
        CustomPanel2.Size = New Size(268, 134)
        CustomPanel2.TabIndex = 2
        ' 
        ' lblStatStudents
        ' 
        lblStatStudents.Dock = DockStyle.Top
        lblStatStudents.FlatStyle = FlatStyle.Flat
        lblStatStudents.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblStatStudents.ForeColor = Color.DarkGreen
        lblStatStudents.Location = New Point(11, 13)
        lblStatStudents.Name = "lblStatStudents"
        lblStatStudents.Size = New Size(246, 83)
        lblStatStudents.TabIndex = 3
        lblStatStudents.Text = "Total Students"
        lblStatStudents.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.FlatStyle = FlatStyle.Flat
        Label6.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label6.ForeColor = Color.DarkGreen
        Label6.Location = New Point(69, 93)
        Label6.Name = "Label6"
        Label6.Size = New Size(148, 28)
        Label6.TabIndex = 2
        Label6.Text = "Total Students"
        Label6.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' pnlDashHeader
        ' 
        pnlDashHeader.Controls.Add(CustomPanel1)
        pnlDashHeader.Controls.Add(Label2)
        pnlDashHeader.Controls.Add(PictureBox2)
        pnlDashHeader.Dock = DockStyle.Top
        pnlDashHeader.Location = New Point(11, 13)
        pnlDashHeader.Margin = New Padding(3, 4, 3, 4)
        pnlDashHeader.Name = "pnlDashHeader"
        pnlDashHeader.Size = New Size(1256, 75)
        pnlDashHeader.TabIndex = 0
        ' 
        ' CustomPanel1
        ' 
        CustomPanel1.BackColor = Color.DarkGreen
        CustomPanel1.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        CustomPanel1.Controls.Add(Label5)
        CustomPanel1.CornerRadius = 11
        CustomPanel1.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        CustomPanel1.ForeColor = Color.White
        CustomPanel1.Location = New Point(273, 17)
        CustomPanel1.Margin = New Padding(3, 4, 3, 4)
        CustomPanel1.Name = "CustomPanel1"
        CustomPanel1.RightToLeft = RightToLeft.Yes
        CustomPanel1.Size = New Size(109, 40)
        CustomPanel1.TabIndex = 2
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label5.ForeColor = Color.White
        Label5.Location = New Point(23, 7)
        Label5.Name = "Label5"
        Label5.Size = New Size(71, 25)
        Label5.TabIndex = 2
        Label5.Text = "Admin"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label2.ForeColor = Color.DarkGreen
        Label2.Location = New Point(78, 24)
        Label2.Name = "Label2"
        Label2.Size = New Size(173, 25)
        Label2.TabIndex = 1
        Label2.Text = "Admin Dashboard"
        ' 
        ' PictureBox2
        ' 
        PictureBox2.Image = CType(resources.GetObject("PictureBox2.Image"), Image)
        PictureBox2.Location = New Point(8, 13)
        PictureBox2.Margin = New Padding(3, 4, 3, 4)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(62, 47)
        PictureBox2.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox2.TabIndex = 0
        PictureBox2.TabStop = False
        ' 
        ' pnlStudents
        ' 
        pnlStudents.BackColor = Color.WhiteSmoke
        pnlStudents.Controls.Add(pnlstudentcard)
        pnlStudents.Dock = DockStyle.Fill
        pnlStudents.Location = New Point(0, 0)
        pnlStudents.Name = "pnlStudents"
        pnlStudents.Padding = New Padding(23, 27, 23, 16)
        pnlStudents.Size = New Size(1370, 610)
        pnlStudents.TabIndex = 1
        ' 
        ' pnlstudentcard
        ' 
        pnlstudentcard.BackColor = Color.White
        pnlstudentcard.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        pnlstudentcard.Controls.Add(dgvstudents)
        pnlstudentcard.Controls.Add(Panel2)
        pnlstudentcard.Controls.Add(flowstudentpager)
        pnlstudentcard.Dock = DockStyle.Fill
        pnlstudentcard.Location = New Point(23, 27)
        pnlstudentcard.Margin = New Padding(3, 4, 3, 4)
        pnlstudentcard.Name = "pnlstudentcard"
        pnlstudentcard.Padding = New Padding(16, 7, 16, 7)
        pnlstudentcard.Size = New Size(1324, 567)
        pnlstudentcard.TabIndex = 0
        ' 
        ' dgvstudents
        ' 
        dgvstudents.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvstudents.Dock = DockStyle.Fill
        dgvstudents.Location = New Point(16, 75)
        dgvstudents.Name = "dgvstudents"
        dgvstudents.RowHeadersWidth = 51
        dgvstudents.Size = New Size(1292, 442)
        dgvstudents.TabIndex = 4
        ' 
        ' Panel2
        ' 
        Panel2.Controls.Add(lbltotalstudents)
        Panel2.Controls.Add(btnaddstudent)
        Panel2.Controls.Add(btnStudentReset)
        Panel2.Controls.Add(btnStudentFilter)
        Panel2.Controls.Add(Label4)
        Panel2.Controls.Add(Label3)
        Panel2.Controls.Add(cboSection)
        Panel2.Controls.Add(cboYearLevel)
        Panel2.Controls.Add(txtStudentSearch)
        Panel2.Dock = DockStyle.Top
        Panel2.Location = New Point(16, 7)
        Panel2.Margin = New Padding(3, 4, 3, 4)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(1292, 68)
        Panel2.TabIndex = 5
        ' 
        ' lbltotalstudents
        ' 
        lbltotalstudents.AutoSize = True
        lbltotalstudents.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lbltotalstudents.ForeColor = Color.DarkGreen
        lbltotalstudents.Location = New Point(15, 45)
        lbltotalstudents.Name = "lbltotalstudents"
        lbltotalstudents.Size = New Size(81, 20)
        lbltotalstudents.TabIndex = 8
        lbltotalstudents.Text = "studentno"
        ' 
        ' btnaddstudent
        ' 
        btnaddstudent.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnaddstudent.BackColor = Color.DarkGreen
        btnaddstudent.BorderColor = Color.DarkGreen
        btnaddstudent.BorderSize = 3
        btnaddstudent.CornerRadius = 10
        btnaddstudent.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnaddstudent.ForeColor = Color.White
        btnaddstudent.HoverColor = Color.LightGray
        btnaddstudent.Location = New Point(1170, 7)
        btnaddstudent.Margin = New Padding(3, 4, 3, 4)
        btnaddstudent.Name = "btnaddstudent"
        btnaddstudent.NormalColor = Color.DarkGreen
        btnaddstudent.Size = New Size(110, 44)
        btnaddstudent.TabIndex = 7
        btnaddstudent.Text = "+ Add Students"
        btnaddstudent.UseVisualStyleBackColor = False
        ' 
        ' btnStudentReset
        ' 
        btnStudentReset.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnStudentReset.BackColor = Color.White
        btnStudentReset.BorderColor = Color.DarkGreen
        btnStudentReset.BorderSize = 2
        btnStudentReset.CornerRadius = 10
        btnStudentReset.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnStudentReset.ForeColor = Color.DarkGreen
        btnStudentReset.HoverColor = Color.LightGray
        btnStudentReset.Location = New Point(1065, 13)
        btnStudentReset.Margin = New Padding(3, 4, 3, 4)
        btnStudentReset.Name = "btnStudentReset"
        btnStudentReset.NormalColor = Color.White
        btnStudentReset.Size = New Size(86, 31)
        btnStudentReset.TabIndex = 6
        btnStudentReset.Text = "Reset"
        btnStudentReset.UseVisualStyleBackColor = False
        ' 
        ' btnStudentFilter
        ' 
        btnStudentFilter.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnStudentFilter.BackColor = Color.White
        btnStudentFilter.BorderColor = Color.DarkGreen
        btnStudentFilter.BorderSize = 2
        btnStudentFilter.CornerRadius = 10
        btnStudentFilter.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnStudentFilter.ForeColor = Color.DarkGreen
        btnStudentFilter.HoverColor = Color.LightGray
        btnStudentFilter.Location = New Point(965, 15)
        btnStudentFilter.Margin = New Padding(3, 4, 3, 4)
        btnStudentFilter.Name = "btnStudentFilter"
        btnStudentFilter.NormalColor = Color.White
        btnStudentFilter.Size = New Size(86, 31)
        btnStudentFilter.TabIndex = 5
        btnStudentFilter.Text = "Filter"
        btnStudentFilter.UseVisualStyleBackColor = False
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Font = New Font("Segoe UI", 8F)
        Label4.ForeColor = Color.DarkGreen
        Label4.Location = New Point(383, -4)
        Label4.Name = "Label4"
        Label4.Size = New Size(53, 19)
        Label4.TabIndex = 4
        Label4.Text = "Section"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label3.ForeColor = Color.DarkGreen
        Label3.Location = New Point(230, -4)
        Label3.Name = "Label3"
        Label3.Size = New Size(66, 19)
        Label3.TabIndex = 3
        Label3.Text = "Year level"
        ' 
        ' cboSection
        ' 
        cboSection.DropDownStyle = ComboBoxStyle.DropDownList
        cboSection.FormattingEnabled = True
        cboSection.Location = New Point(379, 15)
        cboSection.Margin = New Padding(3, 4, 3, 4)
        cboSection.Name = "cboSection"
        cboSection.Size = New Size(138, 28)
        cboSection.TabIndex = 2
        ' 
        ' cboYearLevel
        ' 
        cboYearLevel.DropDownStyle = ComboBoxStyle.DropDownList
        cboYearLevel.FormattingEnabled = True
        cboYearLevel.Location = New Point(233, 15)
        cboYearLevel.Margin = New Padding(3, 4, 3, 4)
        cboYearLevel.Name = "cboYearLevel"
        cboYearLevel.Size = New Size(138, 28)
        cboYearLevel.TabIndex = 1
        ' 
        ' txtStudentSearch
        ' 
        txtStudentSearch.Location = New Point(16, 15)
        txtStudentSearch.Margin = New Padding(3, 4, 3, 4)
        txtStudentSearch.Name = "txtStudentSearch"
        txtStudentSearch.Size = New Size(210, 27)
        txtStudentSearch.TabIndex = 0
        ' 
        ' flowstudentpager
        ' 
        flowstudentpager.Dock = DockStyle.Bottom
        flowstudentpager.FlowDirection = FlowDirection.RightToLeft
        flowstudentpager.Location = New Point(16, 517)
        flowstudentpager.Margin = New Padding(3, 4, 3, 4)
        flowstudentpager.Name = "flowstudentpager"
        flowstudentpager.Size = New Size(1292, 43)
        flowstudentpager.TabIndex = 8
        ' 
        ' Admin
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1370, 749)
        Controls.Add(pnlContent)
        Controls.Add(pnlFooter)
        Controls.Add(pnlTabs)
        Controls.Add(pnlUser)
        Controls.Add(pnlTitle)
        Name = "Admin"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Admin"
        WindowState = FormWindowState.Maximized
        pnlTitle.ResumeLayout(False)
        pnlTitle.PerformLayout()
        pnlUser.ResumeLayout(False)
        pnlUser.PerformLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        pnlTabs.ResumeLayout(False)
        pnlContent.ResumeLayout(False)
        pnlProfessors.ResumeLayout(False)
        pnlProfCard.ResumeLayout(False)
        pnlProfCard.PerformLayout()
        CType(dgvProf, ComponentModel.ISupportInitialize).EndInit()
        pnlProfPager.ResumeLayout(False)
        pnlProfTop.ResumeLayout(False)
        pnlProfTop.PerformLayout()
        pnlDashboard.ResumeLayout(False)
        cardDashboard.ResumeLayout(False)
        tlpDashBottom.ResumeLayout(False)
        pnlupdatesheader.ResumeLayout(False)
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        CType(dgvupdates, ComponentModel.ISupportInitialize).EndInit()
        CustomPanel6.ResumeLayout(False)
        tlpActions.ResumeLayout(False)
        pnlactionsheader.ResumeLayout(False)
        pnlactionsheader.PerformLayout()
        CType(PictureBox4, ComponentModel.ISupportInitialize).EndInit()
        tlpdashstats.ResumeLayout(False)
        CustomPanel5.ResumeLayout(False)
        CustomPanel5.PerformLayout()
        CustomPanel4.ResumeLayout(False)
        CustomPanel4.PerformLayout()
        CustomPanel3.ResumeLayout(False)
        CustomPanel3.PerformLayout()
        CustomPanel2.ResumeLayout(False)
        CustomPanel2.PerformLayout()
        pnlDashHeader.ResumeLayout(False)
        pnlDashHeader.PerformLayout()
        CustomPanel1.ResumeLayout(False)
        CustomPanel1.PerformLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        pnlStudents.ResumeLayout(False)
        pnlstudentcard.ResumeLayout(False)
        CType(dgvstudents, ComponentModel.ISupportInitialize).EndInit()
        Panel2.ResumeLayout(False)
        Panel2.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlTitle As Panel
    Friend WithEvents Label1 As Label
    Friend WithEvents pnlUser As Panel
    Friend WithEvents pnlTabs As Panel
    Friend WithEvents pnlFooter As Panel
    Friend WithEvents pnlContent As Panel
    Friend WithEvents btnLogout As CustomButton
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents lblRole As PillLabel
    Friend WithEvents lblAdminName As Label
    Friend WithEvents btnProfessors As CustomButton
    Friend WithEvents btnStudents As CustomButton
    Friend WithEvents btnDashboard As CustomButton
    Friend WithEvents pnlDashboard As Panel
    Friend WithEvents pnlStudents As Panel
    Friend WithEvents pnlProfessors As Panel
    Friend WithEvents pnlProfCard As Panel
    Friend WithEvents pnlProfTop As Panel
    Friend WithEvents txtSearchProf As TextBox
    Friend WithEvents btnAddProf As CustomButton
    Friend WithEvents cboDeptProf As ComboBox
    Friend WithEvents lblTotalProf As Label
    Friend WithEvents pnlProfPager As Panel
    Friend WithEvents dgvProf As DataGridView
    Friend WithEvents flowProfPager As FlowLayoutPanel
    Friend WithEvents pnlstudentcard As CustomPanel
    Friend WithEvents dgvstudents As DataGridView
    Friend WithEvents Panel2 As Panel
    Friend WithEvents txtStudentSearch As TextBox
    Friend WithEvents cboSection As ComboBox
    Friend WithEvents cboYearLevel As ComboBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents btnaddstudent As CustomButton
    Friend WithEvents btnStudentReset As CustomButton
    Friend WithEvents btnStudentFilter As CustomButton
    Friend WithEvents flowstudentpager As FlowLayoutPanel
    Friend WithEvents lbltotalstudents As Label
    Friend WithEvents cardDashboard As CustomPanel
    Friend WithEvents pnlDashHeader As Panel
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents tlpdashstats As TableLayoutPanel
    Friend WithEvents CustomPanel1 As CustomPanel
    Friend WithEvents Label5 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents CustomPanel5 As CustomPanel
    Friend WithEvents CustomPanel4 As CustomPanel
    Friend WithEvents CustomPanel3 As CustomPanel
    Friend WithEvents CustomPanel2 As CustomPanel
    Friend WithEvents Label6 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents lblStatStudents As Label
    Friend WithEvents lblStatPending As Label
    Friend WithEvents lblStatClasses As Label
    Friend WithEvents lblStatProfs As Label
    Friend WithEvents tlpDashBottom As TableLayoutPanel
    Friend WithEvents pnlupdatesheader As CustomPanel
    Friend WithEvents btnupdatesfilter As Button
    Friend WithEvents dgvupdates As DataGridView
    Friend WithEvents Panel1 As Panel
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents Label10 As Label
    Friend WithEvents CustomPanel6 As CustomPanel
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents Label11 As Label
    Friend WithEvents pnlactionsheader As Panel
    Friend WithEvents tlpActions As TableLayoutPanel
    Friend WithEvents btnAddClass As CustomButton
    Friend WithEvents btnEnterGrades As CustomButton
    Friend WithEvents btnViewRequests As CustomButton
    Friend WithEvents btnViewLogs As CustomButton
    Friend WithEvents Panel3 As Panel
    Friend WithEvents CustomPanel7 As CustomPanel
End Class
