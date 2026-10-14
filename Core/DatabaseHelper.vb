Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.IO

Public Class DatabaseHelper
        Private Shared ConnectionString As String = ""

    Shared Sub New()
        InitializeConnectionString()
    End Sub

    Private Shared Sub InitializeConnectionString()
            Dim server As String = "127.0.0.1"
            Dim port As String = "3306"
        Dim dbName As String = "grading_system_test"
        Dim uid As String = "root"
            Dim pwd As String = ""


        Try
                Dim configPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.json")
            If File.Exists(configPath) Then
                Dim jsonText As String = File.ReadAllText(configPath)

                If jsonText.Contains("Database") Then

                End If
            End If
        Catch ex As Exception

        End Try

            ConnectionString = $"Server={server};Port={port};Database={dbName};Uid={uid};Pwd={pwd};SslMode=Preferred;AllowZeroDateTime=True;"
        End Sub

        Public Shared Function GetConnection() As MySqlConnection
            Dim conn As New MySqlConnection(ConnectionString)
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

        Public Shared Function ExecuteScalar(sql As String, Optional params As MySqlParameter() = Nothing) As Object
            Using conn As MySqlConnection = GetConnection()
                Using cmd As New MySqlCommand(sql, conn)
                    If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                    Return cmd.ExecuteScalar()
                End Using
            End Using
        End Function
    End Class
