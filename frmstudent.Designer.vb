<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmstudent
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmstudent))
        Pnlheaderheader = New Panel()
        Label3 = New Label()
        Pnlfooter = New Panel()
        Pnltabs = New Panel()
        Panel5 = New Panel()
        btnMyPerformance = New Button()
        btnRequestRevision = New Button()
        btnGradeHistory = New Button()
        btnMyGrades = New Button()
        pnlContent = New Panel()
        pnlRequestRevision = New Panel()
        tlprevision = New TableLayoutPanel()
        cardSubmit = New CustomPanel()
        btnSubmitRequest = New Button()
        txtReason = New TextBox()
        Label9 = New Label()
        txtCurrentGrade = New TextBox()
        Label8 = New Label()
        Label7 = New Label()
        cboAssessment = New ComboBox()
        Panel2 = New Panel()
        pnlSubmitHeader = New Panel()
        PictureBox4 = New PictureBox()
        Label6 = New Label()
        cardStatus = New CustomPanel()
        dgvRequests = New DataGridView()
        pnlStatusHeader = New Panel()
        PictureBox5 = New PictureBox()
        lblStatusTitle = New Label()
        pnlclassbar = New Panel()
        lblClass = New Label()
        cboclass = New ComboBox()
        pnlMyPerformance = New Panel()
        cardPerformance = New CustomPanel()
        tlpbars = New TableLayoutPanel()
        trackProject = New CustomPanel()
        fillproject = New CustomPanel()
        lblprojectpct = New Label()
        trackExam = New CustomPanel()
        fillexam = New CustomPanel()
        lblexampct = New Label()
        trackActivity = New CustomPanel()
        fillactivity = New CustomPanel()
        lblactivitypct = New Label()
        trackQuiz = New CustomPanel()
        fillQuiz = New CustomPanel()
        lblQuizPct = New Label()
        Label12 = New Label()
        Label13 = New Label()
        Label14 = New Label()
        Label11 = New Label()
        tlpstats = New TableLayoutPanel()
        RoundedPanel6 = New CustomPanel()
        lblAverageCaption = New Label()
        lblAverageValue = New Label()
        RoundedPanel8 = New CustomPanel()
        lblPassedCaption = New Label()
        lblPassedValue = New Label()
        RoundedPanel7 = New CustomPanel()
        lblPendingCaption = New Label()
        lblPendingValue = New Label()
        RoundedPanel9 = New CustomPanel()
        lblHighestCaption = New Label()
        lblHighestValue = New Label()
        pnlperfheader = New Panel()
        cboperfterm = New ComboBox()
        cboperfcourse = New ComboBox()
        Label10 = New Label()
        PictureBox6 = New PictureBox()
        Panel1 = New Panel()
        pnlMyGrades = New Panel()
        Label1 = New Label()
        RoundedPanel1 = New CustomPanel()
        lblstanding = New Label()
        dgvgrades = New DataGridView()
        Panel3 = New Panel()
        Panel4 = New Panel()
        cboterm = New ComboBox()
        PictureBox2 = New PictureBox()
        cboCourse = New ComboBox()
        pnlGradeHistory = New Panel()
        RoundedPanel2 = New CustomPanel()
        dgvHistory = New DataGridView()
        Label5 = New Label()
        pnlHistoryHeader = New Panel()
        PictureBox3 = New PictureBox()
        Label4 = New Label()
        Panel7 = New Panel()
        pnlheader = New Panel()
        btnChangePassword = New Button()
        Panel6 = New Panel()
        lblrole = New Label()
        lblstudentname = New Label()
        PictureBox1 = New PictureBox()
        btnLogout = New Button()
        BackgroundWorker1 = New ComponentModel.BackgroundWorker()
        Pnlheaderheader.SuspendLayout()
        Pnltabs.SuspendLayout()
        pnlContent.SuspendLayout()
        pnlRequestRevision.SuspendLayout()
        tlprevision.SuspendLayout()
        cardSubmit.SuspendLayout()
        pnlSubmitHeader.SuspendLayout()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        cardStatus.SuspendLayout()
        CType(dgvRequests, ComponentModel.ISupportInitialize).BeginInit()
        pnlStatusHeader.SuspendLayout()
        CType(PictureBox5, ComponentModel.ISupportInitialize).BeginInit()
        pnlclassbar.SuspendLayout()
        pnlMyPerformance.SuspendLayout()
        cardPerformance.SuspendLayout()
        tlpbars.SuspendLayout()
        trackProject.SuspendLayout()
        fillproject.SuspendLayout()
        trackExam.SuspendLayout()
        fillexam.SuspendLayout()
        trackActivity.SuspendLayout()
        fillactivity.SuspendLayout()
        trackQuiz.SuspendLayout()
        fillQuiz.SuspendLayout()
        tlpstats.SuspendLayout()
        RoundedPanel6.SuspendLayout()
        RoundedPanel8.SuspendLayout()
        RoundedPanel7.SuspendLayout()
        RoundedPanel9.SuspendLayout()
        pnlperfheader.SuspendLayout()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        pnlMyGrades.SuspendLayout()
        RoundedPanel1.SuspendLayout()
        CType(dgvgrades, ComponentModel.ISupportInitialize).BeginInit()
        Panel4.SuspendLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        pnlGradeHistory.SuspendLayout()
        RoundedPanel2.SuspendLayout()
        CType(dgvHistory, ComponentModel.ISupportInitialize).BeginInit()
        pnlHistoryHeader.SuspendLayout()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        pnlheader.SuspendLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Pnlheaderheader
        ' 
        Pnlheaderheader.BackColor = Color.DarkGreen
        Pnlheaderheader.Controls.Add(Label3)
        Pnlheaderheader.Dock = DockStyle.Top
        Pnlheaderheader.Location = New Point(0, 0)
        Pnlheaderheader.Margin = New Padding(3, 4, 3, 4)
        Pnlheaderheader.Name = "Pnlheaderheader"
        Pnlheaderheader.Size = New Size(1370, 37)
        Pnlheaderheader.TabIndex = 0
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label3.ForeColor = Color.White
        Label3.Location = New Point(8, 9)
        Label3.Name = "Label3"
        Label3.Size = New Size(252, 20)
        Label3.TabIndex = 0
        Label3.Text = "Dauntless Wolves' Grading System"
        ' 
        ' Pnlfooter
        ' 
        Pnlfooter.BackColor = Color.DarkGreen
        Pnlfooter.Dock = DockStyle.Bottom
        Pnlfooter.Location = New Point(0, 713)
        Pnlfooter.Margin = New Padding(3, 4, 3, 4)
        Pnlfooter.Name = "Pnlfooter"
        Pnlfooter.Size = New Size(1370, 36)
        Pnlfooter.TabIndex = 1
        ' 
        ' Pnltabs
        ' 
        Pnltabs.BackColor = Color.AliceBlue
        Pnltabs.Controls.Add(Panel5)
        Pnltabs.Controls.Add(btnMyPerformance)
        Pnltabs.Controls.Add(btnRequestRevision)
        Pnltabs.Controls.Add(btnGradeHistory)
        Pnltabs.Controls.Add(btnMyGrades)
        Pnltabs.Dock = DockStyle.Top
        Pnltabs.Location = New Point(0, 85)
        Pnltabs.Margin = New Padding(3, 4, 3, 4)
        Pnltabs.Name = "Pnltabs"
        Pnltabs.Size = New Size(1370, 67)
        Pnltabs.TabIndex = 4
        ' 
        ' Panel5
        ' 
        Panel5.BackColor = Color.DimGray
        Panel5.Dock = DockStyle.Top
        Panel5.Location = New Point(0, 0)
        Panel5.Margin = New Padding(3, 4, 3, 4)
        Panel5.Name = "Panel5"
        Panel5.Size = New Size(1370, 3)
        Panel5.TabIndex = 7
        ' 
        ' btnMyPerformance
        ' 
        btnMyPerformance.BackColor = Color.Gainsboro
        btnMyPerformance.FlatStyle = FlatStyle.Flat
        btnMyPerformance.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnMyPerformance.ForeColor = Color.DarkGreen
        btnMyPerformance.ImageAlign = ContentAlignment.MiddleLeft
        btnMyPerformance.Location = New Point(477, 19)
        btnMyPerformance.Margin = New Padding(3, 4, 3, 4)
        btnMyPerformance.Name = "btnMyPerformance"
        btnMyPerformance.Size = New Size(165, 47)
        btnMyPerformance.TabIndex = 6
        btnMyPerformance.Text = "My Performance"
        btnMyPerformance.TextImageRelation = TextImageRelation.ImageBeforeText
        btnMyPerformance.UseVisualStyleBackColor = False
        ' 
        ' btnRequestRevision
        ' 
        btnRequestRevision.BackColor = Color.Gainsboro
        btnRequestRevision.FlatStyle = FlatStyle.Flat
        btnRequestRevision.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnRequestRevision.ForeColor = Color.DarkGreen
        btnRequestRevision.ImageAlign = ContentAlignment.MiddleLeft
        btnRequestRevision.Location = New Point(296, 19)
        btnRequestRevision.Margin = New Padding(3, 4, 3, 4)
        btnRequestRevision.Name = "btnRequestRevision"
        btnRequestRevision.Size = New Size(166, 47)
        btnRequestRevision.TabIndex = 6
        btnRequestRevision.Text = "Request Clarification"
        btnRequestRevision.TextImageRelation = TextImageRelation.ImageBeforeText
        btnRequestRevision.UseVisualStyleBackColor = False
        ' 
        ' btnGradeHistory
        ' 
        btnGradeHistory.BackColor = Color.Gainsboro
        btnGradeHistory.FlatStyle = FlatStyle.Flat
        btnGradeHistory.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnGradeHistory.ForeColor = Color.DarkGreen
        btnGradeHistory.ImageAlign = ContentAlignment.MiddleLeft
        btnGradeHistory.Location = New Point(155, 19)
        btnGradeHistory.Margin = New Padding(3, 4, 3, 4)
        btnGradeHistory.Name = "btnGradeHistory"
        btnGradeHistory.Size = New Size(129, 47)
        btnGradeHistory.TabIndex = 6
        btnGradeHistory.Text = "Grade History"
        btnGradeHistory.TextImageRelation = TextImageRelation.ImageBeforeText
        btnGradeHistory.UseVisualStyleBackColor = False
        ' 
        ' btnMyGrades
        ' 
        btnMyGrades.AutoSizeMode = AutoSizeMode.GrowAndShrink
        btnMyGrades.BackColor = Color.Gainsboro
        btnMyGrades.BackgroundImageLayout = ImageLayout.Zoom
        btnMyGrades.FlatStyle = FlatStyle.Flat
        btnMyGrades.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnMyGrades.ForeColor = Color.DarkGreen
        btnMyGrades.ImageAlign = ContentAlignment.MiddleLeft
        btnMyGrades.Location = New Point(16, 19)
        btnMyGrades.Margin = New Padding(3, 4, 3, 4)
        btnMyGrades.Name = "btnMyGrades"
        btnMyGrades.Size = New Size(129, 47)
        btnMyGrades.TabIndex = 5
        btnMyGrades.Text = "My Grades"
        btnMyGrades.TextImageRelation = TextImageRelation.ImageBeforeText
        btnMyGrades.UseVisualStyleBackColor = False
        ' 
        ' pnlContent
        ' 
        pnlContent.BackColor = Color.White
        pnlContent.Controls.Add(pnlRequestRevision)
        pnlContent.Controls.Add(pnlMyPerformance)
        pnlContent.Controls.Add(pnlMyGrades)
        pnlContent.Controls.Add(pnlGradeHistory)
        pnlContent.Controls.Add(Panel7)
        pnlContent.Dock = DockStyle.Fill
        pnlContent.Location = New Point(0, 152)
        pnlContent.Margin = New Padding(3, 4, 3, 4)
        pnlContent.Name = "pnlContent"
        pnlContent.Size = New Size(1370, 561)
        pnlContent.TabIndex = 7
        ' 
        ' pnlRequestRevision
        ' 
        pnlRequestRevision.BackColor = Color.WhiteSmoke
        pnlRequestRevision.Controls.Add(tlprevision)
        pnlRequestRevision.Controls.Add(pnlclassbar)
        pnlRequestRevision.Dock = DockStyle.Fill
        pnlRequestRevision.Location = New Point(0, 4)
        pnlRequestRevision.Margin = New Padding(3, 4, 3, 4)
        pnlRequestRevision.Name = "pnlRequestRevision"
        pnlRequestRevision.Padding = New Padding(34, 20, 34, 40)
        pnlRequestRevision.Size = New Size(1370, 557)
        pnlRequestRevision.TabIndex = 10
        pnlRequestRevision.Visible = False
        ' 
        ' tlprevision
        ' 
        tlprevision.BackColor = Color.WhiteSmoke
        tlprevision.ColumnCount = 2
        tlprevision.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlprevision.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlprevision.Controls.Add(cardSubmit, 0, 0)
        tlprevision.Controls.Add(cardStatus, 1, 0)
        tlprevision.Dock = DockStyle.Fill
        tlprevision.Location = New Point(34, 53)
        tlprevision.Margin = New Padding(3, 4, 3, 4)
        tlprevision.Name = "tlprevision"
        tlprevision.RowCount = 1
        tlprevision.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlprevision.RowStyles.Add(New RowStyle(SizeType.Absolute, 464F))
        tlprevision.Size = New Size(1302, 464)
        tlprevision.TabIndex = 1
        ' 
        ' cardSubmit
        ' 
        cardSubmit.BackColor = Color.White
        cardSubmit.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        cardSubmit.Controls.Add(btnSubmitRequest)
        cardSubmit.Controls.Add(txtReason)
        cardSubmit.Controls.Add(Label9)
        cardSubmit.Controls.Add(txtCurrentGrade)
        cardSubmit.Controls.Add(Label8)
        cardSubmit.Controls.Add(Label7)
        cardSubmit.Controls.Add(cboAssessment)
        cardSubmit.Controls.Add(Panel2)
        cardSubmit.Controls.Add(pnlSubmitHeader)
        cardSubmit.Dock = DockStyle.Fill
        cardSubmit.Location = New Point(11, 13)
        cardSubmit.Margin = New Padding(11, 13, 11, 13)
        cardSubmit.Name = "cardSubmit"
        cardSubmit.Padding = New Padding(16, 19, 16, 19)
        cardSubmit.Size = New Size(629, 438)
        cardSubmit.TabIndex = 0
        ' 
        ' btnSubmitRequest
        ' 
        btnSubmitRequest.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnSubmitRequest.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnSubmitRequest.ForeColor = Color.DarkGreen
        btnSubmitRequest.ImageAlign = ContentAlignment.MiddleLeft
        btnSubmitRequest.Location = New Point(75, 535)
        btnSubmitRequest.Margin = New Padding(3, 4, 3, 4)
        btnSubmitRequest.Name = "btnSubmitRequest"
        btnSubmitRequest.Size = New Size(458, 43)
        btnSubmitRequest.TabIndex = 19
        btnSubmitRequest.Text = "Submit Request"
        btnSubmitRequest.UseVisualStyleBackColor = True
        ' 
        ' txtReason
        ' 
        txtReason.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtReason.BackColor = Color.WhiteSmoke
        txtReason.Location = New Point(41, 376)
        txtReason.Margin = New Padding(3, 4, 3, 4)
        txtReason.Multiline = True
        txtReason.Name = "txtReason"
        txtReason.Size = New Size(529, 139)
        txtReason.TabIndex = 18
        ' 
        ' Label9
        ' 
        Label9.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label9.AutoSize = True
        Label9.BackColor = Color.White
        Label9.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        Label9.ForeColor = Color.DarkGreen
        Label9.Location = New Point(41, 348)
        Label9.Name = "Label9"
        Label9.Size = New Size(164, 23)
        Label9.TabIndex = 17
        Label9.Text = "Reason for Request"
        ' 
        ' txtCurrentGrade
        ' 
        txtCurrentGrade.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtCurrentGrade.BackColor = Color.WhiteSmoke
        txtCurrentGrade.Location = New Point(42, 273)
        txtCurrentGrade.Margin = New Padding(3, 4, 3, 4)
        txtCurrentGrade.Name = "txtCurrentGrade"
        txtCurrentGrade.ReadOnly = True
        txtCurrentGrade.Size = New Size(529, 27)
        txtCurrentGrade.TabIndex = 16
        ' 
        ' Label8
        ' 
        Label8.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label8.AutoSize = True
        Label8.BackColor = Color.White
        Label8.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        Label8.ForeColor = Color.DarkGreen
        Label8.Location = New Point(41, 247)
        Label8.Name = "Label8"
        Label8.Size = New Size(124, 23)
        Label8.TabIndex = 15
        Label8.Text = "Current Grade"
        ' 
        ' Label7
        ' 
        Label7.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Label7.AutoSize = True
        Label7.BackColor = Color.White
        Label7.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        Label7.ForeColor = Color.DarkGreen
        Label7.Location = New Point(40, 135)
        Label7.Name = "Label7"
        Label7.Size = New Size(101, 23)
        Label7.TabIndex = 13
        Label7.Text = "Assessment"
        ' 
        ' cboAssessment
        ' 
        cboAssessment.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cboAssessment.BackColor = Color.WhiteSmoke
        cboAssessment.DropDownStyle = ComboBoxStyle.DropDownList
        cboAssessment.FormattingEnabled = True
        cboAssessment.ItemHeight = 20
        cboAssessment.Location = New Point(42, 160)
        cboAssessment.Margin = New Padding(3, 4, 3, 4)
        cboAssessment.MaxDropDownItems = 10
        cboAssessment.Name = "cboAssessment"
        cboAssessment.Size = New Size(529, 28)
        cboAssessment.TabIndex = 12
        ' 
        ' Panel2
        ' 
        Panel2.BackColor = Color.DimGray
        Panel2.Dock = DockStyle.Top
        Panel2.Location = New Point(16, 107)
        Panel2.Margin = New Padding(3, 4, 3, 4)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(597, 3)
        Panel2.TabIndex = 20
        ' 
        ' pnlSubmitHeader
        ' 
        pnlSubmitHeader.Controls.Add(PictureBox4)
        pnlSubmitHeader.Controls.Add(Label6)
        pnlSubmitHeader.Dock = DockStyle.Top
        pnlSubmitHeader.Location = New Point(16, 19)
        pnlSubmitHeader.Margin = New Padding(3, 4, 3, 4)
        pnlSubmitHeader.Name = "pnlSubmitHeader"
        pnlSubmitHeader.Size = New Size(597, 88)
        pnlSubmitHeader.TabIndex = 21
        ' 
        ' PictureBox4
        ' 
        PictureBox4.Image = CType(resources.GetObject("PictureBox4.Image"), Image)
        PictureBox4.Location = New Point(10, 7)
        PictureBox4.Margin = New Padding(3, 4, 3, 4)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New Size(62, 76)
        PictureBox4.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox4.TabIndex = 11
        PictureBox4.TabStop = False
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.BackColor = Color.White
        Label6.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label6.ForeColor = Color.DarkGreen
        Label6.Location = New Point(74, 23)
        Label6.Name = "Label6"
        Label6.Size = New Size(383, 37)
        Label6.TabIndex = 10
        Label6.Text = "Submit Clarification Request"
        ' 
        ' cardStatus
        ' 
        cardStatus.BackColor = Color.White
        cardStatus.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        cardStatus.Controls.Add(dgvRequests)
        cardStatus.Controls.Add(pnlStatusHeader)
        cardStatus.Dock = DockStyle.Fill
        cardStatus.Location = New Point(662, 13)
        cardStatus.Margin = New Padding(11, 13, 11, 13)
        cardStatus.Name = "cardStatus"
        cardStatus.Padding = New Padding(16, 19, 16, 19)
        cardStatus.Size = New Size(629, 438)
        cardStatus.TabIndex = 0
        ' 
        ' dgvRequests
        ' 
        dgvRequests.BackgroundColor = Color.White
        dgvRequests.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvRequests.Dock = DockStyle.Fill
        dgvRequests.Location = New Point(16, 100)
        dgvRequests.Margin = New Padding(3, 4, 3, 4)
        dgvRequests.Name = "dgvRequests"
        dgvRequests.RowHeadersWidth = 51
        dgvRequests.Size = New Size(597, 319)
        dgvRequests.TabIndex = 1
        ' 
        ' pnlStatusHeader
        ' 
        pnlStatusHeader.Controls.Add(PictureBox5)
        pnlStatusHeader.Controls.Add(lblStatusTitle)
        pnlStatusHeader.Dock = DockStyle.Top
        pnlStatusHeader.Location = New Point(16, 19)
        pnlStatusHeader.Margin = New Padding(3, 4, 3, 4)
        pnlStatusHeader.Name = "pnlStatusHeader"
        pnlStatusHeader.Size = New Size(597, 81)
        pnlStatusHeader.TabIndex = 13
        ' 
        ' PictureBox5
        ' 
        PictureBox5.Image = CType(resources.GetObject("PictureBox5.Image"), Image)
        PictureBox5.Location = New Point(5, 5)
        PictureBox5.Margin = New Padding(3, 4, 3, 4)
        PictureBox5.Name = "PictureBox5"
        PictureBox5.Size = New Size(72, 69)
        PictureBox5.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox5.TabIndex = 12
        PictureBox5.TabStop = False
        ' 
        ' lblStatusTitle
        ' 
        lblStatusTitle.AutoSize = True
        lblStatusTitle.BackColor = Color.White
        lblStatusTitle.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblStatusTitle.ForeColor = Color.DarkGreen
        lblStatusTitle.Location = New Point(75, 20)
        lblStatusTitle.Name = "lblStatusTitle"
        lblStatusTitle.Size = New Size(254, 37)
        lblStatusTitle.TabIndex = 11
        lblStatusTitle.Text = "My Request Status"
        ' 
        ' pnlclassbar
        ' 
        pnlclassbar.Controls.Add(lblClass)
        pnlclassbar.Controls.Add(cboclass)
        pnlclassbar.Dock = DockStyle.Top
        pnlclassbar.Location = New Point(34, 20)
        pnlclassbar.Margin = New Padding(3, 4, 3, 4)
        pnlclassbar.Name = "pnlclassbar"
        pnlclassbar.Size = New Size(1302, 33)
        pnlclassbar.TabIndex = 22
        ' 
        ' lblClass
        ' 
        lblClass.AutoSize = True
        lblClass.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblClass.ForeColor = Color.DarkGreen
        lblClass.Location = New Point(16, 3)
        lblClass.Name = "lblClass"
        lblClass.Size = New Size(74, 25)
        lblClass.TabIndex = 1
        lblClass.Text = "Course"
        ' 
        ' cboclass
        ' 
        cboclass.BackColor = Color.White
        cboclass.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        cboclass.ForeColor = Color.FromArgb(CByte(0), CByte(64), CByte(0))
        cboclass.FormattingEnabled = True
        cboclass.Location = New Point(81, 3)
        cboclass.Margin = New Padding(3, 4, 3, 4)
        cboclass.Name = "cboclass"
        cboclass.Size = New Size(302, 28)
        cboclass.TabIndex = 0
        cboclass.Text = "CS - 103 OBJECT ORIENTED PROGRAMMING"
        ' 
        ' pnlMyPerformance
        ' 
        pnlMyPerformance.BackColor = Color.WhiteSmoke
        pnlMyPerformance.Controls.Add(cardPerformance)
        pnlMyPerformance.Dock = DockStyle.Fill
        pnlMyPerformance.Location = New Point(0, 4)
        pnlMyPerformance.Margin = New Padding(3, 4, 3, 4)
        pnlMyPerformance.Name = "pnlMyPerformance"
        pnlMyPerformance.Padding = New Padding(34, 40, 34, 40)
        pnlMyPerformance.Size = New Size(1370, 557)
        pnlMyPerformance.TabIndex = 8
        pnlMyPerformance.Visible = False
        ' 
        ' cardPerformance
        ' 
        cardPerformance.BackColor = Color.White
        cardPerformance.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        cardPerformance.Controls.Add(tlpbars)
        cardPerformance.Controls.Add(tlpstats)
        cardPerformance.Controls.Add(pnlperfheader)
        cardPerformance.Controls.Add(Panel1)
        cardPerformance.Dock = DockStyle.Fill
        cardPerformance.Location = New Point(34, 40)
        cardPerformance.Margin = New Padding(3, 4, 3, 4)
        cardPerformance.Name = "cardPerformance"
        cardPerformance.Padding = New Padding(23, 27, 23, 27)
        cardPerformance.Size = New Size(1302, 477)
        cardPerformance.TabIndex = 0
        ' 
        ' tlpbars
        ' 
        tlpbars.ColumnCount = 2
        tlpbars.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 126F))
        tlpbars.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpbars.Controls.Add(trackProject, 1, 3)
        tlpbars.Controls.Add(trackExam, 1, 2)
        tlpbars.Controls.Add(trackActivity, 1, 1)
        tlpbars.Controls.Add(trackQuiz, 1, 0)
        tlpbars.Controls.Add(Label12, 0, 3)
        tlpbars.Controls.Add(Label13, 0, 2)
        tlpbars.Controls.Add(Label14, 0, 1)
        tlpbars.Controls.Add(Label11, 0, 0)
        tlpbars.Dock = DockStyle.Fill
        tlpbars.Location = New Point(23, 256)
        tlpbars.Margin = New Padding(0)
        tlpbars.Name = "tlpbars"
        tlpbars.RowCount = 5
        tlpbars.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        tlpbars.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        tlpbars.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        tlpbars.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        tlpbars.RowStyles.Add(New RowStyle(SizeType.Absolute, 27F))
        tlpbars.Size = New Size(1256, 194)
        tlpbars.TabIndex = 18
        ' 
        ' trackProject
        ' 
        trackProject.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        trackProject.BackColor = Color.White
        trackProject.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        trackProject.Controls.Add(fillproject)
        trackProject.CornerRadius = 13
        trackProject.Location = New Point(129, 127)
        trackProject.Margin = New Padding(3, 4, 3, 4)
        trackProject.Name = "trackProject"
        trackProject.Padding = New Padding(8, 9, 8, 9)
        trackProject.Size = New Size(1124, 33)
        trackProject.TabIndex = 22
        ' 
        ' fillproject
        ' 
        fillproject.BackColor = Color.DarkGreen
        fillproject.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        fillproject.BorderThickness = 0
        fillproject.Controls.Add(lblprojectpct)
        fillproject.CornerRadius = 11
        fillproject.Dock = DockStyle.Left
        fillproject.Location = New Point(8, 9)
        fillproject.Margin = New Padding(3, 4, 3, 4)
        fillproject.Name = "fillproject"
        fillproject.Size = New Size(997, 15)
        fillproject.TabIndex = 1
        ' 
        ' lblprojectpct
        ' 
        lblprojectpct.AutoSize = True
        lblprojectpct.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblprojectpct.ForeColor = Color.White
        lblprojectpct.Location = New Point(13, 4)
        lblprojectpct.Name = "lblprojectpct"
        lblprojectpct.Size = New Size(35, 23)
        lblprojectpct.TabIndex = 1
        lblprojectpct.Text = "0%"
        ' 
        ' trackExam
        ' 
        trackExam.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        trackExam.BackColor = Color.White
        trackExam.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        trackExam.Controls.Add(fillexam)
        trackExam.CornerRadius = 13
        trackExam.Location = New Point(129, 86)
        trackExam.Margin = New Padding(3, 4, 3, 4)
        trackExam.Name = "trackExam"
        trackExam.Padding = New Padding(7, 8, 7, 8)
        trackExam.Size = New Size(1124, 33)
        trackExam.TabIndex = 21
        ' 
        ' fillexam
        ' 
        fillexam.BackColor = Color.DarkGreen
        fillexam.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        fillexam.BorderThickness = 0
        fillexam.Controls.Add(lblexampct)
        fillexam.CornerRadius = 10
        fillexam.Dock = DockStyle.Left
        fillexam.Location = New Point(7, 8)
        fillexam.Margin = New Padding(0)
        fillexam.Name = "fillexam"
        fillexam.Size = New Size(997, 17)
        fillexam.TabIndex = 1
        ' 
        ' lblexampct
        ' 
        lblexampct.AutoSize = True
        lblexampct.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblexampct.ForeColor = Color.White
        lblexampct.Location = New Point(13, 3)
        lblexampct.Name = "lblexampct"
        lblexampct.Size = New Size(35, 23)
        lblexampct.TabIndex = 1
        lblexampct.Text = "0%"
        ' 
        ' trackActivity
        ' 
        trackActivity.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        trackActivity.BackColor = Color.White
        trackActivity.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        trackActivity.Controls.Add(fillactivity)
        trackActivity.CornerRadius = 13
        trackActivity.Location = New Point(129, 45)
        trackActivity.Margin = New Padding(3, 4, 3, 4)
        trackActivity.Name = "trackActivity"
        trackActivity.Padding = New Padding(7, 8, 7, 8)
        trackActivity.Size = New Size(1124, 33)
        trackActivity.TabIndex = 20
        ' 
        ' fillactivity
        ' 
        fillactivity.BackColor = Color.DarkGreen
        fillactivity.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        fillactivity.BorderThickness = 0
        fillactivity.Controls.Add(lblactivitypct)
        fillactivity.CornerRadius = 11
        fillactivity.Dock = DockStyle.Left
        fillactivity.Location = New Point(7, 8)
        fillactivity.Margin = New Padding(3, 4, 3, 4)
        fillactivity.Name = "fillactivity"
        fillactivity.Size = New Size(998, 17)
        fillactivity.TabIndex = 1
        ' 
        ' lblactivitypct
        ' 
        lblactivitypct.AutoSize = True
        lblactivitypct.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblactivitypct.ForeColor = Color.White
        lblactivitypct.Location = New Point(14, 4)
        lblactivitypct.Name = "lblactivitypct"
        lblactivitypct.Size = New Size(35, 23)
        lblactivitypct.TabIndex = 1
        lblactivitypct.Text = "0%"
        ' 
        ' trackQuiz
        ' 
        trackQuiz.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        trackQuiz.BackColor = Color.White
        trackQuiz.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        trackQuiz.Controls.Add(fillQuiz)
        trackQuiz.CornerRadius = 13
        trackQuiz.Location = New Point(129, 4)
        trackQuiz.Margin = New Padding(3, 4, 3, 4)
        trackQuiz.Name = "trackQuiz"
        trackQuiz.Padding = New Padding(7, 8, 7, 8)
        trackQuiz.Size = New Size(1124, 33)
        trackQuiz.TabIndex = 19
        ' 
        ' fillQuiz
        ' 
        fillQuiz.BackColor = Color.DarkGreen
        fillQuiz.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        fillQuiz.BorderThickness = 0
        fillQuiz.Controls.Add(lblQuizPct)
        fillQuiz.CornerRadius = 11
        fillQuiz.Dock = DockStyle.Left
        fillQuiz.Location = New Point(7, 8)
        fillQuiz.Margin = New Padding(3, 4, 3, 4)
        fillQuiz.Name = "fillQuiz"
        fillQuiz.Size = New Size(998, 17)
        fillQuiz.TabIndex = 0
        ' 
        ' lblQuizPct
        ' 
        lblQuizPct.AutoSize = True
        lblQuizPct.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblQuizPct.ForeColor = Color.White
        lblQuizPct.Location = New Point(14, 4)
        lblQuizPct.Name = "lblQuizPct"
        lblQuizPct.Size = New Size(35, 23)
        lblQuizPct.TabIndex = 0
        lblQuizPct.Text = "0%"
        ' 
        ' Label12
        ' 
        Label12.Anchor = AnchorStyles.None
        Label12.BackColor = Color.White
        Label12.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label12.ForeColor = Color.DarkGreen
        Label12.Location = New Point(31, 132)
        Label12.Name = "Label12"
        Label12.Size = New Size(63, 23)
        Label12.TabIndex = 15
        Label12.Text = "Projects"
        ' 
        ' Label13
        ' 
        Label13.Anchor = AnchorStyles.None
        Label13.BackColor = Color.White
        Label13.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label13.ForeColor = Color.DarkGreen
        Label13.Location = New Point(31, 91)
        Label13.Name = "Label13"
        Label13.Size = New Size(63, 23)
        Label13.TabIndex = 16
        Label13.Text = "Exams "
        ' 
        ' Label14
        ' 
        Label14.Anchor = AnchorStyles.None
        Label14.BackColor = Color.White
        Label14.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label14.ForeColor = Color.DarkGreen
        Label14.Location = New Point(24, 49)
        Label14.Name = "Label14"
        Label14.Size = New Size(77, 24)
        Label14.TabIndex = 17
        Label14.Text = "Activities"
        ' 
        ' Label11
        ' 
        Label11.Anchor = AnchorStyles.None
        Label11.BackColor = Color.White
        Label11.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label11.ForeColor = Color.DarkGreen
        Label11.Location = New Point(30, 8)
        Label11.Name = "Label11"
        Label11.Size = New Size(66, 25)
        Label11.TabIndex = 14
        Label11.Text = "Quizzes"
        ' 
        ' tlpstats
        ' 
        tlpstats.ColumnCount = 4
        tlpstats.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25F))
        tlpstats.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25F))
        tlpstats.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25F))
        tlpstats.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25F))
        tlpstats.Controls.Add(RoundedPanel6, 0, 0)
        tlpstats.Controls.Add(RoundedPanel8, 1, 0)
        tlpstats.Controls.Add(RoundedPanel7, 2, 0)
        tlpstats.Controls.Add(RoundedPanel9, 3, 0)
        tlpstats.Dock = DockStyle.Top
        tlpstats.Location = New Point(23, 107)
        tlpstats.Margin = New Padding(3, 4, 3, 4)
        tlpstats.Name = "tlpstats"
        tlpstats.RowCount = 1
        tlpstats.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpstats.Size = New Size(1256, 149)
        tlpstats.TabIndex = 21
        ' 
        ' RoundedPanel6
        ' 
        RoundedPanel6.BackColor = Color.AliceBlue
        RoundedPanel6.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        RoundedPanel6.Controls.Add(lblAverageCaption)
        RoundedPanel6.Controls.Add(lblAverageValue)
        RoundedPanel6.Dock = DockStyle.Fill
        RoundedPanel6.Location = New Point(11, 13)
        RoundedPanel6.Margin = New Padding(11, 13, 11, 13)
        RoundedPanel6.Name = "RoundedPanel6"
        RoundedPanel6.Padding = New Padding(7, 8, 7, 8)
        RoundedPanel6.Size = New Size(292, 123)
        RoundedPanel6.TabIndex = 11
        ' 
        ' lblAverageCaption
        ' 
        lblAverageCaption.BackColor = Color.Transparent
        lblAverageCaption.Dock = DockStyle.Fill
        lblAverageCaption.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblAverageCaption.ForeColor = Color.DarkGreen
        lblAverageCaption.Location = New Point(7, 85)
        lblAverageCaption.Name = "lblAverageCaption"
        lblAverageCaption.Size = New Size(278, 30)
        lblAverageCaption.TabIndex = 15
        lblAverageCaption.Text = "Current Average"
        lblAverageCaption.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblAverageValue
        ' 
        lblAverageValue.BackColor = Color.Transparent
        lblAverageValue.Dock = DockStyle.Top
        lblAverageValue.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblAverageValue.ForeColor = Color.DarkGreen
        lblAverageValue.Location = New Point(7, 8)
        lblAverageValue.Name = "lblAverageValue"
        lblAverageValue.Size = New Size(278, 77)
        lblAverageValue.TabIndex = 20
        lblAverageValue.Text = "0"
        lblAverageValue.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' RoundedPanel8
        ' 
        RoundedPanel8.BackColor = Color.AliceBlue
        RoundedPanel8.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        RoundedPanel8.Controls.Add(lblPassedCaption)
        RoundedPanel8.Controls.Add(lblPassedValue)
        RoundedPanel8.Dock = DockStyle.Bottom
        RoundedPanel8.Location = New Point(325, 13)
        RoundedPanel8.Margin = New Padding(11, 13, 11, 13)
        RoundedPanel8.Name = "RoundedPanel8"
        RoundedPanel8.Padding = New Padding(7, 8, 7, 8)
        RoundedPanel8.Size = New Size(292, 123)
        RoundedPanel8.TabIndex = 12
        ' 
        ' lblPassedCaption
        ' 
        lblPassedCaption.BackColor = Color.Transparent
        lblPassedCaption.Dock = DockStyle.Fill
        lblPassedCaption.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblPassedCaption.ForeColor = Color.DarkGreen
        lblPassedCaption.Location = New Point(7, 88)
        lblPassedCaption.Name = "lblPassedCaption"
        lblPassedCaption.Size = New Size(278, 27)
        lblPassedCaption.TabIndex = 16
        lblPassedCaption.Text = "Activities Passed"
        lblPassedCaption.TextAlign = ContentAlignment.TopCenter
        ' 
        ' lblPassedValue
        ' 
        lblPassedValue.BackColor = Color.Transparent
        lblPassedValue.Dock = DockStyle.Top
        lblPassedValue.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblPassedValue.ForeColor = Color.DarkGreen
        lblPassedValue.Location = New Point(7, 8)
        lblPassedValue.Name = "lblPassedValue"
        lblPassedValue.Size = New Size(278, 80)
        lblPassedValue.TabIndex = 19
        lblPassedValue.Text = "0"
        lblPassedValue.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' RoundedPanel7
        ' 
        RoundedPanel7.BackColor = Color.AliceBlue
        RoundedPanel7.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        RoundedPanel7.Controls.Add(lblPendingCaption)
        RoundedPanel7.Controls.Add(lblPendingValue)
        RoundedPanel7.Dock = DockStyle.Fill
        RoundedPanel7.Location = New Point(639, 13)
        RoundedPanel7.Margin = New Padding(11, 13, 11, 13)
        RoundedPanel7.Name = "RoundedPanel7"
        RoundedPanel7.Padding = New Padding(7, 8, 7, 8)
        RoundedPanel7.Size = New Size(292, 123)
        RoundedPanel7.TabIndex = 12
        ' 
        ' lblPendingCaption
        ' 
        lblPendingCaption.BackColor = Color.Transparent
        lblPendingCaption.Dock = DockStyle.Fill
        lblPendingCaption.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblPendingCaption.ForeColor = Color.DarkGreen
        lblPendingCaption.Location = New Point(7, 87)
        lblPendingCaption.Name = "lblPendingCaption"
        lblPendingCaption.Size = New Size(278, 28)
        lblPendingCaption.TabIndex = 16
        lblPendingCaption.Text = "Pending"
        lblPendingCaption.TextAlign = ContentAlignment.TopCenter
        ' 
        ' lblPendingValue
        ' 
        lblPendingValue.BackColor = Color.Transparent
        lblPendingValue.Dock = DockStyle.Top
        lblPendingValue.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblPendingValue.ForeColor = Color.DarkGreen
        lblPendingValue.Location = New Point(7, 8)
        lblPendingValue.Name = "lblPendingValue"
        lblPendingValue.Size = New Size(278, 79)
        lblPendingValue.TabIndex = 19
        lblPendingValue.Text = "0"
        lblPendingValue.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' RoundedPanel9
        ' 
        RoundedPanel9.BackColor = Color.AliceBlue
        RoundedPanel9.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        RoundedPanel9.Controls.Add(lblHighestCaption)
        RoundedPanel9.Controls.Add(lblHighestValue)
        RoundedPanel9.Dock = DockStyle.Fill
        RoundedPanel9.Location = New Point(953, 13)
        RoundedPanel9.Margin = New Padding(11, 13, 11, 13)
        RoundedPanel9.Name = "RoundedPanel9"
        RoundedPanel9.Padding = New Padding(7, 8, 7, 8)
        RoundedPanel9.Size = New Size(292, 123)
        RoundedPanel9.TabIndex = 12
        ' 
        ' lblHighestCaption
        ' 
        lblHighestCaption.BackColor = Color.Transparent
        lblHighestCaption.Dock = DockStyle.Fill
        lblHighestCaption.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblHighestCaption.ForeColor = Color.DarkGreen
        lblHighestCaption.Location = New Point(7, 88)
        lblHighestCaption.Name = "lblHighestCaption"
        lblHighestCaption.Size = New Size(278, 27)
        lblHighestCaption.TabIndex = 17
        lblHighestCaption.Text = "Highest Score"
        lblHighestCaption.TextAlign = ContentAlignment.TopCenter
        ' 
        ' lblHighestValue
        ' 
        lblHighestValue.BackColor = Color.Transparent
        lblHighestValue.Dock = DockStyle.Top
        lblHighestValue.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblHighestValue.ForeColor = Color.DarkGreen
        lblHighestValue.Location = New Point(7, 8)
        lblHighestValue.Name = "lblHighestValue"
        lblHighestValue.Size = New Size(278, 80)
        lblHighestValue.TabIndex = 18
        lblHighestValue.Text = "0"
        lblHighestValue.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' pnlperfheader
        ' 
        pnlperfheader.Controls.Add(cboperfterm)
        pnlperfheader.Controls.Add(cboperfcourse)
        pnlperfheader.Controls.Add(Label10)
        pnlperfheader.Controls.Add(PictureBox6)
        pnlperfheader.Dock = DockStyle.Top
        pnlperfheader.Location = New Point(23, 27)
        pnlperfheader.Margin = New Padding(3, 4, 3, 4)
        pnlperfheader.Name = "pnlperfheader"
        pnlperfheader.Size = New Size(1256, 80)
        pnlperfheader.TabIndex = 20
        ' 
        ' cboperfterm
        ' 
        cboperfterm.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cboperfterm.DropDownStyle = ComboBoxStyle.DropDownList
        cboperfterm.FormattingEnabled = True
        cboperfterm.Location = New Point(775, 25)
        cboperfterm.Margin = New Padding(3, 4, 3, 4)
        cboperfterm.Name = "cboperfterm"
        cboperfterm.Size = New Size(138, 28)
        cboperfterm.TabIndex = 12
        ' 
        ' cboperfcourse
        ' 
        cboperfcourse.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cboperfcourse.DropDownStyle = ComboBoxStyle.DropDownList
        cboperfcourse.FormattingEnabled = True
        cboperfcourse.Location = New Point(927, 25)
        cboperfcourse.Margin = New Padding(3, 4, 3, 4)
        cboperfcourse.Name = "cboperfcourse"
        cboperfcourse.Size = New Size(314, 28)
        cboperfcourse.TabIndex = 11
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.BackColor = Color.White
        Label10.Font = New Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label10.ForeColor = Color.DarkGreen
        Label10.Location = New Point(82, 16)
        Label10.Name = "Label10"
        Label10.Size = New Size(391, 41)
        Label10.TabIndex = 10
        Label10.Text = "My Performance Overview"
        ' 
        ' PictureBox6
        ' 
        PictureBox6.Image = CType(resources.GetObject("PictureBox6.Image"), Image)
        PictureBox6.Location = New Point(10, 1)
        PictureBox6.Margin = New Padding(3, 4, 3, 4)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New Size(67, 73)
        PictureBox6.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox6.TabIndex = 1
        PictureBox6.TabStop = False
        ' 
        ' Panel1
        ' 
        Panel1.BackColor = Color.DimGray
        Panel1.Location = New Point(34, 103)
        Panel1.Margin = New Padding(3, 4, 3, 4)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(1168, 3)
        Panel1.TabIndex = 19
        ' 
        ' pnlMyGrades
        ' 
        pnlMyGrades.BackColor = Color.WhiteSmoke
        pnlMyGrades.Controls.Add(Label1)
        pnlMyGrades.Controls.Add(RoundedPanel1)
        pnlMyGrades.Dock = DockStyle.Fill
        pnlMyGrades.Location = New Point(0, 4)
        pnlMyGrades.Margin = New Padding(3, 4, 3, 4)
        pnlMyGrades.Name = "pnlMyGrades"
        pnlMyGrades.Size = New Size(1370, 557)
        pnlMyGrades.TabIndex = 7
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.BackColor = Color.WhiteSmoke
        Label1.Font = New Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.DarkGreen
        Label1.Location = New Point(165, 5)
        Label1.Name = "Label1"
        Label1.Size = New Size(207, 41)
        Label1.TabIndex = 10
        Label1.Text = "Grade Report"
        ' 
        ' RoundedPanel1
        ' 
        RoundedPanel1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        RoundedPanel1.BackColor = Color.White
        RoundedPanel1.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        RoundedPanel1.Controls.Add(lblstanding)
        RoundedPanel1.Controls.Add(dgvgrades)
        RoundedPanel1.Controls.Add(Panel3)
        RoundedPanel1.Controls.Add(Panel4)
        RoundedPanel1.Location = New Point(160, 53)
        RoundedPanel1.Margin = New Padding(3, 4, 3, 4)
        RoundedPanel1.Name = "RoundedPanel1"
        RoundedPanel1.Padding = New Padding(16, 19, 16, 19)
        RoundedPanel1.Size = New Size(1062, 433)
        RoundedPanel1.TabIndex = 0
        ' 
        ' lblstanding
        ' 
        lblstanding.BackColor = Color.White
        lblstanding.Dock = DockStyle.Bottom
        lblstanding.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblstanding.ForeColor = Color.DarkGreen
        lblstanding.Location = New Point(16, 378)
        lblstanding.Name = "lblstanding"
        lblstanding.Padding = New Padding(11, 0, 0, 13)
        lblstanding.Size = New Size(1030, 36)
        lblstanding.TabIndex = 12
        lblstanding.Text = "Current Standing: 0%"
        lblstanding.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' dgvgrades
        ' 
        dgvgrades.BackgroundColor = Color.White
        dgvgrades.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvgrades.Dock = DockStyle.Fill
        dgvgrades.Location = New Point(16, 89)
        dgvgrades.Margin = New Padding(3, 4, 3, 4)
        dgvgrades.Name = "dgvgrades"
        dgvgrades.RowHeadersWidth = 51
        dgvgrades.Size = New Size(1030, 325)
        dgvgrades.TabIndex = 3
        ' 
        ' Panel3
        ' 
        Panel3.BackColor = Color.DimGray
        Panel3.Dock = DockStyle.Top
        Panel3.Location = New Point(16, 86)
        Panel3.Margin = New Padding(3, 4, 3, 4)
        Panel3.Name = "Panel3"
        Panel3.Size = New Size(1030, 3)
        Panel3.TabIndex = 13
        ' 
        ' Panel4
        ' 
        Panel4.BackColor = Color.White
        Panel4.Controls.Add(cboterm)
        Panel4.Controls.Add(PictureBox2)
        Panel4.Controls.Add(cboCourse)
        Panel4.Dock = DockStyle.Top
        Panel4.Location = New Point(16, 19)
        Panel4.Margin = New Padding(3, 4, 3, 4)
        Panel4.Name = "Panel4"
        Panel4.Size = New Size(1030, 67)
        Panel4.TabIndex = 14
        ' 
        ' cboterm
        ' 
        cboterm.DropDownStyle = ComboBoxStyle.DropDownList
        cboterm.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        cboterm.ForeColor = Color.DarkGreen
        cboterm.FormattingEnabled = True
        cboterm.Location = New Point(423, 19)
        cboterm.Margin = New Padding(3, 4, 3, 4)
        cboterm.Name = "cboterm"
        cboterm.Size = New Size(138, 28)
        cboterm.TabIndex = 3
        ' 
        ' PictureBox2
        ' 
        PictureBox2.Image = My.Resources.Resources.Gemini_Generated_Image_902bw4902bw4902b_removebg_preview
        PictureBox2.Location = New Point(0, 4)
        PictureBox2.Margin = New Padding(3, 4, 3, 4)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(77, 57)
        PictureBox2.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox2.TabIndex = 1
        PictureBox2.TabStop = False
        ' 
        ' cboCourse
        ' 
        cboCourse.BackColor = Color.FloralWhite
        cboCourse.DropDownStyle = ComboBoxStyle.DropDownList
        cboCourse.FormattingEnabled = True
        cboCourse.Location = New Point(91, 19)
        cboCourse.Margin = New Padding(3, 4, 3, 4)
        cboCourse.Name = "cboCourse"
        cboCourse.Size = New Size(316, 28)
        cboCourse.TabIndex = 2
        ' 
        ' pnlGradeHistory
        ' 
        pnlGradeHistory.BackColor = Color.White
        pnlGradeHistory.Controls.Add(RoundedPanel2)
        pnlGradeHistory.Dock = DockStyle.Fill
        pnlGradeHistory.Location = New Point(0, 4)
        pnlGradeHistory.Margin = New Padding(3, 4, 3, 4)
        pnlGradeHistory.Name = "pnlGradeHistory"
        pnlGradeHistory.Size = New Size(1370, 557)
        pnlGradeHistory.TabIndex = 9
        pnlGradeHistory.Visible = False
        ' 
        ' RoundedPanel2
        ' 
        RoundedPanel2.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        RoundedPanel2.BackColor = Color.White
        RoundedPanel2.BorderColor = Color.FromArgb(CByte(0), CByte(85), CByte(0))
        RoundedPanel2.Controls.Add(dgvHistory)
        RoundedPanel2.Controls.Add(Label5)
        RoundedPanel2.Controls.Add(pnlHistoryHeader)
        RoundedPanel2.Location = New Point(91, 67)
        RoundedPanel2.Margin = New Padding(3, 4, 3, 4)
        RoundedPanel2.Name = "RoundedPanel2"
        RoundedPanel2.Padding = New Padding(16, 19, 16, 19)
        RoundedPanel2.Size = New Size(1173, 304)
        RoundedPanel2.TabIndex = 0
        ' 
        ' dgvHistory
        ' 
        dgvHistory.BackgroundColor = Color.White
        dgvHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvHistory.Dock = DockStyle.Fill
        dgvHistory.Location = New Point(16, 99)
        dgvHistory.Margin = New Padding(3, 4, 3, 4)
        dgvHistory.Name = "dgvHistory"
        dgvHistory.RowHeadersWidth = 51
        dgvHistory.Size = New Size(1141, 146)
        dgvHistory.TabIndex = 3
        ' 
        ' Label5
        ' 
        Label5.Dock = DockStyle.Bottom
        Label5.Font = New Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label5.ForeColor = Color.DarkGreen
        Label5.Location = New Point(16, 245)
        Label5.Name = "Label5"
        Label5.Size = New Size(1141, 40)
        Label5.TabIndex = 4
        Label5.Text = "Every grade is stored in MYSQL"
        Label5.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlHistoryHeader
        ' 
        pnlHistoryHeader.Controls.Add(PictureBox3)
        pnlHistoryHeader.Controls.Add(Label4)
        pnlHistoryHeader.Dock = DockStyle.Top
        pnlHistoryHeader.Location = New Point(16, 19)
        pnlHistoryHeader.Margin = New Padding(3, 4, 3, 4)
        pnlHistoryHeader.Name = "pnlHistoryHeader"
        pnlHistoryHeader.Size = New Size(1141, 80)
        pnlHistoryHeader.TabIndex = 5
        ' 
        ' PictureBox3
        ' 
        PictureBox3.Image = My.Resources.Resources.Gemini_Generated_Image_w8efogw8efogw8ef_removebg_preview
        PictureBox3.Location = New Point(5, 7)
        PictureBox3.Margin = New Padding(3, 4, 3, 4)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New Size(90, 67)
        PictureBox3.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox3.TabIndex = 0
        PictureBox3.TabStop = False
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label4.ForeColor = Color.DarkGreen
        Label4.Location = New Point(97, 21)
        Label4.Name = "Label4"
        Label4.Size = New Size(245, 37)
        Label4.TabIndex = 1
        Label4.Text = "My Grade History"
        ' 
        ' Panel7
        ' 
        Panel7.BackColor = Color.DimGray
        Panel7.Dock = DockStyle.Top
        Panel7.Location = New Point(0, 0)
        Panel7.Margin = New Padding(3, 4, 3, 4)
        Panel7.Name = "Panel7"
        Panel7.Size = New Size(1370, 4)
        Panel7.TabIndex = 11
        ' 
        ' pnlheader
        ' 
        pnlheader.BackColor = Color.Honeydew
        pnlheader.Controls.Add(btnChangePassword)
        pnlheader.Controls.Add(Panel6)
        pnlheader.Controls.Add(lblrole)
        pnlheader.Controls.Add(lblstudentname)
        pnlheader.Controls.Add(PictureBox1)
        pnlheader.Controls.Add(btnLogout)
        pnlheader.Dock = DockStyle.Top
        pnlheader.Location = New Point(0, 37)
        pnlheader.Margin = New Padding(3, 4, 3, 4)
        pnlheader.Name = "pnlheader"
        pnlheader.Size = New Size(1370, 48)
        pnlheader.TabIndex = 2
        ' 
        ' btnChangePassword
        ' 
        btnChangePassword.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnChangePassword.FlatStyle = FlatStyle.Flat
        btnChangePassword.Font = New Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnChangePassword.Location = New Point(1121, 5)
        btnChangePassword.Margin = New Padding(3, 4, 3, 4)
        btnChangePassword.Name = "btnChangePassword"
        btnChangePassword.Size = New Size(154, 39)
        btnChangePassword.TabIndex = 11
        btnChangePassword.Text = "Change Password"
        btnChangePassword.UseVisualStyleBackColor = True
        ' 
        ' Panel6
        ' 
        Panel6.BackColor = Color.DimGray
        Panel6.Dock = DockStyle.Top
        Panel6.Location = New Point(0, 0)
        Panel6.Margin = New Padding(3, 4, 3, 4)
        Panel6.Name = "Panel6"
        Panel6.Size = New Size(1370, 3)
        Panel6.TabIndex = 10
        ' 
        ' lblrole
        ' 
        lblrole.AutoSize = True
        lblrole.BackColor = Color.DarkGreen
        lblrole.Font = New Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblrole.ForeColor = Color.White
        lblrole.Location = New Point(241, 9)
        lblrole.Name = "lblrole"
        lblrole.Size = New Size(98, 25)
        lblrole.TabIndex = 9
        lblrole.Text = "STUDENT"
        ' 
        ' lblstudentname
        ' 
        lblstudentname.AutoSize = True
        lblstudentname.ForeColor = Color.DarkGreen
        lblstudentname.Location = New Point(66, 13)
        lblstudentname.Name = "lblstudentname"
        lblstudentname.Size = New Size(154, 20)
        lblstudentname.TabIndex = 0
        lblstudentname.Text = "Condes, Hope Gian D."
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Image = My.Resources.Resources.Gemini_Generated_Image_3yvygi3yvygi3yvy_removebg_preview
        PictureBox1.Location = New Point(16, 4)
        PictureBox1.Margin = New Padding(3, 4, 3, 4)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(45, 39)
        PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox1.TabIndex = 0
        PictureBox1.TabStop = False
        ' 
        ' btnLogout
        ' 
        btnLogout.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnLogout.FlatStyle = FlatStyle.Flat
        btnLogout.Font = New Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnLogout.Location = New Point(1282, 5)
        btnLogout.Margin = New Padding(3, 4, 3, 4)
        btnLogout.Name = "btnLogout"
        btnLogout.Size = New Size(79, 39)
        btnLogout.TabIndex = 8
        btnLogout.Text = "Log out"
        btnLogout.UseVisualStyleBackColor = True
        ' 
        ' frmstudent
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1370, 749)
        Controls.Add(pnlContent)
        Controls.Add(Pnltabs)
        Controls.Add(pnlheader)
        Controls.Add(Pnlfooter)
        Controls.Add(Pnlheaderheader)
        Margin = New Padding(3, 4, 3, 4)
        Name = "frmstudent"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Transparent Grading System - Dauntless Wolves"
        WindowState = FormWindowState.Maximized
        Pnlheaderheader.ResumeLayout(False)
        Pnlheaderheader.PerformLayout()
        Pnltabs.ResumeLayout(False)
        pnlContent.ResumeLayout(False)
        pnlRequestRevision.ResumeLayout(False)
        tlprevision.ResumeLayout(False)
        cardSubmit.ResumeLayout(False)
        cardSubmit.PerformLayout()
        pnlSubmitHeader.ResumeLayout(False)
        pnlSubmitHeader.PerformLayout()
        CType(PictureBox4, ComponentModel.ISupportInitialize).EndInit()
        cardStatus.ResumeLayout(False)
        CType(dgvRequests, ComponentModel.ISupportInitialize).EndInit()
        pnlStatusHeader.ResumeLayout(False)
        pnlStatusHeader.PerformLayout()
        CType(PictureBox5, ComponentModel.ISupportInitialize).EndInit()
        pnlclassbar.ResumeLayout(False)
        pnlclassbar.PerformLayout()
        pnlMyPerformance.ResumeLayout(False)
        cardPerformance.ResumeLayout(False)
        tlpbars.ResumeLayout(False)
        trackProject.ResumeLayout(False)
        fillproject.ResumeLayout(False)
        fillproject.PerformLayout()
        trackExam.ResumeLayout(False)
        fillexam.ResumeLayout(False)
        fillexam.PerformLayout()
        trackActivity.ResumeLayout(False)
        fillactivity.ResumeLayout(False)
        fillactivity.PerformLayout()
        trackQuiz.ResumeLayout(False)
        fillQuiz.ResumeLayout(False)
        fillQuiz.PerformLayout()
        tlpstats.ResumeLayout(False)
        RoundedPanel6.ResumeLayout(False)
        RoundedPanel8.ResumeLayout(False)
        RoundedPanel7.ResumeLayout(False)
        RoundedPanel9.ResumeLayout(False)
        pnlperfheader.ResumeLayout(False)
        pnlperfheader.PerformLayout()
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        pnlMyGrades.ResumeLayout(False)
        pnlMyGrades.PerformLayout()
        RoundedPanel1.ResumeLayout(False)
        CType(dgvgrades, ComponentModel.ISupportInitialize).EndInit()
        Panel4.ResumeLayout(False)
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        pnlGradeHistory.ResumeLayout(False)
        RoundedPanel2.ResumeLayout(False)
        CType(dgvHistory, ComponentModel.ISupportInitialize).EndInit()
        pnlHistoryHeader.ResumeLayout(False)
        pnlHistoryHeader.PerformLayout()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        pnlheader.ResumeLayout(False)
        pnlheader.PerformLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub
    Friend WithEvents Pnlheaderheader As Panel
    Friend WithEvents Pnlfooter As Panel
    Friend WithEvents Pnltabs As Panel
    Friend WithEvents btnGradeHistory As Button
    Friend WithEvents btnMyGrades As Button
    Friend WithEvents btnRequestRevision As Button
    Friend WithEvents btnMyPerformance As Button
    Friend WithEvents pnlContent As Panel
    Friend WithEvents pnlMyGrades As Panel
    Friend WithEvents pnlheader As Panel
    Friend WithEvents pnlRequestRevision As Panel
    Friend WithEvents pnlGradeHistory As Panel
    Friend WithEvents pnlMyPerformance As Panel
    Friend WithEvents btnLogout As Button
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents lblrole As Label
    Friend WithEvents lblstudentname As Label
    Friend WithEvents RoundedPanel1 As CustomPanel
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents cboCourse As ComboBox
    Friend WithEvents dgvgrades As DataGridView
    Friend WithEvents lblstanding As Label
    Friend WithEvents RoundedPanel2 As CustomPanel
    Friend WithEvents Label4 As Label
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents dgvHistory As DataGridView
    Friend WithEvents Label5 As Label
    Friend WithEvents cardStatus As CustomPanel
    Friend WithEvents cardSubmit As CustomPanel
    Friend WithEvents Label6 As Label
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents cboAssessment As ComboBox
    Friend WithEvents Label7 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents txtCurrentGrade As TextBox
    Friend WithEvents btnSubmitRequest As Button
    Friend WithEvents txtReason As TextBox
    Friend WithEvents Label9 As Label
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents lblStatusTitle As Label
    Friend WithEvents dgvRequests As DataGridView
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents cardPerformance As CustomPanel
    Friend WithEvents RoundedPanel6 As CustomPanel
    Friend WithEvents Label10 As Label
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents RoundedPanel8 As CustomPanel
    Friend WithEvents RoundedPanel9 As CustomPanel
    Friend WithEvents RoundedPanel7 As CustomPanel
    Friend WithEvents Label14 As Label
    Friend WithEvents Label13 As Label
    Friend WithEvents Label12 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents lblPassedCaption As Label
    Friend WithEvents lblHighestCaption As Label
    Friend WithEvents lblPendingCaption As Label
    Friend WithEvents lblAverageCaption As Label
    Friend WithEvents lblHighestValue As Label
    Friend WithEvents lblPendingValue As Label
    Friend WithEvents lblPassedValue As Label
    Friend WithEvents lblAverageValue As Label
    Friend WithEvents tlpbars As TableLayoutPanel
    Friend WithEvents trackProject As CustomPanel
    Friend WithEvents trackExam As CustomPanel
    Friend WithEvents trackActivity As CustomPanel
    Friend WithEvents trackQuiz As CustomPanel
    Friend WithEvents fillQuiz As CustomPanel
    Friend WithEvents fillproject As CustomPanel
    Friend WithEvents fillexam As CustomPanel
    Friend WithEvents fillactivity As CustomPanel
    Friend WithEvents lblQuizPct As Label
    Friend WithEvents lblprojectpct As Label
    Friend WithEvents lblexampct As Label
    Friend WithEvents lblactivitypct As Label
    Friend WithEvents Panel2 As Panel
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Label3 As Label
    Friend WithEvents Panel3 As Panel
    Friend WithEvents Panel4 As Panel
    Friend WithEvents pnlHistoryHeader As Panel
    Friend WithEvents tlprevision As TableLayoutPanel
    Friend WithEvents pnlStatusHeader As Panel
    Friend WithEvents pnlSubmitHeader As Panel
    Friend WithEvents pnlperfheader As Panel
    Friend WithEvents tlpstats As TableLayoutPanel
    Friend WithEvents Panel5 As Panel
    Friend WithEvents Panel7 As Panel
    Friend WithEvents Panel6 As Panel
    Friend WithEvents pnlclassbar As Panel
    Friend WithEvents lblClass As Label
    Friend WithEvents cboclass As ComboBox
    Friend WithEvents cboterm As ComboBox
    Friend WithEvents cboperfterm As ComboBox
    Friend WithEvents cboperfcourse As ComboBox
    Friend WithEvents btnChangePassword As Button

End Class
