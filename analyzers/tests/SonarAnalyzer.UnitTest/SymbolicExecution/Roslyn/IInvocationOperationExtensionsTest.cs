/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
using SonarAnalyzer.UnitTest;
using StyleCop.Analyzers.Lightup;

[TestClass]
public class IInvocationOperationExtensionsTest
{
    [TestMethod]
    public void ArgumentValue()
    {
        var testClass = $$"""
            class Test
            {
                static void M(string stringParam)
                    => M("param");
            }
            """;
        var (tree, model) = TestHelper.CompileCS(testClass);
        var invocation = tree.GetRoot().DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Single();
        var operation = IInvocationOperationWrapper.FromOperation(model.GetOperation(invocation));
        var argument = operation.ArgumentValue("stringParam").Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.ConstantValue.Value.Should().Be("param");
    }

}
