Public Class AuditLog

    Public Property LogId As Long
    Public Property StudentId As String
    Public Property TeacherId As String
    Public Property AdminId As Nullable(Of Integer)
    Public Property Action As String
    Public Property EntityType As String
    Public Property EntityId As String
    Public Property OldValue As String
    Public Property NewValue As String
    Public Property Timestamp As DateTime
    Public Property Details As String


End Class
