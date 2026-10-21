
Public Class quickactions

    Public Enum FormMode
        EnterGrades
        ViewRequests
        ViewAuditLogs
    End Enum

    Private _mode As FormMode

    ' ===== Switch between the three panels =====
    Public Sub ShowMode(mode As FormMode)
        _mode = mode
        Me.DialogResult = DialogResult.None

        pnlGrades.Visible = False
        pnlRequests.Visible = False
        pnlLogs.Visible = False

        btnPrimary.Visible = True

        Select Case mode
            Case FormMode.EnterGrades
                Me.Text = "Enter Grades"
                Me.ClientSize = New Size(460, 470)
                pnlGrades.Visible = True
                btnPrimary.Text = "Save Grade"
                txtGrade.Clear()
                LoadStudents()
                LoadSubjects()
                cboStudent.SelectedIndex = -1
                cboSubject.SelectedIndex = -1

            Case FormMode.ViewRequests
                Me.Text = "Pending Requests"
                Me.ClientSize = New Size(700, 420)
                pnlRequests.Visible = True
                btnPrimary.Text = "Approve"

                LoadRequests()

            Case FormMode.ViewAuditLogs
                Me.Text = "Audit Logs"
                Me.ClientSize = New Size(780, 450)
                pnlLogs.Visible = True
                btnPrimary.Visible = False
                LoadLogs()
        End Select

        Me.CenterToParent()
    End Sub

    ' ===== Bottom bar buttons =====
    Private Sub btnPrimary_Click(sender As Object, e As EventArgs) Handles btnPrimary.Click
        Select Case _mode
            Case FormMode.EnterGrades : SaveGrade()
            Case FormMode.ViewRequests : ApproveSelected()
        End Select
    End Sub





    ' ===== Enter Grades =====
    Private Sub SaveGrade()
        Dim grade As Decimal
        If cboStudent.SelectedIndex = -1 OrElse cboSubject.SelectedIndex = -1 Then
            MessageBox.Show("Select a student and a subject.")
            Return
        End If
        If Not Decimal.TryParse(txtGrade.Text, grade) Then
            MessageBox.Show("Enter a valid numeric grade.")
            Return
        End If
        ' TODO: save to MySQL + audit log row
        MessageBox.Show("Grade saved!")
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub LoadStudents()
        cboStudent.Items.Clear()
        cboStudent.Items.AddRange(New String() {
            "Mariano, Geoff Andreii",
            "Condes, Hope Gian D.",
            "Lalata, Carl Jade",
            "Santos, Maria Angelica",
            "Oropilla, Charles",
            "Puyat, Ron Jacob",
            "Buluran, Family I.",
            "Rodas, Rodas P.",
            "Ramos, Reago W.",
            "Salceda, Poknat X."
        })
    End Sub

    Private Sub LoadSubjects()
        cboSubject.Items.Clear()
        cboSubject.Items.AddRange(New String() {"DBMS2", "IT101"})   ' temporary test data
    End Sub

    ' ===== View Requests =====
    Private Sub ApproveSelected()
        If dgvrequests.CurrentRow Is Nothing Then Return
        ' TODO: approve + audit log row
        LoadRequests()
    End Sub

    Private Sub RejectSelected()
        If dgvrequests.CurrentRow Is Nothing Then Return
        ' TODO: reject + audit log row
        LoadRequests()
    End Sub

    Private Sub LoadRequests()
        dgvrequests.AutoGenerateColumns = True

        Dim dt As New DataTable()
        For Each c In {"Student", "Assessment", "Current Grade", "Requested", "Reason", "Status"}
            dt.Columns.Add(c)
        Next
        dt.Rows.Add("Lalata, Carl Jade", "Quiz#1", "18/20", "+2pts", "Misread Answer", "Pending")
        dt.Rows.Add("Condes, Hope Gian D.", "Final Exam", "--", "Recheck", "Item 7 correct", "Pending")
        dt.Rows.Add("Mariano, Geoff Andreii", "Activity#2", "43/50", "+5pts", "Late Submission", "Approved")

        dgvrequests.DataSource = dt
        StyleGrid(dgvrequests)
        dgvrequests.ClearSelection()
    End Sub

    ' ===== Audit Logs =====
    Private Sub LoadLogs()
        dgvLogs.AutoGenerateColumns = True

        Dim dt As New DataTable()
        For Each c In {"Action", "Student", "Item", "Date & Time", "Changed By"}
            dt.Columns.Add(c)
        Next
        dt.Rows.Add("Grades Updated", "Condes, Hope Gian D.", "Midterms", "2026-08-24 09:14", "Prof. Jhun")
        dt.Rows.Add("Revision Approved", "Mariano, Geoff Andreii", "Activity#2", "2026-08-23 15:40", "Prof. Jhun")
        dt.Rows.Add("Grades Updated", "Lalata, Carl Jade", "Quiz#1", "2026-08-22 11:05", "Prof. Jhun")

        dgvLogs.DataSource = dt
        StyleGrid(dgvLogs)
        dgvLogs.ClearSelection()
    End Sub

    ' ===== Shared grid styling =====
    Private Sub StyleGrid(g As DataGridView)
        With g
            .AllowUserToAddRows = False          ' removes the empty * row
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .ReadOnly = True                     ' no editing
            .RowHeadersVisible = False           ' removes the left gray column
            .MultiSelect = False
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect

            .BackgroundColor = Color.White       ' removes the gray area
            .BorderStyle = BorderStyle.None
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            .GridColor = Color.Gainsboro
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill   ' fills the width
            .RowTemplate.Height = 32

            .EnableHeadersVisualStyles = False
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 34
            .ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            With .ColumnHeadersDefaultCellStyle
                .BackColor = Color.FromArgb(224, 247, 250)
                .ForeColor = Color.FromArgb(0, 80, 0)
                .Font = New Font("Segoe UI", 9, FontStyle.Bold)
                .SelectionBackColor = Color.FromArgb(224, 247, 250)
                .SelectionForeColor = Color.FromArgb(0, 80, 0)
            End With

            With .DefaultCellStyle
                .BackColor = Color.White
                .ForeColor = Color.FromArgb(0, 100, 0)
                .Font = New Font("Segoe UI", 9)
                .SelectionBackColor = Color.FromArgb(235, 250, 240)
                .SelectionForeColor = Color.FromArgb(0, 100, 0)
            End With

            For Each c As DataGridViewColumn In .Columns
                c.SortMode = DataGridViewColumnSortMode.NotSortable
            Next
        End With
    End Sub

    Private Sub AddLogColumn(name As String, header As String, weight As Single)
        Dim c As New DataGridViewTextBoxColumn()
        c.Name = name
        c.DataPropertyName = name
        c.HeaderText = header
        c.FillWeight = weight
        dgvLogs.Columns.Add(c)
    End Sub

End Class