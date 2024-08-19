/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Test.Common
{
    [TestClass]
    public class VisualBasicExecutableLinesMetricTest
    {
        [TestMethod]
        public void AttributeList() =>
            AssertLinesOfCode(
@"<Serializable()>
Public Class Sample
    <Runtime.InteropServices.DllImport(""user32.dll"")>
    Shared Sub SampleMethod()
    End Sub
End Class");

        [TestMethod]
        public void SyncLockStatement() =>
            AssertLinesOfCode(
@"Class simpleMessageList
    Private messagesLock As New Object

    Public Sub addAnotherMessage(ByVal newMessage As String)
        SyncLock messagesLock ' +1
        End SyncLock
    End Sub
End Class", 5);

        [TestMethod]
        public void UsingStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Using MS As New IO.MemoryStream
        End Using
    End Sub
End Module", 3);

        [TestMethod]
        public void DoUntilStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Dim index As Integer = 0
        Do
        Loop Until index > 10
    End Sub
End Module");

        [TestMethod]
        public void DoWhileStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Dim index As Integer = 0
        Do While index <= 10
        Loop
    End Sub
End Module", 4);

        [TestMethod]
        public void ForEachStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        For Each item As String In {""a"", ""b"", ""c""} ' +1
        Next
    End Sub
End Module", 3);

        [TestMethod]
        public void ForStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        For index As Integer = 1 To 5
        Next
    End Sub
End Module", 3);

        [TestMethod]
        public void WhileStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go(Index As Integer)
        While Index <= 10
        End While
    End Sub
End Module", 3);

        [TestMethod]
        public void IfStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go(Count As Integer)
        Dim Message As String
        If Count = 0 Then
            Message = ""There are no items.""
        ElseIf Count = 1 Then
            Message = ""There is 1 item.""
        Else
            Message = $""There are many items.""
        End If
    End Sub
End Module", 4, 5, 6, 7, 9);

        [TestMethod]
        public void SelectStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Dim number As Integer = 8
        Select Case number ' +1
            Case 1 To 5
                Debug.WriteLine(""f"") ' +1
            Case Else
                Debug.WriteLine(""f"") ' +1
        End Select
    End Sub
End Module", 4, 6, 8);

        [TestMethod]
        public void ConditionalAccessExpression() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go(Customers() As Integer)
        Dim length As Integer? = Customers?.Length
    End Sub
End Module", 3);

        [TestMethod]
        public void BinaryConditionalExpression() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Function Go(First As String, Second As String) As String
        Return If(First, Second)
    End Function
End Module", 3);

        [TestMethod]
        public void TernaryConditionalExpression() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go(A As String, B As String, C As String, D As String)
        Dim Ret As String = If(A = B, C, D)
    End Sub
End Module", 3);

        [TestMethod]
        public void GoToStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        GoTo LastLine
LastLine:
    End Sub
End Module", 3, 4);

        [TestMethod]
        public void ThrowStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Throw New Exception()
    End Sub
End Module", 3);

        [TestMethod]
        public void ReturnStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Function getAgePhrase(ByVal age As Integer) As String
        Return ""Infant""
    End Function
End Module", 3);

        [TestMethod]
        public void ExitDoStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go(Index As Integer)
        Do While Index <= 100
            If Index > 10 Then
                Exit Do
            End If
        Loop
    End Sub
End Module", 3, 4, 5);

        [TestMethod]
        public void ExitForStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        For index As Integer = 1 To 100000
            Exit For
        Next
    End Sub
End Module", 3, 4);

        [TestMethod]
        public void ExitWhileStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go(Index As Integer)
        While Index < 100000
            Exit While
        End While
    End Sub
End Module", 3, 4);

        [TestMethod]
        public void ContinueDoStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go(Cnt As Integer)
        Do
            Continue Do
        Loop While Cnt < 20
    End Sub
End Module", 4);

        [TestMethod]
        public void ContinueForStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        For index As Integer = 1 To 100000
            Continue For
        Next
    End Sub
End Module", 3, 4);

        [TestMethod]
        public void SimpleMemberAccessExpression() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Console.WriteLine(""Found"")
    End Sub
End Module", 3);

        [TestMethod]
        public void InvocationExpression() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub foo()
    End Sub

    Public Sub bar()
        foo()
    End Sub
End Module", 6);

        [TestMethod]
        public void SingleLineSubLambdaExpression() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Dim writeline1 = Sub(x) Console.WriteLine(x)
    End Sub
End Module", 3);

        [TestMethod]
        public void SingleLineFunctionLambdaExpression() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Dim increment1 = Function(x) x + 1
    End Sub
End Module", 3);

        [TestMethod]
        public void MultiLineSubLambdaExpression() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Dim writeline2 = Sub(x)
                            Console.WriteLine(x)
                         End Sub
    End Sub
End Module", 3, 4);

        [TestMethod]
        public void MultiLineFunctionLambdaExpression() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Go()
        Dim increment2 = Function(x)
                             Return x + 2
                         End Function
    End Sub
End Module", 3, 4);

        [TestMethod]
        public void StructureStatement() =>
            AssertLinesOfCode(
@"Structure foo
End Structure");

        [TestMethod]
        public void ClassStatement() =>
            AssertLinesOfCode(
@"Class foo
End Class");

        [TestMethod]
        public void ModuleStatement() =>
            AssertLinesOfCode(
@"Module foo
End Module");

        [TestMethod]
        public void FunctionStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Function myFunction(ByVal j As Integer) As Double
        Return 3.87 * j
    End Function
End Module", 3);

        [TestMethod]
        public void SubStatement() =>
            AssertLinesOfCode(
@"Public Module Sample
    Sub computeArea(ByVal length As Double, ByVal width As Double)
    End Sub
End Module");

        [TestMethod]
        public void SubNewStatement() =>
            AssertLinesOfCode(
@"Public Class Sample
    Public Sub New()
    End Sub
End Class");

        [TestMethod]
        public void PropertyStatement() =>
            AssertLinesOfCode(
@"Public Class Sample
    Public Property Name As String
End Class");

        [TestMethod]
        public void EventStatement() =>
            AssertLinesOfCode(
@"Public Class Sample
    Public Event LogonCompleted(ByVal UserName As String)
End Class");

        [TestMethod]
        public void AddHandlerAccessorStatement() =>
            AssertLinesOfCode(
@"Public Class Sample
    Public Event SomeEvent

    Sub TestEvents()
        Dim S As New Sample
        AddHandler S.SomeEvent, AddressOf OnSomeEvent
    End Sub

    Private Sub OnSomeEvent()
    End Sub
End Class", 6);

        [TestMethod]
        public void RemoveHandlerAccessorStatement() =>
            AssertLinesOfCode(
@"Public Class Sample
    Public Event SomeEvent

    Sub TestEvents()
        Dim S As New Sample
        RemoveHandler S.SomeEvent, AddressOf OnSomeEvent
    End Sub

    Private Sub OnSomeEvent()
    End Sub
End Class", 6);

        [TestMethod]
        public void SetAccessorStatement() =>
            AssertLinesOfCode(
@"Public Class Sample
    Private QuoteValue As String

    Public WriteOnly Property QuoteForTheDay() As String
        Set(ByVal Value As String)
            QuoteValue = Value
        End Set
    End Property
End Class", 6);

        [TestMethod]
        public void GetAccessorStatement() =>
            AssertLinesOfCode(
@"Public Class Sample
    ReadOnly Property quoteForTheDay() As String
        Get
        End Get
    End Property
End Class");

        [TestMethod]
        public void Assignments() =>
            AssertLinesOfCode(
@"Public Module Sample
    Public Sub Foo(ByVal flag _
                    As Boolean)
        If flag _
            Then
            flag = True : flag = False : flag = True
        End If
    End Sub
End Module", 4, 6);

        [TestMethod]
        public void ExcludedFromCodeCoverage_Class() =>
            AssertLinesOfCode(
                """
                Imports System.Diagnostics.CodeAnalysis

                <ExcludeFromCodeCoverage>
                Public Module Class1
                    Public Sub TestMe()
                        Console.WriteLine("Exclude me") ' +1, FP
                    End Sub
                End Module
                """, 6); // There should be no executable lines in the module

        [TestMethod]
        public void ExcludedFromCodeCoverage_Methods() =>
            AssertLinesOfCode(
                """
                Imports System.Diagnostics.CodeAnalysis

                Public Module Class3
                    <ExcludeFromCodeCoverage>
                    Public Sub Excluded()
                        Console.WriteLine("Exclude me") ' +1, FP
                    End Sub
                    Public Sub NotCovered()
                        Console.WriteLine("Not covered") ' +1
                    End Sub
                End Module
                """, 6, 9);

        private static void AssertLinesOfCode(string code, params int[] expectedExecutableLines)
        {
            var (syntaxTree, semanticModel) = TestHelper.CompileVB(code);
            Metrics.VisualBasic.VisualBasicExecutableLinesMetric.GetLineNumbers(syntaxTree, semanticModel).Should().BeEquivalentTo(expectedExecutableLines);
        }
    }
}
