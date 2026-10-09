Imports System.ComponentModel
Imports System.Drawing.Drawing2D

Public Class CustomPanel
    Inherits Panel

    Private _cornerRadius As Integer = 20
    Private _borderColor As Color = Color.FromArgb(0, 85, 0)
    Private _borderThickness As Integer = 2

    <Category("Appearance"), DefaultValue(20)>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    <Category("Appearance")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            Invalidate()
        End Set
    End Property

    <Category("Appearance"), DefaultValue(2)>
    Public Property BorderThickness As Integer
        Get
            Return _borderThickness
        End Get
        Set(value As Integer)
            _borderThickness = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    Private _clipCorners As Boolean = False

    <Category("Appearance"), DefaultValue(False)>
    Public Property ClipCorners As Boolean
        Get
            Return _clipCorners
        End Get
        Set(value As Boolean)
            _clipCorners = value
            UpdateRegion()
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        UpdateRegion()
    End Sub

    Private Sub UpdateRegion()
        If _clipCorners AndAlso Width > 0 AndAlso Height > 0 Then
            Using path As GraphicsPath = RoundedRect(New Rectangle(0, 0, Width, Height), _cornerRadius)
                Region = New Region(path)
            End Using
        Else
            Region = Nothing
        End If
    End Sub

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.UserPaint, True)
        BackColor = Color.White
    End Sub

    ' Paint the corners with the parent's color so they look transparent
    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        Dim c As Color = If(Parent IsNot Nothing, Parent.BackColor, SystemColors.Control)
        e.Graphics.Clear(c)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim half As Integer = _borderThickness \ 2
        Dim rect As New Rectangle(half, half,
                                  Width - _borderThickness - 1,
                                  Height - _borderThickness - 1)

        Using path As GraphicsPath = RoundedRect(rect, _cornerRadius)
            Using fill As New SolidBrush(BackColor)
                e.Graphics.FillPath(fill, path)
            End Using
            If _borderThickness > 0 Then
                Using pen As New Pen(_borderColor, _borderThickness)
                    e.Graphics.DrawPath(pen, path)
                End Using
            End If
        End Using
    End Sub

    Public Shared Function RoundedRect(r As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        If radius <= 0 Then
            path.AddRectangle(r)
            Return path
        End If
        Dim d As Integer = radius * 2
        path.AddArc(r.X, r.Y, d, d, 180, 90)
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90)
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function

End Class
