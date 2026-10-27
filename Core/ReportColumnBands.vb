' Horizontal report pages repeat identity columns and keep every data column readable.
Public Class ReportColumnBands
    Public Shared Function Build(widths As IReadOnlyList(Of Single), identityColumns As IEnumerable(Of Integer), pageWidth As Single) As List(Of Integer())
        If pageWidth <= 0 Then Throw New ArgumentException("Printable width must be positive.")
        If widths.Any(Function(width) width <= 0 OrElse width > pageWidth) Then Throw New ArgumentException("A report column does not fit the printable width.")
        Dim repeated = identityColumns.Distinct().Where(Function(index) index >= 0 AndAlso index < widths.Count).OrderBy(Function(index) index).ToList()
        Dim pending = Enumerable.Range(0, widths.Count).Where(Function(index) Not repeated.Contains(index)).ToList()
        Dim result As New List(Of Integer())
        Dim identityWidth = repeated.Sum(Function(index) widths(index))
        If identityWidth > pageWidth Then Throw New ArgumentException("Identity columns do not fit the printable width.")
        While pending.Count > 0
            Dim band As New List(Of Integer)(repeated)
            Dim used = identityWidth
            While pending.Count > 0 AndAlso used + widths(pending(0)) <= pageWidth
                Dim index = pending(0)
                pending.RemoveAt(0) : band.Add(index) : used += widths(index)
            End While
            If band.Count = repeated.Count Then Throw New ArgumentException("No data column fits beside the identity columns.")
            result.Add(band.OrderBy(Function(index) index).ToArray())
        End While
        If result.Count = 0 AndAlso repeated.Count > 0 Then result.Add(repeated.ToArray())
        Return result
    End Function
End Class
