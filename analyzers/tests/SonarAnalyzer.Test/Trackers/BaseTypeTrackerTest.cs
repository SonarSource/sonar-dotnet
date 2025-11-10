/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Trackers;
using SonarAnalyzer.CSharp.Core.Trackers;
using SonarAnalyzer.VisualBasic.Core.Trackers;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.Trackers;

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
