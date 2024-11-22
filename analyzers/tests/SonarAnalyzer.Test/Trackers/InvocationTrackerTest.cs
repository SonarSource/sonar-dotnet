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
public class InvocationTrackerTest
{
    private const string TestInputCS = @"
public class Base
{
    private void MyMethod(string a, string b, bool c, int d, object e) {}

    private void Usage(string notAConst)
    {
        MyMethod(notAConst, ""myConst"", true, 4, new object());
        Undefined();
    }
}";

    private const string TestInputVB = @"
Public Class Base
    Public Sub MyMethod(ByVal a As String, b As String, ByVal c As Boolean, ByVal d As Integer, ByRef e As Integer, ByVal f As Object)
    End Sub

    Public Sub NoArgs()
    End Sub

    Public Sub Usage(ByVal notAConst As String)
        MyMethod(notAConst, ""myConst"", True, 4, 5, New Object())
        NoArgs 'and no ParameterList
    End Sub
End Class";

#if NET
    private const string IsIHeadersDictionaryCode = @"
namespace WebPoc
{
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

public class PermissiveCorsSimple
{
    public void MoreParameters() => new Dictionary<string, StringValues>().Add(""a"", new StringValues(), ""c"");

    public void WrongTypeFirstParameter() => new Dictionary<string, StringValues>().Add(1, new StringValues());

    public void WrongTypeSecondParameter() => new Dictionary<string, StringValues>().Add(""a"", 1);

    public void RightCall() => new Dictionary<string, StringValues>().Add(""A"", new StringValues());
}

public static class Extensions
{
    public static void Add(this Dictionary<string, StringValues> d, string a, string b, string c) { }

    public static void Add(this Dictionary<string, StringValues> d, int a, StringValues b) { }

    public static void Add(this Dictionary<string, StringValues> d, string a, int b) { }
}
}
";
#endif

    [TestMethod]
    public void ConstArgumentForParameter_CS()
    {
        var context = CreateContext<CS.InvocationExpressionSyntax>(TestInputCS, "MyMethod", AnalyzerLanguage.CSharp);
        var tracker = new CSharpInvocationTracker();

        tracker.ConstArgumentForParameter(context, "a").Should().BeNull();
        tracker.ConstArgumentForParameter(context, "b").Should().Be("myConst");
        tracker.ConstArgumentForParameter(context, "c").Should().Be(true);
        tracker.ConstArgumentForParameter(context, "d").Should().Be(4);
        tracker.ConstArgumentForParameter(context, "e").Should().BeNull();
        tracker.ConstArgumentForParameter(context, "nonExistingParameterName").Should().BeNull();
    }

    [TestMethod]
    public void ArgumentIsBoolConstant_CS()
    {
        var context = CreateContext<CS.InvocationExpressionSyntax>(TestInputCS, "MyMethod", AnalyzerLanguage.CSharp);
        var tracker = new CSharpInvocationTracker();

        tracker.ArgumentIsBoolConstant("a", true)(context).Should().Be(false);
        tracker.ArgumentIsBoolConstant("a", false)(context).Should().Be(false);
        tracker.ArgumentIsBoolConstant("c", true)(context).Should().Be(true);
        tracker.ArgumentIsBoolConstant("c", false)(context).Should().Be(false);
        tracker.ArgumentIsBoolConstant("nonExistingParameterName", true)(context).Should().Be(false);
        tracker.ArgumentIsBoolConstant("nonExistingParameterName", false)(context).Should().Be(false);
    }

