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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class PropertyAccessTrackerTest
    {
        private const string TestInputCS = @"
public class Base
{
    private int MyProperty {get; set;}

    private void Usage()
    {
        var x = this.MyProperty;
    }
}";

        private const string TestInputVB = @"
Public Class Base
    Public Property MyProperty As Integer

    Public Sub Usage()
        Dim x As Integer = Me.MyProperty
    End Sub
End Class";

        [TestMethod]
        public void MatchesGetter_CS()
        {
            var context = CreateContext<CSharpSyntax.MemberAccessExpressionSyntax>(TestInputCS, "MyProperty", AnalyzerLanguage.CSharp);
            var tracker = new CSharpPropertyAccessTracker();

            tracker.MatchGetter()(context).Should().BeTrue();
            tracker.MatchSetter()(context).Should().BeFalse();
        }

        [TestMethod]
        public void MatchesGetter_VB()
        {
            var context = CreateContext<VBSyntax.MemberAccessExpressionSyntax>(TestInputVB, "MyProperty", AnalyzerLanguage.VisualBasic);
            var tracker = new VisualBasicPropertyAccessTracker();

            tracker.MatchGetter()(context).Should().BeTrue();
            tracker.MatchSetter()(context).Should().BeFalse();
        }

        private static PropertyAccessContext CreateContext<TSyntaxNodeType>(string testInput, string propertyName, AnalyzerLanguage language) where TSyntaxNodeType : SyntaxNode
        {
            var testCode = new SnippetCompiler(testInput, false, language);
            var expression = testCode.GetNodes<TSyntaxNodeType>().First();
            return new PropertyAccessContext(expression, propertyName, testCode.SemanticModel);
        }
    }
}
