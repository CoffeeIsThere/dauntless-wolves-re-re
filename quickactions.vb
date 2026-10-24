Imports System.Data
Imports MySql.Data.MySqlClient

Public Class quickactions

    Public Enum FormMode
        EnterGrades
        ViewRequests
        ViewAuditLogs
    End Enum

    Private Sub quickactions_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Safely attach click listener to close button without requiring designer WithEvents
        Dim btn = FindControl(Of Button)("btnClose")
        If btn Is Nothing Then btn = FindControl(Of Button)("btnCancel")
        If btn IsNot Nothing Then
            RemoveHandler btn.Click, AddressOf CloseForm
            AddHandler btn.Click, AddressOf CloseForm
        End If
    End Sub

    Private Sub CloseForm(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    ' Helper method to resolve controls safely from the designer form
    Private Function FindControl(Of T As Control)(varName As String) As T
        Dim matches = Me.Controls.Find(varName, True)
        If matches.Length > 0 AndAlso TypeOf matches(0) Is T Then
            Return DirectCast(matches(0), T)
        End If
        Return Nothing
    End Function

    Public Sub ShowMode(mode As FormMode)
        Dim lblTitle = FindControl(Of Label)("lblTitle")
        If lblTitle Is Nothing Then lblTitle = FindControl(Of Label)("lblHeader")

        Dim pnlGrades = FindControl(Of Panel)("pnlGrades")
        Dim pnlRequests = FindControl(Of Panel)("pnlRequests")
        Dim pnlLogs = FindControl(Of Panel)("pnlLogs")

        Select Case mode
            Case FormMode.EnterGrades
                If lblTitle IsNot Nothing Then lblTitle.Text = "Quick Grade Entry"
                If pnlGrades IsNot Nothing Then
                    pnlGrades.Visible = True
                    pnlGrades.BringToFront()
                End If
                LoadClassesForGradeEntry()

            Case FormMode.ViewRequests
                If lblTitle IsNot Nothing Then lblTitle.Text = "Pending Revision Requests"
                If pnlRequests IsNot Nothing Then
                    pnlRequests.Visible = True
                    pnlRequests.BringToFront()
                End If
                LoadLiveRequests()

            Case FormMode.ViewAuditLogs
                If lblTitle IsNot Nothing Then lblTitle.Text = "System Audit Logs"
                If pnlLogs IsNot Nothing Then
                    pnlLogs.Visible = True
                    pnlLogs.BringToFront()
                End If
                LoadLiveAuditLogs()
        End Select
    End Sub

    ' ==========================================
    ' 1. LIVE REVISION REQUESTS (MariaDB)
    ' ==========================================
    Private Sub LoadLiveRequests()
        Dim dgvRequests = FindControl(Of DataGridView)("dgvRequests")
        If dgvRequests Is Nothing Then Return

        Dim sql As String = "SELECT " &
                           "    r.request_id, " &
                           "    s.full_name AS student_name, " &
                           "    a.title AS assessment_title, " &
                           "    IF(sg.score IS NULL, '--', CONCAT(CAST(sg.score AS CHAR), '/', CAST(a.total_score AS CHAR))) AS current_grade, " &
                           "    IF(r.requested_points IS NULL, 'Recheck', CONCAT('+', CAST(r.requested_points AS CHAR), ' pts')) AS requested, " &
                           "    r.reason, " &
                           "    r.status " &
                           "FROM revisionrequest r " &
                           "JOIN studentgrade sg ON r.grade_id = sg.grade_id " &
                           "JOIN assessment a ON sg.assessment_id = a.assessment_id " &
                           "JOIN enrollment e ON sg.enrollment_id = e.enrollment_id " &
                           "JOIN student s ON e.student_id = s.student_id " &
                           "ORDER BY r.requested_at DESC"

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql)

        If dt IsNot Nothing Then
            dgvRequests.DataSource = dt

            ' Format Header Column Titles
            If dgvRequests.Columns.Contains("student_name") Then
                dgvRequests.Columns("student_name").HeaderText = "Student"
                dgvRequests.Columns("assessment_title").HeaderText = "Assessment"
                dgvRequests.Columns("current_grade").HeaderText = "Current Grade"
                dgvRequests.Columns("requested").HeaderText = "Requested"
                dgvRequests.Columns("reason").HeaderText = "Reason"
                dgvRequests.Columns("status").HeaderText = "Status"
            End If

            If dgvRequests.Columns.Contains("request_id") Then
                dgvRequests.Columns("request_id").Visible = False
            End If

            dgvRequests.ClearSelection()
        End If
    End Sub

    ' ==========================================
    ' 2. LIVE AUDIT LOGS (MariaDB)
    ' ==========================================
    Private Sub LoadLiveAuditLogs()
        Dim dgvAuditLogs = FindControl(Of DataGridView)("dgvAuditLogs")
        If dgvAuditLogs Is Nothing Then dgvAuditLogs = FindControl(Of DataGridView)("dgvLogs")
        If dgvAuditLogs Is Nothing Then dgvAuditLogs = FindControl(Of DataGridView)("dgvAudit")
        If dgvAuditLogs Is Nothing Then Return

        Dim sql As String = "SELECT " &
                           "    al.log_id AS `Log ID`, " &
                           "    IFNULL(s.full_name, IFNULL(t.full_name, IFNULL(adm.full_name, 'System'))) AS `User`, " &
                           "    al.action AS `Action`, " &
                           "    al.entity_type AS `Target Entity`, " &
                           "    IFNULL(al.old_value, '---') AS `Old Value`, " &
                           "    IFNULL(al.new_value, '---') AS `New Value`, " &
                           "    DATE_FORMAT(al.logged_at, '%Y-%m-%d %H:%i:%s') AS `Timestamp`, " &
                           "    IFNULL(al.details, 'N/A') AS `Details` " &
                           "FROM auditlog al " &
                           "LEFT JOIN student s ON al.student_id = s.student_id " &
                           "LEFT JOIN teacher t ON al.teacher_id = t.teacher_id " &
                           "LEFT JOIN admin adm ON al.admin_id = adm.admin_id " &
                           "ORDER BY al.logged_at DESC"

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql)

        If dt IsNot Nothing Then
            dgvAuditLogs.DataSource = dt
            dgvAuditLogs.ClearSelection()
        End If
    End Sub

    ' ==========================================
    ' 3. CLASS SELECTOR FOR QUICK GRADE ENTRY
    ' ==========================================
    Private Sub LoadClassesForGradeEntry()
        Dim cmbClasses = FindControl(Of ComboBox)("cmbClasses")
        If cmbClasses Is Nothing Then cmbClasses = FindControl(Of ComboBox)("cboclass")
        If cmbClasses Is Nothing Then Return

        Dim sql As String = "SELECT c.class_id, CONCAT(co.course_code, ' - ', co.course_name, ' (', c.section, ')') AS class_title " &
                           "FROM class c " &
                           "JOIN course co ON c.course_code = co.course_code"
        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql)
        cmbClasses.DataSource = dt
        cmbClasses.DisplayMember = "class_title"
        cmbClasses.ValueMember = "class_id"
    End Sub

End Class