    [TestMethod]
    public void ConstArgumentForParameter_VB()
    {
        var context = CreateContext<VB.InvocationExpressionSyntax>(TestInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
        var tracker = new VisualBasicInvocationTracker();

        tracker.ConstArgumentForParameter(context, "a").Should().BeNull();
        tracker.ConstArgumentForParameter(context, "b").Should().Be("myConst");
        tracker.ConstArgumentForParameter(context, "c").Should().Be(true);
        tracker.ConstArgumentForParameter(context, "d").Should().Be(4);
        tracker.ConstArgumentForParameter(context, "e").Should().Be(5);
        tracker.ConstArgumentForParameter(context, "f").Should().BeNull();
        tracker.ConstArgumentForParameter(context, "nonExistingParameterName").Should().BeNull();
    }

    [TestMethod]
    public void ArgumentIsBoolConstant_VB()
    {
        var context = CreateContext<VB.InvocationExpressionSyntax>(TestInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
        var tracker = new VisualBasicInvocationTracker();

        tracker.ArgumentIsBoolConstant("a", true)(context).Should().Be(false);
        tracker.ArgumentIsBoolConstant("a", false)(context).Should().Be(false);
        tracker.ArgumentIsBoolConstant("c", true)(context).Should().Be(true);
        tracker.ArgumentIsBoolConstant("c", false)(context).Should().Be(false);
        tracker.ArgumentIsBoolConstant("nonExistingParameterName", true)(context).Should().Be(false);
        tracker.ArgumentIsBoolConstant("nonExistingParameterName", false)(context).Should().Be(false);
    }

    [TestMethod]
    public void ArgumentAtIndexIsStringConstant_CS()
    {
        var context = CreateContext<CS.InvocationExpressionSyntax>(TestInputCS, "MyMethod", AnalyzerLanguage.CSharp);
        var tracker = new CSharpInvocationTracker();
        tracker.ArgumentAtIndexIsStringConstant(0)(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsStringConstant(1)(context).Should().BeTrue();
        tracker.ArgumentAtIndexIsStringConstant(2)(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsStringConstant(3)(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsStringConstant(4)(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsStringConstant(42)(context).Should().BeFalse();
    }

    [TestMethod]
    public void ArgumentAtIndexIsStringConstant_VB()
    {
        var context = CreateContext<VB.InvocationExpressionSyntax>(TestInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
        var tracker = new VisualBasicInvocationTracker();
        tracker.ArgumentAtIndexIsStringConstant(0)(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsStringConstant(1)(context).Should().BeTrue();
        tracker.ArgumentAtIndexIsStringConstant(2)(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsStringConstant(3)(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsStringConstant(4)(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsStringConstant(5)(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsStringConstant(42)(context).Should().BeFalse();

        context = CreateContext<VB.InvocationExpressionSyntax>(TestInputVB, "NoArgs", AnalyzerLanguage.VisualBasic, 1);
        tracker.ArgumentAtIndexIsStringConstant(0)(context).Should().BeFalse();
    }

    [TestMethod]
    public void ArgumentAtIndexIsAny_CS()
    {
        var context = CreateContext<CS.InvocationExpressionSyntax>(TestInputCS, "MyMethod", AnalyzerLanguage.CSharp);
        var tracker = new CSharpInvocationTracker();
        tracker.ArgumentAtIndexIsAny(0, "myConst")(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsAny(1, "myConst")(context).Should().BeTrue();
        tracker.ArgumentAtIndexIsAny(1, "a", "b", "myConst")(context).Should().BeTrue();
        tracker.ArgumentAtIndexIsAny(1, "a", "b")(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsAny(2, "true")(context).Should().BeFalse();   // Not a string
        tracker.ArgumentAtIndexIsAny(42, "myConst")(context).Should().BeFalse();
    }

    [TestMethod]
    public void ArgumentAtIndexIsAny_VB()
    {
        var context = CreateContext<VB.InvocationExpressionSyntax>(TestInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
        var tracker = new VisualBasicInvocationTracker();
        tracker.ArgumentAtIndexIsAny(0, "myConst")(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsAny(1, "myConst")(context).Should().BeTrue();
        tracker.ArgumentAtIndexIsAny(1, "a", "b", "myConst")(context).Should().BeTrue();
        tracker.ArgumentAtIndexIsAny(1, "a", "b")(context).Should().BeFalse();
        tracker.ArgumentAtIndexIsAny(2, "true")(context).Should().BeFalse();   // Not a string
        tracker.ArgumentAtIndexIsAny(42, "myConst")(context).Should().BeFalse();

        context = CreateContext<VB.InvocationExpressionSyntax>(TestInputVB, "NoArgs", AnalyzerLanguage.VisualBasic, 1);
        tracker.ArgumentAtIndexIsAny(0, "myConst")(context).Should().BeFalse();
    }

    [TestMethod]
    public void InvocationConditionForUndefinedMethod()
    {
        var context = CreateContext<CS.InvocationExpressionSyntax>(TestInputCS, "Undefined", AnalyzerLanguage.CSharp, 1);
        var tracker = new CSharpInvocationTracker();
        tracker.MethodIsStatic()(context).Should().BeFalse();
        tracker.MethodIsExtension()(context).Should().BeFalse();
        tracker.MethodHasParameters(0)(context).Should().BeFalse();
        tracker.MethodReturnTypeIs(KnownType.Void)(context).Should().BeFalse();
    }

#if NET
    [TestMethod]
    [DataRow(0, false)]
    [DataRow(1, false)]
    [DataRow(2, false)]
    [DataRow(3, true)]
    public void IsIHeadersDictionary(int invocationsToSkip, bool expectedValue)
    {
        var context = CreateContext<CS.InvocationExpressionSyntax>(IsIHeadersDictionaryCode,
                                                                             "MethodName",
                                                                             AnalyzerLanguage.CSharp,
                                                                             invocationsToSkip,
                                                                             new[] { AspNetCoreMetadataReference.MicrosoftExtensionsPrimitives});
        var sut = new CSharpInvocationTracker();
        sut.IsIHeadersDictionary()(context).Should().Be(expectedValue);
    }
#endif

    private static InvocationContext CreateContext<TSyntaxNodeType>(string testInput,
                                                                    string methodName,
                                                                    AnalyzerLanguage language,
                                                                    int skip = 0,
                                                                    IEnumerable<MetadataReference> references = null)
        where TSyntaxNodeType : SyntaxNode
    {
        var testCode = new SnippetCompiler(testInput, true, language, references);
        var invocationSyntaxNode = testCode.GetNodes<TSyntaxNodeType>().Skip(skip).First();
        var context = new InvocationContext(invocationSyntaxNode, methodName, testCode.SemanticModel);
        return context;
    }
}
