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
    // Next/indexer enable building '.'/'?.'/'[x]'/'?[x]' chains for NET-4157's conditional-access coverage.
    private const string SnippetPreamble = """
        using System.Threading;
        public class Dep
        {
            public void M() { }
            public void M(CancellationToken ct) { }
            public Dep Next => this;
            public Dep this[int i] => this;
        }
        """;

    // ─── IHostApplicationLifetime one-shot tokens excluded (NET-4059) ────────
    // No leading 'using' — would land after SnippetPreamble's 'using' with a namespace between (CS1529); System.Threading already covered.
    private const string HostLifetimePreamble = """
        namespace Microsoft.Extensions.Hosting
        {
            public interface IHostApplicationLifetime
            {
                CancellationToken ApplicationStarted { get; }
                CancellationToken ApplicationStopping { get; }
                CancellationToken ApplicationStopped { get; }
            }
        }
        """;

    // ─── NET-4109: readonly field / get-only property fixed to new CancellationToken(true)/new(true) is not suggested ──
    private static readonly (string Initializer, bool Noncompliant)[] FixedCancelledTokenValues =
    [
        (null, true),                                // no initializer — live source, still Noncompliant
        ("default", true),
        ("default(CancellationToken)", true),
        ("new(false)", true),                        // false is behaviorally identical to None/default — not excluded
        ("new CancellationToken(false)", true),
        ("new(true)", false),                        // fixed, always-cancelled sentinel — excluded
        ("new CancellationToken(true)", false),
        ("CancellationToken.None", true),
    ];

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
                        _dep.M(); // Noncompliant {{Pass the '{{{expectedSource}}}' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
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
                        _dep.M(); // Noncompliant {{Pass the '{{{expectedSource}}}' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
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
                        M(); // Noncompliant {{Pass the '{{{expectedSource}}}' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
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

    public static IEnumerable<object[]> FixedCancelledTokenMemberData() =>
        from isField in new[] { true, false }
        from value in FixedCancelledTokenValues
        select new object[] { isField, value.Initializer, value.Noncompliant };

    [TestMethod]
    [DynamicData(nameof(FixedCancelledTokenMemberData))]
    public void CancellationTokenShouldBeUsed_FixedCancelledToken_Member(bool isField, string initializer, bool noncompliant)
    {
        var member = isField
            ? $"readonly CancellationToken _ct{(initializer is null ? string.Empty : $" = {initializer}")};"
            : $"CancellationToken Ct {{ get; }}{(initializer is null ? string.Empty : $" = {initializer};")}";
        var builder = builderLatest.AddSnippet($$"""
            {{SnippetPreamble}}
            public class C
            {
                Dep _dep = new Dep();
                {{member}} {{(noncompliant ? "// Secondary" : string.Empty)}}
                public void M()
                {
                    _dep.M(); {{(noncompliant ? "// Noncompliant" : string.Empty)}}
                }
            }
            """);
        if (noncompliant)
        {
            builder.Verify();
        }
        else
        {
            builder.VerifyNoIssues();
        }
    }

    // ─── NET-4109: expression-bodied property fixed to new CancellationToken(true)/new(true) is not suggested ──
    // No "no initializer" variant — an expression-bodied property always has a body.
    [TestMethod]
    [DataRow(" => default;", true)]
    [DataRow(" => default(CancellationToken);", true)]
    [DataRow(" => new(false);", true)]
    [DataRow(" => new CancellationToken(false);", true)]
    [DataRow(" => new(true);", false)]
    [DataRow(" => new CancellationToken(true);", false)]
    [DataRow(" => CancellationToken.None;", true)]
    public void CancellationTokenShouldBeUsed_FixedCancelledToken_ExpressionBodiedProperty(string propertyTail, bool noncompliant)
    {
        var builder = builderLatest.AddSnippet($$"""
            {{SnippetPreamble}}
            public class C
            {
                Dep _dep = new Dep();
                CancellationToken Ct{{propertyTail}} {{(noncompliant ? "// Secondary" : string.Empty)}}
                public void M()
                {
                    _dep.M(); {{(noncompliant ? "// Noncompliant" : string.Empty)}}
                }
            }
            """);
        if (noncompliant)
        {
            builder.Verify();
        }
        else
        {
            builder.VerifyNoIssues();
        }
    }

    // ─── NET-4109: mutable members fixed to new(true) are still Noncompliant — exclusion needs a readonly/get-only member.
    [TestMethod]
    [DataRow("CancellationToken _ct = new(true);")]                             // non-readonly field
    [DataRow("CancellationToken _ct = new CancellationToken(true);")]
    [DataRow("CancellationToken Ct { get; set; } = new(true);")]                // auto-property with setter
    [DataRow("CancellationToken Ct { get { return new(true); } }")]             // get-only via accessor body, not an Initializer/ExpressionBody
    [DataRow("CancellationToken Ct { get { return new(true); } set { } }")]     // explicit get/set accessor bodies
    [DataRow("CancellationToken Ct { get; set => field = value; } = new(true);")] // C# 14 semi-auto property — field-keyword setter still makes it mutable
    [DataRow("static CancellationToken _ct = new(true);")]                       // static, non-readonly field
    [DataRow("static CancellationToken Ct { get; set; } = new(true);")]          // static property with setter
    public void CancellationTokenShouldBeUsed_FixedCancelledToken_NotExcludedForMutableMembers(string memberDeclaration) =>
        builderLatest
            .AddSnippet($$"""
                {{SnippetPreamble}}
                public class C
                {
                    Dep _dep = new Dep();
                    {{memberDeclaration}} // Secondary
                    public void M()
                    {
                        _dep.M(); // Noncompliant
                    }
                }
                """)
            .Verify();

    [TestMethod]
    [DataRow("Microsoft.Extensions.Hosting.IHostApplicationLifetime")]
    [DataRow("ApplicationLifetime")]
    public void CancellationTokenShouldBeUsed_HostApplicationLifetime_MemberSource(string lifetimeType) =>
        builderLatest
            .AddSnippet($$$"""
                {{{SnippetPreamble}}}
                {{{HostLifetimePreamble}}}
                public class ApplicationLifetime : Microsoft.Extensions.Hosting.IHostApplicationLifetime
                {
                    public CancellationToken ApplicationStarted { get; }
                    public CancellationToken ApplicationStopping { get; }
                    public CancellationToken ApplicationStopped { get; }
                }
                public class C
                {
                    Dep _dep = new Dep();
                    {{{lifetimeType}}} Lifetime { get; } // Secondary
                    public void M()
                    {
                        _dep.M(); // Noncompliant {{Pass the 'this.Lifetime.ApplicationStopping' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
                    }
                }
                """)
            .Verify();

    [TestMethod]
    public void CancellationTokenShouldBeUsed_HostApplicationLifetime_ParamSource() =>
        builderLatest
            .AddSnippet($$$"""
                {{{SnippetPreamble}}}
                {{{HostLifetimePreamble}}}
                public class C
                {
                    Dep _dep = new Dep();
                    public void M(Microsoft.Extensions.Hosting.IHostApplicationLifetime lifetime) // Secondary
                    {
                        _dep.M(); // Noncompliant {{Pass the 'lifetime.ApplicationStopping' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
                    }
                }
                """)
            .Verify();

    // No eligible CT member remains once the one-shot tokens are excluded - must not fall back to them.
    [TestMethod]
    public void CancellationTokenShouldBeUsed_HostApplicationLifetime_OnlyOneShotTokens_Compliant() =>
        builderLatest
            .AddSnippet($$"""
                {{SnippetPreamble}}
                namespace Microsoft.Extensions.Hosting
                {
                    public interface IHostApplicationLifetime
                    {
                        CancellationToken ApplicationStarted { get; }
                        CancellationToken ApplicationStopped { get; }
                    }
                }
                public class C
                {
                    Dep _dep = new Dep();
                    Microsoft.Extensions.Hosting.IHostApplicationLifetime Lifetime { get; }
                    public void M()
                    {
                        _dep.M();
                    }
                }
                """)
            .VerifyNoIssues();

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

    // ─── NET-4157: conditional-access call site used to crash CanSpeculativelyPassCt (AD0001, see roslyn#25262) ──
    // Covers the call sitting anywhere on a '.'/'?.'/'[x]'/'?[x]' chain's conditional-access spine.
    [TestMethod]
    [DataRow("_dep?.M()")]
    [DataRow("_dep?.Next.M()")]
    [DataRow("_dep?.Next?.M()")]
    [DataRow("_dep.Next?.M()")]
    [DataRow("_dep?[0].M()")]
    [DataRow("_dep?[0]?.M()")]
    [DataRow("_dep[0]?.M()")]
    [DataRow("_dep?.Next[0]?.M()")]
    [DataRow("_dep?.Next?.Next.M()")]
    [DataRow("_dep?.Next.Next?.M()")]
    [DataRow("_dep?[0]?.Next?.M()")]
    public void CancellationTokenShouldBeUsed_ConditionalAccessInvocation(string call) =>
        builderLatest
            .AddSnippet($$$"""
                {{{SnippetPreamble}}}
                public class C
                {
                    Dep _dep = new Dep();
                    public void M(CancellationToken token) // Secondary
                    {
                        {{{call}}}; // Noncompliant {{Pass the 'token' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
                    }
                }
                """)
            .Verify();
}
