Public Class OperationResult
    ' What the service methods return, so the UI can show a real message
    Public Property Success As Boolean
    Public Property Message As String

    Public Shared Function Ok(Optional message As String = "") As OperationResult
        Return New OperationResult With {.Success = True, .Message = message}
    End Function

    Public Shared Function Fail(message As String) As OperationResult
        Return New OperationResult With {.Success = False, .Message = message}
    End Function
End Class