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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CancellationTokenShouldBeUsedTest
{
    // Minimal dependency type included in every snippet to keep tests self-contained.
    private const string SnippetPreamble = """
        using System.Threading;
        public class Dep
        {
            public void M() { }
            public void M(CancellationToken ct) { }
        }
        """;

    private readonly VerifierBuilder builder = new VerifierBuilder<CancellationTokenShouldBeUsed>();

    private readonly VerifierBuilder builderLatest = new VerifierBuilder<CancellationTokenShouldBeUsed>()
        .WithOptions(LanguageOptions.CSharpLatest);

    [TestMethod]
    public void CancellationTokenShouldBeUsed_CS() =>
        builder.AddPaths("CancellationTokenShouldBeUsed.cs").Verify();

    [TestMethod]
    public void CancellationTokenShouldBeUsed_Latest_CS() =>
        builderLatest.AddPaths("CancellationTokenShouldBeUsed.Latest.cs").Verify();

    // ─── Noncompliant: member CT sources (field / property) ──────────────────
    // Secondary location points to the member declaration line of the snippet.
    [TestMethod]
    [DataRow("CancellationToken _ct;", "this._ct")]
    [DataRow("CancellationToken Ct { get; }", "this.Ct")]
    [DataRow("static CancellationToken s_ct;", "s_ct")]
    [DataRow("CancellationTokenSource _cts = new CancellationTokenSource();", "this._cts.Token")]
    [DataRow("CancellationTokenSource Cts { get; } = new CancellationTokenSource();", "this.Cts.Token")]
    // Nullable<CT> member sources
    [DataRow("CancellationToken? _nullableCt;", "this._nullableCt.GetValueOrDefault()")]
    [DataRow("CancellationToken? NullableCt { get; }", "this.NullableCt.GetValueOrDefault()")]
    [DataRow("static CancellationToken? s_nullableCt;", "s_nullableCt.GetValueOrDefault()")]
    // AsyncLocal<CT> member sources
    [DataRow("AsyncLocal<CancellationToken> _asyncLocalCt;", "this._asyncLocalCt.Value")]
    [DataRow("readonly AsyncLocal<CancellationToken> _asyncLocalCt;", "this._asyncLocalCt.Value")]
    [DataRow("AsyncLocal<CancellationToken> AsyncLocalCt { get; }", "this.AsyncLocalCt.Value")]
    [DataRow("static AsyncLocal<CancellationToken> s_asyncLocalCt;", "s_asyncLocalCt.Value")]
    [DataRow("static readonly AsyncLocal<CancellationToken> s_asyncLocalCt;", "s_asyncLocalCt.Value")]
    // ThreadLocal<CT> member sources
    [DataRow("ThreadLocal<CancellationToken> _threadLocalCt;", "this._threadLocalCt.Value")]
    [DataRow("readonly ThreadLocal<CancellationToken> _threadLocalCt;", "this._threadLocalCt.Value")]
    [DataRow("ThreadLocal<CancellationToken> ThreadLocalCt { get; }", "this.ThreadLocalCt.Value")]
    [DataRow("static ThreadLocal<CancellationToken> s_threadLocalCt;", "s_threadLocalCt.Value")]
    [DataRow("static readonly ThreadLocal<CancellationToken> s_threadLocalCt;", "s_threadLocalCt.Value")]
    public void CancellationTokenShouldBeUsed_Noncompliant_MemberSource(string memberDecl, string expectedSource) =>
        builderLatest
            .AddSnippet($$$"""
                {{{SnippetPreamble}}}
                public class C
                {
                    Dep _dep = new Dep();
                    {{{memberDecl}}} // Secondary
                    public void M()
                    {
                        _dep.M(); // Noncompliant {{Pass the '{{{expectedSource}}}' to this method to allow cancellation of the operation.}}
                    }
                }
                """)
            .Verify();

    // ─── Noncompliant: parameter CT sources ──────────────────────────────────
    // Secondary location points to the parameter declaration on the method signature line.
    [TestMethod]
    [DataRow("CancellationToken token", "token")]
    [DataRow("CancellationTokenSource cts", "cts.Token")]
    // Nullable<CT> parameter sources
    [DataRow("CancellationToken? token", "token.GetValueOrDefault()")]
    // AsyncLocal<CT> and ThreadLocal<CT> parameter sources
    [DataRow("AsyncLocal<CancellationToken> asyncLocalCt", "asyncLocalCt.Value")]
    [DataRow("ThreadLocal<CancellationToken> threadLocalCt", "threadLocalCt.Value")]
    public void CancellationTokenShouldBeUsed_Noncompliant_ParamSource(string methodParams, string expectedSource) =>
        builderLatest
            .AddSnippet($$$"""
                {{{SnippetPreamble}}}
                public class C
                {
                    Dep _dep = new Dep();
                    public void M({{{methodParams}}}) // Secondary
                    {
                        _dep.M(); // Noncompliant {{Pass the '{{{expectedSource}}}' to this method to allow cancellation of the operation.}}
                    }
                }
                """)
            .Verify();

    // ─── Noncompliant: default / default(CancellationToken) as CT argument ────
    [TestMethod]
    [DataRow("_dep.M(default(CancellationToken))")]
    [DataRow("_dep.M(default)")]
    [DataRow("_dep.M(ct: default(CancellationToken))")]
    [DataRow("_dep.M(ct: default)")]
    public void CancellationTokenShouldBeUsed_DefaultTokenIsNoncompliant(string call) =>
        builderLatest
            .AddSnippet($$$"""
                {{{SnippetPreamble}}}
                public class C
                {
                    Dep _dep = new Dep();
                    public void M(CancellationToken token) // Secondary
                    {
                        {{{call}}}; // Noncompliant {{Pass the 'token' instead of 'default' to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
                    }
                }
                """)
            .Verify();

    // ─── Noncompliant: default passed to a CancellationToken? overload ───────
    // The 'already-passing CT' guard must match CT? sinks too; without it the
    // guard was skipped and speculative binding added a duplicate arg → FN.
    [TestMethod]
    [DataRow("_dep.M(ct: default(CancellationToken))")]
    [DataRow("_dep.M(ct: default)")]
    public void CancellationTokenShouldBeUsed_DefaultAgainstNullableSink(string call) =>
        builderLatest
            .AddSnippet($$$"""
                using System.Threading;
                public class Dep
                {
                    public void M() { }
                    public void M(CancellationToken? ct) { }
                }
                public class C
                {
                    Dep _dep = new Dep();
                    public void M(CancellationToken token) // Secondary
                    {
                        {{{call}}}; // Noncompliant {{Pass the 'token' instead of 'default' to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
                    }
                }
                """)
            .Verify();

    // ─── Noncompliant: CT × Nullable<CT> source/sink matrix ──────────────────
    // Covers both CT? shorthand and Nullable<CT> explicit syntax for source and sink overload.
    [TestMethod]
    [DataRow("CancellationToken ct", "CancellationToken token", "token")]                                         // CT  sink — CT  source (baseline)
    [DataRow("CancellationToken? ct", "CancellationToken token", "token")]                                        // CT  sink — CT? source shorthand
    [DataRow("Nullable<CancellationToken> ct", "CancellationToken token", "token")]                               // CT  sink — CT? source explicit
    [DataRow("CancellationToken ct", "CancellationToken? token", "token.GetValueOrDefault()")]                    // CT? sink — CT  source shorthand
    [DataRow("CancellationToken ct", "Nullable<CancellationToken> token", "token.GetValueOrDefault()")]           // CT? sink — CT  source explicit
    [DataRow("CancellationToken? ct", "CancellationToken? token", "token.GetValueOrDefault()")]                   // CT? sink — CT? source both shorthand
    [DataRow("Nullable<CancellationToken> ct", "Nullable<CancellationToken> token", "token.GetValueOrDefault()")] // CT? sink — CT? source both explicit
    public void CancellationTokenShouldBeUsed_NullableCt(string overloadParam, string callerParam, string expectedSource) =>
        builder
            .AddSnippet($$$"""
                using System;
                using System.Threading;
                public class Sample
                {
                    public void M() { }
                    public void M({{{overloadParam}}}) { }
                    public void Caller({{{callerParam}}}) // Secondary
                    {
                        M(); // Noncompliant {{Pass the '{{{expectedSource}}}' to this method to allow cancellation of the operation.}}
                    }
                }
                """)
            .Verify();

    // ─── Unsupported wrapper types ────────────────────────────────────────────
    // Compliant — intentionally excluded; accessing .Result on tasks blocks the thread
    [TestMethod]
    [DataRow("System.Threading.Tasks.Task<CancellationToken> taskCt")]
    [DataRow("System.Threading.Tasks.ValueTask<CancellationToken> vtaskCt")]
    [DataRow("System.Lazy<CancellationToken> lazyCt")]                        // .Value triggers factory, may throw
    // FN — not targeted; niche, no evidence of real-world usage as parameter or member
    [DataRow("System.Tuple<CancellationToken, string> tupleCt")]
    [DataRow("(CancellationToken, string) valueTupleCt")]
    [DataRow("(CancellationToken Token, string Name) namedTupleCt")]
    [DataRow("System.Collections.Generic.KeyValuePair<string, CancellationToken> kvpCt")]
    [DataRow("ActionResult<CancellationToken> result")]
    [DataRow("Either<string, CancellationToken> either")]
    [DataRow("Either<CancellationToken, string> either")]
    public void CancellationTokenShouldBeUsed_WrapperType_NotSupported(string methodParam) =>
        builderLatest
            .AddSnippet($$"""
                {{SnippetPreamble}}
                public class ActionResult<T>(T Value);
                public class Either<TLeft, TRight>(TLeft Left, TRight Right);
                public class Sample
                {
                    private readonly Dep _dep = new();
                    public void M({{methodParam}})
                    {
                        _dep.M();
                    }
                }
                """)
            .VerifyNoIssues();

    // ─── MSTest TestContext.CancellationToken (RSPEC example) ────────────────
    [TestMethod]
    public void CancellationTokenShouldBeUsed_MSTestTestContext() =>
        builderLatest
            .AddReferences([
                ..NuGetMetadataReference.MSTestTestFramework(TestConstants.NuGetLatestVersion),
                ..MetadataReferenceFacade.SystemNetHttp])
            .AddSnippet("""
                using System.Net.Http;
                using System.Threading.Tasks;
                using Microsoft.VisualStudio.TestTools.UnitTesting;

                [TestClass]
                public class MyTests
                {
                    private readonly HttpClient httpClient = new();

                    public TestContext TestContext { get; set; } // Secondary

                    [TestMethod]
                    public async Task MyTest()
                    {
                        await httpClient.GetStringAsync("https://example.com"); // Noncompliant
                    }
                }
                """)
            .Verify();

    // ─── FN: locals as CT sources (NET-4014) ─────────────────────────────────
    // The rule only sees CT parameters and type members. All local variable kinds
    // are FNs — 'ct' is declared but not recognised as a CT source.
    [TestMethod]
    [DataRow("CancellationToken ct = default;")]
    [DataRow("var ct = new CancellationToken();")]
    [DataRow("CancellationToken ct = new();")]
    [DataRow("var (ct, _) = (default(CancellationToken), 0);")]
    [DataRow("_ = (object)default(CancellationToken) is CancellationToken ct;")]
    [DataRow("foreach (var ct in new CancellationToken[0])")]
    public void CancellationTokenShouldBeUsed_LocalCtSource_FN(string localDecl) =>
        builderLatest
            .AddSnippet($$"""
                {{SnippetPreamble}}
                public class Sample
                {
                    private readonly Dep _dep = new();
                    public void M()
                    {
                        {{localDecl}}
                        _dep.M(); // FN — locals not supported as CT source
                    }
                }
                """)
            .VerifyNoIssues();
}
