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
        Public Group As String    ' "Quiz" or "CW"
    End Class
    Private loadingClass As Boolean
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
        lblSummary.Tag = "Assessment scores are saved. Attendance, project percentage, exam summary, and other worksheet inputs are local calculations only."
        classTip.SetToolTip(dgvGrades, CStr(lblSummary.Tag))

        cboCourse.DropDownStyle = ComboBoxStyle.DropDownList

        ' Load real active classes from MySQL database
        LoadClassesFromDatabase()

        SelectClass(InitialClassId)
        ConfigureToolbar()
        UpdateSummary()
    End Sub

    Private Sub LoadClassesFromDatabase()
        cboCourse.Items.Clear()
        classes.Clear()

        ' Fetch classes from MySQL for current logged-in teacher
        Dim dt As DataTable = _classService.GetClassesByTeacher(AppSession.CurrentUser.UserId)

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            For Each row As DataRow In dt.Rows
                Dim code As String = Convert.ToString(row("class_label"))
                Dim name As String = Convert.ToString(row("course_name"))
                Dim term As String = Convert.ToString(row("term"))
                Dim sy As String = Convert.ToString(row("academic_year"))
                Dim sched As String = Convert.ToString(row("schedule"))

                Dim info As New ClassInfo With {
                    .Code = code, .Name = name, .Term = term, .SY = sy, .Schedule = sched, .Id = Convert.ToInt32(row("class_id"))
                }
                classes(code) = info

                Dim displayText As String = code
                cboCourse.Items.Add(displayText)
            Next

            If cboCourse.Items.Count > 0 Then
                cboCourse.SelectedIndex = 0
            End If
        End If
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
        If m > 0 Then Return "Please enter a number from 0 to " & m & "."
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
        If Not TryNum(text, v) OrElse v < 0 OrElse (maxVal > 0 AndAlso v > maxVal) Then
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
        Dim n As Integer = 1
        For Each it As AssessItem In items
            If String.Equals(it.Type, type, StringComparison.OrdinalIgnoreCase) Then n += 1
        Next
        Return type & " " & n
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
        AddCol(key & "GrA", title & vbLf & "GrA", "calc", at + 1)

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
        Dim c As Integer = String.Compare(Surname(a), Surname(b), StringComparison.CurrentCultureIgnoreCase)
        If c <> 0 Then Return c
        Return String.Compare(GivenName(a), GivenName(b), StringComparison.CurrentCultureIgnoreCase)
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

        studentList.Sort(Function(a, b) CompareNames(Convert.ToString(a.Item1(nameIdx)), Convert.ToString(b.Item1(nameIdx))))

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

    ' ===== Computation (same logic as the Excel gradesheet) =====
    ' Averages the GrA of every item in a group, times the group's weight.
    ' A blank score counts as GrA 50 (like the sheet) and makes the student "inc".
    Private Function ComputeGroup(row As DataGridViewRow, grp As String, weight As Double,
                                  ByRef missing As Boolean) As Double
        Dim v As Double
        Dim graSum As Double = 0
        Dim count As Integer = 0

        For Each it As AssessItem In items
            If it.Group <> grp Then Continue For
            Dim gra As Double = 50
            If TryNum(row.Cells(it.Key & "Raw").Value, v) Then
                gra = v / maxScores(it.Key & "Raw") * 50 + 50
            Else
                missing = True
            End If
            row.Cells(it.Key & "GrA").Value = gra.ToString("0.00")
            graSum += gra
            count += 1
        Next

        If count = 0 Then
            missing = True   ' nothing added yet for this component
            row.Cells(grp & "Comp").Value = ""
            Return 0
        End If

        Dim result As Double = graSum / count / 100 * weight
        row.Cells(grp & "Comp").Value = result.ToString("0.00")
        Return result
    End Function

    Private Sub ComputeRow(row As DataGridViewRow)
        If Not IsStudentRow(row) Then Return

        Dim v As Double
        Dim missing As Boolean = False

        ' Part: absences vs. number of meetings
        Dim meets As Double = Meetings()
        Dim absent As Double = 0
        If TryNum(row.Cells("Part").Value, v) Then absent = Math.Min(Math.Max(v, 0), meets)
        Dim partGrA As Double = W_PART * (meets - absent) / meets
        row.Cells("PartGrA").Value = partGrA.ToString("0.00")

        ' Extra (optional)
        Dim extra As Double = 0
        If TryNum(row.Cells("Extra").Value, v) Then extra = v / 100 * W_EXTRA

        ' Class work and quizzes
        Dim cw As Double = ComputeGroup(row, "CW", W_CW, missing)
        Dim quiz As Double = ComputeGroup(row, "Quiz", W_QUIZ, missing)

        ' Project (percent)
        Dim proj As Double = 0
        If TryNum(row.Cells("Project").Value, v) Then
            proj = v / 100 * W_PROJECT
        Else
            missing = True
        End If
        row.Cells("ProjectGrA").Value = proj.ToString("0.00")

        ' Major exam
        Dim examGrA As Double = 50
        If TryNum(row.Cells("ExamRaw").Value, v) Then
            examGrA = v / maxScores("ExamRaw") * 50 + 50
        Else
            missing = True
        End If
        row.Cells("ExamGrA").Value = examGrA.ToString("0.00")
        Dim major As Double = examGrA / 100 * W_EXAM
        row.Cells("MajorExam").Value = major.ToString("0.00")

        ' Grade + remarks (Grade stays blank while incomplete or dropped, like the sheet)
        Dim total As Double = partGrA + extra + cw + quiz + proj + major
        Dim dropped As Boolean = Convert.ToString(row.Cells("Drp").Value).Trim().ToLower() = "drp"

        If dropped Then
            row.Cells("Total").Value = ""
            row.Cells("Remarks").Value = "drp"
        ElseIf missing Then
            row.Cells("Total").Value = ""
            row.Cells("Remarks").Value = "inc"
        Else
            row.Cells("Total").Value = total.ToString("0.00")
            row.Cells("Remarks").Value = If(total >= PASSING, "pass", "fail")
        End If
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
                Case "inc" : inc += 1
                Case "pass" : pass += 1
                Case "fail" : fail += 1
            End Select
        Next

        Dim total As Integer = drp + inc + pass + fail
        lblSummary.Text = "SUMMARY OF GRADES     " &
                          "DRP " & drp & " (" & Pct(drp, total) & "%)   |   " &
                          "INC " & inc & " (" & Pct(inc, total) & "%)   |   " &
                          "PASS " & pass & " (" & Pct(pass, total) & "%)   |   " &
                          "FAIL " & fail & " (" & Pct(fail, total) & "%)   |   " &
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
                Case "pass" : e.CellStyle.ForeColor = Color.FromArgb(0, 128, 60)
                Case "fail" : e.CellStyle.ForeColor = Color.FromArgb(170, 0, 0)
                Case "inc" : e.CellStyle.ForeColor = Color.FromArgb(214, 120, 0)
                Case "drp" : e.CellStyle.ForeColor = Color.DimGray
            End Select
        End If
    End Sub

    ' Only cells on real student rows can be edited
    Private Sub dgvGrades_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles dgvGrades.CellBeginEdit
        If Not IsStudentRow(dgvGrades.Rows(e.RowIndex)) Then e.Cancel = True
    End Sub

    Private Sub dgvGrades_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGrades.CellEndEdit
        If e.RowIndex < 0 Then Return

        Dim row As DataGridViewRow = dgvGrades.Rows(e.RowIndex)
        If Not IsStudentRow(row) Then Return

        Dim cell As DataGridViewCell = row.Cells(e.ColumnIndex)
        Dim colName As String = dgvGrades.Columns(e.ColumnIndex).Name

        If Not ApplyScore(cell, Convert.ToString(cell.Value)) Then
            MessageBox.Show(InputHint(colName), "Digital Gradebook")
        End If

        ComputeRow(row)
        UpdateSummary()

        If Not loadingClass AndAlso assessmentIds.ContainsKey(colName) Then
            Dim result = _gradeService.SaveAssessmentScore(
                Convert.ToString(row.Cells("StudentName").Tag), GetClassIdFromCode(SelectedClassCode()),
                assessmentIds(colName), If(cell.Value Is Nothing, CType(Nothing, Double?), CDbl(cell.Value)), AppSession.CurrentUser.UserId)
            If Not result.Success Then MessageBox.Show(result.Message, "Score not saved", MessageBoxButtons.OK, MessageBoxIcon.Error)
            LoadClassData()
        End If
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

        Dim lines() As String = Clipboard.GetText().Replace(vbCrLf, vbLf).TrimEnd(ControlChars.Lf).Split(ControlChars.Lf)
        Dim r0 As Integer = dgvGrades.CurrentCell.RowIndex
        Dim c0 As Integer = dgvGrades.CurrentCell.ColumnIndex

        For i As Integer = 0 To lines.Length - 1
            If r0 + i >= dgvGrades.Rows.Count Then Exit For
            Dim parts() As String = lines(i).Split(ControlChars.Tab)
            For j As Integer = 0 To parts.Length - 1
                If c0 + j >= dgvGrades.Columns.Count Then Exit For
                Dim cell As DataGridViewCell = dgvGrades.Rows(r0 + i).Cells(c0 + j)
                If CanEditCell(cell) AndAlso ApplyScore(cell, parts(j)) AndAlso assessmentIds.ContainsKey(dgvGrades.Columns(cell.ColumnIndex).Name) Then
                    Dim result = _gradeService.SaveAssessmentScore(Convert.ToString(dgvGrades.Rows(cell.RowIndex).Cells("StudentName").Tag), GetClassIdFromCode(SelectedClassCode()), assessmentIds(dgvGrades.Columns(cell.ColumnIndex).Name), If(cell.Value Is Nothing, CType(Nothing, Double?), CDbl(cell.Value)), AppSession.CurrentUser.UserId)
                    If Not result.Success Then
                        MessageBox.Show(result.Message, "Paste stopped")
                        LoadClassData()
                        Return
                    End If
                End If
            Next
        Next

        ComputeAll()
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
                    Dim printDoc As New Printing.PrintDocument()
                    printDoc.DefaultPageSettings.Landscape = True

                    AddHandler printDoc.PrintPage, Sub(s, args)
                                                       Dim g As Graphics = args.Graphics
                                                       Dim titleFont As New Font("Segoe UI", 13, FontStyle.Bold)
                                                       Dim subFont As New Font("Segoe UI", 8.5F, FontStyle.Regular)
                                                       Dim headerFont As New Font("Segoe UI", 8.0F, FontStyle.Bold)
                                                       Dim bodyFont As New Font("Segoe UI", 8.0F, FontStyle.Regular)

                                                       Dim startX As Integer = 35
                                                       Dim y As Integer = 35
                                                       Dim selectedCourseText As String = If(cboCourse.SelectedItem IsNot Nothing, cboCourse.SelectedItem.ToString(), "Master Gradebook")

                                                       ' Report Title Header
                                                       g.DrawString($"DAUNTLESS WOLVES - DIGITAL GRADEBOOK ({selectedCourseText})", titleFont, Brushes.DarkGreen, startX, y)
                                                       y += 24
                                                       g.DrawString($"Meetings: {Meetings()} | Total Students: {studentRowCount} | Generated: {DateTime.Now:MMM d, yyyy HH:mm}", subFont, Brushes.Gray, startX, y)
                                                       y += 28

                                                       ' Short Clean Header Label Mapper
                                                       Dim getCleanHeader As Func(Of DataGridViewColumn, String) =
                                                           Function(col As DataGridViewColumn)
                                                               Select Case col.Name.ToLower()
                                                                   Case "no" : Return "No."
                                                                   Case "studentname" : Return "Student Name"
                                                                   Case "drp" : Return "Drp"
                                                                   Case "part" : Return "Abs"
                                                                   Case "partgra" : Return "Part%"
                                                                   Case "extra" : Return "Ext%"
                                                                   Case "cwcomp" : Return "CW%"
                                                                   Case "quizcomp" : Return "Quiz%"
                                                                   Case "project" : Return "Proj"
                                                                   Case "projectgra" : Return "Proj%"
                                                                   Case "examraw" : Return "Exam"
                                                                   Case "examgra" : Return "ExGrA"
                                                                   Case "majorexam" : Return "Major%"
                                                                   Case "total" : Return "Grade"
                                                                   Case "remarks" : Return "Status"
                                                                   Case Else
                                                                       Dim lines() As String = col.HeaderText.Split({vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
                                                                       Return If(lines.Length > 0, lines(0), col.HeaderText)
                                                               End Select
                                                           End Function

                                                       ' Column Width Allocator
                                                       Dim getColWidth As Func(Of DataGridViewColumn, Integer) =
                                                           Function(col As DataGridViewColumn)
                                                               Select Case col.Name.ToLower()
                                                                   Case "no" : Return 30
                                                                   Case "studentname" : Return 160
                                                                   Case "drp", "part" : Return 38
                                                                   Case "partgra", "extra", "cwcomp", "quizcomp", "project", "projectgra", "examraw", "examgra", "majorexam" : Return 52
                                                                   Case "total", "remarks" : Return 50
                                                                   Case Else : Return 48
                                                               End Select
                                                           End Function

                                                       ' 1. Print Header Row
                                                       Dim x As Integer = startX
                                                       For Each col As DataGridViewColumn In dgvGrades.Columns
                                                           If col.Visible Then
                                                               Dim colW As Integer = getColWidth(col)
                                                               Dim headerText As String = getCleanHeader(col)
                                                               g.DrawString(headerText, headerFont, Brushes.DarkGreen, x, y)
                                                               x += colW
                                                           End If
                                                       Next

                                                       y += 18
                                                       g.DrawLine(Pens.DarkGreen, startX, y, x, y)
                                                       y += 8

                                                       ' 2. Print Student Rows
                                                       For Each row As DataGridViewRow In dgvGrades.Rows
                                                           If IsStudentRow(row) Then
                                                               x = startX
                                                               For Each col As DataGridViewColumn In dgvGrades.Columns
                                                                   If col.Visible Then
                                                                       Dim colW As Integer = getColWidth(col)
                                                                       Dim cellVal As String = If(row.Cells(col.Index).Value IsNot Nothing, row.Cells(col.Index).Value.ToString(), "")

                                                                       g.DrawString(cellVal, bodyFont, Brushes.Black, x, y)
                                                                       x += colW
                                                                   End If
                                                               Next
                                                               y += 22
                                                           End If
                                                       Next

                                                       ' 3. Bottom Summary Bar
                                                       y += 10
                                                       g.DrawLine(Pens.Gray, startX, y, x, y)
                                                       y += 8
                                                       g.DrawString(lblSummary.Text, headerFont, Brushes.DarkGreen, startX, y)
                                                   End Sub

                    If _exportService.ExportToPDF(printDoc, sfd.FileName) Then
                        MessageBox.Show("Digital gradebook exported to PDF successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
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
    Private Function CodeExists(code As String) As Boolean
        For Each it As Object In cboCourse.Items
            If CStr(it).StartsWith(code & " - ", StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

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
        LoadClassData()
    End Sub

    Private Sub LoadClassData()
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
            If classId = 0 Then Return
            For Each a As DataRow In _gradeService.GetAssessmentsByClass(classId, AppSession.CurrentUser.UserId).Rows
                Dim key = "Assessment" & a("assessment_id").ToString()
                Dim title = a("title").ToString()
                Dim type = a("assessment_type").ToString()
                Dim grp = GroupFor(type)
                Dim at = dgvGrades.Columns(grp & "Comp").Index
                AddCol(key & "Raw", RawHeader(title, CDbl(a("total_score"))), "input", at)
                AddCol(key & "GrA", title & vbLf & "GrA", "calc", at + 1)
                items.Add(New AssessItem With {.Key = key, .Title = title, .Type = type, .Group = grp})
                maxScores(key & "Raw") = CDbl(a("total_score"))
                assessmentIds(key & "Raw") = CInt(a("assessment_id"))
            Next
            Dim scores = _gradeService.GetScoresByClass(classId)
            For Each student As DataRow In _gradeService.GetEnrollmentsByClass(classId, AppSession.CurrentUser.UserId).Rows
                Dim idx = dgvGrades.Rows.Add()
                Dim row = dgvGrades.Rows(idx)
                row.Cells("StudentName").Value = student("full_name")
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
            loadingClass = False
            FillEmptyRows()
            ComputeAll()
        End Try
    End Sub

    ' ===== Add Student: name goes into the grid, list re-sorts A-Z and renumbers =====
    Private Sub btnAddStudent_Click(sender As Object, e As EventArgs) Handles btnAddStudent.Click
        Dim fullName As String = ""
        Dim studentNo As String = ""
        Dim course As String = ""

        If Not ShowStudentDialog(fullName, studentNo, course) Then Return

        Dim result = _classService.EnrollStudent(studentNo, GetClassIdFromCode(SelectedClassCode()), "Teacher", AppSession.CurrentUser.UserId)
        If Not result.Success Then MessageBox.Show(result.Message, "Enrollment failed")
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
        Dim dark As Color = Color.FromArgb(0, 100, 0)
        Dim green As Color = Color.FromArgb(0, 191, 99)
        Dim accepted As Boolean = False

        Using dlg As New Form()
            dlg.FormBorderStyle = FormBorderStyle.None
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.Size = New Size(380, 440)
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
                .Text = "Add Student",
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 12.0F),
                .AutoSize = True,
                .Location = New Point(18, 12)
            }
            header.Controls.Add(lblTitle)
            header.Controls.Add(MakeImportButton(dlg,
                {"From file (CSV)...", "From MySQL (this class)..."},
                Function(choice) ImportStudents(choice)))

            ' Surname
            Dim lblSurname As New Label() With {
                .Text = "Surname",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 62)
            }
            Dim txtSurname As New TextBox() With {
                .BorderStyle = BorderStyle.None,
                .Font = New Font("Segoe UI", 12.0F),
                .Location = New Point(24, 84),
                .Width = 332
            }
            Dim lineSurname As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 112),
                .Size = New Size(332, 2)
            }

            ' First name + M.I. on the same row
            Dim lblFirst As New Label() With {
                .Text = "First Name",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 128)
            }
            Dim txtFirst As New TextBox() With {
                .BorderStyle = BorderStyle.None,
                .Font = New Font("Segoe UI", 12.0F),
                .Location = New Point(24, 150),
                .Width = 250
            }
            Dim lineFirst As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 178),
                .Size = New Size(250, 2)
            }

            Dim lblMI As New Label() With {
                .Text = "M.I.",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(292, 128)
            }
            Dim txtMI As New TextBox() With {
                .BorderStyle = BorderStyle.None,
                .Font = New Font("Segoe UI", 12.0F),
                .Location = New Point(292, 150),
                .Width = 64,
                .MaxLength = 3
            }
            Dim lineMI As New Panel() With {
                .BackColor = green,
                .Location = New Point(292, 178),
                .Size = New Size(64, 2)
            }

            ' Student Number
            Dim lblNo As New Label() With {
                .Text = "Student Number",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 194)
            }
            Dim txtNo As New TextBox() With {
                .BorderStyle = BorderStyle.None,
                .Font = New Font("Segoe UI", 12.0F),
                .Location = New Point(24, 216),
                .Width = 332
            }
            Dim lineNo As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 244),
                .Size = New Size(332, 2)
            }

            ' Course
            Dim lblCourse As New Label() With {
                .Text = "Course",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 260)
            }
            Dim cboDlgCourse As New ComboBox() With {
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = Color.White,
                .Font = New Font("Segoe UI", 11.0F),
                .Location = New Point(24, 282),
                .Width = 332
            }
            For Each item In cboCourse.Items
                cboDlgCourse.Items.Add(item)
            Next
            cboDlgCourse.SelectedIndex = cboCourse.SelectedIndex
            Dim lineCourse As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 312),
                .Size = New Size(332, 2)
            }

            Dim lblError As New Label() With {
                .Text = "",
                .ForeColor = Color.Firebrick,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 326)
            }

            Dim btnOk As New Button() With {
                .Text = "Add",
                .FlatStyle = FlatStyle.Flat,
                .BackColor = dark,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 10.0F),
                .Size = New Size(100, 36),
                .Location = New Point(256, 380),
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
                .Location = New Point(146, 380),
                .Cursor = Cursors.Hand
            }
            btnCancel.FlatAppearance.BorderColor = dark
            btnCancel.FlatAppearance.BorderSize = 1

            AddHandler btnOk.Click,
                Sub(s, ev)
                    If txtSurname.Text.Replace(",", "").Trim() = "" Then
                        lblError.Text = "Please enter the surname."
                        txtSurname.Focus()
                    ElseIf txtFirst.Text.Replace(",", "").Trim() = "" Then
                        lblError.Text = "Please enter the first name."
                        txtFirst.Focus()
                    ElseIf txtNo.Text.Trim() = "" Then
                        lblError.Text = "Please enter the student number."
                        txtNo.Focus()
                    ElseIf cboDlgCourse.SelectedIndex = -1 Then
                        lblError.Text = "Please select a course."
                    Else
                        accepted = True
                        dlg.DialogResult = DialogResult.OK
                    End If
                End Sub

            dlg.Controls.AddRange({lblSurname, txtSurname, lineSurname,
                                   lblFirst, txtFirst, lineFirst,
                                   lblMI, txtMI, lineMI,
                                   lblNo, txtNo, lineNo,
                                   lblCourse, cboDlgCourse, lineCourse,
                                   lblError, btnOk, btnCancel, header})
            dlg.AcceptButton = btnOk
            dlg.CancelButton = btnCancel
            dlg.ActiveControl = txtSurname

            dlg.ShowDialog(If(assessmentOwner, Me))

            If accepted Then
                fullName = BuildName(txtSurname.Text, txtFirst.Text, txtMI.Text)
                studentNo = txtNo.Text.Trim()
                course = CStr(cboDlgCourse.SelectedItem)
            End If
        End Using

        Return accepted
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
                    ElseIf CodeExists(code) Then
                        lblError.Text = "That class code already exists."
                        txtCode.Focus()
                    ElseIf txtName.Text.Trim() = "" Then
                        lblError.Text = "Please enter the class name."
                        txtName.Focus()
                    ElseIf txtSched.Text.Trim() = "" Then
                        lblError.Text = "Please enter the schedule."
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

            ' Counts toward
            Dim lblGroup As New Label() With {
                .Text = "Counts toward",
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
            cboGroup.Items.Add("Quiz (" & W_QUIZ & "%)")
            cboGroup.Items.Add("Class Work (" & W_CW & "%)")
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
    Private Function GetConnectionString() As String
        ' TODO: replace with your real connection (or read it from dbconfig.json)
        Return "Server=localhost;Port=3306;Database=gradebook;Uid=root;Pwd=;"
    End Function

    ' "IT 112 - Information Assurance" -> "IT 112"
    Private Function SelectedClassCode() As String
        Return If(cboCourse.SelectedItem Is Nothing, "", cboCourse.SelectedItem.ToString())
    End Function

    ' pairs = "@name", value, "@name", value ...
    Private Function RunQuery(sql As String, ParamArray pairs() As Object) As DataTable
        Try
            Using con As New MySqlConnection(GetConnectionString())
                Using cmd As New MySqlCommand(sql, con)
                    For i As Integer = 0 To pairs.Length - 2 Step 2
                        cmd.Parameters.AddWithValue(CStr(pairs(i)), pairs(i + 1))
                    Next
                    Dim dt As New DataTable()
                    Using da As New MySqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using
                    Return dt
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not read from MySQL: " & ex.Message, "Digital Gradebook")
            Return Nothing
        End Try
    End Function

    Private Function RunNonQuery(sql As String, ParamArray pairs() As Object) As Boolean
        Try
            Using con As New MySqlConnection(GetConnectionString())
                Using cmd As New MySqlCommand(sql, con)
                    For i As Integer = 0 To pairs.Length - 2 Step 2
                        cmd.Parameters.AddWithValue(CStr(pairs(i)), pairs(i + 1))
                    Next
                    con.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            Return True
        Catch ex As Exception
            MessageBox.Show("Could not update MySQL: " & ex.Message, "Digital Gradebook")
            Return False
        End Try
    End Function

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
    Private Function AddStudentNames(names As List(Of String)) As Integer
        Dim existing As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each r As DataGridViewRow In dgvGrades.Rows
            If IsStudentRow(r) Then existing.Add(Convert.ToString(r.Cells("StudentName").Value))
        Next

        Dim added As Integer = 0
        For Each n As String In names
            If existing.Add(n) Then
                Dim vals(dgvGrades.Columns.Count - 1) As Object
                vals(dgvGrades.Columns("StudentName").Index) = n
                dgvGrades.Rows.Add(vals)
                added += 1
            End If
        Next

        If added > 0 Then SortAndRenumber()
        Return added
    End Function

    ' File columns: Surname, First Name, M.I., Student Number
    Private Function ImportStudents(choice As String) As Boolean
        Dim names As New List(Of String)

        If choice.StartsWith("From file") Then
            Dim rows As List(Of String()) = LoadCsvRows("surname")
            If rows Is Nothing Then Return False
            For Each f As String() In rows
                If f.Length < 2 OrElse f(0).Trim() = "" OrElse f(1).Trim() = "" Then Continue For
                names.Add(BuildName(f(0), f(1), If(f.Length > 2, f(2), "")))
            Next
        Else
            Dim code As String = SelectedClassCode()
            If code = "" Then
                MessageBox.Show("Please select a class first.", "Digital Gradebook")
                Return False
            End If
            Dim dt As DataTable = RunQuery("SELECT surname, first_name, mi FROM students WHERE class_code = @c", "@c", code)
            If dt Is Nothing Then Return False
            For Each r As DataRow In dt.Rows
                names.Add(BuildName(Convert.ToString(r("surname")), Convert.ToString(r("first_name")),
                                    Convert.ToString(r("mi"))))
            Next
        End If

        Dim added As Integer = AddStudentNames(names)
        If added = 0 Then
            MessageBox.Show("No new students were found (empty, incomplete or already in the list).", "Digital Gradebook")
            Return False
        End If
        MessageBox.Show("Imported " & added & " of " & names.Count & " students.", "Digital Gradebook")
        Return True
    End Function

    ' ----- Classes -----
    ' Each row = code, name, schedule, term, SY
    Private Function AddClassRows(rows As List(Of String())) As Integer
        Dim added As Integer = 0
        For Each f As String() In rows
            If f.Length < 5 Then Continue For
            Dim code As String = System.Text.RegularExpressions.Regex.Replace(f(0).Trim(), "\s+", " ").ToUpper()
            If code = "" OrElse code.Contains(" - ") OrElse CodeExists(code) OrElse f(1).Trim() = "" Then Continue For

            Dim info As New ClassInfo With {
                .Code = code, .Name = f(1).Trim(), .Schedule = f(2).Trim(),
                .Term = f(3).Trim(), .SY = f(4).Trim()
            }
            classes(code) = info
            cboCourse.Items.Add(code & " - " & info.Name)
            added += 1
        Next
        If added > 0 Then cboCourse.SelectedIndex = cboCourse.Items.Count - 1
        Return added
    End Function

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
    Private Function ImportFromAdmin() As Boolean
        Dim dt As DataTable = RunQuery("SELECT class_code, class_name, schedule, term, sy FROM classes WHERE status = 'pending'")
        If dt Is Nothing Then Return False
        If dt.Rows.Count = 0 Then
            MessageBox.Show("There are no pending class requests from the admin.", "Digital Gradebook")
            Return False
        End If

        Dim approvedAny As Boolean = False

        Using dlg As New Form()
            dlg.FormBorderStyle = FormBorderStyle.None
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.Size = New Size(600, 420)
            dlg.BackColor = Color.White
            dlg.ShowInTaskbar = False

            AddHandler dlg.Paint,
                Sub(s, ev)
                    Using pen As New Pen(DarkGreen, 2)
                        ev.Graphics.DrawRectangle(pen, 1, 1, dlg.Width - 2, dlg.Height - 2)
                    End Using
                End Sub

            Dim header As New Panel() With {.Dock = DockStyle.Top, .Height = 48, .BackColor = DarkGreen}
            header.Controls.Add(New Label() With {
                .Text = "Class Requests from Admin",
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 12.0F),
                .AutoSize = True,
                .Location = New Point(18, 12)
            })

            Dim lv As New ListView() With {
                .View = View.Details,
                .FullRowSelect = True,
                .MultiSelect = False,
                .HideSelection = False,
                .GridLines = True,
                .Font = New Font("Segoe UI", 10.0F),
                .Location = New Point(24, 64),
                .Size = New Size(552, 262)
            }
            lv.Columns.Add("Code", 80)
            lv.Columns.Add("Class Name", 190)
            lv.Columns.Add("Schedule", 110)
            lv.Columns.Add("Term", 90)
            lv.Columns.Add("SY", 80)
            For Each r As DataRow In dt.Rows
                Dim row As New ListViewItem(Convert.ToString(r("class_code")))
                row.SubItems.Add(Convert.ToString(r("class_name")))
                row.SubItems.Add(Convert.ToString(r("schedule")))
                row.SubItems.Add(Convert.ToString(r("term")))
                row.SubItems.Add(Convert.ToString(r("sy")))
                lv.Items.Add(row)
            Next

            Dim lblInfo As New Label() With {
                .Text = "Select a class, then Approve or Decline.",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 336)
            }

            Dim btnApprove As New Button() With {
                .Text = "Approve", .FlatStyle = FlatStyle.Flat, .BackColor = DarkGreen, .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 10.0F), .Size = New Size(100, 36),
                .Location = New Point(366, 368), .Cursor = Cursors.Hand
            }
            btnApprove.FlatAppearance.BorderSize = 0

            Dim btnDecline As New Button() With {
                .Text = "Decline", .FlatStyle = FlatStyle.Flat, .BackColor = Color.White, .ForeColor = Color.Firebrick,
                .Font = New Font("Segoe UI Semibold", 10.0F), .Size = New Size(100, 36),
                .Location = New Point(256, 368), .Cursor = Cursors.Hand
            }
            btnDecline.FlatAppearance.BorderColor = Color.Firebrick
            btnDecline.FlatAppearance.BorderSize = 1

            Dim btnClose As New Button() With {
                .Text = "Close", .DialogResult = DialogResult.Cancel, .FlatStyle = FlatStyle.Flat,
                .BackColor = Color.White, .ForeColor = DarkGreen,
                .Font = New Font("Segoe UI Semibold", 10.0F), .Size = New Size(100, 36),
                .Location = New Point(476, 368), .Cursor = Cursors.Hand
            }
            btnClose.FlatAppearance.BorderColor = DarkGreen
            btnClose.FlatAppearance.BorderSize = 1

            AddHandler btnApprove.Click,
                Sub(s, ev)
                    If lv.SelectedItems.Count = 0 Then
                        lblInfo.Text = "Select a class first."
                        Return
                    End If
                    Dim sel As ListViewItem = lv.SelectedItems(0)
                    If RunNonQuery("UPDATE classes SET status = 'approved' WHERE class_code = @c", "@c", sel.Text) Then
                        AddClassRows(New List(Of String()) From {
                            New String() {sel.Text, sel.SubItems(1).Text, sel.SubItems(2).Text,
                                          sel.SubItems(3).Text, sel.SubItems(4).Text}})
                        approvedAny = True
                        lblInfo.Text = "Approved " & sel.Text & "."
                        lv.Items.Remove(sel)
                    End If
                End Sub

            AddHandler btnDecline.Click,
                Sub(s, ev)
                    If lv.SelectedItems.Count = 0 Then
                        lblInfo.Text = "Select a class first."
                        Return
                    End If
                    Dim sel As ListViewItem = lv.SelectedItems(0)
                    If RunNonQuery("UPDATE classes SET status = 'declined' WHERE class_code = @c", "@c", sel.Text) Then
                        lblInfo.Text = "Declined " & sel.Text & "."
                        lv.Items.Remove(sel)
                    End If
                End Sub

            dlg.Controls.AddRange({lv, lblInfo, btnApprove, btnDecline, btnClose, header})
            dlg.CancelButton = btnClose
            dlg.ShowDialog(Form.ActiveForm)
        End Using

        Return approvedAny
    End Function

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
        Dim controls() As Control = {Label1, cboCourse, btnAddClass, btnAddStudent, btnAddAssessment, CustomButton2, lblMeetings, txtMeetings, CustomButton1}
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
        ' Keep the meetings label and value together when the toolbar wraps.
        Dim meetingsGroup As New FlowLayoutPanel With {.AutoSize = True, .WrapContents = False, .Margin = New Padding(4, 3, 4, 3)}
        lblMeetings.AutoSize = True
        lblMeetings.Margin = New Padding(0, 4, 4, 0)
        txtMeetings.Width = 42
        txtMeetings.Margin = New Padding(0)
        meetingsGroup.Controls.AddRange({lblMeetings, txtMeetings})
        toolbar.Controls.Add(meetingsGroup)
        toolbar.Controls.SetChildIndex(meetingsGroup, toolbar.Controls.IndexOf(CustomButton1))
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