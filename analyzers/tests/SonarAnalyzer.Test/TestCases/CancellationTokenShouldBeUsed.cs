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

using System;
using System.Threading;
using System.Threading.Tasks;

// Self-contained helper types for controlled testing
public class Dependency
{
    public void Do() { }
    public void Do(CancellationToken ct) { }

    public void Do(string s) { }
    public void Do(string s, CancellationToken ct) { }

    public void NoOverload() { }

    // Overload with optional param between original args and CT
    public void Do(string s, bool flag) { }
    public void Do(string s, bool flag, CancellationToken ct) { }

    // CT overload has incompatible return type (Task<int> vs Task<string>)
    public Task<int> ComputeAsync() => Task.FromResult(0);
    public Task<string> ComputeAsync(CancellationToken ct) => Task.FromResult(string.Empty);

    // Task-returning pair needed as expression in field/property/ctor initializers
    public Task DoAsync() => Task.CompletedTask;
    public Task DoAsync(CancellationToken ct) => Task.CompletedTask;

    // Generic pair: the CT overload's raw return type is an unresolved type parameter.
    // After speculative binding, type inference yields the concrete return type.
    public T Identity<T>(T value) => value;
    public T Identity<T>(T value, CancellationToken ct) => value;
}

public static class DependencyExtensions
{
    public static void Run(this Dependency d, string data) { }
    public static void Run(this Dependency d, string data, CancellationToken ct) { }
}

// ─── Basic cases: CancellationToken parameter ─────────────────────────────────

public class BasicParameterCases
{
    private readonly Dependency _dep = new Dependency();

    public void Noncompliant(CancellationToken token) // Secondary [nc1, nc2]
    //                                         ^^^^^
    {
        _dep.Do();    // Noncompliant [nc1] {{Pass the 'token' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
        _dep.Do("s"); // Noncompliant [nc2] {{Pass the 'token' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }

    public void AlreadyPassingToken(CancellationToken token)
    {
        _dep.Do(token);      // Compliant
        _dep.Do("s", token); // Compliant
    }

    public void NoneIsCompliant(CancellationToken token)
    {
        _dep.Do(CancellationToken.None); // Compliant — deliberate opt-out
    }

    public void NoTokenInScope()
    {
        _dep.Do();    // Compliant — no CancellationToken in scope
        _dep.Do("s"); // Compliant — no CancellationToken in scope
    }

    public void NoOverloadExists(CancellationToken token)
    {
        _dep.NoOverload(); // Compliant — no CancellationToken overload
    }

    public void ReturnTypeMismatch(CancellationToken token)
    {
        _ = _dep.ComputeAsync(); // Compliant — CT overload returns Task<string>, not assignable to Task<int>
    }
}

// ─── Optional parameter between original args and CT ─────────────────────────

public class OptionalParamCases
{
    private readonly Dependency _dep = new Dependency();

    public void Noncompliant(CancellationToken token) // Secondary
    {
        _dep.Do("s", false); // Noncompliant — Do(string, bool, CancellationToken) resolved via speculative binding
    }

    public void Compliant(CancellationToken token)
    {
        _dep.Do("s", false, token); // Compliant
    }
}

// ─── Extension methods ────────────────────────────────────────────────────────

public class ExtensionMethodCases
{
    private readonly Dependency _dep = new Dependency();

    public void Noncompliant(CancellationToken token) // Secondary
    {
        _dep.Run("data"); // Noncompliant
    }

    public void Compliant(CancellationToken token)
    {
        _dep.Run("data", token); // Compliant
    }
}

// ─── Verbatim identifier parameters (@this) ───────────────────────────────────
// CancellationToken/@this as a CT source must produce "@this" / "@this.Token" in the message,
// not the bare keyword "this" / "this.Token".

public class AtThisParameterCases
{
    private readonly Dependency _dep = new Dependency();

