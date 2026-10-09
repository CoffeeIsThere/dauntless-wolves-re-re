Public Class frmGradebook

    Private Sub frmGradebook_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = Color.White
        SetupGrid()

        ' Test row (delete later when you load real data)
        dgvGrades.Rows.Add("25-06767", "Lalata, Carl Jade", 18, 18, 18, "", "", "", "", "", "")
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

End Class