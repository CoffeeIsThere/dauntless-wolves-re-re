Imports System.Data
Imports System.IO
Imports System.Text.Json
Imports MySql.Data.MySqlClient

Public Class DatabaseHelper

    Private Shared _connString As String
    Private Shared ReadOnly LockObj As New Object()

    Private Shared Function GetConnString() As String
        If _connString Is Nothing Then
            SyncLock LockObj
                If _connString Is Nothing Then
                    _connString = BuildConnString()
                End If
            End SyncLock
        End If
        Return _connString
    End Function

    Private Shared Function BuildConnString() As String
        Dim host As String = "127.0.0.1"
        Dim port As Integer = 3306
        Dim db As String = "transparent_grading_system"
        Dim user As String = "root"
        Dim pass As String = ""

        Dim configPath As String = Path.Combine(AppContext.BaseDirectory, "dbconfig.json")
        If File.Exists(configPath) Then
            Try
                Using doc As JsonDocument = JsonDocument.Parse(File.ReadAllText(configPath))
                    Dim root As JsonElement = doc.RootElement
                    host = ReadText(root, "Host", host)
                    db = ReadText(root, "Database", db)
                    user = ReadText(root, "User", user)
                    pass = ReadText(root, "Password", pass)

                    Dim portEl As JsonElement
                    If root.TryGetProperty("Port", portEl) AndAlso portEl.ValueKind = JsonValueKind.Number Then
                        port = portEl.GetInt32()
                    End If
                End Using
            Catch ex As JsonException
                Throw New InvalidOperationException("dbconfig.json is not valid JSON: " & ex.Message)
            End Try
        End If

        Dim b As New MySqlConnectionStringBuilder()
        b.Server = host
        b.Port = CUInt(port)
        b.Database = db
        b.UserID = user
        b.Password = pass
        b.SslMode = MySqlSslMode.Preferred
        b.AllowZeroDateTime = True
        Return b.ConnectionString
    End Function

    Private Shared Function ReadText(root As JsonElement, key As String, fallback As String) As String
        Dim el As JsonElement
        If root.TryGetProperty(key, el) AndAlso el.ValueKind = JsonValueKind.String Then
            Return el.GetString()
        End If
        Return fallback
    End Function

    Public Shared Function GetConnection() As MySqlConnection
        Dim conn As New MySqlConnection(GetConnString())
        conn.Open()
        Return conn
    End Function


    Public Shared Function ExecuteQuery(sql As String, ParamArray params As MySqlParameter()) As DataTable
        Dim dt As New DataTable()
        Using conn As MySqlConnection = GetConnection()
            Using cmd As New MySqlCommand(sql, conn)
                If params IsNot Nothing AndAlso params.Length > 0 Then
                    cmd.Parameters.AddRange(params)
                End If
                Using adapter As New MySqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
            End Using
        End Using
        Return dt
    End Function

    Public Shared Function ExecuteNonQuery(sql As String, ParamArray params As MySqlParameter()) As Integer
        Using conn As MySqlConnection = GetConnection()
            Using cmd As New MySqlCommand(sql, conn)
                If params IsNot Nothing AndAlso params.Length > 0 Then
                    cmd.Parameters.AddRange(params)
                End If
                Return cmd.ExecuteNonQuery()
            End Using
        End Using
    End Function

    Public Shared Function ExecuteScalar(sql As String, ParamArray params As MySqlParameter()) As Object
        Using conn As MySqlConnection = GetConnection()
            Using cmd As New MySqlCommand(sql, conn)
                If params IsNot Nothing AndAlso params.Length > 0 Then
                    cmd.Parameters.AddRange(params)
                End If
                Return cmd.ExecuteScalar()
            End Using
        End Using
    End Function

End Class