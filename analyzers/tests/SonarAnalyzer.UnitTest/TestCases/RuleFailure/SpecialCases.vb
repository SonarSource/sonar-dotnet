Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices

Namespace TestCasesForRuleFailure

    Public Class SpecialCases

        Public Sub ParamsMethod(i As Integer, ParamArray j() As Integer)
        End Sub

        Public Sub ArgListMethod(__arglist)
            ArgListMethod("__arglist"(""))
        End Sub

        Public Shared Widening Operator CType(c As SpecialCases) As String
            Return ""
        End Operator

        Public Shared Narrowing Operator CType(c As SpecialCases) As Integer
            Return ""
        End Operator

        Public Shared Operator +(a As SpecialCases, b As SpecialCases) As SpecialCases
            Return Nothing
        End Operator

        Default Public Property Indexer(x As Integer, y As Integer) As String
            Get

            End Get
            Set(value As String)

            End Set
        End Property

        <DllImport("User32.dll", CharSet:=CharSet.Unicode)>
        Public Shared Function MessageBox(h As IntPtr, m As String, c As String, type As Integer) As Integer
        End Function

        Private Declare Function GetWindowText Lib "user32.dll" (ByVal hwnd As IntPtr, ByVal lpString As StringBuilder, ByVal cch As Integer) As Integer

        Shared Sub Main()
            MessageBox(IntPtr.Zero, "My message", "My Message Box", 0)
        End Sub

        Public Sub NullConditionalIndexing(List As List(Of Integer)
            Dim Value As Integer = List?(0)
        End Sub

    End Class

    Public Module MyExtensions

        <Extension>
        Public Sub Ext(s As String)
            Dim ss As String = Nothing
            ss.Ext()
            Ext(Nothing)
        End Sub

        Public Function GetDict() As Dictionary(Of String, String)
            Return New Dictionary(Of String, String) From {{"a", "b"}, {"c", "d"}}
        End Function

    End Module

End Namespace
