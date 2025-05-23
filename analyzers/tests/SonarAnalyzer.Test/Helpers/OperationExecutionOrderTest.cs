﻿/*
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

using System.Collections;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.Helpers
{
    [TestClass]
    public class OperationExecutionOrderTest
    {
        [DataTestMethod]
        [DataRow(false, DisplayName = "Execution order")]
        [DataRow(true, DisplayName = "Reversed execution order")]
        public void Linear(bool reverseOrder)
        {
            const string code = """
                var a = 1;
                var b = 2;
                a = b + 1;
                Method();
                """;
            var expected = new[]
            {
                "Literal: 1",
                "VariableInitializer: = 1",
                "VariableDeclarator: a = 1",
                "VariableDeclaration: var a = 1",
                "VariableDeclarationGroup: var a = 1;",
                "Literal: 2",
                "VariableInitializer: = 2",
                "VariableDeclarator: b = 2",
                "VariableDeclaration: var b = 2",
                "VariableDeclarationGroup: var b = 2;",
                "LocalReference: a",
                "LocalReference: b",
                "Literal: 1",
                "Binary: b + 1",
                "SimpleAssignment: a = b + 1",
                "ExpressionStatement: a = b + 1;",
                "Invocation: Method()",
                "ExpressionStatement: Method();"
            };
            ValidateOrder(code, expected, reverseOrder);
        }

        [DataTestMethod]
        [DataRow(false, DisplayName = "Execution order")]
        [DataRow(true, DisplayName = "Reversed execution order")]
        public void Nested(bool reverseOrder)
        {
            const string code = """
                Method(0);
                Method(1, Nested(40 + 2), 3);
                Method(4);
                """;
            var expected = new[]
            {
                "Literal: 0",
                "Invocation: Method(0)",
                "ExpressionStatement: Method(0);",
                "Literal: 1",
                "Literal: 40",
                "Literal: 2",
                "Binary: 40 + 2",
                "Invocation: Nested(40 + 2)",
                "Literal: 3",
                "Invocation: Method(1, Nested(40 + 2), 3)",
                "ExpressionStatement: Method(1, Nested(40 + 2), 3);",
                "Literal: 4",
                "Invocation: Method(4)",
                "ExpressionStatement: Method(4);"
            };
            ValidateOrder(code, expected, reverseOrder);
        }

        [TestMethod]
        public void InterruptedEvaluation()
        {
            var enumerator = Compile("Method(0);", false).GetEnumerator();
            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Should().NotBeNull();
            enumerator.Current.Instance.Should().NotBeNull();

            enumerator.Reset();
            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Should().NotBeNull();
            enumerator.Current.Instance.Should().NotBeNull();

            enumerator.Dispose();
            enumerator.MoveNext().Should().BeFalse();
            enumerator.Current.Should().NotBeNull();
            enumerator.Current.Instance.Should().BeNull();
        }

        [TestMethod]
        public void AsIEnumerable()
        {
            var enumerator = ((IEnumerable)Compile("Method(0);", false)).GetEnumerator();
            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Should().NotBeNull();
        }

        private static void ValidateOrder(string code, string[] expected, bool reverseOrder)
        {
            var sut = Compile(code, reverseOrder);
            var list = new List<string>();
            if (reverseOrder)
            {
                expected = expected.Reverse().ToArray();
            }
            foreach (var operation in sut)   // Act
            {
                if (!operation.IsImplicit)
                {
                    list.Add(operation.Instance.Kind + ": " + operation.Instance.Syntax);
                }
            }
            list.Should().Equal(expected);
        }

        private static OperationExecutionOrder Compile(string methodBody, bool reverseOrder)
        {
            var code = $$"""
                public class Sample
                {
                    public void Main()
                    {
                {{methodBody}}
                    }

                    public int Method(params int[] values) => 0;
                    public int Nested(params int[] values) => 0;
                }
                """;
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var body = tree.First<BlockSyntax>();
            var rootOperation = new IOperationWrapperSonar(semanticModel.GetOperation(body));
            return new OperationExecutionOrder(rootOperation.Children, reverseOrder);
        }
    }
}