    public void NoncompliantCt(CancellationToken @this) // Secondary
    {
        _dep.Do(); // Noncompliant {{Pass the '@this' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }

    public void NoncompliantCts(CancellationTokenSource @this) // Secondary
    {
        _dep.Do(); // Noncompliant {{Pass the '@this.Token' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }

    public void Compliant(CancellationToken @this)
    {
        _dep.Do(@this); // Compliant
    }
}

// ─── Cache hit: same method called multiple times in one block ────────────────

public class CacheHitCases
{
    private readonly Dependency _dep = new Dependency();

    public void Method(CancellationToken token) // Secondary [nc3, nc4, nc5]
    {
        _dep.Do(); // Noncompliant [nc3] — cache miss
        _dep.Do(); // Noncompliant [nc4] — cache hit
        _dep.Do(); // Noncompliant [nc5] — cache hit
    }
}

// ─── Parameter CT preferred over member CT ────────────────────────────────────
// When both a method parameter and a member field/property provide a CT, the
// parameter is closer in scope and more idiomatic — suggest it, not the member.

public class ParameterPrecedenceOverMemberCases
{
    private readonly Dependency _dep = new Dependency();
    private CancellationToken _memberCt;

    public void Method(CancellationToken token) // Secondary
    {
        _dep.Do(); // Noncompliant {{Pass the 'token' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }
}

// ─── Non-CT parameter shadows CT member ──────────────────────────────────────
// A non-CT parameter with the same name as a CT field/property does NOT take
// precedence; FindParameterCtSource skips non-CT params, so the member is used.
// A CT parameter with the same name DOES take precedence; secondary points to it.

public class ShadowingFieldCases
{
    private readonly Dependency _dep = new Dependency();
    private CancellationToken _ct; // Secondary [shadow1]
    //                        ^^^

    public void NonCtParamShadowsCtField(int _ct)
    {
        _dep.Do(); // Noncompliant [shadow1] {{Pass the 'this._ct' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }

    public void CtParamShadowsCtField(CancellationToken _ct) // Secondary
    {
        _dep.Do(); // Noncompliant {{Pass the '_ct' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }
}

public class ShadowingPropertyCases
{
    private readonly Dependency _dep = new Dependency();
    public CancellationToken Ct { get; } // Secondary [shadow2]
    //                       ^^

    public void NonCtParamShadowsCtProperty(int Ct)
    {
        _dep.Do(); // Noncompliant [shadow2] {{Pass the 'this.Ct' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }

    public void CtParamShadowsCtProperty(CancellationToken Ct) // Secondary
    {
        _dep.Do(); // Noncompliant {{Pass the 'Ct' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }
}

// ─── Static context ───────────────────────────────────────────────────────────

public class StaticContextCases
{
    private static readonly Dependency _dep = new Dependency();
    private static CancellationToken s_staticCt; // Secondary [nc6, nc7]
    private CancellationToken _instanceCt;

    public static void StaticMethod()
    {
        _dep.Do(); // Noncompliant [nc6] — s_staticCt is accessible
    }

    public void InstanceMethod()
    {
        _dep.Do(); // Noncompliant [nc7] — _instanceCt or s_staticCt is accessible
    }
}

// ─── Static method, instance CT only ─────────────────────────────────────────
// When no static CT exists, a static method has no accessible CT — compliant.

public class StaticMethodInstanceCtOnlyCases
{
    private static readonly Dependency _dep = new Dependency();
    private CancellationToken _ct;

    public static void StaticMethod()
    {
        _dep.Do(); // Compliant — _ct is an instance field, not accessible in static method
    }
}

// ─── Delegate invocations: never flagged (no CT overloads on delegates) ────────

public class DelegateCases
{
    public void Method(CancellationToken token)
    {
        Action action = () => { };
        action(); // Compliant — delegate invocation, no CT overload
    }
}

// ─── Dynamic invocations: never flagged (symbol unresolved at compile time) ────
// GetSymbolInfo returns null for dynamic dispatch — the invocation expression
// does not resolve to an IMethodSymbol, so the analysis skips it entirely.

public class DynamicCases
{
    private readonly Dependency _dep = new Dependency();

    public void Method(CancellationToken token)
    {
        dynamic d = _dep;
        d.Do(); // Compliant — dynamic dispatch, no compile-time method symbol
    }
}

// ─── Local functions and lambdas ──────────────────────────────────────────────
// RegisterCodeBlockStartAction fires for the enclosing named-type member (outer
// method); the code block includes nested local functions and lambdas. So the
// outer method's CT source is used for all nested invocations.

public class LocalFunctionCases
{
    private readonly Dependency _dep = new Dependency();

    public void OuterHasToken(CancellationToken token) // Secondary [nc8, nc9, nc10]
    {
        _dep.Do(); // Noncompliant [nc8]

        void LocalFunction()
        {
            _dep.Do(); // Noncompliant [nc9] — outer method's code block includes local function body; 'token' is in scope
        }

        Action lambda = () =>
        {
            _dep.Do(); // Noncompliant [nc10] — outer method's code block includes lambda body; 'token' is in scope
        };
    }
}

// ─── Named CT arguments ───────────────────────────────────────────────────────
// ParameterLookup maps named arguments to their parameters regardless of call-site position.

public class NamedCtArgCases
{
    private readonly Dependency _dep = new Dependency();

    public void Compliant(CancellationToken token)
    {
        _dep.Do(s: "hello", ct: token); // Compliant — named real CT
        _dep.Do(ct: token, s: "hello"); // Compliant — reordered named real CT
    }

    public void Noncompliant(CancellationToken token) // Secondary
    {
        _dep.Do(ct: default(CancellationToken), s: "hello"); // Noncompliant {{Pass the 'token' instead of 'default' to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }
}

// ─── default(T) passed to non-CT argument ────────────────────────────────────
// The second check (method-has-CT-param AND arg-is-default(CT)) does not fire because
// GetTypeInfo(default(T)) returns a non-CT type. Speculative binding handles these instead.
// Note: untyped 'default' in these positions is CS0121-ambiguous (code doesn't compile), so
// the resulting FN is acceptable — the user must fix the ambiguity first.

public class DefaultForNonCtArgCases
{
    private readonly Dependency _dep = new Dependency();

    public void Method(CancellationToken token) // Secondary [nc11, nc12]
    {
        _dep.Do(default(string));    // Noncompliant [nc11] — speculative binding resolves Do(default(string), ct: token)
        _dep.Do("s", default(bool)); // Noncompliant [nc12] — speculative binding resolves Do("s", default(bool), ct: token)
    }
}

// ─── Field and property initializers ─────────────────────────────────────────
// RegisterCodeBlockStartAction does not fire for these (OwningSymbol is IFieldSymbol/IPropertySymbol,
// filtered by the IMethodSymbol check). FNs are acceptable.

public class InitializerNotAnalysedCases
{
    private static readonly Dependency s_dep = new Dependency();
    private static CancellationToken s_ct;

    private readonly Task _task = s_dep.DoAsync(); // FN — field initializer is not analysed
    public Task Task { get; } = s_dep.DoAsync();   // FN — property initializer is not analysed
}

// ─── Constructor initializers ─────────────────────────────────────────────────
// base()/this() arguments are skipped via span intersection to avoid FPs (instance CT inaccessible).
// Static CT would be passable but is treated as FN — acceptable.

public class CtorInitializerBase
{
    protected CtorInitializerBase(Task t) { }
}

public class CtorInitializerBaseCase : CtorInitializerBase
{
    private static readonly Dependency s_dep = new Dependency();
    private CancellationToken _ct; // instance — not accessible in base()

    public CtorInitializerBaseCase() : base(s_dep.DoAsync()) { } // FN — initializer skipped to avoid FP
}

public class CtorInitializerThisCase
{
    private static readonly Dependency s_dep = new Dependency();
    private CancellationToken _ct; // instance — not accessible in this()

    public CtorInitializerThisCase(Task t) { }

    public CtorInitializerThisCase() : this(s_dep.DoAsync()) { } // FN — initializer skipped to avoid FP
}

public class CtorInitializerStaticCtCase : CtorInitializerBase
{
    private static readonly Dependency s_dep = new Dependency();
    private static CancellationToken s_ct; // static — IS accessible but initializer is skipped entirely

    public CtorInitializerStaticCtCase() : base(s_dep.DoAsync()) { } // FN — acceptable
}

public class CtorInitializerBodyStillAnalysed : CtorInitializerBase
{
    private readonly Dependency _dep = new Dependency();
    private CancellationToken _ct; // Secondary

    public CtorInitializerBodyStillAnalysed() : base(Task.CompletedTask)
    {
        _dep.Do(); // Noncompliant — constructor body is still analysed
    }
}

// ─── CancellationToken.Register callback ─────────────────────────────────────
// True positive: TrySetCanceled() has a CT overload, and 'token' is in scope,
// so we raise. However our message "to allow cancellation of the operation" is
// misleading here — cancellation is already guaranteed by the outer Register.
// The real issue is that 'token' is not embedded in the resulting
// OperationCanceledException, so callers cannot filter on 'e.CancellationToken'.
// A better message would be: "Pass the '{0}' to embed it in the
// OperationCanceledException so callers can identify the cancellation source."
// The real fix avoids the closure capture entirely:
//   token.Register(static (state, ct) => ((TaskCompletionSource<int>)state!).TrySetCanceled(ct), tcs)

public class RegisterCallbackCases
{
    public void Noncompliant(CancellationToken token) // Secondary
    {
        var tcs = new TaskCompletionSource<int>();
        token.Register(() => tcs.TrySetCanceled()); // Noncompliant {{Pass the 'token' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
    }

    public void Compliant(CancellationToken token)
    {
        var tcs = new TaskCompletionSource<int>();
        token.Register(() => tcs.TrySetCanceled(token)); // Compliant
    }
}

// ─── Generic overloads: return type resolved via type inference ───────────────
// The CT overload's raw return type is an unresolved type parameter T.
// Pre-filtering overloads by return type before speculative binding would compare
// the raw T against the concrete call-site return type (e.g. string) and discard
// the overload as incompatible, producing a false negative.
// The post-binding check on the resolved symbol avoids this because type arguments
// have been inferred by then and the concrete return type is available.

public class GenericReturnTypeCases
{
    private readonly Dependency _dep = new Dependency();

    public void Noncompliant(CancellationToken token) // Secondary
    {
        _ = _dep.Identity("hello"); // Noncompliant
    }

    public void Compliant(CancellationToken token)
    {
        _ = _dep.Identity("hello", token); // Compliant
    }
}

// ─── Cross-method CT gap (FN — NET-4014 / NET-4003) ──────────────────────────
// The rule only sees CT sources in scope within the analysed method body.
// When Caller(CT ct) delegates to Inner(), and Inner() calls the dependency,
// the rule has no CT in scope inside Inner() and produces no diagnostic.
// Resolving this requires the intra-type call-graph utility planned under NET-4003.

public class CrossMethodFNCases
{
    private readonly Dependency _dep = new Dependency();

    public void Caller(CancellationToken ct) => Inner();

    private void Inner()
    {
        _dep.Do(); // FN — 'ct' is available in Caller() but not visible here
    }
}
