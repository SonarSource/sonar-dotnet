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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class Dep
{
    public void M() { }
    public void M(CancellationToken ct) { }
}


// ─── Speculative binding validation ──────────────────────────────────────────
// The candidate cache finds a CT-accepting overload with a compatible return type,
// but speculative binding verifies whether the overload actually resolves at the
// call site given the existing arguments.

public class SpeculativeBindingService
{
    // default parameter between original args and CT: named arg can skip verbose
    public void M(string s) { }
    public void M(string s, bool verbose = false, CancellationToken ct = default) { }

    // CT overload has a required non-CT param not present at the call site
    public void N(string s) { }
    public void N(string s, int required, CancellationToken ct) { }

    // byte→int is implicit: CT overload accepts int, call passes byte
    public void O(byte b) { }
    public void O(int i, CancellationToken ct) { }

    // no string→byte: CT overload accepts byte, call passes string
    public void P(string s) { }
    public void P(byte b, CancellationToken ct) { }

    // generic CT overload: T is inferred from the non-CT argument
    public void Q(string s) { }
    public void Q<T>(T s, CancellationToken ct) { }
}

public class SpeculativeBindingCases
{
    SpeculativeBindingService _svc = new SpeculativeBindingService();

    public void Tests(CancellationToken token) // Secondary [l1, l2, l3]
    {
        _svc.M("hello"); // Noncompliant [l1] — M("hello", ct: token) resolves; verbose uses its default

        _svc.N("hello"); // Compliant — N("hello", ct: token) leaves 'required' unbound

        byte b = 5;
        _svc.O(b);       // Noncompliant [l2] — O(b, ct: token) resolves via byte→int implicit conversion

        _svc.P("hello"); // Compliant — P("hello", ct: token) cannot bind; no string→byte conversion

        _svc.Q("test");  // Noncompliant [l3] — Q<string>("test", ct: token) resolves with T inferred as string
    }
}

// ─── C# 14 extension blocks ──────────────────────────────────────────────────
// Extension members defined in extension blocks should be handled the same way
// as traditional extension methods.

public class ExtTarget { }

public static class ExtTargetExtensions
{
    extension(ExtTarget t)
    {
        public void ExtM() { }
        public void ExtM(CancellationToken ct) { }
    }
}

public class ExtensionBlockCases
{
    public void Method(CancellationToken token) // Secondary
    {
        var t = new ExtTarget();
        t.ExtM(); // Noncompliant
        t.ExtM(token); // Compliant
    }
}

// ─── Static local functions (C# 8+) ──────────────────────────────────────────
// The outer method's code block includes the static local function body.
// Speculative binding resolves 'token' from the enclosing scope, which is a
// known limitation: the static local cannot name 'token' directly.

public class StaticLocalFunctionCases
{
    public void Method(CancellationToken token) // Secondary [l4, l5]
    {
        var dep = new Dep();
        dep.M(); // Noncompliant [l4]

        static void StaticLocal()
        {
            new Dep().M(); // Noncompliant [l5] — speculative binding resolves 'token' even inside static local; known limitation
        }
    }
}

// ─── Producer call already configured via .WithCancellation(...)/.ConfigureAwait(...) — S9426 territory, not S8949's ──

public class AlreadyConfiguredWithCancellationCases
{
    public static async IAsyncEnumerable<int> GetItemsAsync([EnumeratorCancellation] CancellationToken token = default) { yield return 1; await Task.Yield(); }
    public static async IAsyncEnumerable<int> GetItemsAsyncUndecorated(CancellationToken token = default) { yield return 1; await Task.Yield(); } // Error [CS8425]

    public async Task Compliant_WithCancellation(CancellationToken token)
    {
        await foreach (var item in GetItemsAsync().WithCancellation(token)) // Compliant — WithCancellation already wires cancellation, S8949 must not also suggest passing the token directly
        {
        }
    }

    public async Task Compliant_WithCancellationThenConfigureAwait(CancellationToken token)
    {
        await foreach (var item in GetItemsAsync().WithCancellation(token).ConfigureAwait(false)) // Compliant — WithCancellation anywhere in the chain is enough
        {
        }
    }

    public async Task Compliant_ConfigureAwaitThenWithCancellation(CancellationToken token)
    {
        await foreach (var item in GetItemsAsync().ConfigureAwait(false).WithCancellation(token)) // Compliant
        {
        }
    }

