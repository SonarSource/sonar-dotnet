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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

[TestClass]
public class IObjectCreationExtensionsTest
{
    [DataTestMethod]
    [DataRow("""1, "Test" """, "intParam", 1)]
    [DataRow("""1, "Test" """, "stringParam", "Test")]
    [DataRow("""1, "Test" """, "optionalBoolParam", true)]
    [DataRow("""1, "Test" """, "optionalIntParam", 1)]
    [DataRow("""1, "Test", true """, "optionalBoolParam", true)]
    [DataRow("""stringParam: "Test", intParam: 1 """, "stringParam", "Test")]
    [DataRow("""stringParam: "Test", intParam: 1, optionalIntParam: 2 """, "optionalIntParam", 2)]
    public void ArgumentValue(string objectCreationArguments, string parameterName, object expected)
    {
        var testClass = $$"""
            class Test
            {
                Test(int intParam, string stringParam, bool optionalBoolParam = true, int optionalIntParam = 1) { }

                static void Create() =>
                    new Test({{objectCreationArguments}});
            }
            """;
        var (tree, model) = TestHelper.CompileCS(testClass);
        var objectCreation = tree.GetRoot().DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().Single();
        var operation = IObjectCreationOperationWrapper.FromOperation(model.GetOperation(objectCreation));
        operation.ArgumentValue(parameterName).Should().NotBeNull().And.BeAssignableTo<IOperation>().Which.ConstantValue.Value.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""  """)]
    [DataRow(""" "param1", "param2" """, "param1", "param2")]
    [DataRow(""" null, null """, null, null)]
    [DataRow(""" new[] {"param1", "param2"} """, "param1", "param2")]
    public void ArgumentValue_Params(string arguments, params string[] expected)
    {
        var testClass = $$"""
            class Test
            {
                Test(params string[] stringParams) { }

                static void Create() =>
                    new Test({{arguments}});
            }
            """;
        var (tree, model) = TestHelper.CompileCS(testClass);
        var objectCreation = tree.GetRoot().DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().Single();
        var operation = IObjectCreationOperationWrapper.FromOperation(model.GetOperation(objectCreation));
        var argument = operation.ArgumentValue("stringParams").Should().NotBeNull().And.BeAssignableTo<IOperation>().Subject;
        var argumentArray = IArrayCreationOperationWrapper.FromOperation(argument);
        var result = argumentArray.Initializer.ElementValues.Select(x => x.ConstantValue.Value).ToArray();
        result.Should().BeEquivalentTo(expected);
    }
}
