Imports MySql.Data.MySqlClient
Imports System.Data

Public Class DatabaseHelper
    ' Update these 4 lines with your Database Specialist's Radmin IP and Credentials
    Private Shared ReadOnly RadminIP As String = "127.0.0.1"
    Private Shared ReadOnly DBName As String = "grading_system"
    Private Shared ReadOnly DBUser As String = "root"
    Private Shared ReadOnly DBPass As String = ""

    Private Shared ReadOnly ConnString As String =
        $"Server={RadminIP};Port=3306;Database={DBName};Uid={DBUser};Pwd={DBPass};SslMode=Preferred;"

    Public Shared Function GetConnection() As MySqlConnection
        Dim conn As New MySqlConnection(ConnString)
        conn.Open()
        Return conn
    End Function

    Public Shared Function ExecuteQuery(sql As String, Optional params As MySqlParameter() = Nothing) As DataTable
        Dim dt As New DataTable()
        Using conn As MySqlConnection = GetConnection()
            Using cmd As New MySqlCommand(sql, conn)
                If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                Using adapter As New MySqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
            End Using
        End Using
        Return dt
    End Function

    Public Shared Function ExecuteNonQuery(sql As String, Optional params As MySqlParameter() = Nothing) As Integer
        Using conn As MySqlConnection = GetConnection()
            Using cmd As New MySqlCommand(sql, conn)
                If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                Return cmd.ExecuteNonQuery()
            End Using
        End Using
    End Function
End Class
