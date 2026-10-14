Imports System.Data
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Text

Public Class ExportService


    Public Shared Function ExportToCSV(dt As DataTable, filePath As String) As Boolean
        Try
            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                Console.WriteLine("[EXPORT WARNING] No data available to export.")
                Return False
            End If

            Using writer As New StreamWriter(filePath, False, Encoding.UTF8)
                ' Header Row
                Dim columnHeaders As New List(Of String)()
                For Each col As DataColumn In dt.Columns
                    columnHeaders.Add($"""{col.ColumnName.Replace("""", """""")}""")
                Next
                writer.WriteLine(String.Join(",", columnHeaders))

                ' Data Rows
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
            Console.WriteLine("[CSV EXPORT ERROR] " & ex.Message)
            Return False
        End Try
    End Function


    Public Shared Function ExportToPDF(dt As DataTable, documentTitle As String, filePath As String) As Boolean
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then Return False

        Try
            Dim printDoc As New PrintDocument()
            printDoc.PrinterSettings.PrinterName = "Microsoft Print to PDF"
            printDoc.PrinterSettings.PrintToFile = True
            printDoc.PrinterSettings.PrintFileName = filePath

            Dim rowIndex As Integer = 0

            AddHandler printDoc.PrintPage, Sub(sender As Object, e As PrintPageEventArgs)
                                               Dim fontHeader As New Font("Arial", 14, FontStyle.Bold)
                                               Dim fontColumn As New Font("Arial", 10, FontStyle.Bold)
                                               Dim fontBody As New Font("Arial", 9, FontStyle.Regular)

                                               Dim x As Single = 40
                                               Dim y As Single = 40
                                               Dim rowHeight As Single = 25

                                               ' Title
                                               e.Graphics.DrawString(documentTitle, fontHeader, Brushes.Black, x, y)
                                               y += 40

                                               ' Table Headers
                                               Dim colWidth As Single = (e.MarginBounds.Width - 40) / dt.Columns.Count
                                               For c As Integer = 0 To dt.Columns.Count - 1
                                                   e.Graphics.DrawString(dt.Columns(c).ColumnName, fontColumn, Brushes.DarkBlue, x + (c * colWidth), y)
                                               Next
                                               y += rowHeight
                                               e.Graphics.DrawLine(Pens.Black, x, y, x + (dt.Columns.Count * colWidth), y)
                                               y += 5

                                               ' Data Rows
                                               While rowIndex < dt.Rows.Count
                                                   For c As Integer = 0 To dt.Columns.Count - 1
                                                       Dim cellVal As String = dt.Rows(rowIndex)(c).ToString()
                                                       e.Graphics.DrawString(cellVal, fontBody, Brushes.Black, x + (c * colWidth), y)
                                                   Next
                                                   y += rowHeight
                                                   rowIndex += 1

                                                   ' Check for Page Overflow
                                                   If y + rowHeight > e.MarginBounds.Bottom Then
                                                       e.HasMorePages = True
                                                       Return
                                                   End If
                                               End While

                                               e.HasMorePages = False
                                           End Sub

            printDoc.Print()
            Return True
        Catch ex As Exception
            Console.WriteLine("[PDF EXPORT ERROR] " & ex.Message)
            Return False
        End Try
    End Function

End Class