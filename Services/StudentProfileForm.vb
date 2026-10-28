Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class StudentProfileForm
    Inherits Form

    Private ReadOnly values As New Dictionary(Of String, Label)
    Private ReadOnly enrolled As New Label()
    Private ReadOnly requests As New Label()
    Private ReadOnly grid As New DataGridView()
    Private ReadOnly empty As New Label With {.Text = "No enrolled classes yet.", .Dock = DockStyle.Top, .Height = 30, .ForeColor = Color.DimGray, .Visible = False}

    Public Sub New()
        UiKit.StyleDialog(Me, "My Profile", 800, 650)
        FormBorderStyle = FormBorderStyle.Sizable
        MinimumSize = New Size(680, 620)
        MaximizeBox = True
        Dim body As New TableLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(20, 12, 20, 12), .ColumnCount = 1, .RowCount = 5}
        body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        body.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        body.RowStyles.Add(New RowStyle(SizeType.Absolute, 76))
        body.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        body.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        body.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Dim details As New TableLayoutPanel With {.Dock = DockStyle.Top, .AutoSize = True, .ColumnCount = 2}
        details.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130))
        details.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        For Each field In {("full_name", "Full name"), ("student_id", "Student ID"), ("email", "Email"), ("program", "Program"), ("year_level", "Year level"), ("section", "Section"), ("status", "Account status")}
            Dim row = details.RowCount
            details.RowCount += 1
            details.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            details.Controls.Add(New Label With {.Text = field.Item2, .AutoSize = True, .Margin = New Padding(0, 4, 8, 4), .ForeColor = Color.DimGray}, 0, row)
            Dim value As New Label With {.Text = "—", .AutoSize = True, .Dock = DockStyle.Fill, .Margin = New Padding(0, 4, 0, 4), .ForeColor = Color.DarkGreen}
            details.Controls.Add(value, 1, row)
            values.Add(field.Item1, value)
        Next
        body.Controls.Add(details, 0, 0)
        Dim cards As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2}
        cards.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        cards.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        For Each card In {enrolled, requests}
            card.Dock = DockStyle.Fill
            card.TextAlign = ContentAlignment.MiddleCenter
            card.BackColor = UiKit.SoftGreenColor
            card.ForeColor = Color.DarkGreen
            card.Font = New Font("Segoe UI", 11, FontStyle.Bold)
            card.Margin = New Padding(0, 8, 10, 8)
            cards.Controls.Add(card)
        Next
        enrolled.Text = "—" & vbCrLf & "Enrolled Classes"
        requests.Text = "—" & vbCrLf & "Open Requests"
        body.Controls.Add(cards, 0, 1)
        body.Controls.Add(New Label With {.Text = "Class Performance · Current running grades", .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft, .ForeColor = Color.DarkGreen}, 0, 2)
        grid.Dock = DockStyle.Fill
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.RowHeadersVisible = False
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True
        grid.DefaultCellStyle.NullValue = "—"
        grid.DefaultCellStyle.SelectionBackColor = UiKit.SoftGreenColor
        grid.DefaultCellStyle.SelectionForeColor = Color.DarkGreen
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGreen
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersHeight = 42
        Dim tableHost As New Panel With {.Dock = DockStyle.Fill}
        tableHost.Controls.Add(grid)
        tableHost.Controls.Add(empty)
        body.Controls.Add(tableHost, 0, 3)
        body.Controls.Add(New Label With {.Text = "Open requests include Pending and Under Review. Ungraded work is excluded from running grades.", .AutoSize = True, .Dock = DockStyle.Fill, .ForeColor = Color.DimGray, .Margin = New Padding(0, 10, 0, 0)}, 0, 4)
        Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 54, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(12, 6, 12, 6)}
        Dim close = UiKit.MakeSecondaryButton("Close")
        close.DialogResult = DialogResult.Cancel
        Dim refresh = UiKit.MakePrimaryButton("Refresh")
        AddHandler refresh.Click, Sub() LoadProfile()
        footer.Controls.Add(close)
        footer.Controls.Add(refresh)
        CancelButton = close
        Controls.Add(body)
        Controls.Add(footer)
        Controls.Add(UiKit.MakeHeader("My Profile"))
        AddHandler Shown, Sub() LoadProfile()
    End Sub

    Private Sub LoadProfile()
        UiKit.TryLoadData(Me, "your profile", Sub()
            Dim snapshot = New StudentProfileService().LoadCurrent()
            For Each item In values
                Dim value = Convert.ToString(snapshot.Profile(item.Key))
                item.Value.Text = If(String.IsNullOrWhiteSpace(value), "Not recorded", value)
            Next
            enrolled.Text = snapshot.Classes.Rows.Count.ToString() & vbCrLf & "Enrolled Classes"
            requests.Text = snapshot.OpenRequests.ToString() & vbCrLf & "Open Requests"
            grid.DataSource = snapshot.Classes
            grid.Columns("Class").FillWeight = 65
            grid.Columns("Running Grade (%)").FillWeight = 19
            grid.Columns("Running Grade (%)").DefaultCellStyle.Format = "0.00"
            grid.Columns("Standing").FillWeight = 16
            For Each row As DataGridViewRow In grid.Rows
                Dim standing = Convert.ToString(row.Cells("Standing").Value)
                row.Cells("Standing").Style.ForeColor = If(standing = "At risk", Color.Firebrick, If(standing = "Passed", Color.DarkGreen, Color.DimGray))
            Next
            empty.Visible = snapshot.Classes.Rows.Count = 0
        End Sub)
    End Sub
End Class
