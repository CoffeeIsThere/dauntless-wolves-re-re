Imports DAUNTLESS_WOLVES_PROF.Models ' <-- Replace "WinFormsApp2" with your actual root project name if different

Namespace Core
    Public Module AppSession

        Public Property CurrentUser As UserModel

        Public ReadOnly Property IsLoggedIn As Boolean
            Get
                Return CurrentUser IsNot Nothing
            End Get
        End Property

        Public ReadOnly Property IsAdmin As Boolean
            Get
                Return IsLoggedIn AndAlso CurrentUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

        Public ReadOnly Property IsTeacher As Boolean
            Get
                Return IsLoggedIn AndAlso CurrentUser.Role.Equals("Teacher", StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

        Public ReadOnly Property IsStudent As Boolean
            Get
                Return IsLoggedIn AndAlso CurrentUser.Role.Equals("Student", StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

        Public Sub StartSession(user As UserModel)
            CurrentUser = user
        End Sub

        Public Sub ClearSession()
            CurrentUser = Nothing
        End Sub

    End Module
End Namespace