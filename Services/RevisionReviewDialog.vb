Imports System.Globalization

' The dashboard and gradebook share the same decision workflow.
Public Class RevisionReviewDialog
    Public Shared Function Review(owner As Form, requestId As Integer, teacherId As String) As Boolean
        Dim service As New RevisionRequestService()
        Dim requests As DataTable
        Try
            requests = service.GetRequestsByTeacher(teacherId, "All")
        Catch ex As Exception
            MessageBox.Show(owner, "Could not load the request: " & ex.Message)
            Return False
        End Try
        Dim matches = requests.Select("request_id=" & requestId)
        If matches.Length = 0 Then
            MessageBox.Show(owner, "Request not found or no longer available.")
            Return False
        End If
        Dim request = matches(0)
        If Not {"Pending", "Under Review"}.Contains(request("status").ToString()) Then
            MessageBox.Show(owner, "This request has already been " & request("status").ToString().ToLowerInvariant() & ".")
            Return False
        End If
        Using dialog As New Form With {.Text = "Review clarification", .ClientSize = New Size(520, 570), .StartPosition = FormStartPosition.CenterParent, .FormBorderStyle = FormBorderStyle.FixedDialog, .MaximizeBox = False, .MinimizeBox = False, .ShowInTaskbar = False, .BackColor = Color.White, .Font = New Font("Segoe UI", 10), .AutoScaleMode = AutoScaleMode.Font}
            Dim layout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(20), .ColumnCount = 1, .RowCount = 11}
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            For Each rowHeight As Integer In {34, 68, 24, 76, 32, 38, 40, 30, 24, 110, 48}
                layout.RowStyles.Add(New RowStyle(SizeType.Absolute, rowHeight))
            Next
            Dim title As New Label With {.Text = "Review grade clarification", .Dock = DockStyle.Fill, .ForeColor = Color.DarkGreen, .Font = New Font("Segoe UI", 13, FontStyle.Bold)}
            Dim currentScore = If(IsDBNull(request("current_score")), "Not graded", Convert.ToString(request("current_score")))
            Dim requested = If(IsDBNull(request("requested_points")), "Re-check", "+" & Convert.ToString(request("requested_points")) & " points")
            Dim details As New Label With {.Dock = DockStyle.Fill, .Text = request("student_name").ToString() & " · " & request("assessment_title").ToString() & vbCrLf & "Current: " & currentScore & "/" & request("total_score").ToString() & "   Requested: " & requested & vbCrLf & "Status: " & request("status").ToString()}
            Dim reasonLabel As New Label With {.Text = "Student's reason", .Dock = DockStyle.Fill, .ForeColor = Color.DarkGreen}
            Dim reason As New TextBox With {.Text = request("reason").ToString(), .Dock = DockStyle.Fill, .Multiline = True, .ReadOnly = True, .ScrollBars = ScrollBars.Vertical, .BackColor = Color.FromArgb(240, 248, 240)}
            Dim actionLabel As New Label With {.Text = "Action", .Dock = DockStyle.Fill, .ForeColor = Color.DarkGreen}
            Dim actions As New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDownList}
            actions.Items.AddRange({"Approve", "Mark Under Review", "Reject / Close"})
            Dim hint As New Label With {.Dock = DockStyle.Fill}
            Dim score As New TextBox With {.Dock = DockStyle.Fill}
            Dim responseLabel As New Label With {.Text = "Response to student (optional)", .Dock = DockStyle.Fill, .ForeColor = Color.DarkGreen}
            Dim response As New TextBox With {.Dock = DockStyle.Fill, .Multiline = True, .MaxLength = 350, .ScrollBars = ScrollBars.Vertical}
            Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
            Dim apply As New Button With {.Text = "Approve", .AutoSize = True, .Padding = New Padding(12, 4, 12, 4), .BackColor = Color.DarkGreen, .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat}
            Dim cancel As New Button With {.Text = "Cancel", .AutoSize = True, .Padding = New Padding(12, 4, 12, 4), .DialogResult = DialogResult.Cancel}
            buttons.Controls.AddRange({apply, cancel})
            AddHandler actions.SelectedIndexChanged, Sub(sender, args)
                                                         score.Enabled = actions.SelectedIndex = 0
                                                         response.Enabled = actions.SelectedIndex <> 1
                                                         hint.Text = If(actions.SelectedIndex = 0, "Corrected score (required for re-checks; otherwise optional)", If(actions.SelectedIndex = 1, "Keep this request open while you investigate.", "Explain your decision in the response below."))
                                                         apply.Text = If(actions.SelectedIndex = 0, "Approve", If(actions.SelectedIndex = 1, "Mark Under Review", "Close request"))
                                                     End Sub
            Dim saved = False
            AddHandler apply.Click, Sub(sender, args)
                                        Dim result As OperationResult
                                        If actions.SelectedIndex = 0 Then
                                            Dim corrected As Double? = Nothing
                                            Dim number As Double
                                            If score.Text.Trim() <> "" Then
                                                If Not Double.TryParse(score.Text, NumberStyles.Float, CultureInfo.CurrentCulture, number) OrElse Not Double.IsFinite(number) OrElse number < 0 OrElse number > CDbl(request("total_score")) Then
                                                    MessageBox.Show(dialog, "Enter a score between 0 and " & request("total_score").ToString() & ".")
                                                    score.Focus()
                                                    Return
                                                End If
                                                corrected = number
                                            End If
                                            result = service.ApproveAndApplyRevision(requestId, teacherId, corrected, response.Text.Trim())
                                        ElseIf actions.SelectedIndex = 1 Then
                                            result = service.ReviewRequestResult(requestId, "Under Review", teacherId)
                                        Else
                                            result = service.RejectRequestResult(requestId, teacherId, response.Text.Trim())
                                        End If
                                        If Not result.Success Then
                                            MessageBox.Show(dialog, If(result.Message = "NEEDSCORE", "Enter the corrected score for this re-check.", result.Message), "Request not updated")
                                            Return
                                        End If
                                        saved = True
                                        dialog.DialogResult = DialogResult.OK
                                    End Sub
            actions.SelectedIndex = 0
            Dim controls As Control() = {title, details, reasonLabel, reason, actionLabel, actions, hint, score, responseLabel, response, buttons}
            For i = 0 To controls.Length - 1
                layout.Controls.Add(controls(i), 0, i)
            Next
            dialog.Controls.Add(layout)
            dialog.AcceptButton = apply
            dialog.CancelButton = cancel
            dialog.ShowDialog(owner)
            Return saved
        End Using
    End Function
End Class
