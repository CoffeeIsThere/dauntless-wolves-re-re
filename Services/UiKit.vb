Imports System.Drawing
Imports System.Windows.Forms

' Small helpers so the dialogs that are built in code look like the rest of the app (dark green style).
Public Module UiKit

    Public ReadOnly DarkGreenColor As Color = Color.FromArgb(0, 100, 0)
    Public ReadOnly SoftGreenColor As Color = Color.FromArgb(238, 246, 241)

    ' Basic dialog look: fixed size, centered on its parent, green title bar
    Public Sub StyleDialog(f As Form, title As String, width As Integer, height As Integer)
        f.Text = title
        f.FormBorderStyle = FormBorderStyle.FixedDialog
        f.MaximizeBox = False
        f.MinimizeBox = False
        f.ShowInTaskbar = False
        f.StartPosition = FormStartPosition.CenterParent
        f.Font = New Font("Segoe UI", 9.5F)
        f.BackColor = Color.White
        f.ClientSize = New Size(width, height)
    End Sub

    Public Function MakeHeader(text As String) As Label
        Dim lbl As New Label()
        lbl.Text = text
        lbl.Dock = DockStyle.Top
        lbl.Height = 46
        lbl.BackColor = DarkGreenColor
        lbl.ForeColor = Color.White
        lbl.Font = New Font("Segoe UI Semibold", 12.0F)
        lbl.TextAlign = ContentAlignment.MiddleLeft
        lbl.Padding = New Padding(16, 0, 0, 0)
        Return lbl
    End Function

    Public Function MakePrimaryButton(text As String, Optional width As Integer = 110) As Button
        Dim b As New Button()
        b.Text = text
        b.Width = width
        b.Height = 34
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.BackColor = DarkGreenColor
        b.ForeColor = Color.White
        b.Font = New Font("Segoe UI Semibold", 9.5F)
        b.Cursor = Cursors.Hand
        Return b
    End Function

    Public Function MakeSecondaryButton(text As String, Optional width As Integer = 110) As Button
        Dim b As New Button()
        b.Text = text
        b.Width = width
        b.Height = 34
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderColor = DarkGreenColor
        b.BackColor = Color.White
        b.ForeColor = DarkGreenColor
        b.Font = New Font("Segoe UI Semibold", 9.5F)
        b.Cursor = Cursors.Hand
        Return b
    End Function

    ' Small popup that asks for a password (typed characters are hidden). Returns Nothing when cancelled.
    Public Function PromptPassword(owner As IWin32Window, title As String, prompt As String) As String
        Using dlg As New Form()
            StyleDialog(dlg, title, 400, 170)

            Dim lbl As New Label()
            lbl.Text = prompt
            lbl.AutoSize = False
            lbl.SetBounds(20, 18, 360, 40)

            Dim txt As New TextBox()
            txt.UseSystemPasswordChar = True
            txt.SetBounds(20, 62, 360, 26)

            Dim ok As Button = MakePrimaryButton("OK", 90)
            ok.Location = New Point(200, 108)
            ok.DialogResult = DialogResult.OK

            Dim cancel As Button = MakeSecondaryButton("Cancel", 90)
            cancel.Location = New Point(298, 108)
            cancel.DialogResult = DialogResult.Cancel

            dlg.Controls.AddRange(New Control() {lbl, txt, ok, cancel})
            dlg.AcceptButton = ok
            dlg.CancelButton = cancel

            If dlg.ShowDialog(owner) = DialogResult.OK Then Return txt.Text
            Return Nothing
        End Using
    End Function

End Module