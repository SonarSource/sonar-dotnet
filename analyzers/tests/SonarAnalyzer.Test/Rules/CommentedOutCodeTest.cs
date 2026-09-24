/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CommentedOutCodeTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<CommentedOutCode>();

    [TestMethod]
    public void CommentedOutCode_Nonconcurrent() =>
        builder.AddPaths("CommentedOutCode_Nonconcurrent.cs").WithConcurrentAnalysis(false).Verify();

    [TestMethod]
    public void CommentedOutCode_CS() =>
        builder.AddPaths("CommentedOutCode.cs").Verify();

    [TestMethod]
    public void CommentedOutCode_NoDocumentation() =>
        builder.AddPaths("CommentedOutCode.cs")
            .WithConcurrentAnalysis(false)
            .WithOptions(ImmutableArray.Create<ParseOptions>(new CSharpParseOptions(documentationMode: DocumentationMode.None)))
            .Verify();

    [TestMethod]
    public void CommentedOutCode_CodeFix_SingleLine() =>
        builder.AddPaths("CommentedOutCode.SingleLine.ToFix.cs")
            .WithCodeFix<CommentedOutCodeCodeFix>()
            .WithCodeFixedPaths("CommentedOutCode.SingleLine.Fixed.cs")
            .VerifyCodeFix();

    [TestMethod]
    public void CommentedOutCode_CodeFix_MultiLine() =>
       builder.AddPaths("CommentedOutCode.MultiLine.ToFix.cs")
        .WithCodeFix<CommentedOutCodeCodeFix>()
        .WithCodeFixedPaths("CommentedOutCode.MultiLine.Fixed.cs")
        .VerifyCodeFix();

    [TestMethod]
    [DataRow("TODO")]
    [DataRow("FIXME   ")]
    [DataRow("TODO: x = 1;")]
    [DataRow("fixme x = 1;")]
    [DataRow(" * HACK: x = 1;")]
    [DataRow(" // XXX: x = 1;")]
    [DataRow("[alice] TODO: x = 1;")]
    [DataRow("Note: XXX x = 1;")]
    [DataRow("2026-09-17 Victor - TODO: x = 1;")]
    [DataRow("[a.contributor.with.a.long.name] 2026-09-17 - TODO: x = 1;")]
    public void IsCode_TaskCommentsAreExcluded(string line) =>
        CommentedOutCode.IsCode(line).Should().BeFalse();

    [TestMethod]
    [DataRow("var message = \"TODO: retry\";")]
    [DataRow("name = \"xxx\";")]
    [DataRow("hack = ComputeHack();")]
    [DataRow("HACK += ComputeHack();")]
    [DataRow("TODO();")]
    [DataRow("FIXME.Run();")]
    [DataRow("return TODO;")]
    [DataRow("int TODO = 1;")]
    [DataRow("XXX[0] = 1;")]
    [DataRow("TODO<T>();")]
    [DataRow("if (ready) { TODO")]
    public void IsCode_TaskTagsInsideCodeAreDetected(string line) =>
        CommentedOutCode.IsCode(line).Should().BeTrue();

    [TestMethod]
    [DataRow("printing returns", 0)]
    [DataRow("INT RETURN", 0)]
    [DataRow("get set from where select scoped", 0)]
    [DataRow("int; int.x int[x] int<T>", 0)]
    [DataRow("int\nreturn", 0)]
    [DataRow("int x;", 1)]
    [DataRow("return result", 1)]
    [DataRow("int Method(int i)", 2)]
    [DataRow("var async await yield nameof nint nuint notnull", 8)]
    public void StrongKeywords_CountMatches(string line, int expected) =>
        VerifyCountMatches(CommentedOutCode.StrongKeywords, line, expected);

    [TestMethod]
    [DataRow("float++", 0)]
    [DataRow("i++;", 0)]
    [DataRow("a ++ b", 1)]
    public void Increment_CountMatches(string line, int expected) =>
        VerifyCountMatches(CommentedOutCode.Increment, line, expected);

    [TestMethod]
    [DataRow("a == b", 0)]
    [DataRow("a != b", 0)]
    [DataRow("a <= b", 0)]
    [DataRow("a >= b", 0)]
    [DataRow("a => b", 0)]
    [DataRow("a <>= b", 0)]
    [DataRow("(?<key>([^=\\s\\p{Cc}]|\\s+==|==)+)", 0)]
    [DataRow("nothing here", 0)]
    [DataRow("flag = true;", 1)]
    [DataRow("buffer[position&mask]=current=lookup[index];", 1)]
    [DataRow("k=2, n=5 (2^5=32)", 1)]
    [DataRow("a += b", 1)]
    [DataRow("a -= b", 1)]
    [DataRow("a *= b", 1)]
    [DataRow("a /= b", 1)]
    [DataRow("a %= b", 1)]
    [DataRow("a &= b", 1)]
    [DataRow("a |= b", 1)]
    [DataRow("a ^= b", 1)]
    [DataRow("a <<= b", 1)]
    [DataRow("a >>= b", 1)]
    [DataRow("a >>>= b", 1)]
    [DataRow("a = b = c", 1)]
    [DataRow("flags ^= Mask;", 1)]
    [DataRow("buffer[^1] = value;", 1)]
    public void Assignment_CountMatches(string line, int expected) =>
        VerifyCountMatches(CommentedOutCode.Assignment, line, expected);

    [TestMethod]
    [DataRow("if(x)", 0)]
    [DataRow("while(x)", 0)]
    [DataRow("Handler.Notify (a)", 0)]
    [DataRow("Foo(", 1)]
    [DataRow("Foo<T>(", 1)]
    [DataRow("Handler.Notify(a, b, c);", 1)]
    [DataRow("try_catch(x)", 1)]
    [DataRow("value2_switch(x)", 1)]
    [DataRow("if(Foo(x))", 1)]
    [DataRow("\u00e9(\u0664(x))", 2)]
    [DataRow("Foo(Bar(x), Baz(y))", 3)]
    public void Call_CountMatches(string line, int expected) =>
        VerifyCountMatches(CommentedOutCode.Call, line, expected);

    [TestMethod]
    [DataRow("{ \"json\": \"fragment\" }", 0)]
    [DataRow("plain prose", 0)]
    [DataRow("}", 1)]
    [DataRow("if (x) {", 1)]
    [DataRow("DoWork(); }", 1)]
    [DataRow("{}", 1)]
    public void BlockBoundaryAtLineEnd_CountMatches(string line, int expected) =>
        VerifyCountMatches(CommentedOutCode.BlockBoundaryAtLineEnd, line, expected);

    [TestMethod]
    [DataRow("=====================", 0)]
    [DataRow("a === b", 0)]
    [DataRow("= = = =", 0)]
    [DataRow("plain prose", 0)]
    [DataRow("a == b", 1)]
    [DataRow("a != b || c <= d", 2)]
    [DataRow("a ! = b <\t= c", 2)]
    public void Comparison_CountMatches(string line, int expected) =>
        VerifyCountMatches(CommentedOutCode.Comparison, line, expected);

    [TestMethod]
    [DataRow("return Wrap(x) ? if_(y) : z", 0)]
    [DataRow("wrapped in a catch (Exception) so the caller never sees it", 0)]
    [DataRow("returning (x)", 0)]
    [DataRow("returning if(x)", 0)]
    [DataRow("if (ready) {", 1)]
    [DataRow("} else if (ready) {", 1)]
    [DataRow("* if (ready) {", 1)]
    [DataRow("if (x) { if (y)", 1)]
    [DataRow("* i f\t(x)", 1)]
    [DataRow("try { if (ready) {", 2)]
    [DataRow("try{else{try{", 3)]
    public void ControlFlowStart_CountMatches(string line, int expected) =>
        VerifyCountMatches(CommentedOutCode.ControlFlowStart, line, expected);

    [TestMethod]
    [DataRow("", 0.0)]
    [DataRow("ordinary prose", 0.0)]
    [DataRow("intint", 0.0)]
    [DataRow("int", 0.35)]
    [DataRow("int int", 0.5775)]
    [DataRow("x = y = z", 0.60)]
    [DataRow("int x;", 0.935)]
    [DataRow(" \t}\u00a0", 0.95)]
    [DataRow("x = 1;", 0.96)]
    public void CalculateCodeScore_CombinesAndAccumulatesEvidence(string line, double expected) =>
        CommentedOutCode.CalculateCodeScore(line, CommentedOutCode.RemoveWhitespace(line)).Should().BeApproximately(expected, 0.0000000001);

    [TestMethod]
    [DataRow("&&&& ||||", 0)]
    [DataRow("a&&b a||b a??b", 0)]
    [DataRow("&& || ??", 3)]
    public void LogicalOperators_CountMatches(string line, int expected) =>
        VerifyCountMatches(CommentedOutCode.LogicalOperators, line, expected);

    [TestMethod]
    [DataRow("statement; */", 0)]
    [DataRow("statement; **", 0)]
    [DataRow("statement; more text", 0)]
    [DataRow("statement;", 1)]
    [DataRow("statement; \t\r\n", 1)]
    public void TrailingSemicolon_CountMatches(string line, int expected) =>
        VerifyCountMatches(CommentedOutCode.TrailingSemicolon, line, expected);

    private static void VerifyCountMatches(CommentedOutCode.Detector detector, string line, int expected) =>
        CommentedOutCode.CountMatches(detector, line, CommentedOutCode.RemoveWhitespace(line)).Should().Be(expected);
}
