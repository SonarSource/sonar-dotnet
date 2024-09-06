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

namespace SonarAnalyzer.VisualBasic.Core.Test.Syntax.Utilities;

[TestClass]
public class SafeVisualBasicSyntaxWalkerTest
{
    [TestMethod]
    public void GivenSyntaxNodeWithReasonableDepth_SafeVisit_ReturnsTrue() =>
        new Walker().SafeVisit(SyntaxFactory.ParseSyntaxTree("Public Function Main(Arg as Boolean) As Boolean").GetRoot()).Should().BeTrue();

    [TestMethod]
    public void GivenSyntaxNodeWithHighDepth_SafeVisit_ReturnsFalse()
    {
        var longExpression = Enumerable.Repeat("AndAlso Arg", 7000).JoinStr(" ");
        var code = $"""
            Public Class Sample
                Public Function Main(Arg as Boolean) As Boolean
                    Return Arg {longExpression}
                End Function
            End Class
            """;

        new Walker().SafeVisit(VisualBasicSyntaxTree.ParseText(code).GetCompilationUnitRoot()).Should().BeFalse();
    }

    private class Walker : SafeVisualBasicSyntaxWalker { }
}
