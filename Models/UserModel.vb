Namespace Models
    Public Class UserModel
        Public Property UserId As Integer
        Public Property Username As String
        Public Property FullName As String
        Public Property Email As String
        Public Property Role As String          ' "Admin", "Teacher", or "Student"
        Public Property StudentNumber As String ' Null or empty if not a student

        ' Default empty constructor
        Public Sub New()
        End Sub

        ' Parameterized constructor for quick creation
        Public Sub New(id As Integer, username As String, fullName As String, role As String)
            Me.UserId = id
            Me.Username = username
            Me.FullName = fullName
            Me.Role = role
        End Sub
    End Class
End Namespace