    public async Task Noncompliant_UndecoratedProducer_WithCancellationDoesNotWireAnything(CancellationToken token) // Secondary
    {
        // CS8425 on the declaration is a different signal at a different location; WithCancellation here is a confirmed no-op (probed at runtime).
        await foreach (var item in GetItemsAsyncUndecorated().WithCancellation(token)) // Noncompliant {{Pass the 'token' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
        {
        }
    }

    // A non-iterator (no 'yield') wrapper can never carry [EnumeratorCancellation] - its body runs eagerly, before any later WithCancellation.
    public static IAsyncEnumerable<int> AcquireStream(CancellationToken token = default) => GetItemsAsync(token);

    public async Task Noncompliant_NonIteratorWrapper_EagerBodyNeverSeesTheChainedToken(CancellationToken token) // Secondary
    {
        await foreach (var item in AcquireStream().WithCancellation(token)) // Noncompliant {{Pass the 'token' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
        {
        }
    }

    // Separate overloads, not a default parameter: the attribute lives on a sibling that never runs for this call - must check the resolved overload, not the method group.
    public static async IAsyncEnumerable<int> GetItemsOverloaded() { yield return 1; await Task.Yield(); }
    public static async IAsyncEnumerable<int> GetItemsOverloaded([EnumeratorCancellation] CancellationToken token) { yield return 1; await Task.Yield(); }

    public async Task Noncompliant_SeparateOverload_SiblingAttributeDoesNotApply(CancellationToken token) // Secondary
    {
        await foreach (var item in GetItemsOverloaded().WithCancellation(token)) // Noncompliant {{Pass the 'token' to this method to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.}}
        {
        }
    }

    public async Task Compliant_WithCancellationNone(CancellationToken token)
    {
        // Compliant — CancellationToken.None is the established explicit opt-out marker (see S8949's own message text), respected like any other explicit token
        await foreach (var item in GetItemsAsync().WithCancellation(CancellationToken.None))
        {
        }
    }

    public async Task Noncompliant_WithCancellationDefaultLiteral(CancellationToken token) // Secondary [l8, l9]
    {
        // 'default' does not override the [EnumeratorCancellation] parameter (the override only triggers for a non-default token),
        // so unlike CancellationToken.None it is not a deliberate opt-out - S8949 should still suggest the real token
        await foreach (var item in GetItemsAsync().WithCancellation(default))
        //                         ^^^^^^^^^^^^^ Noncompliant [l8]
        //                                         ^^^^^^^^^^^^^^^^ Noncompliant@-1 [l9]
        {
        }
    }

    public async Task Noncompliant_ConfigureAwaitOnly(CancellationToken token) // Secondary
    {
        // ConfigureAwait alone does not wire cancellation - unlike WithCancellation, it must not exclude S8949's suggestion
        await foreach (var item in GetItemsAsync().ConfigureAwait(false)) // Noncompliant
        {
        }
    }

    public void Compliant_ConditionalAccessWithCancellation(CancellationToken token)
    {
        GetItemsAsync()?.WithCancellation(token); // Compliant — '?.' must not defeat the already-configured check
    }

    public async Task Noncompliant_FP_VariableIndirectionNotDetected(CancellationToken token) // Secondary
    {
        // False positive: the chain walk only follows direct syntax on this expression, not a WithCancellation applied to a variable later - genuinely already configured here.
        var stream = GetItemsAsync(); // Noncompliant
        await foreach (var item in stream.WithCancellation(token))
        {
        }
    }

    public void Compliant_ToBlockingEnumerable(CancellationToken token)
    {
        foreach (var item in GetItemsAsync().ToBlockingEnumerable(token)) // Compliant — ToBlockingEnumerable also threads the token into GetAsyncEnumerator
        {
        }
    }

    public void Noncompliant_ToBlockingEnumerableWithoutArgument(CancellationToken token) // Secondary [l6, l7]
    {
        foreach (var item in GetItemsAsync().ToBlockingEnumerable())
        //                   ^^^^^^^^^^^^^ Noncompliant [l6]
        //                                   ^^^^^^^^^^^^^^^^^^^^ Noncompliant@-1 [l7]
        {
        }
    }
}
