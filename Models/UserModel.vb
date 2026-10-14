Namespace Models
    Public Class UserModel
        Public Property UserId As String
        Public Property FullName As String
        Public Property Email As String
        Public Property Role As String
        Public Property DepartmentOrProgram As String


        Public Sub New()
        End Sub


        Public Sub New(id As String, fullName As String, email As String, role As String, Optional deptOrProg As String = "")
            Me.UserId = id
            Me.FullName = fullName
            Me.Email = email
            Me.Role = role
            Me.DepartmentOrProgram = deptOrProg
        End Sub
    End Class
End Namespace