Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Drawing
Imports System.Drawing.Printing

Public Class quickactions
    Public Property OnlyOpenRequests As Boolean

    Public Enum FormMode
        EnterGrades
        ViewRequests
        ViewAuditLogs
    End Enum
    Private _currentMode As FormMode = FormMode.EnterGrades

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
        _currentMode = mode
        Dim lblTitle = FindControl(Of Label)("lblTitle")
        If lblTitle Is Nothing Then lblTitle = FindControl(Of Label)("lblHeader")

        Dim pnlGrades = FindControl(Of Panel)("pnlGrades")
        Dim pnlRequests = FindControl(Of Panel)("pnlRequests")
        Dim pnlLogs = FindControl(Of Panel)("pnlLogs")


        Select Case mode
            Case FormMode.EnterGrades

                If lblTitle IsNot Nothing Then
                    lblTitle.Text = "Quick Grade Entry"
                End If

                pnlGrades.Visible = True
                pnlRequests.Visible = False
                pnlLogs.Visible = False

                pnlGrades.BringToFront()

                btnPrimary.Text = "Save"
                btnPrimary.Enabled = True

                LoadClassesForGradeEntry()

            Case FormMode.ViewRequests

                If lblTitle IsNot Nothing Then
                    lblTitle.Text = "Revision Requests"
                End If

                pnlGrades.Visible = False
                pnlRequests.Visible = True
                pnlLogs.Visible = False

                pnlRequests.BringToFront()

                btnPrimary.Text = "Export PDF"
                btnPrimary.Enabled = True

                LoadLiveRequests()

            Case FormMode.ViewAuditLogs

                If lblTitle IsNot Nothing Then
                    lblTitle.Text = "System Audit Logs"
                End If

                pnlGrades.Visible = False
                pnlRequests.Visible = False
                pnlLogs.Visible = True

                pnlLogs.BringToFront()

                btnPrimary.Text = "Export PDF"
                btnPrimary.Enabled = True

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
                           If(OnlyOpenRequests, "WHERE r.status IN ('Pending','Under Review') ", "") &
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

        Try
            ' Use the existing Subject dropdown as the Class selector
            cboSubject.DataSource = Nothing
            cboSubject.Items.Clear()
            cboSubject.DropDownStyle = ComboBoxStyle.DropDownList

            Dim sql As String =
                "SELECT c.class_id, " &
                "CONCAT(c.course_code, ' - ', co.course_name, " &
                "' (Section ', c.section, ')') AS class_title " &
                "FROM `class` c " &
                "JOIN course co ON c.course_code = co.course_code " &
                "ORDER BY c.course_code, c.section"

            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql)

            cboSubject.DisplayMember = "class_title"
            cboSubject.ValueMember = "class_id"
            cboSubject.DataSource = dt

            ' Require the Admin to explicitly select a class
            cboSubject.SelectedIndex = -1

        Catch ex As Exception

            MessageBox.Show(
                "Unable to load classes: " & ex.Message,
                "Quick Grade Entry",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

        End Try

    End Sub

    Private Sub LoadStudentsForSelectedClass()

        ' Clear students from any previous class
        cboStudent.DataSource = Nothing
        cboStudent.Items.Clear()
        cboStudent.DropDownStyle = ComboBoxStyle.DropDownList
        cboStudent.SelectedIndex = -1
        cboStudent.Enabled = False

        ' Make sure a class has been selected
        If cboSubject.SelectedIndex = -1 OrElse
       cboSubject.SelectedValue Is Nothing OrElse
       TypeOf cboSubject.SelectedValue Is DataRowView Then
            Return
        End If

        Dim classId As Integer

        If Not Integer.TryParse(
        cboSubject.SelectedValue.ToString(),
        classId
    ) Then
            Return
        End If

        Try

            Dim sql As String =
            "SELECT e.enrollment_id, " &
            "CONCAT(s.full_name, ' (', s.student_id, ')') AS student_name " &
            "FROM enrollment e " &
            "INNER JOIN student s ON e.student_id = s.student_id " &
            "WHERE e.class_id = @ClassId " &
            "ORDER BY s.full_name"

            Dim parameters As MySqlParameter() = {
            New MySqlParameter("@ClassId", classId)
        }

            Dim dt As DataTable =
            DatabaseHelper.ExecuteQuery(sql, parameters)

            cboStudent.DisplayMember = "student_name"
            cboStudent.ValueMember = "enrollment_id"
            cboStudent.DataSource = dt
            cboStudent.SelectedIndex = -1
            cboStudent.Enabled = (dt IsNot Nothing AndAlso dt.Rows.Count > 0)

        Catch ex As Exception

            MessageBox.Show(
            "Unable to load students: " & ex.Message,
            "Quick Grade Entry",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        )

        End Try

    End Sub

    Private Sub LoadAssessmentsForSelectedClass()

        cboAssessment.DataSource = Nothing
        cboAssessment.Items.Clear()
        cboAssessment.Enabled = False

        If cboSubject.SelectedIndex = -1 OrElse
       cboSubject.SelectedValue Is Nothing OrElse
       TypeOf cboSubject.SelectedValue Is DataRowView Then
            Return
        End If

        Dim classId As Integer

        If Not Integer.TryParse(
        cboSubject.SelectedValue.ToString(),
        classId
    ) Then
            Return
        End If

        Try

            Dim sql As String =
            "SELECT assessment_id, total_score, " &
            "CONCAT(title, ' (', assessment_type, ', Max: ', " &
            "total_score, ' pts)') AS assessment_name " &
            "FROM assessment " &
            "WHERE class_id = @ClassId " &
            "ORDER BY assessment_id"

            Dim parameters As MySqlParameter() = {
            New MySqlParameter("@ClassId", classId)
        }

            Dim dt As DataTable =
            DatabaseHelper.ExecuteQuery(sql, parameters)

            cboAssessment.DisplayMember = "assessment_name"
            cboAssessment.ValueMember = "assessment_id"
            cboAssessment.DataSource = dt
            cboAssessment.SelectedIndex = -1
            cboAssessment.Enabled = (dt IsNot Nothing AndAlso dt.Rows.Count > 0)

        Catch ex As Exception

            MessageBox.Show(
            "Unable to load assessments: " & ex.Message,
            "Quick Grade Entry",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        )

        End Try

    End Sub

    Private Function TryValidateGrade(ByRef score As Decimal) As Boolean

        If cboSubject.SelectedIndex < 0 OrElse
       cboStudent.SelectedIndex < 0 OrElse
       cboAssessment.SelectedIndex < 0 Then

            MessageBox.Show(
            "Please select a class, student, and assessment.",
            "Incomplete Information",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )
            Return False
        End If

        If Not Decimal.TryParse(txtGrade.Text.Trim(), score) Then
            MessageBox.Show(
            "Please enter a valid numeric score.",
            "Invalid Score",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )
            Return False
        End If

        Dim assessmentRow As DataRowView =
        TryCast(cboAssessment.SelectedItem, DataRowView)

        If assessmentRow Is Nothing Then
            MessageBox.Show(
            "Please select a valid assessment.",
            "Invalid Assessment",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )
            Return False
        End If

        Dim maxScore As Decimal =
        Convert.ToDecimal(assessmentRow("total_score"))

        If score < 0D OrElse score > maxScore Then
            MessageBox.Show(
            $"Score must be between 0 and {maxScore}.",
            "Invalid Score",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )
            Return False
        End If

        Return True

    End Function

    Private Sub cboSubject_SelectedIndexChanged(
        sender As Object,
        e As EventArgs
    ) Handles cboSubject.SelectedIndexChanged

        LoadStudentsForSelectedClass()
        LoadAssessmentsForSelectedClass()

    End Sub



    Private Sub btnPrimary_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnPrimary.Click


        ' Export Revision Requests
        If _currentMode = FormMode.ViewRequests Then
            ExportRequestsToPdf()
            Return
        End If

        ' Export System Audit Logs
        If _currentMode = FormMode.ViewAuditLogs Then
            ExportAuditLogsToPdf()
            Return
        End If

        ' Only Admin Enter Grades mode can save scores.
        If _currentMode <> FormMode.EnterGrades Then Return


        ' Confirm the current user is an Administrator.
        If Not AppSession.IsAdmin Then
            MessageBox.Show(
                "Administrator access is required.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
            Return
        End If

        ' Reuse our existing score validation.
        Dim score As Decimal = 0D

        If Not TryValidateGrade(score) Then Return

        ' Retrieve the selected enrollment and assessment IDs.
        Dim enrollmentId As Integer
        Dim assessmentId As Integer

        If Not Integer.TryParse(
            cboStudent.SelectedValue.ToString(),
            enrollmentId
        ) OrElse Not Integer.TryParse(
            cboAssessment.SelectedValue.ToString(),
            assessmentId
        ) Then
            MessageBox.Show(
                "Invalid student or assessment selection.",
                "Quick Grade Entry",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
            Return
        End If

        ' Ask for confirmation before changing a grade.
        Dim message As String =
            "Student: " & cboStudent.Text & vbCrLf &
            "Class: " & cboSubject.Text & vbCrLf &
            "Assessment: " & cboAssessment.Text & vbCrLf &
            "Score: " & score.ToString() & vbCrLf & vbCrLf &
            "Do you want to save this score?"

        Dim confirmation As DialogResult =
            MessageBox.Show(
                message,
                "Confirm Grade Entry",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

        If confirmation <> DialogResult.Yes Then Return

        ' Prevent repeated clicks while saving.
        btnPrimary.Enabled = False

        Try
            Dim gradeService As New GradeService()

            Dim result As OperationResult =
                gradeService.RecordScoreAsAdmin(
                    enrollmentId,
                    assessmentId,
                    Convert.ToDouble(score),
                    "Grade entered through Admin Quick Grade Entry"
                )

            If result.Success Then

                MessageBox.Show(
                    result.Message,
                    "Grade Entry Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )

                ' Clear the score to avoid accidental repeat entry.
                txtGrade.Clear()
                txtGrade.Focus()

            Else

                MessageBox.Show(
                    result.Message,
                    "Unable to Save Grade",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )

            End If

        Catch ex As Exception

            MessageBox.Show(
                "An unexpected error occurred: " & ex.Message,
                "Grade Entry Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

        Finally
            btnPrimary.Enabled = True
        End Try

    End Sub


    Private Sub ExportRequestsToPdf()

        Dim dt As DataTable = TryCast(dgvrequests.DataSource, DataTable)

        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            MessageBox.Show(
            "There are no revision requests to export.",
            "Export PDF",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )
            Return
        End If

        ' ExportService requires a PDF printer for a proper PDF file.
        Dim pdfPrinterFound As Boolean = False

        For Each printer As String In PrinterSettings.InstalledPrinters
            If String.Equals(printer, "Microsoft Print to PDF",
                         StringComparison.OrdinalIgnoreCase) Then
                pdfPrinterFound = True
                Exit For
            End If
        Next

        If Not pdfPrinterFound Then
            MessageBox.Show(
            "Microsoft Print to PDF is not installed or enabled.",
            "Export PDF",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )
            Return
        End If

        Using dialog As New SaveFileDialog()

            dialog.Title = "Export Revision Requests"
            dialog.Filter = "PDF Files (*.pdf)|*.pdf"
            dialog.DefaultExt = "pdf"
            dialog.AddExtension = True
            dialog.FileName =
            "RevisionRequests_" &
            DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".pdf"

            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

            ' Create the complete report from the loaded database records.
            Dim reportLines As New List(Of String)()

            For i As Integer = 0 To dt.Rows.Count - 1

                Dim r As DataRow = dt.Rows(i)

                reportLines.Add(
                "REQUEST " & (i + 1).ToString() &
                " OF " & dt.Rows.Count.ToString()
            )
                reportLines.Add(New String("-"c, 85))

                AddReportField(reportLines, "Student",
                           Convert.ToString(r("student_name")))
                AddReportField(reportLines, "Assessment",
                           Convert.ToString(r("assessment_title")))
                AddReportField(reportLines, "Current Grade",
                           Convert.ToString(r("current_grade")))
                AddReportField(reportLines, "Requested",
                           Convert.ToString(r("requested")))
                AddReportField(reportLines, "Status",
                           Convert.ToString(r("status")))
                AddReportField(reportLines, "Reason",
                           Convert.ToString(r("reason")))

                reportLines.Add("")
                reportLines.Add("")

            Next

            Dim lineIndex As Integer = 0
            Dim pageNumber As Integer = 0

            Using doc As New PrintDocument()
                Using bodyFont As New Font("Consolas", 9.0F)
                    Using titleFont As New Font(
                    "Segoe UI", 14.0F, FontStyle.Bold)

                        doc.DocumentName = "Revision Requests Report"
                        doc.DefaultPageSettings.Margins =
                        New Margins(60, 60, 70, 60)

                        AddHandler doc.PrintPage,
                        Sub(sender As Object, e As PrintPageEventArgs)

                            pageNumber += 1

                            Dim g As Graphics = e.Graphics
                            Dim x As Single = e.MarginBounds.Left
                            Dim y As Single = e.MarginBounds.Top

                            g.DrawString(
                                "REVISION REQUESTS REPORT",
                                titleFont,
                                Brushes.DarkGreen,
                                x, y
                            )

                            y += 35

                            g.DrawString(
                                "Generated: " &
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                                bodyFont,
                                Brushes.Black,
                                x, y
                            )

                            y += 35

                            Dim lineHeight As Single =
                                bodyFont.GetHeight(g) + 4

                            Dim bottom As Single =
                                e.MarginBounds.Bottom - 25

                            While lineIndex < reportLines.Count AndAlso
                                  y + lineHeight <= bottom

                                g.DrawString(
                                    reportLines(lineIndex),
                                    bodyFont,
                                    Brushes.Black,
                                    x, y
                                )

                                y += lineHeight
                                lineIndex += 1

                            End While

                            g.DrawString(
                                "Page " & pageNumber.ToString(),
                                bodyFont,
                                Brushes.Gray,
                                x,
                                e.MarginBounds.Bottom - 15
                            )

                            e.HasMorePages =
                                (lineIndex < reportLines.Count)

                        End Sub

                        Dim exporter As New ExportService()

                        If exporter.ExportToPDF(doc, dialog.FileName) Then

                            If System.IO.File.Exists(dialog.FileName) AndAlso
                           New System.IO.FileInfo(dialog.FileName).Length > 0 Then

                                MessageBox.Show(
                                "Revision requests exported successfully!" &
                                vbCrLf & vbCrLf & dialog.FileName,
                                "Export Complete",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            )

                            Else

                                MessageBox.Show(
                                "Printing finished, but the PDF could not " &
                                "be verified at the selected location.",
                                "Export PDF",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            )

                            End If

                        End If

                    End Using
                End Using
            End Using

        End Using

    End Sub


    Private Sub ExportAuditLogsToPdf()

        Dim dt As DataTable = TryCast(dgvLogs.DataSource, DataTable)

        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            MessageBox.Show(
            "There are no audit logs to export.",
            "Export PDF",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )
            Return
        End If

        Dim pdfPrinterFound As Boolean = False

        For Each printer As String In PrinterSettings.InstalledPrinters
            If String.Equals(
            printer,
            "Microsoft Print to PDF",
            StringComparison.OrdinalIgnoreCase
        ) Then
                pdfPrinterFound = True
                Exit For
            End If
        Next

        If Not pdfPrinterFound Then
            MessageBox.Show(
            "Microsoft Print to PDF is not installed or enabled.",
            "Export PDF",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )
            Return
        End If

        Using dialog As New SaveFileDialog()

            dialog.Title = "Export System Audit Logs"
            dialog.Filter = "PDF Files (*.pdf)|*.pdf"
            dialog.DefaultExt = "pdf"
            dialog.AddExtension = True
            dialog.FileName =
            "AuditLogs_" &
            DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".pdf"

            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

            Dim reportLines As New List(Of String)()

            For i As Integer = 0 To dt.Rows.Count - 1

                Dim r As DataRow = dt.Rows(i)

                reportLines.Add(
                "AUDIT LOG " & (i + 1).ToString() &
                " OF " & dt.Rows.Count.ToString()
            )

                reportLines.Add(New String("-"c, 85))

                AddReportField(
                reportLines, "Log ID",
                Convert.ToString(r("Log ID"))
            )

                AddReportField(
                reportLines, "User",
                Convert.ToString(r("User"))
            )

                AddReportField(
                reportLines, "Action",
                Convert.ToString(r("Action"))
            )

                AddReportField(
                reportLines, "Target Entity",
                Convert.ToString(r("Target Entity"))
            )

                AddReportField(
                reportLines, "Old Value",
                Convert.ToString(r("Old Value"))
            )

                AddReportField(
                reportLines, "New Value",
                Convert.ToString(r("New Value"))
            )

                AddReportField(
                reportLines, "Timestamp",
                Convert.ToString(r("Timestamp"))
            )

                AddReportField(
                reportLines, "Details",
                Convert.ToString(r("Details"))
            )

                reportLines.Add("")
                reportLines.Add("")

            Next

            Dim lineIndex As Integer = 0
            Dim pageNumber As Integer = 0
            Dim generatedAt As DateTime = DateTime.Now

            Using doc As New PrintDocument()
                Using bodyFont As New Font("Consolas", 9.0F)
                    Using titleFont As New Font(
                    "Segoe UI", 14.0F, FontStyle.Bold)

                        doc.DocumentName = "System Audit Logs Report"

                        doc.DefaultPageSettings.Margins =
                        New Margins(60, 60, 70, 60)

                        AddHandler doc.PrintPage,
                        Sub(sender As Object, e As PrintPageEventArgs)

                            pageNumber += 1

                            Dim g As Graphics = e.Graphics
                            Dim x As Single = e.MarginBounds.Left
                            Dim y As Single = e.MarginBounds.Top

                            g.DrawString(
                                "SYSTEM AUDIT LOGS REPORT",
                                titleFont,
                                Brushes.DarkGreen,
                                x, y
                            )

                            y += 35

                            g.DrawString(
                                "Generated: " &
                                generatedAt.ToString("yyyy-MM-dd HH:mm"),
                                bodyFont,
                                Brushes.Black,
                                x, y
                            )

                            y += 35

                            Dim lineHeight As Single =
                                bodyFont.GetHeight(g) + 4

                            Dim bottom As Single =
                                e.MarginBounds.Bottom - 25

                            While lineIndex < reportLines.Count AndAlso
                                  y + lineHeight <= bottom

                                g.DrawString(
                                    reportLines(lineIndex),
                                    bodyFont,
                                    Brushes.Black,
                                    x, y
                                )

                                y += lineHeight
                                lineIndex += 1

                            End While

                            g.DrawString(
                                "Page " & pageNumber.ToString(),
                                bodyFont,
                                Brushes.Gray,
                                x,
                                e.MarginBounds.Bottom - 15
                            )

                            e.HasMorePages =
                                (lineIndex < reportLines.Count)

                        End Sub

                        Dim exporter As New ExportService()

                        If exporter.ExportToPDF(doc, dialog.FileName) Then

                            If System.IO.File.Exists(dialog.FileName) AndAlso
                           New System.IO.FileInfo(dialog.FileName).Length > 0 Then

                                MessageBox.Show(
                                "Audit logs exported successfully!" &
                                vbCrLf & vbCrLf & dialog.FileName,
                                "Export Complete",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            )

                            Else

                                MessageBox.Show(
                                "The PDF could not be verified at " &
                                "the selected location.",
                                "Export PDF",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            )

                            End If

                        End If

                    End Using
                End Using
            End Using

        End Using

    End Sub



    Private Sub AddReportField(
    lines As List(Of String),
    fieldName As String,
    fieldValue As String
)

        Dim prefix As String = fieldName & ": "
        Dim continuation As String = New String(" "c, prefix.Length)

        Dim normalized As String =
        If(fieldValue, "").Replace(vbCrLf, vbLf).
        Replace(vbCr, vbLf)

        Dim paragraphs As String() = normalized.Split(ChrW(10))

        For Each paragraph As String In paragraphs

            Dim remaining As String = paragraph.Trim()
            Dim currentPrefix As String = prefix

            If remaining.Length = 0 Then
                lines.Add(currentPrefix)
                prefix = continuation
                Continue For
            End If

            While remaining.Length > 0

                Dim available As Integer = 85 - currentPrefix.Length

                If remaining.Length <= available Then
                    lines.Add(currentPrefix & remaining)
                    Exit While
                End If

                Dim breakAt As Integer =
                remaining.LastIndexOf(" "c, available)

                If breakAt <= 0 Then breakAt = available

                lines.Add(
                currentPrefix &
                remaining.Substring(0, breakAt).TrimEnd()
            )

                remaining = remaining.Substring(breakAt).TrimStart()
                currentPrefix = continuation

            End While

            prefix = continuation

        Next

    End Sub


End Class