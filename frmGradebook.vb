Imports System.Data
Imports MySql.Data.MySqlClient

Public Class frmGradebook

    ' ===== Grading setup (weights from the gradesheet; change here if needed) =====
    Private Const W_PART As Double = 5
    Private Const W_EXTRA As Double = 0
    Private Const W_CW As Double = 10
    Private Const W_QUIZ As Double = 20
    Private Const W_PROJECT As Double = 25
    Private Const W_EXAM As Double = 40
    Private Const PASSING As Double = 75

    ' Suggestions in the Add Assessment popup (the prof can also type any other type)
    Private ReadOnly PresetTypes As String() = {"Quiz", "Homework", "Activity", "Seatwork",
                                                "Laboratory", "Recitation"}

    ' Assessments the prof has added (starts empty)
    Private Class AssessItem
        Public Key As String
        Public Title As String
        Public Type As String     ' any name: Quiz, Homework, Laboratory, ...
        Public Group As String    ' "Quiz" or "CW"
    End Class
    Private items As New List(Of AssessItem)

    ' Class info (from Add Class / imports)
    Private Class ClassInfo
        Public Code As String
        Public Name As String
        Public Schedule As String
        Public Term As String
        Public SY As String
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

    Private Sub frmGradebook_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = Color.White
        EnableDoubleBuffer()
        SetupGrid()
        CreateMeetingsBox()
        CreateSummaryLabel()

        cboCourse.DropDownStyle = ComboBoxStyle.DropDownList
        cboCourse.Items.AddRange({"IT101 - Intro to Programming",
                                  "IT102 - Data Structures",
                                  "IT103 - Database Systems"})

        UpdateSummary()
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
        Dim students As New List(Of Object())
        For Each r As DataGridViewRow In dgvGrades.Rows
            If IsStudentRow(r) Then
                Dim vals(dgvGrades.Columns.Count - 1) As Object
                For i As Integer = 0 To vals.Length - 1
                    vals(i) = r.Cells(i).Value
                Next
                students.Add(vals)
            End If
        Next

        Dim nameIdx As Integer = dgvGrades.Columns("StudentName").Index
        Dim noIdx As Integer = dgvGrades.Columns("No").Index
        students.Sort(Function(a, b) CompareNames(Convert.ToString(a(nameIdx)), Convert.ToString(b(nameIdx))))

        dgvGrades.Rows.Clear()

        For i As Integer = 0 To students.Count - 1
            students(i)(noIdx) = i + 1
            dgvGrades.Rows.Add(students(i))
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
    End Sub

    ' Excel-style keys: Delete clears, Ctrl+V pastes (Ctrl+C copy is built in)
    Private Sub dgvGrades_KeyDown(sender As Object, e As KeyEventArgs) Handles dgvGrades.KeyDown
        If e.KeyCode = Keys.Delete Then
            For Each c As DataGridViewCell In dgvGrades.SelectedCells
                If CanEditCell(c) Then c.Value = Nothing
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
                If CanEditCell(cell) Then ApplyScore(cell, parts(j))
            Next
        Next

        ComputeAll()
    End Sub

    Private Sub CustomButton1_Click(sender As Object, e As EventArgs) Handles CustomButton1.Click

    End Sub

    ' ===== Add Class =====
    Private Function CodeExists(code As String) As Boolean
        For Each it As Object In cboCourse.Items
            If CStr(it).StartsWith(code & " - ", StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    Private Sub btnAddClass_Click(sender As Object, e As EventArgs) Handles btnAddClass.Click
        Dim info As ClassInfo = ShowClassDialog()
        If info Is Nothing Then Return

        Dim display As String = info.Code & " - " & info.Name
        classes(info.Code) = info
        cboCourse.Items.Add(display)
        cboCourse.SelectedItem = display

        ' TODO: save the class (code, name, schedule, term, SY) to your database
    End Sub

    ' Hover over the class box to see its schedule, term and school year
    Private Sub cboCourse_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboCourse.SelectedIndexChanged
        Dim code As String = SelectedClassCode()
        If classes.ContainsKey(code) Then
            Dim c As ClassInfo = classes(code)
            classTip.SetToolTip(cboCourse, c.Schedule & vbLf & c.Term & ", SY " & c.SY)
        Else
            classTip.SetToolTip(cboCourse, "")
        End If
    End Sub

    ' ===== Add Student: name goes into the grid, list re-sorts A-Z and renumbers =====
    Private Sub btnAddStudent_Click(sender As Object, e As EventArgs) Handles btnAddStudent.Click
        Dim fullName As String = ""
        Dim studentNo As String = ""
        Dim course As String = ""

        If Not ShowStudentDialog(fullName, studentNo, course) Then Return

        For Each r As DataGridViewRow In dgvGrades.Rows
            If IsStudentRow(r) AndAlso String.Equals(Convert.ToString(r.Cells("StudentName").Value),
                                                     fullName, StringComparison.OrdinalIgnoreCase) Then
                MessageBox.Show("That student is already in the list.", "Digital Gradebook")
                Return
            End If
        Next

        Dim vals(dgvGrades.Columns.Count - 1) As Object
        vals(dgvGrades.Columns("StudentName").Index) = fullName
        dgvGrades.Rows.Add(vals)

        SortAndRenumber()

        ' TODO: save fullName, studentNo and course to your database
    End Sub

    ' ===== Add Assessment: type anything, it numbers itself (Quiz 1, Quiz 2, Laboratory 1 ...) =====
    Private Sub btnAddAssessment_Click(sender As Object, e As EventArgs) Handles btnAddAssessment.Click
        If cboCourse.SelectedIndex = -1 Then
            MessageBox.Show("Please select a course first.", "Digital Gradebook")
            Return
        End If

        Dim type As String = ""
        Dim grp As String = ""
        Dim maxScore As Double = 0
        If Not ShowAssessmentDialog(type, grp, maxScore) Then Return

        AddAssessment(type, grp, maxScore)

        ' TODO: save the new assessment for the selected course
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

            dlg.ShowDialog(Me)

            If accepted Then
                fullName = BuildName(txtSurname.Text, txtFirst.Text, txtMI.Text)
                studentNo = txtNo.Text.Trim()
                course = CStr(cboDlgCourse.SelectedItem)
            End If
        End Using

        Return accepted
    End Function

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
                {"From file (CSV)...", "From MySQL (approved classes)...", "Admin requests (approve / decline)..."},
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
            Dim lblSched As New Label() With {.Text = "Schedule (e.g. MWF 8:00-9:30 AM)", .ForeColor = grey, .Font = small, .AutoSize = True, .Location = New Point(24, 194)}
            Dim txtSched As New TextBox() With {.BorderStyle = BorderStyle.None, .Font = big, .Location = New Point(24, 216), .Width = 332, .MaxLength = 60}
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
            cboTerm.Items.AddRange({"1st Semester", "2nd Semester", "Midyear", "Summer"})
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

            dlg.ShowDialog(Me)
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
                .Text = "Type (pick one or type your own, e.g. Laboratory)",
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
                        lblError.Text = "Please enter a type (e.g. Quiz, Laboratory)."
                        cboType.Focus()
                    ElseIf Not System.Text.RegularExpressions.Regex.IsMatch(t, "^[A-Za-z0-9 ]+$") Then
                        lblError.Text = "Use letters, numbers and spaces only."
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

            dlg.ShowDialog(Me)
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

            If dlg.ShowDialog(Me) = DialogResult.OK Then
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
        If cboCourse.SelectedIndex = -1 Then Return ""
        Dim s As String = Convert.ToString(cboCourse.SelectedItem)
        Dim i As Integer = s.IndexOf(" - ")
        Return If(i > 0, s.Substring(0, i), s)
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
        If choice.StartsWith("Admin") Then Return ImportFromAdmin()

        Dim rows As New List(Of String())
        If choice.StartsWith("From file") Then
            Dim loaded As List(Of String()) = LoadCsvRows("code", "classcode")
            If loaded Is Nothing Then Return False
            rows = loaded
        Else
            Dim dt As DataTable = RunQuery("SELECT class_code, class_name, schedule, term, sy FROM classes WHERE status = 'approved'")
            If dt Is Nothing Then Return False
            For Each r As DataRow In dt.Rows
                rows.Add(New String() {Convert.ToString(r("class_code")), Convert.ToString(r("class_name")),
                                       Convert.ToString(r("schedule")), Convert.ToString(r("term")),
                                       Convert.ToString(r("sy"))})
            Next
        End If

        Dim added As Integer = AddClassRows(rows)
        If added = 0 Then
            MessageBox.Show("No new classes were found (empty, incomplete or already in the list).", "Digital Gradebook")
            Return False
        End If
        MessageBox.Show("Imported " & added & " of " & rows.Count & " classes.", "Digital Gradebook")
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
        If cboCourse.SelectedIndex = -1 Then
            MessageBox.Show("Please select a class first.", "Digital Gradebook")
            Return False
        End If

        Dim rows As New List(Of String())
        If choice.StartsWith("From file") Then
            Dim loaded As List(Of String()) = LoadCsvRows("type")
            If loaded Is Nothing Then Return False
            rows = loaded
        Else
            Dim dt As DataTable = RunQuery("SELECT type, group_name, max_score FROM assessments WHERE class_code = @c",
                                           "@c", SelectedClassCode())
            If dt Is Nothing Then Return False
            For Each r As DataRow In dt.Rows
                rows.Add(New String() {Convert.ToString(r("type")), Convert.ToString(r("group_name")),
                                       Convert.ToString(r("max_score"))})
            Next
        End If

        Dim added As Integer = 0
        For Each f As String() In rows
            If f.Length < 3 Then Continue For
            Dim t As String = CanonType(f(0))
            Dim m As Double
            If t = "" OrElse Not System.Text.RegularExpressions.Regex.IsMatch(t, "^[A-Za-z0-9 ]+$") Then Continue For
            If Not (TryNum(f(2), m) AndAlso m > 0) Then Continue For

            Dim grp As String = If(f(1).Trim().ToLower().StartsWith("q"), "Quiz", "CW")
            AddAssessment(t, grp, m)
            added += 1
        Next

        If added = 0 Then
            MessageBox.Show("No valid assessments were found.", "Digital Gradebook")
            Return False
        End If
        MessageBox.Show("Imported " & added & " assessments.", "Digital Gradebook")
        Return True
    End Function
End Class