/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class CSharpSyntaxWalkerExtensionsTest
    {
        [TestMethod]
        public void GivenSyntaxNodeWithReasonableDepth_SafeVisit_ReturnsTrue()
        {
            var result = new Walker().SafeVisit(SyntaxFactory.ParseSyntaxTree("void Method() { }").GetRoot());
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GivenSyntaxNodeWithHighDepth_SafeVisit_ReturnsFalse()
        {
            var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Method");

            var condition = SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression,
                                                           SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("a")),
                                                           SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("b")));

            var ifStatement = SyntaxFactory.IfStatement(condition, SyntaxFactory.Block());

            var node = ifStatement;
            for (var index = 0; index < 5000; index++)
            {
                node = SyntaxFactory.IfStatement(condition, SyntaxFactory.Block().AddStatements(node));
            }

            method = method.AddBodyStatements(node);

            Assert.IsFalse(new Walker().SafeVisit(method));
        }

        private class Walker : CSharpSyntaxWalker { }
    }
}
