Public Class RevisionRequest
    Public Property RequestId As Integer
    Public Property GradeId As Integer
    Public Property Reason As String
    Public Property Status As String
    Public Property RequestedAt As DateTime
    Public Property ReviewedAt As Nullable(Of DateTime)
    Public Property ReviewedBy As Nullable(Of Integer)

End Class
