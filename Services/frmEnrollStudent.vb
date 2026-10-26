Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

' Enroll students into a class. Built in code, so there is no designer file to add.
'   Admin:   Using dlg As New frmEnrollStudent()                      ' pick any class
'   Teacher: Using dlg As New frmEnrollStudent(classId)               ' fixed to one of the teacher's classes
Public Class frmEnrollStudent
    Inherits Form

    Private ReadOnly _classService As New ClassService()
    Private ReadOnly _fixedClassId As Integer

    Private ReadOnly cboClass As New ComboBox()
    Private ReadOnly txtSearch As New TextBox()
    Private ReadOnly lstStudents As New ListBox()
    Private ReadOnly lblInfo As New Label()
    Private ReadOnly btnEnroll As Button = UiKit.MakePrimaryButton("Enroll selected", 140)
    Private ReadOnly btnClose As Button = UiKit.MakeSecondaryButton("Close")
    Private ReadOnly _searchTimer As New System.Windows.Forms.Timer() With {.Interval = 300}

    ' How many students were enrolled while the dialog was open (callers refresh when this is above 0)
    Public Property EnrolledCount As Integer

    Public Sub New(Optional fixedClassId As Integer = 0)
        _fixedClassId = fixedClassId
        UiKit.StyleDialog(Me, "Enroll students", 560, 560)

        Dim header As Label = UiKit.MakeHeader("Enroll students into a class")

        Dim lblClass As New Label() With {.Text = "Class", .AutoSize = True, .Location = New Point(22, 66)}
        cboClass.DropDownStyle = ComboBoxStyle.DropDownList
        cboClass.SetBounds(22, 88, 516, 28)

        Dim lblSearch As New Label() With {.Text = "Search active students not yet in this class (name, ID or email)", .AutoSize = True, .Location = New Point(22, 128)}
        txtSearch.SetBounds(22, 150, 516, 28)

        lstStudents.SelectionMode = SelectionMode.MultiExtended
        lstStudents.SetBounds(22, 188, 516, 250)

        lblInfo.SetBounds(22, 444, 516, 22)
        lblInfo.ForeColor = UiKit.DarkGreenColor

        btnEnroll.Location = New Point(22, 490)
        btnClose.Location = New Point(428, 490)
        btnClose.DialogResult = DialogResult.Cancel

        Me.Controls.AddRange(New Control() {lblClass, cboClass, lblSearch, txtSearch, lstStudents, lblInfo, btnEnroll, btnClose, header})
        Me.CancelButton = btnClose

        AddHandler cboClass.SelectedIndexChanged, AddressOf OnClassChanged
        AddHandler txtSearch.TextChanged, AddressOf OnSearchChanged
        AddHandler _searchTimer.Tick, AddressOf OnSearchTimer
        AddHandler btnEnroll.Click, AddressOf btnEnroll_Click
        AddHandler Me.Load, AddressOf OnLoadForm
    End Sub

    Private Sub OnLoadForm(sender As Object, e As EventArgs)
        Dim classes As DataTable
        If AppSession.IsAdmin Then
            classes = _classService.GetAllClasses()
        Else
            classes = _classService.GetClassesByTeacher(AppSession.CurrentUser.UserId)
        End If

        cboClass.DataSource = classes
        cboClass.DisplayMember = "class_label"
        cboClass.ValueMember = "class_id"

        If _fixedClassId > 0 Then
            ' Find the class by hand: class_id comes from the database as an unsigned number
            For i As Integer = 0 To cboClass.Items.Count - 1
                Dim drv As DataRowView = TryCast(cboClass.Items(i), DataRowView)
                If drv IsNot Nothing AndAlso Convert.ToInt32(drv("class_id")) = _fixedClassId Then
                    cboClass.SelectedIndex = i
                    Exit For
                End If
            Next
            cboClass.Enabled = False
        End If

        If classes.Rows.Count = 0 Then
            lblInfo.Text = "There are no classes yet."
            btnEnroll.Enabled = False
        End If
        LoadStudents()
    End Sub

    Private Function SelectedClassId() As Integer
        Dim drv As DataRowView = TryCast(cboClass.SelectedItem, DataRowView)
        If drv Is Nothing Then Return 0
        Return Convert.ToInt32(drv("class_id"))
    End Function

    Private Sub OnClassChanged(sender As Object, e As EventArgs)
        LoadStudents()
    End Sub

    Private Sub OnSearchChanged(sender As Object, e As EventArgs)
        _searchTimer.Stop()
        _searchTimer.Start()
    End Sub

    Private Sub OnSearchTimer(sender As Object, e As EventArgs)
        _searchTimer.Stop()
        LoadStudents()
    End Sub

    Private Sub LoadStudents()
        Dim classId As Integer = SelectedClassId()
        If classId = 0 Then
            lstStudents.DataSource = Nothing
            Return
        End If

        Dim dt As DataTable = _classService.GetEnrollableStudents(classId, txtSearch.Text)
        lstStudents.DataSource = dt
        lstStudents.DisplayMember = "display_name"
        lstStudents.ValueMember = "student_id"
        lstStudents.ClearSelected()
        lblInfo.Text = dt.Rows.Count.ToString() & " student(s) can be enrolled."
    End Sub

    Private Sub btnEnroll_Click(sender As Object, e As EventArgs)
        Dim classId As Integer = SelectedClassId()
        If classId = 0 Then Return

        If lstStudents.SelectedItems.Count = 0 Then
            MessageBox.Show("Select one or more students first (hold Ctrl to pick several).", "Enroll students",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim role As String = AppSession.CurrentUser.Role
        Dim actorId As String = AppSession.CurrentUser.UserId
        Dim ok As Integer = 0
        Dim problems As New List(Of String)()

        For Each item As Object In lstStudents.SelectedItems
            Dim drv As DataRowView = DirectCast(item, DataRowView)
            Dim sid As String = drv("student_id").ToString()
            Dim res As OperationResult = _classService.EnrollStudent(sid, classId, role, actorId)
            If res.Success Then
                ok += 1
            Else
                problems.Add(sid & ": " & res.Message)
            End If
        Next

        EnrolledCount += ok
        Dim msg As String = ok.ToString() & " student(s) enrolled."
        If problems.Count > 0 Then msg &= vbCrLf & vbCrLf & String.Join(vbCrLf, problems)
        MessageBox.Show(msg, "Enroll students", MessageBoxButtons.OK, If(problems.Count = 0, MessageBoxIcon.Information, MessageBoxIcon.Warning))

        LoadStudents()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then _searchTimer.Dispose()
        MyBase.Dispose(disposing)
    End Sub

End Class