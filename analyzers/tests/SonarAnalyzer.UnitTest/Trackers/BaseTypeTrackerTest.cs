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

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.Trackers;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class BaseTypeTrackerTest
    {
        private const string TestInputCS = @"
public class Sample : System.Exception {}";

        private const string TestInputVB = @"
Public Class Sample
    Inherits System.Exception
End Class";

        [TestMethod]
        public void MatchSubclassesOf_CS()
        {
            var tracker = new CSharpBaseTypeTracker();

            var context = CreateContext<CSharpSyntax.BaseListSyntax>(TestInputCS, AnalyzerLanguage.CSharp, x => Enumerable.Empty<SyntaxNode>());
            tracker.MatchSubclassesOf(KnownType.System_Exception)(context).Should().BeFalse();

            context = CreateContext<CSharpSyntax.BaseListSyntax>(TestInputCS, AnalyzerLanguage.CSharp, x => null);
            tracker.MatchSubclassesOf(KnownType.System_Exception)(context).Should().BeFalse();

            context = CreateContext<CSharpSyntax.BaseListSyntax>(TestInputCS, AnalyzerLanguage.CSharp, x => x.Types.Select(x => x.Type));
            tracker.MatchSubclassesOf(KnownType.System_Exception)(context).Should().BeTrue();
            tracker.MatchSubclassesOf(KnownType.System_Attribute)(context).Should().BeFalse();
        }

        [TestMethod]
        public void MatchSubclassesOf_VB()
        {
            var tracker = new VisualBasicBaseTypeTracker();
            var context = CreateContext<VBSyntax.InheritsStatementSyntax>(TestInputVB, AnalyzerLanguage.VisualBasic, x => Enumerable.Empty<SyntaxNode>());
            tracker.MatchSubclassesOf(KnownType.System_Exception)(context).Should().BeFalse();

            context = CreateContext<VBSyntax.InheritsStatementSyntax>(TestInputVB, AnalyzerLanguage.VisualBasic, x => null);
            tracker.MatchSubclassesOf(KnownType.System_Exception)(context).Should().BeFalse();

            context = CreateContext<VBSyntax.InheritsStatementSyntax>(TestInputVB, AnalyzerLanguage.VisualBasic, x => x.Types);
            tracker.MatchSubclassesOf(KnownType.System_Exception)(context).Should().BeTrue();
            tracker.MatchSubclassesOf(KnownType.System_Attribute)(context).Should().BeFalse();
        }

        private static BaseTypeContext CreateContext<TSyntaxNodeType>(string testInput, AnalyzerLanguage language, Func<TSyntaxNodeType, IEnumerable<SyntaxNode>> baseTypeNodes)
            where TSyntaxNodeType : SyntaxNode
        {
            var testCode = new SnippetCompiler(testInput, false, language);
            var node = testCode.GetNodes<TSyntaxNodeType>().Single();
            return new BaseTypeContext(testCode.CreateAnalysisContext(node), baseTypeNodes(node));
        }
    }
}
