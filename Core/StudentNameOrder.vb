Imports System.Data
Imports System.Globalization
Imports System.Text.RegularExpressions

' The database currently stores full_name rather than a separate surname.
Public Class StudentNameOrder
    Private Shared ReadOnly Suffixes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {"Jr", "Sr", "II", "III", "IV", "V"}
    Private Shared ReadOnly Particles As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {"de", "del", "dela", "la", "las", "los", "van", "von", "san", "santa", "da", "dos", "do", "di"}

    Private Shared Function Parts(name As String) As (Surname As String, Given As String)
        Dim normalized = Regex.Replace(If(name, "").Trim(), "\s+", " ")
        Dim comma = normalized.IndexOf(","c)
        If comma >= 0 AndAlso Not Suffixes.Contains(normalized.Substring(comma + 1).Trim().TrimEnd("."c)) Then
            Return (normalized.Substring(0, comma).Trim(), normalized.Substring(comma + 1).Trim())
        End If
        Dim words = normalized.Replace(",", " ").Split(" "c, StringSplitOptions.RemoveEmptyEntries).ToList()
        While words.Count > 1 AndAlso Suffixes.Contains(words.Last().TrimEnd("."c))
            words.RemoveAt(words.Count - 1)
        End While
        If words.Count = 0 Then Return ("", "")
        Dim start = words.Count - 1
        While start > 0 AndAlso Particles.Contains(words(start - 1))
            start -= 1
        End While
        Return (String.Join(" ", words.Skip(start)), String.Join(" ", words.Take(start)))
    End Function

    Public Shared Function Surname(name As String) As String
        Return Parts(name).Surname
    End Function

    Public Shared Function FormatForGradebook(name As String) As String
        Dim parsed = Parts(name)
        Dim surname = parsed.Surname
        Dim given = parsed.Given
        Dim words = Regex.Replace(If(name, ""), "[,\s]+", " ").Trim().Split(" "c, StringSplitOptions.RemoveEmptyEntries)
        If words.Length > 1 AndAlso Suffixes.Contains(words.Last().TrimEnd("."c)) Then
            Dim suffix = words.Last()
            If given.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) Then given = given.Substring(0, given.Length - suffix.Length).Trim()
            If Not surname.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) Then surname &= " " & suffix
        End If
        ' Preserve complete given names: the schema cannot distinguish a second given
        ' name from a middle name. Only normalize initials explicitly stored as letters.
        given = String.Join(" ", given.Split(" "c, StringSplitOptions.RemoveEmptyEntries).Select(Function(word) If(word.Length = 1 AndAlso Char.IsLetter(word(0)), word & ".", word)))
        Return surname & If(given = "", "", ", " & given)
    End Function

    Public Shared Function CompareNames(left As String, right As String) As Integer
        Dim first = Parts(left)
        Dim second = Parts(right)
        Dim options = CompareOptions.IgnoreCase Or CompareOptions.IgnoreNonSpace
        Dim result = CultureInfo.InvariantCulture.CompareInfo.Compare(Regex.Replace(first.Surname, "\s+(Jr\.?|Sr\.?|II|III|IV|V)$", "", RegexOptions.IgnoreCase), Regex.Replace(second.Surname, "\s+(Jr\.?|Sr\.?|II|III|IV|V)$", "", RegexOptions.IgnoreCase), options)
        If result <> 0 Then Return result
        Return CultureInfo.InvariantCulture.CompareInfo.Compare(first.Given, second.Given, options)
    End Function

    Public Shared Function Sort(table As DataTable, nameColumn As String, idColumn As String) As DataTable
        Dim rows = table.AsEnumerable().ToList()
        rows.Sort(Function(left, right)
                      Dim result = CompareNames(Convert.ToString(left(nameColumn)), Convert.ToString(right(nameColumn)))
                      If result <> 0 Then Return result
                      Return StringComparer.OrdinalIgnoreCase.Compare(Convert.ToString(left(idColumn)), Convert.ToString(right(idColumn)))
                  End Function)
        Dim sorted = table.Clone()
        For Each row In rows
            sorted.ImportRow(row)
        Next
        Return sorted
    End Function
End Class
