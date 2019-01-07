/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class VisualBasicExecutableLinesMetricTest
    {
        [TestMethod]
        public void AttributeList()
        {
            AssertLinesOfCode(@"
<System.Runtime.InteropServices.DllImport(""user32.dll"")>
<System.Serializable()>
Sub SampleMethod()
End Sub");
        }

        [TestMethod]
        public void SyncLockStatement()
        {
            AssertLinesOfCode(@"
Class simpleMessageList
    Private messagesLock As New Object

    Public Sub addAnotherMessage(ByVal newMessage As String)
        SyncLock messagesLock ' +1
        End SyncLock
    End Sub
End Class",
            6);
        }

        [TestMethod]
        public void UsingStatement()
        {
            AssertLinesOfCode(@"
Using resource As New resourceType
    ' Insert code to work with resource.
End Using ",
            2);
        }

        [TestMethod]
        public void DoUntilStatement()
        {
            AssertLinesOfCode(@"
Dim index As Integer = 0
Do
Loop Until index > 10");
        }

        [TestMethod]
        public void DoWhileStatement()
        {
            AssertLinesOfCode(@"
Dim index As Integer = 0
Do While index <= 10
Loop",
            3);
        }

        [TestMethod]
        public void ForEachStatement()
        {
            AssertLinesOfCode(@"
For Each item As String In lst ' +1
Next",
            2);
        }

        [TestMethod]
        public void ForStatement()
        {
            AssertLinesOfCode(@"
For index As Integer = 1 To 5
Next",
            2);
        }

        [TestMethod]
        public void WhileStatement()
        {
            AssertLinesOfCode(@"
While index <= 10
End While",
            2);
        }

        [TestMethod]
        public void IfStatement()
        {
            AssertLinesOfCode(@"
If count = 0 Then
    message = ""There are no items.""
ElseIf count = 1 Then
    message = ""There is 1 item.""
Else
    message = $""There are many items.""
End If",
            2, 3, 5, 7);
        }

        [TestMethod]
        public void SelectStatement()
        {
            AssertLinesOfCode(@"
Dim number As Integer = 8
Select Case number ' +1
    Case 1 To 5
        Debug.WriteLine(""f"") ' +1
    Case Else
        Debug.WriteLine(""f"") ' +1
End Select",
            3, 5, 7);
        }

        [TestMethod]
        public void ConditionalAccessExpression()
        {
            AssertLinesOfCode(@"Dim length As Integer? = customers?.Length", 1);
        }

        [TestMethod]
        public void BinaryConditionalExpression()
        {
            AssertLinesOfCode(@"If(first, second)", 1);
        }

        [TestMethod]
        public void TernaryConditionalExpression()
        {
            AssertLinesOfCode(@"Dim foo as String = If(bar = buz, cat, dog)", 1);
        }

        [TestMethod]
        public void GoToStatement()
        {
            AssertLinesOfCode(@"GoTo LastLine", 1);
        }

        [TestMethod]
        public void ThrowStatement()
        {
            AssertLinesOfCode(@"Throw New System.Exception()", 1);
        }

        [TestMethod]
        public void ReturnStatement()
        {
            AssertLinesOfCode(@"
Public Function getAgePhrase(ByVal age As Integer) As String
    Return ""Infant""
End Function",
            3);
        }

        [TestMethod]
        public void ExitDoStatement()
        {
            AssertLinesOfCode(@"
Do While index <= 100
    If index > 10 Then
        Exit Do
    End If
Loop",
            2, 3, 4);
        }

        [TestMethod]
        public void ExitForStatement()
        {
            AssertLinesOfCode(@"
For index As Integer = 1 To 100000
    Exit For
End For",
            2, 3);
        }

        [TestMethod]
        public void ExitWhileStatement()
        {
            AssertLinesOfCode(@"
While index < 100000
    Exit While
End While",
            2, 3);
        }

        [TestMethod]
        public void ContinueDoStatement()
        {
            AssertLinesOfCode(@"
Do
    Continue Do
Loop While (a < 20)",
            3);
        }

        [TestMethod]
        public void ContinueForStatement()
        {
            AssertLinesOfCode(@"
For index As Integer = 1 To 100000
    Continue For
End For",
            2, 3);
        }

        [TestMethod]
        public void SimpleMemberAccessExpression()
        {
            AssertLinesOfCode(@" Console.WriteLine(""Found"")", 1);
        }

        [TestMethod]
        public void InvocationExpression()
        {
            AssertLinesOfCode(@"
Public Sub foo()
End Sub

Public Sub bar()
    foo()
End Sub",
            6);
        }

        [TestMethod]
        public void SingleLineSubLambdaExpression()
        {
            AssertLinesOfCode(@"Dim writeline1 = Sub(x) Console.WriteLine(x)", 1);
        }

        [TestMethod]
        public void SingleLineFunctionLambdaExpression()
        {
            AssertLinesOfCode(@"Dim increment1 = Function(x) x + 1", 1);
        }

        [TestMethod]
        public void MultiLineSubLambdaExpression()
        {
            AssertLinesOfCode(@"
Dim writeline2 = Sub(x)
                     Console.WriteLine(x)
                 End Sub",
            2, 3);
        }

        [TestMethod]
        public void MultiLineFunctionLambdaExpression()
        {
            AssertLinesOfCode(@"
Dim increment2 = Function(x)
                     Return x + 2
                 End Function",
            2, 3);
        }

        [TestMethod]
        public void StructureStatement()
        {
            AssertLinesOfCode(@"
Structure foo
End Structure");
        }

        [TestMethod]
        public void ClassStatement()
        {
            AssertLinesOfCode(@"
Class foo
End Class");
        }

        [TestMethod]
        public void ModuleStatement()
        {
            AssertLinesOfCode(@"
Module foo
End Module");
        }

        [TestMethod]
        public void FunctionStatement()
        {
            AssertLinesOfCode(@"
Function myFunction(ByVal j As Integer) As Double
    Return 3.87 * j
End Function",
            3);
        }

        [TestMethod]
        public void SubStatement()
        {
            AssertLinesOfCode(@"
Sub computeArea(ByVal length As Double, ByVal width As Double)
End Sub");
        }

        [TestMethod]
        public void SubNewStatement()
        {
            AssertLinesOfCode(@"
Public Sub New()
End Sub");
        }

        [TestMethod]
        public void PropertyStatement()
        {
            AssertLinesOfCode("Public Property Name As String");
        }

        [TestMethod]
        public void EventStatement()
        {
            AssertLinesOfCode(@"
Public Event LogonCompleted(ByVal UserName As String)");
        }

        [TestMethod]
        public void AddHandlerAccessorStatement()
        {
            AssertLinesOfCode(@"
Sub TestEvents()
    Dim Obj As New Class1
    AddHandler Obj.Ev_Event, AddressOf EventHandler
End Sub", 4);
        }

        [TestMethod]
        public void RemoveHandlerAccessorStatement()
        {
            AssertLinesOfCode(@"
Sub TestEvents()
    Dim Obj As New Class1
    RemoveHandler Obj.Ev_Event, AddressOf EventHandler
End Sub",
            4);
        }

        [TestMethod]
        public void SetAccessorStatement()
        {
            AssertLinesOfCode(@"
Property quoteForTheDay() As String
    Set(ByVal value As String)
        quoteValue = value
    End Set
End Property",
            4);
        }

        [TestMethod]
        public void GetAccessorStatement()
        {
            AssertLinesOfCode(@"
ReadOnly Property quoteForTheDay() As String
    Get
    End Get
End Property");
        }

        [TestMethod]
        public void Assignments()
        {
            AssertLinesOfCode(@"
Public Sub Foo(ByVal flag _
                As Boolean)
    If flag _
        Then
        flag = True : flag = False : flag = True
    End If
End Sub",
            4, 6);
        }

        private static void AssertLinesOfCode(string code, params int[] expectedExecutableLines)
        {
            (var syntaxTree, var semanticModel) = TestHelper.Compile(code, false);
            Metrics.VisualBasic.VisualBasicExecutableLinesMetric.GetLineNumbers(syntaxTree, semanticModel).Should().BeEquivalentTo(expectedExecutableLines);
        }
    }
}
