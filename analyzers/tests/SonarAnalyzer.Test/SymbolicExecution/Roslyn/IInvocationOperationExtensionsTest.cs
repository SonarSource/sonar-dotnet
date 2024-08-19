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

using SonarAnalyzer.SymbolicExecution.Roslyn;
using StyleCop.Analyzers.Lightup;
using SyntaxCS = Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxVB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class IInvocationOperationExtensionsTest
{
    [DataTestMethod]
    [DataRow("sample.Method()", OperationKind.FieldReference)]
    [DataRow("this.Method()", OperationKind.InstanceReference)]
    [DataRow("sample.ExtensionMethod()", OperationKind.FieldReference)]
    [DataRow("sample.GetSample().Method()", OperationKind.Invocation)]

    public void Invocation_GetInstance_ReturnsSymbol_CS(string invocation, OperationKind kind)
    {
        var code = $$"""

            public class Sample
            {
               Sample sample = new Sample();

               public void Method() {}

               void M() => {{invocation}};

               public Sample GetSample() => new Sample();
            }

            public static class Extensions
            {
                public static void ExtensionMethod(this Sample sample) {}
            }
            """;
        var (tree, model) = TestHelper.CompileCS(code);
        var invocationSyntax = tree.GetRoot().DescendantNodesAndSelf().OfType<SyntaxCS.InvocationExpressionSyntax>().First();
        var operation = IInvocationOperationWrapper.FromOperation(model.GetOperation(invocationSyntax));
        operation.Target(ProgramState.Empty).Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.Kind.Should().Be(kind);
    }

    [DataTestMethod]
    [DataRow("sample.Method()", OperationKind.FieldReference)]
    [DataRow("Me.Method()", OperationKind.InstanceReference)]
    [DataRow("sample.ExtensionMethod()", OperationKind.FieldReference)]
    [DataRow("sample.GetSample().Method()", OperationKind.Invocation)]

    public void Invocation_GetInstance_ReturnsSymbol_VB(string invocation, OperationKind kind)
    {
        var code = $$"""
            Public Class Sample
                Private sample As Sample = New Sample()

                Public Sub Method()
                End Sub

                Private Sub M()
                    {{invocation}}
                End Sub

                Public Function GetSample() As Sample
                    Return New Sample()
                End Function

            End Class

            Public Module Extensions

                <System.Runtime.CompilerServices.Extension()>
                    Public Sub ExtensionMethod(sample As Sample)
                End Sub

            End Module
            """;
        var (tree, model) = TestHelper.CompileVB(code);
        var invocationSyntax = tree.GetRoot().DescendantNodesAndSelf().OfType<SyntaxVB.InvocationExpressionSyntax>().First();
        var operation = IInvocationOperationWrapper.FromOperation(model.GetOperation(invocationSyntax));
        operation.Target(ProgramState.Empty).Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.Kind.Should().Be(kind);
    }
}
