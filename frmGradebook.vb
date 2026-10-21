Public Class frmGradebook

    Private Sub frmGradebook_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = Color.White
        SetupGrid()

        ' Test row (delete later when you load real data)
        dgvGrades.Rows.Add("25-06767", "Lalata, Carl Jade", 18, 18, 18, "", "", "", "", "", "")

        ' ADDED: placeholder courses
        cboCourse.DropDownStyle = ComboBoxStyle.DropDownList
        cboCourse.Items.AddRange({"IT101 - Intro to Programming",
                                  "IT102 - Data Structures",
                                  "IT103 - Database Systems"})
    End Sub

    Private Sub frmGradebook_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        FillEmptyRows()
        dgvGrades.ClearSelection()
    End Sub

    Private Sub SetupGrid()
        Dim green As Color = Color.FromArgb(0, 191, 99)

        With dgvGrades
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .AllowUserToResizeRows = False
            .AllowUserToResizeColumns = False
            .ReadOnly = True
            .RowHeadersVisible = False
            .BackgroundColor = Color.White
            .BorderStyle = BorderStyle.None
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

            ' Grid lines
            .CellBorderStyle = DataGridViewCellBorderStyle.Single
            .GridColor = green

            ' Header style
            .EnableHeadersVisualStyles = False
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 32
            .ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            With .ColumnHeadersDefaultCellStyle
                .BackColor = green
                .ForeColor = Color.Black
                .Font = New Font("Consolas", 8.0F, FontStyle.Bold)
                .Alignment = DataGridViewContentAlignment.MiddleCenter
                .WrapMode = DataGridViewTriState.False      ' no two-line headers
                .SelectionBackColor = green
                .SelectionForeColor = Color.Black
            End With

            ' Cell style (selection looks the same as normal, so no light-green row)
            .RowTemplate.Height = 32
            With .DefaultCellStyle
                .BackColor = Color.White
                .ForeColor = Color.Black
                .Alignment = DataGridViewContentAlignment.MiddleCenter
                .SelectionBackColor = Color.White
                .SelectionForeColor = Color.Black
            End With

            ' Columns
            .Columns.Clear()
            For Each name As String In {"Student No.", "Student Name", "SW1", "SW2",
                                        "Quiz1", "Quiz2", "Homework 1", "Homework 2",
                                        "Exam", "Attendance", "Average"}
                .Columns.Add(name.Replace(" ", "").Replace(".", ""), name)
            Next

            ' ADDED: Student Name grows to fit long names
            With .Columns("StudentName")
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                .MinimumWidth = 160
                .DefaultCellStyle.Padding = New Padding(10, 0, 10, 0)
            End With
        End With
    End Sub

    ' Adds blank bordered rows so the grid looks full even with no data
    Private Sub FillEmptyRows()
        Dim totalRows As Integer = (dgvGrades.Height - dgvGrades.ColumnHeadersHeight) \ dgvGrades.RowTemplate.Height
        Dim missing As Integer = totalRows - dgvGrades.Rows.Count
        For i As Integer = 1 To missing
            dgvGrades.Rows.Add()
        Next
    End Sub

    Private Sub dgvGrades_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGrades.CellContentClick

    End Sub

    Private Sub CustomButton1_Click(sender As Object, e As EventArgs) Handles CustomButton1.Click

    End Sub

    Private Sub btnAddClass_Click(sender As Object, e As EventArgs) Handles btnAddClass.Click

    End Sub

    ' ADDED: Add Student button (only the name goes into the grid)
    Private Sub btnAddStudent_Click(sender As Object, e As EventArgs) Handles btnAddStudent.Click
        Dim fullName As String = ""
        Dim studentNo As String = ""
        Dim course As String = ""

        If Not ShowStudentDialog(fullName, studentNo, course) Then Return

        ' Use the first blank row; add a new row if the grid is full
        Dim targetRow As Integer = -1
        For Each r As DataGridViewRow In dgvGrades.Rows
            If String.IsNullOrWhiteSpace(CStr(r.Cells("StudentName").Value)) Then
                targetRow = r.Index
                Exit For
            End If
        Next
        If targetRow = -1 Then targetRow = dgvGrades.Rows.Add()

        dgvGrades.Rows(targetRow).Cells("StudentName").Value = fullName
        dgvGrades.ClearSelection()

        ' TODO: save fullName, studentNo and course to your database
    End Sub

    ' ADDED: modern dark-green popup for adding a student
    Private Function ShowStudentDialog(ByRef fullName As String,
                                       ByRef studentNo As String,
                                       ByRef course As String) As Boolean
        Dim dark As Color = Color.FromArgb(0, 100, 0)
        Dim green As Color = Color.FromArgb(0, 191, 99)
        Dim accepted As Boolean = False

        Using dlg As New Form()
            dlg.FormBorderStyle = FormBorderStyle.None
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.Size = New Size(380, 360)
            dlg.BackColor = Color.White
            dlg.ShowInTaskbar = False

            ' Thin dark green outline around the whole popup
            AddHandler dlg.Paint,
                Sub(s, ev)
                    Using pen As New Pen(dark, 2)
                        ev.Graphics.DrawRectangle(pen, 1, 1, dlg.Width - 2, dlg.Height - 2)
                    End Using
                End Sub

            ' Header bar
            Dim header As New Panel() With {.Dock = DockStyle.Top, .Height = 48, .BackColor = dark}
            Dim lblTitle As New Label() With {
                .Text = "Add Student",
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 12.0F),
                .AutoSize = True,
                .Location = New Point(18, 12)
            }
            header.Controls.Add(lblTitle)

            ' Full Name
            Dim lblName As New Label() With {
                .Text = "Full Name",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 62)
            }
            Dim txtName As New TextBox() With {
                .BorderStyle = BorderStyle.None,
                .Font = New Font("Segoe UI", 12.0F),
                .Location = New Point(24, 84),
                .Width = 332
            }
            Dim lineName As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 112),
                .Size = New Size(332, 2)
            }

            ' Student Number
            Dim lblNo As New Label() With {
                .Text = "Student Number",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 128)
            }
            Dim txtNo As New TextBox() With {
                .BorderStyle = BorderStyle.None,
                .Font = New Font("Segoe UI", 12.0F),
                .Location = New Point(24, 150),
                .Width = 332
            }
            Dim lineNo As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 178),
                .Size = New Size(332, 2)
            }

            ' Course
            Dim lblCourse As New Label() With {
                .Text = "Course",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 194)
            }
            Dim cboDlgCourse As New ComboBox() With {
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = Color.White,
                .Font = New Font("Segoe UI", 11.0F),
                .Location = New Point(24, 216),
                .Width = 332
            }
            For Each item In cboCourse.Items
                cboDlgCourse.Items.Add(item)
            Next
            cboDlgCourse.SelectedIndex = cboCourse.SelectedIndex   ' pre-select the current course
            Dim lineCourse As New Panel() With {
                .BackColor = green,
                .Location = New Point(24, 246),
                .Size = New Size(332, 2)
            }

            ' Error message (hidden until needed)
            Dim lblError As New Label() With {
                .Text = "",
                .ForeColor = Color.Firebrick,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 258)
            }

            ' Buttons
            Dim btnOk As New Button() With {
                .Text = "Add",
                .FlatStyle = FlatStyle.Flat,
                .BackColor = dark,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 10.0F),
                .Size = New Size(100, 36),
                .Location = New Point(256, 300),
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
                .Location = New Point(146, 300),
                .Cursor = Cursors.Hand
            }
            btnCancel.FlatAppearance.BorderColor = dark
            btnCancel.FlatAppearance.BorderSize = 1

            ' Validate before closing
            AddHandler btnOk.Click,
                Sub(s, ev)
                    If txtName.Text.Trim() = "" Then
                        lblError.Text = "Please enter the full name."
                        txtName.Focus()
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

            dlg.Controls.AddRange({lblName, txtName, lineName,
                                   lblNo, txtNo, lineNo,
                                   lblCourse, cboDlgCourse, lineCourse,
                                   lblError, btnOk, btnCancel, header})
            dlg.AcceptButton = btnOk
            dlg.CancelButton = btnCancel
            dlg.ActiveControl = txtName

            dlg.ShowDialog(Me)

            If accepted Then
                fullName = txtName.Text.Trim()
                studentNo = txtNo.Text.Trim()
                course = CStr(cboDlgCourse.SelectedItem)
            End If
        End Using

        Return accepted
    End Function

    ' ADDED: Add Assessment button
    Private Sub btnAddAssessment_Click(sender As Object, e As EventArgs) Handles btnAddAssessment.Click
        If cboCourse.SelectedIndex = -1 Then
            MessageBox.Show("Please select a course first.", "Digital Gradebook")
            Return
        End If

        ' Uses the custom popup instead of InputBox
        Dim name As String = ShowAssessmentDialog()
        If name = "" Then Return

        Dim colKey As String = name.Replace(" ", "").Replace(".", "")

        If dgvGrades.Columns.Contains(colKey) Then
            MessageBox.Show("That assessment already exists.", "Digital Gradebook")
            Return
        End If

        Dim col As New DataGridViewTextBoxColumn()
        col.Name = colKey
        col.HeaderText = name
        col.MinimumWidth = 90

        ' Insert next to its group (Quiz 3 goes after Quiz 2)
        dgvGrades.Columns.Insert(GetInsertIndex(name), col)
    End Sub

    ' ADDED: modern dark-green popup for naming an assessment
    Private Function ShowAssessmentDialog() As String
        Dim dark As Color = Color.FromArgb(0, 100, 0)
        Dim green As Color = Color.FromArgb(0, 191, 99)

        Using dlg As New Form()
            dlg.FormBorderStyle = FormBorderStyle.None
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.Size = New Size(380, 220)
            dlg.BackColor = Color.White
            dlg.ShowInTaskbar = False

            ' Thin dark green outline around the whole popup
            AddHandler dlg.Paint,
                Sub(s, ev)
                    Using pen As New Pen(dark, 2)
                        ev.Graphics.DrawRectangle(pen, 1, 1, dlg.Width - 2, dlg.Height - 2)
                    End Using
                End Sub

            ' Header bar
            Dim header As New Panel() With {.Dock = DockStyle.Top, .Height = 48, .BackColor = dark}
            Dim lblTitle As New Label() With {
                .Text = "Add Assessment",
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 12.0F),
                .AutoSize = True,
                .Location = New Point(18, 12)
            }
            header.Controls.Add(lblTitle)

            ' Prompt text
            Dim lblPrompt As New Label() With {
                .Text = "Assessment name (e.g. Quiz 3)",
                .ForeColor = Color.Gray,
                .Font = New Font("Segoe UI", 9.0F),
                .AutoSize = True,
                .Location = New Point(24, 66)
            }

            ' Text box with a green underline
            Dim txt As New TextBox() With {
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

            ' Buttons
            Dim btnOk As New Button() With {
                .Text = "Add",
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

            If dlg.ShowDialog(Me) = DialogResult.OK Then
                Return txt.Text.Trim()
            End If
            Return ""
        End Using
    End Function

    ' ADDED: finds where a new assessment belongs
    Private Function GetInsertIndex(newName As String) As Integer
        Dim newGroup As String = GroupName(newName)
        Dim insertAt As Integer = dgvGrades.Columns("Average").Index   ' default: before Average

        For Each c As DataGridViewColumn In dgvGrades.Columns
            If c.Name = "StudentNo" OrElse c.Name = "StudentName" OrElse c.Name = "Average" Then Continue For
            If GroupName(c.HeaderText) = newGroup Then
                insertAt = c.Index + 1   ' keep pushing it after the last match
            End If
        Next

        Return insertAt
    End Function

    ' ADDED: "Quiz 3" -> "quiz", "Homework 2" -> "homework", "SW1" -> "sw"
    Private Function GroupName(text As String) As String
        Return System.Text.RegularExpressions.Regex.Replace(text, "[\d\s]+$", "").ToLower()
    End Function
End Class