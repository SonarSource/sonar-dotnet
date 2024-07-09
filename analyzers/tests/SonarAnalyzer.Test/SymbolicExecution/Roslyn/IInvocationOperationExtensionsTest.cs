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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class IInvocationOperationExtensionsTest
{
    [DataTestMethod]
    [DataRow("public void Method() {}", "sample.Method()", OperationKind.FieldReference)]
    [DataRow("", "this.Method()", OperationKind.InstanceReference)]
    [DataRow("", "sample.ExtensionMethod()", OperationKind.FieldReference)]
    [DataRow("public void Method() {}", "sample.GetSample().Method()", OperationKind.Invocation)]

    public void Invocation_GetInstance_ReturnsSymbol(string definition, string invocation, OperationKind kind)
    {
        var code = $$"""
            class Test
            {
                Sample sample = new Sample();
                void Method() {}
                void M() => {{invocation}};
            }

            public class Sample
            {
               {{definition}}
               public Sample GetSample() => new Sample();
            }

            public static class Extensions
            {
                public static void ExtensionMethod(this Sample sample) {}
            }
            """;
        var (tree, model) = TestHelper.CompileCS(code);
        var invocationSyntax = tree.GetRoot().DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().First();
        var operation = IInvocationOperationWrapper.FromOperation(model.GetOperation(invocationSyntax));
        var d = operation.GetInstance(ProgramState.Empty);
        d.Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.Kind.Should().Be(kind);
    }
}
