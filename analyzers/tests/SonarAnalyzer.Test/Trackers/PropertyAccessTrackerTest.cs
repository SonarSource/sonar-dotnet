/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.Trackers;

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
        var context = CreateContext<CS.MemberAccessExpressionSyntax>(TestInputCS, "MyProperty", AnalyzerLanguage.CSharp);
        var tracker = new CSharpPropertyAccessTracker();

        tracker.MatchGetter()(context).Should().BeTrue();
        tracker.MatchSetter()(context).Should().BeFalse();
    }

    [TestMethod]
    public void MatchesGetter_VB()
    {
        var context = CreateContext<VB.MemberAccessExpressionSyntax>(TestInputVB, "MyProperty", AnalyzerLanguage.VisualBasic);
        var tracker = new VisualBasicPropertyAccessTracker();

        tracker.MatchGetter()(context).Should().BeTrue();
        tracker.MatchSetter()(context).Should().BeFalse();
    }

    [TestMethod]
    public void AndCondition()
    {
        var tracker = new CSharpPropertyAccessTracker();
        CSharpPropertyAccessTracker.Condition trueCondition = x => true;
        CSharpPropertyAccessTracker.Condition falseCondition = x => false;

        tracker.And(trueCondition, trueCondition)(null).Should().BeTrue();
        tracker.And(trueCondition, falseCondition)(null).Should().BeFalse();
        tracker.And(falseCondition, trueCondition)(null).Should().BeFalse();
        tracker.And(falseCondition, falseCondition)(null).Should().BeFalse();
    }

    [TestMethod]
    public void OrCondition()
    {
        var tracker = new CSharpPropertyAccessTracker();
        CSharpPropertyAccessTracker.Condition trueCondition = x => true;
        CSharpPropertyAccessTracker.Condition falseCondition = x => false;

        tracker.Or(trueCondition, trueCondition)(null).Should().BeTrue();
        tracker.Or(trueCondition, falseCondition)(null).Should().BeTrue();
        tracker.Or(falseCondition, trueCondition)(null).Should().BeTrue();
        tracker.Or(falseCondition, falseCondition)(null).Should().BeFalse();

        tracker.Or(falseCondition, falseCondition, trueCondition)(null).Should().BeTrue();
        tracker.Or(falseCondition, falseCondition, falseCondition)(null).Should().BeFalse();
    }

    private static PropertyAccessContext CreateContext<TSyntaxNodeType>(string testInput, string propertyName, AnalyzerLanguage language) where TSyntaxNodeType : SyntaxNode
    {
        var testCode = new SnippetCompiler(testInput, false, language);
        var node = testCode.GetNodes<TSyntaxNodeType>().First();
        return new PropertyAccessContext(testCode.CreateAnalysisContext(node), propertyName);
    }
}
