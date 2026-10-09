Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Drawing.Drawing2D

Public Class CustomProgressBar
    Inherits Control

    Private _value As Integer = 0
    Private _maximum As Integer = 100
    Private _barColor As Color = Color.LightGreen
    Private _borderColor As Color = Color.Black
    Private _cornerRadius As Integer = 10
    Private _showText As Boolean = True
    Private _textColor As Color = Color.Black
    Private _textAlign As ContentAlignment = ContentAlignment.MiddleCenter

    <Category("Behavior"), Description("Current progress value.")>
    Public Property Value As Integer
        Get
            Return _value
        End Get
        Set(v As Integer)
            If v < 0 Then v = 0
            If v > _maximum Then v = _maximum
            _value = v
            Me.Invalidate()
        End Set
    End Property

    <Category("Behavior"), Description("Maximum progress value.")>
    Public Property Maximum As Integer
        Get
            Return _maximum
        End Get
        Set(v As Integer)
            If v <= 0 Then v = 1
            _maximum = v
            If _value > _maximum Then _value = _maximum
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), Description("Fill color of the progress bar.")>
    Public Property BarColor As Color
        Get
            Return _barColor
        End Get
        Set(v As Color)
            _barColor = v
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), Description("Border color of the progress bar.")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(v As Color)
            _borderColor = v
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), Description("Corner radius for rounded edges.")>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(v As Integer)
            _cornerRadius = v
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), Description("Show percentage text inside the bar.")>
    Public Property ShowText As Boolean
        Get
            Return _showText
        End Get
        Set(v As Boolean)
            _showText = v
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), Description("Color of the percentage text.")>
    Public Property TextColor As Color
        Get
            Return _textColor
        End Get
        Set(v As Color)
            _textColor = v
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), Description("Alignment of the percentage text.")>
    Public Property TextAlign As ContentAlignment
        Get
            Return _textAlign
        End Get
        Set(v As ContentAlignment)
            _textAlign = v
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim rect As Rectangle = Me.ClientRectangle

        ' Border
        Using borderPen As New Pen(_borderColor, 2)
            Using borderPath As GraphicsPath = GetRoundedRectPath(rect, _cornerRadius)
                g.DrawPath(borderPen, borderPath)
            End Using
        End Using

        ' Progress fill
        Dim progressWidth As Integer = CInt((rect.Width - 4) * (_value / _maximum))
        If progressWidth > 0 Then
            Dim progressRect As New Rectangle(rect.X + 2, rect.Y + 2, progressWidth, rect.Height - 4)
            Using brush As New SolidBrush(_barColor)
                g.FillRectangle(brush, progressRect)
            End Using
        End If

        ' Text
        If _showText Then
            Dim percent As Integer = CInt((_value / _maximum) * 100)
            Dim text As String = percent.ToString() & "%"
            Dim format As New StringFormat()

            Select Case _textAlign
                Case ContentAlignment.TopLeft
                    format.Alignment = StringAlignment.Near
                    format.LineAlignment = StringAlignment.Near
                Case ContentAlignment.TopCenter
                    format.Alignment = StringAlignment.Center
                    format.LineAlignment = StringAlignment.Near
                Case ContentAlignment.TopRight
                    format.Alignment = StringAlignment.Far
                    format.LineAlignment = StringAlignment.Near
                Case ContentAlignment.MiddleLeft
                    format.Alignment = StringAlignment.Near
                    format.LineAlignment = StringAlignment.Center
                Case ContentAlignment.MiddleCenter
                    format.Alignment = StringAlignment.Center
                    format.LineAlignment = StringAlignment.Center
                Case ContentAlignment.MiddleRight
                    format.Alignment = StringAlignment.Far
                    format.LineAlignment = StringAlignment.Center
                Case ContentAlignment.BottomLeft
                    format.Alignment = StringAlignment.Near
                    format.LineAlignment = StringAlignment.Far
                Case ContentAlignment.BottomCenter
                    format.Alignment = StringAlignment.Center
                    format.LineAlignment = StringAlignment.Far
                Case ContentAlignment.BottomRight
                    format.Alignment = StringAlignment.Far
                    format.LineAlignment = StringAlignment.Far
            End Select

            Using textBrush As New SolidBrush(_textColor)
                g.DrawString(text, Me.Font, textBrush, rect, format)
            End Using
        End If
    End Sub

    Private Function GetRoundedRectPath(rect As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90)
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90)
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90)
        path.CloseFigure()
        Return path
    End Function
End Class
