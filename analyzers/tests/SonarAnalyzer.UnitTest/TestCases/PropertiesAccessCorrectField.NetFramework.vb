Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Public Class Repro3442
    Inherits  UserControl

    Public Shared ReadOnly BackgroundColorProperty As DependencyProperty = DependencyProperty.Register("BackgroundColor", GetType(Color), GetType(Color), New PropertyMetadata(""))
    Public Shared ReadOnly ForegroundColorProperty As DependencyProperty = DependencyProperty.Register("ForegroundColor", GetType(Color), GetType(Color), New PropertyMetadata(""))
    Public Shared ReadOnly OldColorProperty As DependencyProperty = DependencyProperty.Register("OldColor", GetType(Color), GetType(Color), New PropertyMetadata(""))

    Private backgroundColor_ As Color
    Private foregroundColor_ As Color
    Private oldColor_ As Color

    Public Property BackgroundColor As Color
        Get
            Return GetValue(BackgroundColorProperty)
        End Get

        Set
            SetValue(BackgroundColorProperty, value)
        End Set
    End Property

    Public Property ForegroundColor As Color
        Get
            Return GetValue(ForegroundColorProperty)
        End Get

        Set
            SetValue(ForegroundColorProperty, value)
        End Set
    End Property

    Public Property OldColor As Color
        Get
            Return GETVALUE(OldColorProperty)
        End Get

        Set
            SETVALUE(OldColorProperty, value)
        End Set
    End Property

End Class
