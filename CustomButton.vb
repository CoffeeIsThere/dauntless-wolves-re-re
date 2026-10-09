Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Drawing.Drawing2D

Public Class CustomButton
    Inherits Button

    Private _borderColor As Color = Color.Black
    Private _borderSize As Integer = 2
    Private _cornerRadius As Integer = 10
    Private _hoverColor As Color = Color.LightGray
    Private _normalColor As Color = Color.LightBlue

    <Category("Appearance"), Description("Border color of the button.")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), Description("Border thickness of the button.")>
    Public Property BorderSize As Integer
        Get
            Return _borderSize
        End Get
        Set(value As Integer)
            _borderSize = value
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), Description("Corner radius for rounded edges.")>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = value
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), Description("Background color when hovered.")>
    Public Property HoverColor As Color
        Get
            Return _hoverColor
        End Get
        Set(value As Color)
            _hoverColor = value
        End Set
    End Property

    <Category("Appearance"), Description("Normal background color.")>
    Public Property NormalColor As Color
        Get
            Return _normalColor
        End Get
        Set(value As Color)
            _normalColor = value
            Me.BackColor = value
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnPaint(pevent As PaintEventArgs)
        Dim g As Graphics = pevent.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim rect As Rectangle = Me.ClientRectangle
        Using path As GraphicsPath = GetRoundedRectPath(rect, _cornerRadius)
            Me.Region = New Region(path)

            ' Fill background
            Using brush As New SolidBrush(Me.BackColor)
                g.FillPath(brush, path)
            End Using

            ' Draw border
            Using pen As New Pen(_borderColor, _borderSize)
                g.DrawPath(pen, path)
            End Using
        End Using

        ' Draw text centered
        Dim sf As New StringFormat() With {
            .Alignment = StringAlignment.Center,
            .LineAlignment = StringAlignment.Center
        }
        Using textBrush As New SolidBrush(Me.ForeColor)
            g.DrawString(Me.Text, Me.Font, textBrush, rect, sf)
        End Using
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        Me.BackColor = _hoverColor
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        Me.BackColor = _normalColor
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
