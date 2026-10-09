Imports System.Data
Imports System.IO
Imports System.Text
Public Class ExportService

    Public Shared Function ExportToCSV(dt As DataTable, filepath As String) As Boolean
        Try
            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                Console.WriteLine("[EXOPORT WARNING] No Data is Available to Export.")
                Return False
            End If

            Using writer As New StreamWriter(filepath, False, Encoding.UTF8)
                Dim columnHeaders As New List(Of String)()
                For Each col As DataColumn In dt.Columns
                    columnHeaders.Add($"""{col.ColumnName.Replace("""", """""")}""")
                Next
                writer.WriteLine(String.Join(",", columnHeaders))

                For Each row As DataRow In dt.Rows
                    Dim rowFields As New List(Of String)()
                    For Each col As DataColumn In dt.Columns
                        Dim cellValue As String = row(col).ToString().Replace("""", """""")
                        rowFields.Add($"""{cellValue}""")
                    Next
                    writer.WriteLine(String.Join(",", rowFields))
                Next
            End Using
            Return True
        Catch ex As Exception
            Console.WriteLine("[EXPORT ERROR] ", ex.Message)
            Return False
        End Try
    End Function

End Class
