Imports System.Data
Imports MySql.Data.MySqlClient

Public Class frmGradebook
    Private ReadOnly _exportService As New ExportService()
    Private ReadOnly _classService As New ClassService()
    Private ReadOnly _gradeService As New GradeService()
    Private ReadOnly _userService As New UserService()
    ' ===== Grading setup (weights from the gradesheet; change here if needed) =====
    Private Const W_PART As Double = 5
    Private Const W_EXTRA As Double = 0
    Private Const W_CW As Double = 10
    Private Const W_QUIZ As Double = 20
    Private Const W_PROJECT As Double = 25
    Private Const W_EXAM As Double = 40
    Private Const PASSING As Double = 75

    ' Suggestions in the Add Assessment popup (the prof can also type any other type)
    Private ReadOnly PresetTypes As String() = {"Quiz", "Activity", "Exam", "Project", "Attendance"}

    ' Assessments the prof has added (starts empty)
    Private Class AssessItem
        Public Key As String
        Public Title As String
        Public Type As String     ' any name: Quiz, Homework, Laboratory, ...
        Public Weight As Double
        Public Group As String    ' "Quiz" or "CW"
    End Class
    Private loadingClass As Boolean
    Private refreshingClassChoices As Boolean
    Private finishingEdit As Boolean
    Private reloadQueued As Boolean
    Private reloadAfterEdit As Boolean
    Private loadedClassId As Integer
    Private originalEditValue As Object
    Private assessmentIds As New Dictionary(Of String, Integer)
    Private items As New List(Of AssessItem)

    ' Class info (from Add Class / imports)
    Private Class ClassInfo
        Public Code As String
        Public Name As String
        Public Schedule As String
        Public Term As String
        Public SY As String
        Public Id As Integer
    End Class
    Private classes As New Dictionary(Of String, ClassInfo)(StringComparer.OrdinalIgnoreCase)
    Private classTip As New ToolTip()

    ' Highest possible score per Raw column (double-click a Raw header to change it)
    Private maxScores As New Dictionary(Of String, Double) From {{"ExamRaw", 100}}

    ' ===== Look =====
    Private ReadOnly DarkGreen As Color = Color.FromArgb(0, 100, 0)
    Private ReadOnly LineColor As Color = Color.FromArgb(205, 218, 210)
    Private ReadOnly InputColor As Color = Color.FromArgb(255, 251, 224)   ' soft yellow = type here
    Private ReadOnly CalcColor As Color = Color.FromArgb(235, 243, 250)    ' soft blue = computed
    Private ReadOnly TotalColor As Color = Color.FromArgb(226, 243, 233)   ' soft green = grade
    Private ReadOnly CellFont As New Font("Segoe UI", 9.5F)
    Private ReadOnly BoldFont As New Font("Segoe UI Semibold", 9.5F)

    Private lblSummary As New Label()
    Private lblMeetings As New Label()
    Private txtMeetings As New TextBox()

    Private assessmentOwner As Form

    Public Sub ShowAssessmentForClass(classId As Integer, owner As Form)
        assessmentOwner = owner
        InitialClassId = classId
        ' Prepare the shared assessment workflow without showing the gradebook window.
        frmGradebook_Load(Me, EventArgs.Empty)
        OpenAddAssessment()
    End Sub

    Public Property InitialClassId As Integer

    Public Sub SelectClass(classId As Integer)
        For Each item In cboCourse.Items
            If classes.ContainsKey(item.ToString()) AndAlso classes(item.ToString()).Id = classId Then
                cboCourse.SelectedItem = item
                Return
            End If
        Next
    End Sub

    Public Sub OpenAddAssessment()
        btnAddAssessment_Click(Me, EventArgs.Empty)
    End Sub

    Private Sub frmGradebook_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "The Dauntless Grading System — Gradebook"
        Me.BackColor = Color.White
        EnableDoubleBuffer()
        SetupGrid()
        CreateMeetingsBox()
        CreateSummaryLabel()
        HookUpRevisionButton()
        lblSummary.Tag = "Assessment scores and running grades use the same assessment weights as My Grades. Ungraded assessments are excluded."
        classTip.SetToolTip(dgvGrades, CStr(lblSummary.Tag))

        cboCourse.DropDownStyle = ComboBoxStyle.DropDownList

        ' Load real active classes from MySQL database
        LoadClassesFromDatabase()

        SelectClass(InitialClassId)
        ConfigureToolbar()
        UpdateSummary()
    End Sub

    Private Sub LoadClassesFromDatabase()
        UiKit.TryLoadData(Me, "assigned classes", AddressOf LoadClassesFromDatabaseCore)
    End Sub

    Private Sub LoadClassesFromDatabaseCore()
        Dim previous = GetClassIdFromCode(SelectedClassCode())
        Dim dt = _classService.GetClassesByTeacher(AppSession.CurrentUser.UserId)
        refreshingClassChoices = True
        Try
            cboCourse.Items.Clear()
            classes.Clear()
            For Each row As DataRow In dt.Rows
                Dim label = row("class_label").ToString()
                classes(label) = New ClassInfo With {.Code = row("course_code").ToString(), .Name = row("course_name").ToString(), .Schedule = row("schedule").ToString(), .Term = row("term").ToString(), .SY = row("academic_year").ToString(), .Id = CInt(row("class_id"))}
                cboCourse.Items.Add(label)
            Next
            If cboCourse.Items.Count > 0 Then cboCourse.SelectedIndex = 0
            SelectClass(previous)
        Finally
            refreshingClassChoices = False
        End Try
        LoadClassData()
    End Sub

    Public Sub RefreshAssignedClasses()
        LoadClassesFromDatabase()
    End Sub

    Private Sub frmGradebook_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        FillEmptyRows()
        dgvGrades.ClearSelection()
    End Sub

    ' Removes flicker when scrolling
    Private Sub EnableDoubleBuffer()
        GetType(DataGridView).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic) _
            .SetValue(dgvGrades, True, Nothing)
    End Sub

    Private Sub SetupGrid()
        With dgvGrades
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .AllowUserToResizeColumns = True
            .ReadOnly = False
            .RowHeadersVisible = False
            .BackgroundColor = Color.White
            .BorderStyle = BorderStyle.None
            .MultiSelect = True
            .SelectionMode = DataGridViewSelectionMode.CellSelect
            .EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2
            .ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

            .CellBorderStyle = DataGridViewCellBorderStyle.Single
            .GridColor = LineColor

            .EnableHeadersVisualStyles = False
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 56
            .ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            With .ColumnHeadersDefaultCellStyle
                .BackColor = DarkGreen
                .ForeColor = Color.White
                .Font = New Font("Segoe UI Semibold", 8.5F)
                .Alignment = DataGridViewContentAlignment.MiddleCenter
                .WrapMode = DataGridViewTriState.True
                .SelectionBackColor = DarkGreen
                .SelectionForeColor = Color.White
            End With

            .RowTemplate.Height = 28
            With .DefaultCellStyle
                .BackColor = Color.White
                .ForeColor = Color.FromArgb(30, 30, 30)
                .Font = CellFont
                .Alignment = DataGridViewContentAlignment.MiddleCenter
                .SelectionBackColor = Color.FromArgb(190, 228, 205)
                .SelectionForeColor = Color.Black
            End With

            ' Columns (same order as the gradesheet). Assessments are inserted before CWComp / QuizComp.
            .Columns.Clear()
            AddCol("No", "No.", "plain")
            AddCol("StudentName", "Student Name" & vbLf & "(Surname, Firstname, M.I.)", "plain")
            AddCol("Drp", "Drp" & vbLf & "(dropped)", "input")
            AddCol("Part", "Part" & vbLf & "(absences)", "input")
            AddCol("PartGrA", "Part GrA" & vbLf & "(" & W_PART & "%)", "calc")
            AddCol("Extra", "Extra in" & vbLf & "(%)", "input")
            AddCol("CWComp", "Class Work" & vbLf & "(" & W_CW & "%)", "calc")
            AddCol("QuizComp", "Quiz" & vbLf & "(" & W_QUIZ & "%)", "calc")
            AddCol("Project", "Project" & vbLf & "in (%)", "input")
            AddCol("ProjectGrA", "Project GrA" & vbLf & "(" & W_PROJECT & "%)", "calc")
            Dim ex As DataGridViewColumn = AddCol("ExamRaw", RawHeader("Exam", maxScores("ExamRaw")), "input")
            ex.ToolTipText = "Double-click to change the max score"
            AddCol("ExamGrA", "Exam" & vbLf & "GrA", "calc")
            AddCol("MajorExam", "Major Exam" & vbLf & "(" & W_EXAM & "%)", "calc")
            AddCol("Total", "Grade", "total")
            AddCol("Remarks", "Remarks", "remark")

            With .Columns("No")
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                .MinimumWidth = 40
                .Width = 44
            End With
            With .Columns("StudentName")
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                .MinimumWidth = 190
                .DefaultCellStyle.Padding = New Padding(10, 0, 10, 0)
                .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
            End With

            For Each obsolete In {"Drp", "Part", "PartGrA", "Extra", "Project", "ProjectGrA", "ExamRaw", "ExamGrA", "MajorExam"}
                .Columns(obsolete).Visible = False
            Next
            .Columns("CWComp").HeaderText = "Other assessments (%)"
            .Columns("QuizComp").HeaderText = "Quizzes (%)"
            .Columns("Total").HeaderText = "Running Grade (%)"
            ' Like Excel's "freeze panes"
            .Columns("StudentName").Frozen = True
        End With
    End Sub

    ' kind: "plain" (locked), "input" (yellow, typed), "calc" (blue, computed), "total", "remark"
    Private Function AddCol(name As String, header As String, kind As String,
                            Optional insertAt As Integer = -1) As DataGridViewColumn
        Dim c As New DataGridViewTextBoxColumn()
        c.Name = name
        c.HeaderText = header
        c.Tag = kind
        c.MinimumWidth = 64
        c.SortMode = DataGridViewColumnSortMode.NotSortable
        c.ReadOnly = (kind <> "input")
        If insertAt >= 0 Then
            dgvGrades.Columns.Insert(insertAt, c)
        Else
            dgvGrades.Columns.Add(c)
        End If
        Return c
    End Function

    Private Function RawHeader(title As String, maxScore As Double) As String
        Return title & vbLf & "Raw /" & maxScore
    End Function

    ' ===== "Number of Meetings" box (like the yellow cell on the gradesheet) =====
    Private Sub CreateMeetingsBox()
        Dim host As Control = btnAddAssessment.Parent

        With lblMeetings
            .AutoSize = False
            .Text = "Number of Meetings"
            .TextAlign = ContentAlignment.MiddleRight
            .Font = New Font("Segoe UI Semibold", 9.5F)
            .ForeColor = DarkGreen
            .Size = New Size(150, 26)
            .Location = New Point(btnAddAssessment.Right + 60, btnAddAssessment.Top)
        End With

        With txtMeetings
            .Text = "18"
            .TextAlign = HorizontalAlignment.Center
            .BorderStyle = BorderStyle.FixedSingle
            .BackColor = InputColor
            .Font = New Font("Segoe UI Semibold", 10.0F)
            .Width = 54
            .Location = New Point(lblMeetings.Right + 8, btnAddAssessment.Top)
        End With

        AddHandler txtMeetings.TextChanged, Sub(s, ev) ComputeAll()
        AddHandler txtMeetings.Leave,
            Sub(s, ev)
                Dim v As Double
                If Not (TryNum(txtMeetings.Text, v) AndAlso v > 0) Then txtMeetings.Text = "18"
            End Sub

        host.Controls.Add(lblMeetings)
        host.Controls.Add(txtMeetings)
        lblMeetings.BringToFront()
        txtMeetings.BringToFront()
    End Sub

    Private Function Meetings() As Double
        Dim v As Double
        If TryNum(txtMeetings.Text, v) AndAlso v > 0 Then Return v
        Return 18
    End Function

    ' Fills the empty space with blank bordered rows, without causing a scrollbar
    Private Sub FillEmptyRows()
        Dim avail As Integer = dgvGrades.ClientSize.Height - dgvGrades.ColumnHeadersHeight -
                               SystemInformation.HorizontalScrollBarHeight
        Dim used As Integer = 0
        For Each r As DataGridViewRow In dgvGrades.Rows
            used += r.Height
        Next

        Do
            Dim idx As Integer = dgvGrades.Rows.Add()
            If used + dgvGrades.Rows(idx).Height > avail Then
                dgvGrades.Rows.RemoveAt(idx)
                Exit Do
            End If
            used += dgvGrades.Rows(idx).Height
        Loop
    End Sub

    ' ===== Row / column helpers =====
    Private Function IsStudentRow(row As DataGridViewRow) As Boolean
        Return Not String.IsNullOrWhiteSpace(Convert.ToString(row.Cells("StudentName").Value))
    End Function

    Private Function CanEditCell(cell As DataGridViewCell) As Boolean
        Return IsStudentRow(dgvGrades.Rows(cell.RowIndex)) AndAlso
               Convert.ToString(dgvGrades.Columns(cell.ColumnIndex).Tag) = "input"
    End Function

    Private Function TryNum(value As Object, ByRef result As Double) As Boolean
        Return Double.TryParse(Convert.ToString(value), result)
    End Function

    ' Highest score allowed in a column (-1 = no limit)
    Private Function MaxFor(colName As String) As Double
        If colName = "Part" Then Return Meetings()
        If maxScores.ContainsKey(colName) Then Return maxScores(colName)
        If colName = "Extra" OrElse colName = "Project" Then Return 100
        Return -1
    End Function

    Private Function InputHint(colName As String) As String
        If colName = "Drp" Then Return "Type Drp to mark a dropped student, or leave it blank."
        Dim m As Double = MaxFor(colName)
        If m > 0 Then Return "Enter a number from 0 to " & m & " with at most two decimal places."
        Return "Please enter a valid number."
    End Function

    ' Validates and stores one typed / pasted value. Returns False if the value was rejected.
    Private Function ApplyScore(cell As DataGridViewCell, text As String) As Boolean
        Dim colName As String = dgvGrades.Columns(cell.ColumnIndex).Name
        text = text.Trim()

        If text = "" Then
            cell.Value = Nothing
            Return True
        End If

        If colName = "Drp" Then
            If text.ToLower() = "drp" Then
                cell.Value = "Drp"
                Return True
            End If
            cell.Value = Nothing
            Return False
        End If

        Dim v As Double
        Dim maxVal As Double = MaxFor(colName)
        If Not TryNum(text, v) OrElse Not ValidationHelper.HasTwoDecimalPlaces(v) OrElse v < 0 OrElse (maxVal > 0 AndAlso v > maxVal) Then
            cell.Value = Nothing
            Return False
        End If

        cell.Value = v
        Return True
    End Function

    ' ===== Assessments (any type name the prof wants) =====
    ' Cleans what was typed: trims, removes a trailing number ("Quiz 1" -> "Quiz"),
    ' and reuses the casing of an existing / suggested type so "quiz" and "Quiz" are the same.
    Private Function CanonType(text As String) As String
        Dim t As String = System.Text.RegularExpressions.Regex.Replace(text.Trim(), "\s+", " ")
        t = System.Text.RegularExpressions.Regex.Replace(t, "\s*\d+$", "").Trim()
        If t = "" Then Return ""

        For Each it As AssessItem In items
            If String.Equals(it.Type, t, StringComparison.OrdinalIgnoreCase) Then Return it.Type
        Next
        For Each p As String In PresetTypes
            If String.Equals(p, t, StringComparison.OrdinalIgnoreCase) Then Return p
        Next
        Return Char.ToUpper(t(0)) & t.Substring(1)
    End Function

    ' Which component a type counts toward by default
    Private Function GroupFor(type As String) As String
        For Each it As AssessItem In items
            If String.Equals(it.Type, type, StringComparison.OrdinalIgnoreCase) Then Return it.Group
        Next
        Return If(String.Equals(type, "Quiz", StringComparison.OrdinalIgnoreCase), "Quiz", "CW")
    End Function

    ' "Laboratory" -> "Laboratory 1" for the first one, "Laboratory 2" for the next, and so on
    Private Function NextTitle(type As String) As String
        Return AssessmentNames.NextTitle(type, items.Select(Function(item) item.Title))
    End Function

    Private Function TitleOf(rawCol As String) As String
        If rawCol = "ExamRaw" Then Return "Exam"
        For Each it As AssessItem In items
            If it.Key & "Raw" = rawCol Then Return it.Title
        Next
        Return rawCol
    End Function

    Private Sub AddAssessment(type As String, grp As String, maxScore As Double)
        Dim title As String = NextTitle(type)

        ' Column name: letters and digits only, kept unique
        Dim baseKey As String = System.Text.RegularExpressions.Regex.Replace(title, "[^A-Za-z0-9]", "")
        Dim key As String = baseKey
        Dim n As Integer = 2
        Do While dgvGrades.Columns.Contains(key & "Raw")
            key = baseKey & "_" & n
            n += 1
        Loop

        Dim at As Integer = dgvGrades.Columns(grp & "Comp").Index

        Dim raw As DataGridViewColumn = AddCol(key & "Raw", RawHeader(title, maxScore), "input", at)
        raw.ToolTipText = "Double-click to change the max score"
        AddCol(key & "GrA", title & vbLf & "Score (%)", "calc", at + 1)

        items.Add(New AssessItem With {.Key = key, .Title = title, .Type = type, .Group = grp})
        maxScores(key & "Raw") = maxScore
        ComputeAll()
    End Sub

    ' Double-click a Raw header to change its max score
    Private Sub dgvGrades_ColumnHeaderMouseDoubleClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvGrades.ColumnHeaderMouseDoubleClick
        Dim col As DataGridViewColumn = dgvGrades.Columns(e.ColumnIndex)
        If Not maxScores.ContainsKey(col.Name) Then Return

        If assessmentIds.ContainsKey(col.Name) Then
            MessageBox.Show("Existing assessment maximum scores cannot be changed from this screen.")
            Return
        End If
        Dim title As String = TitleOf(col.Name)
        Dim txt As String = ShowInputDialog("Max Score", "Highest possible score for " & title,
                                            maxScores(col.Name).ToString())
        If txt = "" Then Return

        Dim m As Double
        If Not (TryNum(txt, m) AndAlso m > 0) Then
            MessageBox.Show("Please enter a number greater than 0.", "Digital Gradebook")
            Return
        End If

        For Each r As DataGridViewRow In dgvGrades.Rows
            Dim v As Double
            If IsStudentRow(r) AndAlso TryNum(r.Cells(col.Index).Value, v) AndAlso v > m Then
                MessageBox.Show("A student already has a score higher than " & m & ".", "Digital Gradebook")
                Return
            End If
        Next

        maxScores(col.Name) = m
        col.HeaderText = RawHeader(title, m)
        ComputeAll()
    End Sub

    ' ===== Names: SURNAME, FIRSTNAME M.I. and alphabetical order by surname =====
    Private Function BuildName(surname As String, first As String, mi As String) As String
        surname = surname.Replace(",", " ").Trim()
        first = first.Replace(",", " ").Trim()
        mi = mi.Replace(".", "").Replace(",", "").Trim()

        Dim result As String = surname & ", " & first
        If mi <> "" Then result &= " " & mi & "."
        Return result.ToUpper()
    End Function

    Private Function Surname(n As String) As String
        Dim i As Integer = n.IndexOf(","c)
        Return If(i >= 0, n.Substring(0, i), n).Trim()
    End Function

    Private Function GivenName(n As String) As String
        Dim i As Integer = n.IndexOf(","c)
        Return If(i >= 0, n.Substring(i + 1), "").Trim()
    End Function

    Private Function CompareNames(a As String, b As String) As Integer
        Return StudentNameOrder.CompareNames(a, b)
    End Function

    ' Sorts all students A-Z by surname, renumbers the No. column 1..n, and refills blank rows
    Private Sub SortAndRenumber()
        Dim studentList As New List(Of Tuple(Of Object(), String))()

        Dim nameIdx As Integer = dgvGrades.Columns("StudentName").Index
        Dim noIdx As Integer = dgvGrades.Columns("No").Index

        For Each r As DataGridViewRow In dgvGrades.Rows
            If IsStudentRow(r) Then
                Dim vals(dgvGrades.Columns.Count - 1) As Object
                For i As Integer = 0 To vals.Length - 1
                    vals(i) = r.Cells(i).Value
                Next
                Dim sId As String = Convert.ToString(r.Cells("StudentName").Tag)
                studentList.Add(Tuple.Create(vals, sId))
            End If
        Next

        studentList.Sort(Function(a, b)
                             Dim order = CompareNames(Convert.ToString(a.Item1(nameIdx)), Convert.ToString(b.Item1(nameIdx)))
                             Return If(order <> 0, order, StringComparer.OrdinalIgnoreCase.Compare(a.Item2, b.Item2))
                         End Function)

        dgvGrades.Rows.Clear()

        For i As Integer = 0 To studentList.Count - 1
            Dim vals() As Object = studentList(i).Item1
            Dim sId As String = studentList(i).Item2

            vals(noIdx) = i + 1
            Dim newIdx As Integer = dgvGrades.Rows.Add(vals)
            dgvGrades.Rows(newIdx).Cells("StudentName").Tag = sId
        Next

        FillEmptyRows()
        ComputeAll()
        dgvGrades.ClearSelection()
    End Sub

    ' Shared weighted calculations over graded assessments only.

    Private Sub ComputeRow(row As DataGridViewRow)
        If Not IsStudentRow(row) Then Return
        Dim scores As New List(Of ScoreItem)
        Dim groups As New Dictionary(Of String, List(Of ScoreItem)) From {{"Quiz", New List(Of ScoreItem)}, {"CW", New List(Of ScoreItem)}}
        For Each item In items
            Dim value As Double
            Dim hasScore = TryNum(row.Cells(item.Key & "Raw").Value, value)
            Dim score As New ScoreItem(If(hasScore, CType(value, Double?), Nothing), maxScores(item.Key & "Raw"), item.Weight)
            scores.Add(score)
            groups(item.Group).Add(score)
            row.Cells(item.Key & "GrA").Value = If(hasScore, CObj(GradeCalculator.CalculateScorePercentage(value, score.TotalScore)), Nothing)
        Next
        For Each group In groups
            Dim average = GradeCalculator.CalculateRunningGrade(group.Value)
            row.Cells(group.Key & "Comp").Value = If(average.HasValue, CObj(average.GetValueOrDefault()), Nothing)
        Next
        Dim running = GradeCalculator.CalculateRunningGrade(scores)
        row.Cells("Total").Value = If(running.HasValue, CObj(running.GetValueOrDefault()), Nothing)
        row.Cells("Remarks").Value = GradeCalculator.GetStatus(running)
    End Sub

    Private Sub ComputeAll()
        For Each r As DataGridViewRow In dgvGrades.Rows
            ComputeRow(r)
        Next
        UpdateSummary()
    End Sub

    ' ===== Summary of Grades (strip under the grid) =====
    Private Sub CreateSummaryLabel()
        dgvGrades.Height -= 46   ' make room so the strip is visible

        With lblSummary
            .AutoSize = False
            .Height = 34
            .Width = dgvGrades.Width
            .Location = New Point(dgvGrades.Left, dgvGrades.Bottom + 8)
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
            .TextAlign = ContentAlignment.MiddleLeft
            .Padding = New Padding(12, 0, 0, 0)
            .BackColor = Color.FromArgb(238, 246, 241)
            .ForeColor = DarkGreen
            .Font = New Font("Segoe UI Semibold", 9.5F)
        End With
        dgvGrades.Parent.Controls.Add(lblSummary)
    End Sub

    Private Sub UpdateSummary()
        Dim drp As Integer = 0, inc As Integer = 0, pass As Integer = 0, fail As Integer = 0

        For Each r As DataGridViewRow In dgvGrades.Rows
            If Not IsStudentRow(r) Then Continue For
            Select Case Convert.ToString(r.Cells("Remarks").Value)
                Case "drp" : drp += 1
                Case "Not Yet" : inc += 1
                Case "Passed" : pass += 1
                Case "At risk" : fail += 1
            End Select
        Next

        Dim total As Integer = drp + inc + pass + fail
        lblSummary.Text = "SUMMARY OF GRADES     " &
                          "NOT YET " & inc & " (" & Pct(inc, total) & "%)   |   " &
                          "PASSED " & pass & " (" & Pct(pass, total) & "%)   |   " &
                          "AT RISK " & fail & " (" & Pct(fail, total) & "%)   |   " &
                          "TOTAL " & total
    End Sub

    Private Function Pct(n As Integer, total As Integer) As String
        Dim p As Double = 0
        If total > 0 Then p = n / total * 100
        Return p.ToString("0.00")
    End Function

    ' ===== Grid events =====
    Private Sub dgvGrades_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvGrades.CellFormatting
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        Dim row As DataGridViewRow = dgvGrades.Rows(e.RowIndex)
        If Not IsStudentRow(row) Then Return

        Dim col As DataGridViewColumn = dgvGrades.Columns(e.ColumnIndex)

        Select Case Convert.ToString(col.Tag)
            Case "input" : e.CellStyle.BackColor = InputColor
            Case "calc" : e.CellStyle.BackColor = CalcColor
            Case "total"
                e.CellStyle.BackColor = TotalColor
                e.CellStyle.Font = BoldFont
        End Select

        If col.Name = "Remarks" Then
            e.CellStyle.Font = BoldFont
            Select Case Convert.ToString(e.Value)
                Case "Passed" : e.CellStyle.ForeColor = Color.FromArgb(0, 128, 60)
                Case "At risk" : e.CellStyle.ForeColor = Color.FromArgb(170, 0, 0)
                Case "Not Yet" : e.CellStyle.ForeColor = Color.FromArgb(214, 120, 0)
                Case "drp" : e.CellStyle.ForeColor = Color.DimGray
            End Select
        End If
    End Sub

    ' Only cells on real student rows can be edited
    Private Sub dgvGrades_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles dgvGrades.CellBeginEdit
        If loadingClass OrElse e.RowIndex < 0 OrElse e.ColumnIndex < 0 OrElse Not IsStudentRow(dgvGrades.Rows(e.RowIndex)) Then
            e.Cancel = True
            Return
        End If
        originalEditValue = dgvGrades.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
    End Sub

    ' Keep this event free of row/column rebuilds: WinForms is still navigating cells.
    Private Sub dgvGrades_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGrades.CellEndEdit
        If loadingClass OrElse finishingEdit OrElse e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        Dim row = dgvGrades.Rows(e.RowIndex)
        If Not IsStudentRow(row) Then Return
        finishingEdit = True
        Try
            Dim cell = row.Cells(e.ColumnIndex)
            SaveEditedCell(cell, Convert.ToString(cell.Value), originalEditValue)
        Finally
            finishingEdit = False
            If reloadAfterEdit Then
                reloadAfterEdit = False
                QueueClassReload()
            End If
        End Try
    End Sub

    Private Function SaveEditedCell(cell As DataGridViewCell, text As String, previousValue As Object) As Boolean
        Dim row = dgvGrades.Rows(cell.RowIndex)
        Dim columnName = dgvGrades.Columns(cell.ColumnIndex).Name
        Dim message As String = Nothing
        If Not ApplyScore(cell, text) Then
            message = InputHint(columnName)
        ElseIf assessmentIds.ContainsKey(columnName) Then
            Try
                Dim result = _gradeService.SaveAssessmentScore(
                    Convert.ToString(row.Cells("StudentName").Tag), loadedClassId,
                    assessmentIds(columnName), If(cell.Value Is Nothing, CType(Nothing, Double?), CDbl(cell.Value)), AppSession.CurrentUser.UserId)
                If Not result.Success Then message = result.Message
            Catch ex As Exception
                message = "Could not save the score: " & ex.Message
            End Try
        End If
        If message IsNot Nothing Then cell.Value = previousValue
        ComputeRow(row)
        UpdateSummary()
        If message IsNot Nothing Then
            MessageBox.Show(message, "Score not saved", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    ' Coalesce structural reloads and finish navigation before touching rows/columns.
    Private Sub QueueClassReload()
        If reloadQueued OrElse IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
        reloadQueued = True
        BeginInvoke(New MethodInvoker(Sub()
                                          reloadQueued = False
                                          If IsDisposed OrElse Disposing Then Return
                                          If finishingEdit Then
                                              reloadAfterEdit = True
                                              Return
                                          End If
                                          If dgvGrades.IsCurrentCellInEditMode AndAlso Not dgvGrades.EndEdit() Then Return
                                          LoadClassData()
                                      End Sub))
    End Sub

    ' Excel-style keys: Delete clears, Ctrl+V pastes (Ctrl+C copy is built in)
    Private Sub dgvGrades_KeyDown(sender As Object, e As KeyEventArgs) Handles dgvGrades.KeyDown
        If e.KeyCode = Keys.Delete Then
            For Each c As DataGridViewCell In dgvGrades.SelectedCells
                If CanEditCell(c) Then
                    If assessmentIds.ContainsKey(dgvGrades.Columns(c.ColumnIndex).Name) Then
                        MessageBox.Show("Enter a corrected score instead of deleting a saved grade.", "Grade not cleared")
                        e.Handled = True
                        Return
                    End If
                    c.Value = Nothing
                End If
            Next
            ComputeAll()
            e.Handled = True
        ElseIf e.Control AndAlso e.KeyCode = Keys.V Then
            PasteFromClipboard()
            e.Handled = True
        End If
    End Sub

    Private Sub PasteFromClipboard()
        If Not Clipboard.ContainsText() OrElse dgvGrades.CurrentCell Is Nothing Then Return
        If dgvGrades.IsCurrentCellInEditMode AndAlso Not dgvGrades.EndEdit() Then Return
        Dim lines = Clipboard.GetText().Replace(vbCrLf, vbLf).TrimEnd(ControlChars.Lf).Split(ControlChars.Lf)
        Dim firstRow = dgvGrades.CurrentCell.RowIndex
        Dim firstColumn = dgvGrades.CurrentCell.ColumnIndex
        Dim updates As New List(Of (Cell As DataGridViewCell, Value As Double))
        Dim batch As New List(Of (StudentId As String, AssessmentId As Integer, Score As Double))
        For i = 0 To lines.Length - 1
            If firstRow + i >= dgvGrades.Rows.Count Then
                MessageBox.Show("Paste exceeds the student list. No pasted scores were saved.")
                Return
            End If
            Dim fields = lines(i).Split(ControlChars.Tab)
            For j = 0 To fields.Length - 1
                If firstColumn + j >= dgvGrades.Columns.Count Then
                    MessageBox.Show("Paste exceeds the available columns. No pasted scores were saved.")
                    Return
                End If
                Dim cell = dgvGrades.Rows(firstRow + i).Cells(firstColumn + j)
                Dim column = dgvGrades.Columns(cell.ColumnIndex).Name
                If Not CanEditCell(cell) OrElse Not assessmentIds.ContainsKey(column) Then Continue For
                Dim value As Double
                If Not TryNum(fields(j), value) OrElse Not ValidationHelper.IsValidScore(value, MaxFor(column)) Then
                    MessageBox.Show("Invalid pasted score for " & TitleOf(column) & ". Use a value within its maximum and at most two decimals. No pasted scores were saved.")
                    Return
                End If
                updates.Add((cell, value))
                batch.Add((Convert.ToString(dgvGrades.Rows(cell.RowIndex).Cells("StudentName").Tag), assessmentIds(column), value))
            Next
        Next
        Dim result = _gradeService.SaveScoreBatch(loadedClassId, batch, AppSession.CurrentUser.UserId)
        If result.Success Then
            For Each change In updates
                change.Cell.Value = change.Value
            Next
            ComputeAll()
        End If
        MessageBox.Show(result.Message, If(result.Success, "Paste saved", "Paste not saved"))
    End Sub

    Private Sub CustomButton1_Click(sender As Object, e As EventArgs) Handles CustomButton1.Click
        ' Verify there are student rows to export
        Dim studentRowCount As Integer = 0
        For Each r As DataGridViewRow In dgvGrades.Rows
            If IsStudentRow(r) Then studentRowCount += 1
        Next

        If studentRowCount = 0 Then
            MessageBox.Show("No student records found in the gradebook to export.", "Export Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Using sfd As New SaveFileDialog()
            sfd.Filter = "PDF Files (*.pdf)|*.pdf|CSV Files (*.csv)|*.csv"
            Dim courseName As String = If(cboCourse.SelectedItem IsNot Nothing, SelectedClassCode(), "Gradebook")
            sfd.FileName = $"{courseName}_Master_Gradebook_{DateTime.Now:yyyyMMdd}.pdf"

            If sfd.ShowDialog() = DialogResult.OK Then
                If sfd.FilterIndex = 1 Then
                    ' --- PDF EXPORT (Landscape Layout with Clean Columns) ---
                    Using printDoc = _exportService.CreateGridPrintDocument(dgvGrades, "DIGITAL GRADEBOOK", cboCourse.Text)
                        If _exportService.ExportToPDF(printDoc, sfd.FileName) Then
                            MessageBox.Show("Digital gradebook exported to PDF successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        End If
                    End Using
                Else
                    ' --- CSV EXPORT ---
                    If _exportService.ExportToCSV(dgvGrades, sfd.FileName) Then
                        MessageBox.Show("Digital gradebook exported to CSV successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End If
            End If
        End Using
    End Sub

    ' ===== Add Class =====
    Private Sub btnAddClass_Click(sender As Object, e As EventArgs) Handles btnAddClass.Click

        pendingClassImport = Nothing
        Dim info = ShowClassDialog()
        If info Is Nothing Then Return
        Dim term = If(pendingClassImport Is Nothing, "Midterm", ShowInputDialog("Assessment term", "Imported assessments belong to Midterm or Finals", "Midterm"))
        If term = "" Then Return
        If pendingClassImport IsNot Nothing AndAlso MessageBox.Show("Import this roster and all assessment columns? Blank scores stay ungraded. Assessments receive equal weight (10 each). Student IDs and names must already be registered in Admin.", "Confirm class import", MessageBoxButtons.YesNo) <> DialogResult.Yes Then Return
        Dim id As Integer
        Dim result = New ClassImportService().ImportClass(info.Code, info.Name, info.Schedule, info.Term, info.SY, term, pendingClassImport, id)
        MessageBox.Show(result.Message, If(result.Success, "Class saved", "Class not saved"))
        If result.Success Then
            LoadClassesFromDatabase()
            SelectClass(id)
            Dim professor = TryCast(Owner, frmprof)
            If professor IsNot Nothing Then professor.ReloadClasses(id)
        End If

    End Sub

    ' Hover over the class box to see its schedule, term and school year
    Private Sub cboCourse_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboCourse.SelectedIndexChanged
        If refreshingClassChoices Then Return
        LoadClassData()
    End Sub

    Private Sub LoadClassData()
        UiKit.TryLoadData(Me, "gradebook", AddressOf LoadClassDataCore)
    End Sub

    Private Sub LoadClassDataCore()
        If loadingClass OrElse IsDisposed OrElse Disposing Then Return
        If finishingEdit Then
            reloadAfterEdit = True
            Return
        End If
        If dgvGrades.IsCurrentCellInEditMode Then
            QueueClassReload()
            Return
        End If
        loadingClass = True
        Try
            dgvGrades.Rows.Clear()
            For Each it In items
                dgvGrades.Columns.Remove(it.Key & "Raw")
                dgvGrades.Columns.Remove(it.Key & "GrA")
            Next
            items.Clear()
            assessmentIds.Clear()
            maxScores.Clear()
            maxScores("ExamRaw") = 100
            Dim classId = GetClassIdFromCode(SelectedClassCode())
            loadedClassId = classId
            If classId = 0 Then Return
            For Each a As DataRow In _gradeService.GetAssessmentsByClass(classId, AppSession.CurrentUser.UserId).Rows
                Dim key = "Assessment" & a("assessment_id").ToString()
                Dim title = a("title").ToString()
                Dim type = a("assessment_type").ToString()
                Dim grp = GroupFor(type)
                Dim at = dgvGrades.Columns(grp & "Comp").Index
                AddCol(key & "Raw", RawHeader(title, CDbl(a("total_score"))), "input", at)
                AddCol(key & "GrA", title & vbLf & "Score (%)", "calc", at + 1)
                items.Add(New AssessItem With {.Key = key, .Title = title, .Type = type, .Group = grp, .Weight = CDbl(a("weight"))})
                maxScores(key & "Raw") = CDbl(a("total_score"))
                assessmentIds(key & "Raw") = CInt(a("assessment_id"))
            Next
            Dim scores = _gradeService.GetScoresByClass(classId)
            For Each student As DataRow In _gradeService.GetEnrollmentsByClass(classId, AppSession.CurrentUser.UserId).Rows
                Dim idx = dgvGrades.Rows.Add()
                Dim row = dgvGrades.Rows(idx)
                row.Cells("StudentName").Value = StudentNameOrder.FormatForGradebook(student("full_name").ToString())
                row.Cells("StudentName").Tag = student("student_id").ToString()
                For Each score As DataRow In scores.Rows
                    Dim col = "Assessment" & score("assessment_id").ToString() & "Raw"
                    If score("student_id").ToString() = student("student_id").ToString() AndAlso assessmentIds.ContainsKey(col) AndAlso Not IsDBNull(score("score")) Then
                        row.Cells(col).Value = score("score")
                    End If
                Next
            Next
            SortAndRenumber()
        Finally
            Try
                FillEmptyRows()
                ComputeAll()
            Finally
                loadingClass = False
            End Try
        End Try
    End Sub

    ' ===== Add Student: name goes into the grid, list re-sorts A-Z and renumbers =====
    Private Sub btnAddStudent_Click(sender As Object, e As EventArgs) Handles btnAddStudent.Click
        UiKit.TryLoadData(Me, "registered students", AddressOf EnrollStudentDialog)
    End Sub

    Private Sub EnrollStudentDialog()
        Dim fullName As String = ""
        Dim studentNo As String = ""
        Dim course As String = ""

        If Not ShowStudentDialog(fullName, studentNo, course) Then Return

        Dim targetClass = GetClassIdFromCode(course)
        Dim result = New RosterImportService().ImportStudents(targetClass, AppSession.CurrentUser.UserId, New List(Of String()) From {New String() {studentNo, fullName}})
        If Not result.Success Then
            MessageBox.Show(result.Message, "Enrollment failed")
            Return
        End If
        SelectClass(targetClass)
        LoadClassData()
    End Sub

    ' ===== Add Assessment: type anything, it numbers itself (Quiz 1, Quiz 2, Laboratory 1 ...) =====
    Private Sub btnAddAssessment_Click(sender As Object, e As EventArgs) Handles btnAddAssessment.Click
        If cboCourse.SelectedIndex = -1 Then
            MessageBox.Show("Please select a course first.", "Digital Gradebook", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim type = ""
        Dim grp = ""
        Dim maxScore As Double = 0
        If Not ShowAssessmentDialog(type, grp, maxScore) Then Return

        Dim currentClassCode = SelectedClassCode()
        Dim classId = GetClassIdFromCode(currentClassCode)
        Dim assessmentTitle = NextTitle(type)
        Dim term = ShowInputDialog("Assessment term", "Enter Midterm or Finals", "Midterm")
        If term = "" Then Return
        Dim weightText = ShowInputDialog("Assessment weight", "Enter the assessment weight (greater than 0, up to 100)", "10")
        Dim weight As Double
        If Not TryNum(weightText, weight) OrElse weight <= 0 OrElse weight > 100 Then
            MessageBox.Show("Enter a weight greater than 0 and up to 100.")
            Return
        End If

        ' Create assessment in MySQL
        Dim res = _gradeService.CreateAssessment(
            classId,
            assessmentTitle,
            type,
            maxScore,
            term,
            CurrentUser.UserId, weight
        )

        If res.Success Then
            LoadClassData()
            MessageBox.Show($"Assessment '{assessmentTitle}' created and saved to database!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show($"Could not create assessment: {res.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    ' ===== Popups (dark green style) =====
    Private Function ShowStudentDialog(ByRef fullName As String,
                                       ByRef studentNo As String,
                                       ByRef course As String) As Boolean
        Dim dark = Color.DarkGreen
        Dim green = Color.FromArgb(0, 191, 99)
        Using dialog As New Form With {.Text = "Add Student", .ClientSize = New Size(420, 350), .FormBorderStyle = FormBorderStyle.None, .StartPosition = FormStartPosition.CenterParent, .ShowInTaskbar = False, .BackColor = Color.White, .Font = New Font("Segoe UI", 10), .AutoScaleMode = AutoScaleMode.Font}
            AddHandler dialog.Paint, Sub(sender, args)
                                         Using pen As New Pen(dark, 2)
                                             args.Graphics.DrawRectangle(pen, 1, 1, dialog.ClientSize.Width - 2, dialog.ClientSize.Height - 2)
                                         End Using
                                     End Sub
            Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 48, .BackColor = dark}
            header.Controls.Add(New Label With {.Text = "Add Student", .AutoSize = True, .Location = New Point(18, 12), .ForeColor = Color.White, .Font = New Font("Segoe UI Semibold", 12)})
            Dim classPicker As New ComboBox With {.Location = New Point(24, 172), .Width = 372, .DropDownWidth = 700, .DropDownStyle = ComboBoxStyle.DropDownList, .FlatStyle = FlatStyle.Flat, .BackColor = Color.White, .Font = New Font("Segoe UI", 11)}
            Dim importButton = MakeImportButton(dialog, {"From file (CSV)...", "Refresh enrolled students from MySQL"}, Function(choice) ImportStudents(choice, GetClassIdFromCode(Convert.ToString(classPicker.SelectedItem))))
            importButton.Location = New Point(dialog.ClientSize.Width - importButton.Width - 18, 10)
            header.Controls.Add(importButton)
            Dim students = StudentNameOrder.Sort(_userService.GetAllStudents(), "full_name", "student_id")
            students.Columns.Add("Display", GetType(String))
            For Each student As DataRow In students.Rows
                student("Display") = student("student_id").ToString() & " - " & StudentNameOrder.FormatForGradebook(student("full_name").ToString())
            Next
            students.DefaultView.RowFilter = "status = 'Active'"
            Dim studentLabel As New Label With {.Text = "Registered student", .AutoSize = True, .Location = New Point(24, 68), .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 9)}
            Dim studentPicker As New ComboBox With {.Location = New Point(24, 92), .Width = 372, .DropDownWidth = 520, .DropDownStyle = ComboBoxStyle.DropDownList, .FlatStyle = FlatStyle.Flat, .BackColor = Color.White, .Font = New Font("Segoe UI", 11), .DisplayMember = "Display", .ValueMember = "student_id", .DataSource = students.DefaultView}
            Dim studentLine As New Panel With {.Location = New Point(24, 124), .Size = New Size(372, 2), .BackColor = green}
            Dim classLabel As New Label With {.Text = "Enroll into class", .AutoSize = True, .Location = New Point(24, 148), .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 9)}
            For Each item In cboCourse.Items
                classPicker.Items.Add(item)
            Next
            classPicker.SelectedIndex = cboCourse.SelectedIndex
            Dim classLine As New Panel With {.Location = New Point(24, 204), .Size = New Size(372, 2), .BackColor = green}
            Dim hint As New Label With {.Text = "Choose an existing student and the class to enroll them in.", .Location = New Point(24, 224), .Size = New Size(372, 36), .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 9)}
            Dim errorLabel As New Label With {.Text = "", .ForeColor = Color.Firebrick, .Location = New Point(24, 262), .Size = New Size(372, 25), .Font = New Font("Segoe UI", 9)}
            Dim add As New Button With {.Text = "Enroll", .Location = New Point(296, 296), .Size = New Size(100, 36), .BackColor = dark, .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .Font = New Font("Segoe UI Semibold", 10)}
            Dim cancel As New Button With {.Text = "Cancel", .Location = New Point(186, 296), .Size = New Size(100, 36), .BackColor = Color.White, .ForeColor = dark, .FlatStyle = FlatStyle.Flat, .Font = New Font("Segoe UI Semibold", 10), .DialogResult = DialogResult.Cancel}
            add.FlatAppearance.BorderSize = 0
            cancel.FlatAppearance.BorderColor = dark
            AddHandler add.Click, Sub(sender, args)
                                      If studentPicker.SelectedItem Is Nothing OrElse classPicker.SelectedItem Is Nothing Then
                                          errorLabel.Text = "Select a registered student and a class."
                                          Return
                                      End If
                                      dialog.DialogResult = DialogResult.OK
                                  End Sub
            dialog.Controls.AddRange({studentLabel, studentPicker, studentLine, classLabel, classPicker, classLine, hint, errorLabel, add, cancel, header})
            dialog.AcceptButton = add
            dialog.CancelButton = cancel
            dialog.ActiveControl = studentPicker
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return False
            Dim selected = DirectCast(studentPicker.SelectedItem, DataRowView)
            fullName = selected("full_name").ToString()
            studentNo = selected("student_id").ToString()
            course = classPicker.SelectedItem.ToString()
            Return True
        End Using
    End Function

    Private pendingClassImport As List(Of String())

    Private Function ShowClassDialog() As ClassInfo
        Dim dark As Color = Color.FromArgb(0, 100, 0)
        Dim green As Color = Color.FromArgb(0, 191, 99)
        Dim result As ClassInfo = Nothing

        ' Default school year: June onward = this year to next year (Oct 2026 -> 2026-2027)
        Dim y As Integer = Date.Today.Year
        If Date.Today.Month < 6 Then y -= 1
        Dim defaultSY As String = y & "-" & (y + 1)

        Using dlg As New Form()
            dlg.FormBorderStyle = FormBorderStyle.None
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.Size = New Size(380, 430)
            dlg.BackColor = Color.White
            dlg.ShowInTaskbar = False

            AddHandler dlg.Paint,
                Sub(s, ev)
                    Using pen As New Pen(dark, 2)
                        ev.Graphics.DrawRectangle(pen, 1, 1, dlg.Width - 2, dlg.Height - 2)
                    End Using
                End Sub

            Dim header As New Panel() With {.Dock = DockStyle.Top, .Height = 48, .BackColor = dark}
            header.Controls.Add(New Label() With {
                .Text = "Add Class",
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 12.0F),
                .AutoSize = True,
                .Location = New Point(18, 12)
            })
            header.Controls.Add(MakeImportButton(dlg,
                {"From file (CSV)...", "Refresh assigned classes from MySQL"},
                Function(choice) ImportClasses(choice)))

            Dim grey As Color = Color.Gray
            Dim small As New Font("Segoe UI", 9.0F)
            Dim big As New Font("Segoe UI", 12.0F)

            ' Class Code
            Dim lblCode As New Label() With {.Text = "Class Code (e.g. IT 112)", .ForeColor = grey, .Font = small, .AutoSize = True, .Location = New Point(24, 62)}
            Dim txtCode As New TextBox() With {.BorderStyle = BorderStyle.None, .Font = big, .Location = New Point(24, 84), .Width = 332, .MaxLength = 20}
            Dim lineCode As New Panel() With {.BackColor = green, .Location = New Point(24, 112), .Size = New Size(332, 2)}

            ' Class Name
            Dim lblName As New Label() With {.Text = "Class Name", .ForeColor = grey, .Font = small, .AutoSize = True, .Location = New Point(24, 128)}
            Dim txtName As New TextBox() With {.BorderStyle = BorderStyle.None, .Font = big, .Location = New Point(24, 150), .Width = 332, .MaxLength = 80}
            Dim lineName As New Panel() With {.BackColor = green, .Location = New Point(24, 178), .Size = New Size(332, 2)}

            ' Schedule
            Dim lblSched As New Label() With {.Text = "Section (e.g. CS2A)", .ForeColor = grey, .Font = small, .AutoSize = True, .Location = New Point(24, 194)}
            Dim txtSched As New TextBox() With {.BorderStyle = BorderStyle.None, .Font = big, .Location = New Point(24, 216), .Width = 332, .MaxLength = 20}
            Dim lineSched As New Panel() With {.BackColor = green, .Location = New Point(24, 244), .Size = New Size(332, 2)}

            ' Term (pick or type) + SY side by side
            Dim lblTerm As New Label() With {.Text = "Term", .ForeColor = grey, .Font = small, .AutoSize = True, .Location = New Point(24, 260)}
            Dim cboTerm As New ComboBox() With {
                .DropDownStyle = ComboBoxStyle.DropDown,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = Color.White,
                .Font = New Font("Segoe UI", 11.0F),
                .Location = New Point(24, 282),
                .Width = 190,
                .MaxLength = 20
            }
            cboTerm.Items.AddRange({"1st Semester", "2nd Semester"})
            Dim lineTerm As New Panel() With {.BackColor = green, .Location = New Point(24, 312), .Size = New Size(190, 2)}

            Dim lblSY As New Label() With {.Text = "SY", .ForeColor = grey, .Font = small, .AutoSize = True, .Location = New Point(232, 260)}
            Dim txtSY As New TextBox() With {.Text = defaultSY, .BorderStyle = BorderStyle.None, .Font = big, .Location = New Point(232, 284), .Width = 124, .MaxLength = 9}
            Dim lineSY As New Panel() With {.BackColor = green, .Location = New Point(232, 312), .Size = New Size(124, 2)}

            Dim lblError As New Label() With {.Text = "", .ForeColor = Color.Firebrick, .Font = small, .AutoSize = True, .Location = New Point(24, 326)}

            Dim btnOk As New Button() With {
                .Text = "Add",
                .FlatStyle = FlatStyle.Flat,
                .BackColor = dark,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 10.0F),
                .Size = New Size(100, 36),
                .Location = New Point(256, 372),
                .Cursor = Cursors.Hand
            }
            btnOk.FlatAppearance.BorderSize = 0

            Dim btnCancel As New Button() With {
                .Text = "Cancel",
                .DialogResult = DialogResult.Cancel,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = Color.White,
                .ForeColor = dark,
                .Font = New Font("Segoe UI Semibold", 10.0F),
                .Size = New Size(100, 36),
                .Location = New Point(146, 372),
                .Cursor = Cursors.Hand
            }
            btnCancel.FlatAppearance.BorderColor = dark
            btnCancel.FlatAppearance.BorderSize = 1

            AddHandler btnOk.Click,
                Sub(s, ev)
                    Dim code As String = System.Text.RegularExpressions.Regex.Replace(txtCode.Text.Trim(), "\s+", " ").ToUpper()
                    Dim sy As String = txtSY.Text.Trim()
                    Dim m As System.Text.RegularExpressions.Match =
                        System.Text.RegularExpressions.Regex.Match(sy, "^(\d{4})-(\d{4})$")

                    If code = "" Then
                        lblError.Text = "Please enter the class code."
                        txtCode.Focus()
                    ElseIf code.Contains(" - ") Then
                        lblError.Text = "Class code can't contain "" - ""."
                        txtCode.Focus()
                    ElseIf txtName.Text.Trim() = "" Then
                        lblError.Text = "Please enter the class name."
                        txtName.Focus()
                    ElseIf txtSched.Text.Trim() = "" Then
                        lblError.Text = "Please enter the section."
                        txtSched.Focus()
                    ElseIf cboTerm.Text.Trim() = "" Then
                        lblError.Text = "Please enter the term."
                        cboTerm.Focus()
                    ElseIf Not m.Success OrElse CInt(m.Groups(2).Value) <> CInt(m.Groups(1).Value) + 1 Then
                        lblError.Text = "SY must look like 2026-2027."
                        txtSY.Focus()
                    Else
                        result = New ClassInfo With {
                            .Code = code,
                            .Name = txtName.Text.Trim(),
                            .Schedule = txtSched.Text.Trim(),
                            .Term = cboTerm.Text.Trim(),
                            .SY = sy
                        }
                        dlg.DialogResult = DialogResult.OK
                    End If
                End Sub

            dlg.Controls.AddRange({lblCode, txtCode, lineCode,
                                   lblName, txtName, lineName,
                                   lblSched, txtSched, lineSched,
                                   lblTerm, cboTerm, lineTerm,
                                   lblSY, txtSY, lineSY,
                                   lblError, btnOk, btnCancel, header})
            dlg.AcceptButton = btnOk
            dlg.CancelButton = btnCancel
            dlg.ActiveControl = txtCode

            dlg.ShowDialog(If(assessmentOwner, Me))
        End Using

        Return result
    End Function

    Private Function ShowAssessmentDialog(ByRef type As String, ByRef grp As String,
                                          ByRef maxScore As Double) As Boolean
        Dim dark As Color = Color.FromArgb(0, 100, 0)
        Dim green As Color = Color.FromArgb(0, 191, 99)
        Dim accepted As Boolean = False
        Dim parsedMax As Double = 0
        Dim chosenType As String = ""
        Dim chosenGroup As String = "CW"

        Using dlg As New Form()
            dlg.FormBorderStyle = FormBorderStyle.None
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.Size = New Size(380, 390)
            dlg.BackColor = Color.White
            dlg.ShowInTaskbar = False

            AddHandler dlg.Paint,
                Sub(s, ev)
                    Using pen As New Pen(dark, 2)
                        ev.Graphics.DrawRectangle(pen, 1, 1, dlg.Width - 2, dlg.Height - 2)
                    End Using
                End Sub

            Dim header As New Panel() With {.Dock = DockStyle.Top, .Height = 48, .BackColor = dark}
            Dim lblTitle As New Label() With {
                .Text = "Add Assessment",
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 12.0F),
                .AutoSize = True,
                .Location = New Point(18, 12)
            }
            header.Controls.Add(lblTitle)
            header.Controls.Add(MakeImportButton(dlg,
                {"From file (CSV)...", "From MySQL (this class)..."},
                Function(choice) ImportAssessments(choice)))

            ' Type: pick a suggestion OR type your own
            Dim lblType As New Label() With {
                .Text = "Assessment type (choose a supported type)",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 62)
            }
            Dim cboType As New ComboBox() With {
                .DropDownStyle = ComboBoxStyle.DropDown,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = Color.White,
                .Font = New Font("Segoe UI", 11.0F),
                .Location = New Point(24, 84),
                .Width = 332,
                .MaxLength = 20
            }
            cboType.Items.AddRange(PresetTypes)
            For Each it As AssessItem In items          ' also offer custom types added earlier
                If Not cboType.Items.Contains(it.Type) Then cboType.Items.Add(it.Type)
            Next
            Dim lineType As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 114),
                .Size = New Size(332, 2)
            }

            ' Display category (not a separate grading weight).
            Dim lblGroup As New Label() With {
                .Text = "Display category (automatic)",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 130)
            }
            Dim cboGroup As New ComboBox() With {
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = Color.White,
                .Font = New Font("Segoe UI", 11.0F),
                .Location = New Point(24, 152),
                .Width = 332
            }
            cboGroup.Enabled = False
            cboGroup.Items.Add("Quiz category")
            cboGroup.Items.Add("Other assessments")
            cboGroup.SelectedIndex = 0
            Dim lineGroup As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 182),
                .Size = New Size(332, 2)
            }

            ' Max score
            Dim lblMax As New Label() With {
                .Text = "Highest possible score",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 198)
            }
            Dim txtMax As New TextBox() With {
                .Text = "20",
                .BorderStyle = BorderStyle.None,
                .Font = New Font("Segoe UI", 12.0F),
                .Location = New Point(24, 220),
                .Width = 332
            }
            Dim lineMax As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 248),
                .Size = New Size(332, 2)
            }

            Dim lblPreview As New Label() With {
                .ForeColor = dark,
                .Font = New Font("Segoe UI Semibold", 9.5F),
                .AutoSize = True,
                .Location = New Point(24, 264)
            }
            Dim lblError As New Label() With {
                .Text = "",
                .ForeColor = Color.Firebrick,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 288)
            }

            ' Live preview + auto-pick the component for known types
            Dim refreshPreview As Action =
                Sub()
                    Dim t As String = CanonType(cboType.Text)
                    lblPreview.Text = If(t = "", "", "Will be added as: " & NextTitle(t))
                End Sub

            AddHandler cboType.TextChanged,
                Sub(s, ev)
                    Dim t As String = CanonType(cboType.Text)
                    If t <> "" Then cboGroup.SelectedIndex = If(GroupFor(t) = "Quiz", 0, 1)
                    refreshPreview()
                End Sub
            AddHandler cboType.SelectedIndexChanged, Sub(s, ev) refreshPreview()

            Dim btnOk As New Button() With {
                .Text = "Add",
                .FlatStyle = FlatStyle.Flat,
                .BackColor = dark,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 10.0F),
                .Size = New Size(100, 36),
                .Location = New Point(256, 332),
                .Cursor = Cursors.Hand
            }
            btnOk.FlatAppearance.BorderSize = 0

            Dim btnCancel As New Button() With {
                .Text = "Cancel",
                .DialogResult = DialogResult.Cancel,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = Color.White,
                .ForeColor = dark,
                .Font = New Font("Segoe UI Semibold", 10.0F),
                .Size = New Size(100, 36),
                .Location = New Point(146, 332),
                .Cursor = Cursors.Hand
            }
            btnCancel.FlatAppearance.BorderColor = dark
            btnCancel.FlatAppearance.BorderSize = 1

            AddHandler btnOk.Click,
                Sub(s, ev)
                    Dim t As String = CanonType(cboType.Text)
                    Dim m As Double
                    If t = "" Then
                        lblError.Text = "Choose an assessment type."
                        cboType.Focus()
                    ElseIf Not PresetTypes.Contains(t) Then
                        lblError.Text = "Choose Quiz, Activity, Exam, Project, or Attendance."
                        cboType.Focus()
                    ElseIf Not (TryNum(txtMax.Text, m) AndAlso m > 0) Then
                        lblError.Text = "Enter a max score greater than 0."
                        txtMax.Focus()
                    Else
                        chosenType = t
                        chosenGroup = If(cboGroup.SelectedIndex = 0, "Quiz", "CW")
                        parsedMax = m
                        accepted = True
                        dlg.DialogResult = DialogResult.OK
                    End If
                End Sub

            dlg.Controls.AddRange({lblType, cboType, lineType,
                                   lblGroup, cboGroup, lineGroup,
                                   lblMax, txtMax, lineMax,
                                   lblPreview, lblError, btnOk, btnCancel, header})
            dlg.AcceptButton = btnOk
            dlg.CancelButton = btnCancel
            dlg.ActiveControl = cboType

            dlg.ShowDialog(If(assessmentOwner, Me))
        End Using

        If accepted Then
            type = chosenType
            grp = chosenGroup
            maxScore = parsedMax
        End If
        Return accepted
    End Function

    ' Small one-field popup (used to change a max score)
    Private Function ShowInputDialog(title As String, prompt As String, initial As String) As String
        Dim dark As Color = Color.FromArgb(0, 100, 0)
        Dim green As Color = Color.FromArgb(0, 191, 99)

        Using dlg As New Form()
            dlg.FormBorderStyle = FormBorderStyle.None
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.Size = New Size(380, 220)
            dlg.BackColor = Color.White
            dlg.ShowInTaskbar = False

            AddHandler dlg.Paint,
                Sub(s, ev)
                    Using pen As New Pen(dark, 2)
                        ev.Graphics.DrawRectangle(pen, 1, 1, dlg.Width - 2, dlg.Height - 2)
                    End Using
                End Sub

            Dim header As New Panel() With {.Dock = DockStyle.Top, .Height = 48, .BackColor = dark}
            Dim lblTitle As New Label() With {
                .Text = title,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 12.0F),
                .AutoSize = True,
                .Location = New Point(18, 12)
            }
            header.Controls.Add(lblTitle)

            Dim lblPrompt As New Label() With {
                .Text = prompt,
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 66)
            }
            Dim txt As New TextBox() With {
                .Text = initial,
                .BorderStyle = BorderStyle.None,
                .Font = New Font("Segoe UI", 12.0F),
                .Location = New Point(24, 90),
                .Width = 332
            }
            Dim underline As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 120),
                .Size = New Size(332, 2)
            }

            Dim btnOk As New Button() With {
                .Text = "Save",
                .DialogResult = DialogResult.OK,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = dark,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 10.0F),
                .Size = New Size(100, 36),
                .Location = New Point(256, 160),
                .Cursor = Cursors.Hand
            }
            btnOk.FlatAppearance.BorderSize = 0

            Dim btnCancel As New Button() With {
                .Text = "Cancel",
                .DialogResult = DialogResult.Cancel,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = Color.White,
                .ForeColor = dark,
                .Font = New Font("Segoe UI Semibold", 10.0F),
                .Size = New Size(100, 36),
                .Location = New Point(146, 160),
                .Cursor = Cursors.Hand
            }
            btnCancel.FlatAppearance.BorderColor = dark
            btnCancel.FlatAppearance.BorderSize = 1

            dlg.Controls.AddRange({lblPrompt, txt, underline, btnOk, btnCancel, header})
            dlg.AcceptButton = btnOk
            dlg.CancelButton = btnCancel
            dlg.ActiveControl = txt
            txt.SelectAll()

            If dlg.ShowDialog(If(assessmentOwner, Me)) = DialogResult.OK Then
                Return txt.Text.Trim()
            End If
            Return ""
        End Using
    End Function

    ' ================= IMPORT / MYSQL / ADMIN =================

    ' "IT 112 - Information Assurance" -> "IT 112"
    Private Function SelectedClassCode() As String
        Return If(cboCourse.SelectedItem Is Nothing, "", cboCourse.SelectedItem.ToString())
    End Function

    ' pairs = "@name", value, "@name", value ...


    ' ----- CSV from File Explorer -----
    Private Function PickCsv() As String
        Using ofd As New OpenFileDialog()
            ofd.Title = "Import from file"
            ofd.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt"
            If ofd.ShowDialog() = DialogResult.OK Then Return ofd.FileName
        End Using
        Return ""
    End Function

    Private Function ReadCsv(path As String) As List(Of String())
        Dim rows As New List(Of String())
        For Each line As String In System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8)
            If line.Trim() = "" Then Continue For
            Dim fields As New List(Of String)
            Dim sb As New System.Text.StringBuilder()
            Dim inQuotes As Boolean = False
            Dim i As Integer = 0
            Do While i < line.Length
                Dim ch As Char = line(i)
                If ch = """"c Then
                    If inQuotes AndAlso i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                        sb.Append(""""c)
                        i += 1
                    Else
                        inQuotes = Not inQuotes
                    End If
                ElseIf ch = ","c AndAlso Not inQuotes Then
                    fields.Add(sb.ToString().Trim())
                    sb.Clear()
                Else
                    sb.Append(ch)
                End If
                i += 1
            Loop
            fields.Add(sb.ToString().Trim())
            rows.Add(fields.ToArray())
        Next
        Return rows
    End Function

    ' Picks a file, reads it, and drops the header row if the first cell matches one of headerWords
    Private Function LoadCsvRows(ParamArray headerWords() As String) As List(Of String())
        Dim path As String = PickCsv()
        If path = "" Then Return Nothing
        Try
            Dim rows As List(Of String()) = ReadCsv(path)
            If rows.Count > 0 Then
                Dim first As String = rows(0)(0).Replace(" ", "").ToLower()
                If Array.IndexOf(headerWords, first) >= 0 Then rows.RemoveAt(0)
            End If
            Return rows
        Catch ex As Exception
            MessageBox.Show("Could not read the file: " & ex.Message, "Digital Gradebook")
            Return Nothing
        End Try
    End Function

    ' ----- The "Import" button that goes inside each popup header -----
    Private Function MakeImportButton(dlg As Form, choices As String(),
                                      onPick As Func(Of String, Boolean)) As Button
        Dim btn As New Button() With {
            .Text = "Import " & ChrW(9662),
            .FlatStyle = FlatStyle.Flat,
            .BackColor = DarkGreen,
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI Semibold", 9.0F),
            .Size = New Size(92, 28),
            .Location = New Point(dlg.Width - 92 - 18, 10),
            .Cursor = Cursors.Hand
        }
        btn.FlatAppearance.BorderColor = Color.White
        btn.FlatAppearance.BorderSize = 1
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 130, 60)

        AddHandler btn.Click,
            Sub(s, ev)
                Dim menu As New ContextMenuStrip()
                menu.Font = New Font("Segoe UI", 10.0F)
                For Each c As String In choices
                    Dim choice As String = c
                    Dim item As ToolStripItem = menu.Items.Add(choice)
                    AddHandler item.Click,
                        Sub(s2, e2)
                            If onPick(choice) Then dlg.DialogResult = DialogResult.Cancel
                        End Sub
                Next
                menu.Show(btn, New Point(0, btn.Height))
            End Sub
        Return btn
    End Function

    ' ----- Students -----
    ' Adds many names at once (skips duplicates), then sorts A-Z and renumbers

    ' CSV columns: student_id,full_name. Import uses the destination selected in the popup.
    Private Function ImportStudents(choice As String, targetClassId As Integer) As Boolean
        If targetClassId = 0 Then
            MessageBox.Show("Select the class to enroll into before importing.")
            Return False
        End If
        If Not choice.StartsWith("From file") Then
            SelectClass(targetClassId)
            LoadClassData()
            MessageBox.Show("Enrolled students refreshed from the configured database.")
            Return True
        End If
        Dim rows = LoadCsvRows("student_id")
        If rows Is Nothing Then Return False
        Dim result = New RosterImportService().ImportStudents(targetClassId, AppSession.CurrentUser.UserId, rows)
        MessageBox.Show(result.Message, If(result.Success, "Students imported", "Import not saved"))
        If result.Success Then
            SelectClass(targetClassId)
            LoadClassData()
        End If
        Return result.Success
    End Function

    ' ----- Classes -----
    ' Each row = code, name, schedule, term, SY

    ' File columns: Class Code, Class Name, Schedule, Term, SY
    Private Function ImportClasses(choice As String) As Boolean
        If choice.StartsWith("From file") Then
            Dim path = PickCsv()
            If path = "" Then Return False
            Try
                pendingClassImport = ReadCsv(path)
                MessageBox.Show("CSV loaded. Fill in the class details, then click Add to validate and save it. Format: student_id,full_name,Quiz 1 /20,Quiz 2 /20,...")
            Catch ex As Exception
                pendingClassImport = Nothing
                MessageBox.Show("Could not read CSV: " & ex.Message)
            End Try
            Return False
        End If
        LoadClassesFromDatabase()
        Return True
    End Function

    ' Pending classes sent by the admin: select one, then Approve or Decline

    ' ----- Assessments -----
    ' File columns: Type, Counts toward (Quiz or Class Work), Max Score
    Private Function ImportAssessments(choice As String) As Boolean
        Dim classId = GetClassIdFromCode(SelectedClassCode())
        If classId = 0 Then
            MessageBox.Show("Select a class before importing.")
            Return False
        End If
        If Not choice.StartsWith("From file") Then
            LoadClassData()
            MessageBox.Show("Existing assessments have been refreshed from the database.")
            Return False
        End If
        Dim rows = LoadCsvRows("type")
        If rows Is Nothing Then Return False
        ' CSV: type, group, max_score, term, weight. Validate every row before saving.
        Dim definitions As New List(Of (Type As String, Max As Double, Term As String, Weight As Double))
        For i = 0 To rows.Count - 1
            Dim fields = rows(i)
            Dim maximum, weight As Double
            If Not ValidationHelper.TryAssessmentImportRow(fields, maximum, weight) Then
                MessageBox.Show("Invalid CSV row " & (i + 1) & ". Expected: type,group,max_score,term,weight. Types: Quiz, Activity, Exam, Project, Attendance; term: Midterm or Finals.")
                Return False
            End If
            definitions.Add((fields(0).Trim(), maximum, fields(3).Trim(), weight))
        Next
        If definitions.Count = 0 Then Return False
        If MessageBox.Show("Save " & definitions.Count & " assessments to the selected class?", "Confirm import", MessageBoxButtons.YesNo) <> DialogResult.Yes Then Return False
        Dim added = 0
        For Each definition In definitions
            Dim result = _gradeService.CreateAssessment(classId, NextTitle(definition.Type), definition.Type, definition.Max, definition.Term, AppSession.CurrentUser.UserId, definition.Weight)
            If Not result.Success Then
                MessageBox.Show("Imported " & added & " assessments before stopping: " & result.Message)
                LoadClassData()
                Return False
            End If
            added += 1
            LoadClassData()
        Next
        MessageBox.Show("Imported " & added & " assessments.")
        Return True
    End Function

    Private Sub ConfigureToolbar()
        Dim toolbar As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .WrapContents = True, .AutoScroll = True, .Padding = New Padding(8), .BackColor = Color.White}
        Dim controls() As Control = {Label1, cboCourse, btnAddClass, btnAddStudent, btnAddAssessment, CustomButton2, CustomButton1}
        For Each control In controls
            control.Dock = DockStyle.None
            control.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            control.Margin = New Padding(4, 3, 4, 3)
            control.Height = 26
            If TypeOf control Is Button Then
                control.Font = New Font("Segoe UI", 9)
                control.Width = TextRenderer.MeasureText(control.Text, control.Font).Width + 20
            End If
            toolbar.Controls.Add(control)
        Next
        Label1.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        Label1.Width = 180
        cboCourse.Width = 190
        lblMeetings.Visible = False
        txtMeetings.Visible = False
        PictureBox1.Visible = False
        Panel2.Controls.Add(toolbar)
        toolbar.BringToFront()
        Dim resizeToolbar As EventHandler = Sub(sender, args)
                                                Panel2.Height = Math.Max(58, toolbar.GetPreferredSize(New Size(Math.Max(300, Panel2.ClientSize.Width), 0)).Height + 12)
                                            End Sub
        AddHandler Panel2.SizeChanged, resizeToolbar
        resizeToolbar(Me, EventArgs.Empty)
    End Sub

    Private Function GetClassIdFromCode(code As String) As Integer
        If code Is Nothing OrElse Not classes.ContainsKey(code) Then Return 0
        Return classes(code).Id
    End Function

    Public Sub ReviewPendingRevisionRequests()
        Using requests As New RevisionRequestsForm(AppSession.CurrentUser.UserId, GetClassIdFromCode(SelectedClassCode()))
            requests.ShowDialog(Me)
        End Using
        LoadClassData()
    End Sub

    Private Sub HookUpRevisionButton()
        For Each c As Control In GetAllControls(Me)
            If c.Name.Equals("btnRevisionRequests", StringComparison.OrdinalIgnoreCase) _
               OrElse c.Name.Equals("btnRevision", StringComparison.OrdinalIgnoreCase) _
               OrElse c.Text.StartsWith("Revis", StringComparison.OrdinalIgnoreCase) Then

                RemoveHandler c.Click, AddressOf btnRevisionRequests_Click
                AddHandler c.Click, AddressOf btnRevisionRequests_Click
                Return
            End If
        Next
    End Sub

    Private Function GetAllControls(parent As Control) As List(Of Control)
        Dim list As New List(Of Control)()
        For Each c As Control In parent.Controls
            list.Add(c)
            If c.HasChildren Then
                list.AddRange(GetAllControls(c))
            End If
        Next
        Return list
    End Function

    Private Sub btnRevisionRequests_Click(sender As Object, e As EventArgs)
        ReviewPendingRevisionRequests()
    End Sub
End Class