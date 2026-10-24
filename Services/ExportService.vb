Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Windows.Forms

Public Class ExportService

    ''' <summary>
    ''' Exports a DataGridView or grade report using Microsoft Print to PDF if installed,
    ''' or gracefully falls back to the system's default printer or a file save fallback.
    ''' </summary>
    Public Function ExportToPDF(printDoc As PrintDocument, targetFilePath As String) As Boolean
        Try
            Dim targetPrinter As String = "Microsoft Print to PDF"
            Dim printerFound As Boolean = False

            ' 1. Check all installed printers on the local host machine
            For Each installedPrinter As String In PrinterSettings.InstalledPrinters
                If installedPrinter.Equals(targetPrinter, StringComparison.OrdinalIgnoreCase) Then
                    printDoc.PrinterSettings.PrinterName = installedPrinter
                    printerFound = True
                    Exit For
                End If
            Next

            ' 2. Fallback: If "Microsoft Print to PDF" is missing, assign system default printer
            If Not printerFound Then
                Dim defaultSettings As New PrinterSettings()
                If String.IsNullOrEmpty(defaultSettings.PrinterName) Then
                    Throw New InvalidPrinterException(New PrinterSettings())
                End If
                printDoc.PrinterSettings.PrinterName = defaultSettings.PrinterName
            End If

            ' 3. Configure print-to-file destination if path provided
            If Not String.IsNullOrEmpty(targetFilePath) Then
                printDoc.PrinterSettings.PrintToFile = True
                printDoc.PrinterSettings.PrintFileName = targetFilePath
            End If

            ' 4. Execute print job safely
            printDoc.Print()
            Return True

        Catch ex As InvalidPrinterException
            MessageBox.Show("No valid printer driver installed on this machine. Please install 'Microsoft Print to PDF' or connect a printer.",
                            "PDF Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False

        Catch ex As Exception
            MessageBox.Show("Failed to generate PDF: " & ex.Message,
                            "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' CSV Export Helper function for DataGridView tables
    ''' </summary>
    Public Function ExportToCSV(dgv As DataGridView, filePath As String) As Boolean
        Try
            Dim lines As New List(Of String)()

            ' Header row
            Dim headers As New List(Of String)()
            For Each col As DataGridViewColumn In dgv.Columns
                If col.Visible Then
                    headers.Add($"""{col.HeaderText.Replace("""", """")}""")
                End If
            Next
            lines.Add(String.Join(",", headers))

            ' Data rows
            For Each row As DataGridViewRow In dgv.Rows
                If Not row.IsNewRow Then
                    Dim cells As New List(Of String)()
                    For Each col As DataGridViewColumn In dgv.Columns
                        If col.Visible Then
                            Dim val As String = If(row.Cells(col.Index).Value IsNot Nothing, row.Cells(col.Index).Value.ToString(), "")
                            cells.Add($"""{val.Replace("""", """")}""")
                        End If
                    Next
                    lines.Add(String.Join(",", cells))
                End If
            Next

            File.WriteAllLines(filePath, lines, System.Text.Encoding.UTF8)
            Return True

        Catch ex As Exception
            MessageBox.Show("Error saving CSV file: " & ex.Message, "CSV Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

End Class