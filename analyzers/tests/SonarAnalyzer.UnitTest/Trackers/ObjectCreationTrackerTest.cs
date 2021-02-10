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
using SonarAnalyzer.Helpers.Trackers;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class ObjectCreationTrackerTest
    {
        [TestMethod]
        public void ConstArgumentForParameter_CS()
        {
            const string testInput = @"
public class Base
{
    private Base(string a, string b, bool c, int d, object e) {}

    private void Usage(string notAConst)
    {
      new Base(notAConst, ""myConst"", true, 4, new object());
    }
}";
            var context = CreateContext<CSharpSyntax.ObjectCreationExpressionSyntax>(testInput, AnalyzerLanguage.CSharp);
            var tracker = new CSharpObjectCreationTracker();

            tracker.ConstArgumentForParameter(context, "a").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "b").Should().Be("myConst");
            tracker.ConstArgumentForParameter(context, "c").Should().Be(true);
            tracker.ConstArgumentForParameter(context, "d").Should().Be(4);
            tracker.ConstArgumentForParameter(context, "e").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "nonExistingParameterName").Should().BeNull();
        }

        [TestMethod]
        public void ConstArgumentForParameter_VB()
        {
            const string testInput = @"
Public Class Base
    Sub New(ByVal a As String, b As String, ByVal c As Boolean, ByVal d As Integer, ByRef e As Integer, ByVal f As Object)
    End Sub

    Public Sub Usage(ByVal notAConst As String)
        Dim tmp = New Base(notAConst, ""myConst"", True, 4, 5, New Object())
    End Sub
End Class";
            var context = CreateContext<VBSyntax.ObjectCreationExpressionSyntax>(testInput, AnalyzerLanguage.VisualBasic);
            var tracker = new VisualBasicObjectCreationTracker();

            tracker.ConstArgumentForParameter(context, "a").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "b").Should().Be("myConst");
            tracker.ConstArgumentForParameter(context, "c").Should().Be(true);
            tracker.ConstArgumentForParameter(context, "d").Should().Be(4);
            tracker.ConstArgumentForParameter(context, "e").Should().Be(5);
            tracker.ConstArgumentForParameter(context, "f").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "nonExistingParameterName").Should().BeNull();
        }

        [TestMethod]
        public void ObjectCreationConditionForUndefinedSymbol()
        {
            const string testInput = @"
public class Base
{
    private void Usage(string notAConst)
    {
      new Undefined(true);
    }
}";
            var context = CreateContext<CSharpSyntax.ObjectCreationExpressionSyntax>(testInput, AnalyzerLanguage.CSharp);
            var tracker = new CSharpObjectCreationTracker();
            tracker.ArgumentAtIndexIs(0, KnownType.System_Boolean)(context).Should().BeFalse();
            tracker.WhenDerivesFrom(KnownType.System_Exception)(context).Should().BeFalse();
            tracker.WhenImplements(KnownType.System_IDisposable)(context).Should().BeFalse();
            tracker.WhenDerivesOrImplementsAny(KnownType.System_Boolean)(context).Should().BeFalse();
            tracker.MatchConstructor(KnownType.System_Boolean)(context).Should().BeFalse();
        }

        [TestMethod]
        public void ObjectCreationConditionForNonconstructorSymbols()
        {
            const string testInput = @"
using System;

public class Base : Exception, IDisposable
{
    private void Method(bool b) { }
    public void Dispose() { }

    public void Usage()
    {
        Method(true);
    }
}";
            var context = CreateContext<CSharpSyntax.InvocationExpressionSyntax>(testInput, AnalyzerLanguage.CSharp);   // Created with wrong syntax
            var tracker = new CSharpObjectCreationTracker();
            tracker.ArgumentAtIndexIs(0, KnownType.System_Boolean)(context).Should().BeTrue();  // Doesn't care about symbol type
            tracker.WhenDerivesFrom(KnownType.System_Exception)(context).Should().BeFalse();
            tracker.WhenImplements(KnownType.System_IDisposable)(context).Should().BeFalse();
            tracker.WhenDerivesOrImplementsAny(KnownType.System_Exception)(context).Should().BeFalse();
            tracker.MatchConstructor(KnownType.System_Boolean)(context).Should().BeFalse();
        }

        [TestMethod]
        public void AndCondition()
        {
            var tracker = new CSharpObjectCreationTracker();
            CSharpObjectCreationTracker.Condition trueCondition = x => true;
            CSharpObjectCreationTracker.Condition falseCondition = x => false;

            tracker.And(trueCondition, trueCondition)(null).Should().BeTrue();
            tracker.And(trueCondition, falseCondition)(null).Should().BeFalse();
            tracker.And(falseCondition, trueCondition)(null).Should().BeFalse();
            tracker.And(falseCondition, falseCondition)(null).Should().BeFalse();
        }

        [TestMethod]
        public void OrCondition()
        {
            var tracker = new CSharpObjectCreationTracker();
            CSharpObjectCreationTracker.Condition trueCondition = x => true;
            CSharpObjectCreationTracker.Condition falseCondition = x => false;

            tracker.Or(trueCondition, trueCondition)(null).Should().BeTrue();
            tracker.Or(trueCondition, falseCondition)(null).Should().BeTrue();
            tracker.Or(falseCondition, trueCondition)(null).Should().BeTrue();
            tracker.Or(falseCondition, falseCondition)(null).Should().BeFalse();

            tracker.Or(falseCondition, falseCondition, trueCondition)(null).Should().BeTrue();
            tracker.Or(falseCondition, falseCondition, falseCondition)(null).Should().BeFalse();
        }

        private static ObjectCreationContext CreateContext<TSyntaxNodeType>(string testInput, AnalyzerLanguage language) where TSyntaxNodeType : SyntaxNode
        {
            var testCode = new SnippetCompiler(testInput, true, language);
            var node = testCode.GetNodes<TSyntaxNodeType>().First();
            var context = new ObjectCreationContext(testCode.CreateAnalysisContext(node));
            return context;
        }
    }
}
