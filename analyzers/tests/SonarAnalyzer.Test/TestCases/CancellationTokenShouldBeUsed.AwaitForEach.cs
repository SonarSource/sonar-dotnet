using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class Producer
{
    public static async IAsyncEnumerable<int> GetItemsAsync() { yield return 1; await Task.Yield(); }
}

// ─── Basic cases: CancellationToken parameter ─────────────────────────────────

public class BasicParameterCases
{
    public async Task Noncompliant(CancellationToken token) // Secondary
    {
        await foreach (var item in Producer.GetItemsAsync()) // Noncompliant {{Pass the 'token' to this 'await foreach' via 'WithCancellation' to allow cancellation of the operation.}}
        {
        }
    }

    public async Task Compliant(CancellationToken token)
    {
        await foreach (var item in Producer.GetItemsAsync().WithCancellation(token)) // Compliant
        {
        }
    }

    public async Task NoTokenInScope()
    {
        await foreach (var item in Producer.GetItemsAsync()) // Compliant — no CancellationToken in scope
        {
        }
    }

    public async Task AlreadyUsingWithCancellation_DifferentToken(CancellationToken token)
    {
        var other = CancellationToken.None;
        await foreach (var item in Producer.GetItemsAsync().WithCancellation(other)) // Compliant — already using WithCancellation, we don't second-guess which token is "more correct"
        {
        }
    }

    public async Task Noncompliant_WithCancellationDefaultLiteral(CancellationToken token) // Secondary
    {
        await foreach (var item in Producer.GetItemsAsync().WithCancellation(default)) // Noncompliant — 'default' is not a deliberate opt-out, unlike 'CancellationToken.None'
        {
        }
    }

    public async Task Compliant_WithCancellationNone(CancellationToken token)
    {
        await foreach (var item in Producer.GetItemsAsync().WithCancellation(CancellationToken.None)) // Compliant — explicit 'CancellationToken.None' is a deliberate opt-out
        {
        }
    }

    public async Task Noncompliant_LocalVariableSource(CancellationToken token) // Secondary
    {
        var producer = Producer.GetItemsAsync();
        await foreach (var item in producer) // Noncompliant — WithCancellation is an extension method, it can still be called on the variable directly
        {
        }
    }

    public async Task Compliant_LocalVariableSourceWithCancellation(CancellationToken token)
    {
        var producer = Producer.GetItemsAsync();
        await foreach (var item in producer.WithCancellation(token)) // Compliant
        {
        }
    }

    public async Task Noncompliant_UnmodifiedParameterSource(IAsyncEnumerable<int> items, CancellationToken token) // Secondary
    {
        await foreach (var item in items) // Noncompliant — 'items' crossed a call boundary, there's no local evidence it's already configured
        {
        }
    }
}

// ─── Parenthesized source expression — must be unwrapped before matching and before the WithCancellation/EnumeratorCancellation exclusions ──

public class ParenthesizedCases
{
    public async Task Noncompliant_ParenthesizedSource(CancellationToken token) // Secondary
    {
        await foreach (var item in (Producer.GetItemsAsync())) // Noncompliant
        {
        }
    }

    public async Task Compliant_ParenthesizedWithCancellation(CancellationToken token)
    {
        await foreach (var item in (Producer.GetItemsAsync().WithCancellation(token))) // Compliant
        {
        }
    }

    public async Task Noncompliant_ParenthesizedWithCancellationDefault(CancellationToken token) // Secondary
    {
        await foreach (var item in (Producer.GetItemsAsync().WithCancellation(default))) // Noncompliant — 'default' is not a deliberate opt-out, same as the unparenthesized case
        {
        }
    }
}

// ─── Member CT sources (field) — proves the shared CT-source-finding logic (common to S8949) is reused ──

public class MemberSourceCases
{
    private CancellationToken _ct; // Secondary

    public async Task Noncompliant()
    {
        await foreach (var item in Producer.GetItemsAsync()) // Noncompliant {{Pass the 'this._ct' to this 'await foreach' via 'WithCancellation' to allow cancellation of the operation.}}
        {
        }
    }
}

// ─── Deconstructing 'await foreach' — 'var (a, b)' parses as ForEachVariableStatementSyntax, not ForEachStatementSyntax,
// exercising the SyntaxKindEx.ForEachVariableStatement / CommonForEachStatementSyntaxWrapper path specifically ──────

public class DeconstructingCases
{
    public static async IAsyncEnumerable<(int Key, string Value)> GetPairsAsync() { yield return (1, "a"); await Task.Yield(); }

    public async Task Noncompliant(CancellationToken token) // Secondary
    {
        await foreach (var (key, value) in GetPairsAsync()) // Noncompliant
        {
        }
    }

    public async Task Compliant(CancellationToken token)
    {
        await foreach (var (key, value) in GetPairsAsync().WithCancellation(token)) // Compliant
        {
        }
    }
}

// ─── Not an 'await foreach' — plain synchronous foreach is never flagged ──────

public class SynchronousForEachCases
{
    public void Compliant(CancellationToken token)
    {
        foreach (var item in new[] { 1, 2, 3 }) // Compliant — not an 'await foreach'
        {
        }
    }
}

// ─── Producer already wires the token via [EnumeratorCancellation] — WithCancellation would be redundant ──

public class EnumeratorCancellationCases
{
    public static async IAsyncEnumerable<int> GetItemsAsync([EnumeratorCancellation] CancellationToken token = default) { yield return 1; await Task.Yield(); }
    public static async IAsyncEnumerable<int> GetItemsAsyncUndecorated(CancellationToken token = default) { yield return 1; await Task.Yield(); }

    public async Task Compliant(CancellationToken token)
    {
        await foreach (var item in GetItemsAsync(token)) // Compliant — already wired via [EnumeratorCancellation], WithCancellation would be redundant
        {
        }
    }

    public async Task Compliant_LocalVariableSource(CancellationToken token)
    {
        var items = GetItemsAsync(token); // token flows into the [EnumeratorCancellation] parameter here, not at the foreach
        await foreach (var item in items) // Compliant — already wired, just stored in a variable before the loop
        {
        }
    }

    public async Task Noncompliant_LocalVariableReassigned(CancellationToken token) // Secondary
    {
        var items = GetItemsAsync(token);
        items = GetItemsAsyncUndecorated(); // reassigned to a producer that does NOT wire the token
        await foreach (var item in items) // Noncompliant — the straight-line-preceding assignment is what actually matters, not the first one
        {
        }
    }

    public async Task Noncompliant_LocalVariableConditionallyReassigned(CancellationToken token, bool flag) // Secondary
    {
        var items = GetItemsAsync(token);
        if (flag)
        {
            items = GetItemsAsyncUndecorated();
        }
        await foreach (var item in items) // Noncompliant — which value 'items' holds here depends on 'flag', so it can't be assumed compliant
        {
        }
    }

    public async Task Noncompliant_ShadowedInLocalFunction(CancellationToken token) // Secondary
    {
        var items = GetItemsAsync(token); // outer 'items' - already configured, must not leak into the local function below

        async Task Local(IAsyncEnumerable<int> items) // inner 'items' parameter shadows the outer local by name only
        {
            await foreach (var item in items) // Noncompliant — the inner 'items' is just a parameter, resolving it to the outer local would be wrong
            {
            }
        }

        await Local(GetItemsAsyncUndecorated());
    }

    public async Task Noncompliant(CancellationToken token) // Secondary
    {
        await foreach (var item in GetItemsAsyncUndecorated(token)) // Noncompliant — parameter is not decorated with [EnumeratorCancellation], the token is not actually wired for cancellation
        {
        }
    }

    public async Task Compliant_AnyTokenSuffices(CancellationToken token)
    {
        await foreach (var item in GetItemsAsync(CancellationToken.None)) // Compliant — some CancellationToken was explicitly passed to the [EnumeratorCancellation] parameter, we don't second-guess which one
        {
        }
    }

    public async Task Noncompliant_RelyingOnDefaultValue(CancellationToken token) // Secondary
    {
        await foreach (var item in GetItemsAsync()) // Noncompliant — no argument supplied, the [EnumeratorCancellation] parameter keeps its default value, nothing is actually wired
        {
        }
    }

    public async Task Compliant_Parenthesized(CancellationToken token)
    {
        await foreach (var item in (GetItemsAsync(token))) // Compliant — parens around the source expression must not defeat the [EnumeratorCancellation] exclusion
        {
        }
    }
}

// ─── Not an IAsyncEnumerable<T> — duck-typed async-enumerable pattern is out of scope ──

public class DuckTypedAsyncEnumerableCases
{
    public class CustomAsyncEnumerable
    {
        public CustomAsyncEnumerator GetAsyncEnumerator(CancellationToken token = default) => new();
    }

    public class CustomAsyncEnumerator
    {
        public int Current => 0;
        public Task<bool> MoveNextAsync() => Task.FromResult(false);
    }

    public async Task Compliant(CancellationToken token)
    {
        await foreach (var item in new CustomAsyncEnumerable()) // Compliant — does not implement IAsyncEnumerable<T>, out of scope
        {
        }
    }
}

// ─── 'ConfigureAwait(false)' chained with/around 'WithCancellation' — common library idiom (e.g. dotnet/runtime, dotnet/reactive, Azure SDK).
// Both 'WithCancellation' and 'ConfigureAwait' return 'ConfiguredCancelableAsyncEnumerable<T>', which does not implement
// IAsyncEnumerable<T> itself, so the two calls must be unwrapped to reach the real source before the exclusion/type checks apply.

public class ConfigureAwaitCases
{
    public static async IAsyncEnumerable<int> GetItemsAsyncEc([EnumeratorCancellation] CancellationToken token = default) { yield return 1; await Task.Yield(); }

    public async Task Noncompliant_PlainConfigureAwait(CancellationToken token) // Secondary
    {
        await foreach (var item in Producer.GetItemsAsync().ConfigureAwait(false))
        //                                  ^^^^^^^^^^^^^ Noncompliant {{Pass the 'token' to this 'await foreach' via 'WithCancellation' to allow cancellation of the operation.}}
        {
        }
    }

    public async Task Compliant_WithCancellationThenConfigureAwait(CancellationToken token)
    {
        await foreach (var item in Producer.GetItemsAsync().WithCancellation(token).ConfigureAwait(false)) // Compliant — WithCancellation precedes ConfigureAwait
        {
        }
    }

    public async Task Compliant_ConfigureAwaitThenWithCancellation(CancellationToken token)
    {
        await foreach (var item in Producer.GetItemsAsync().ConfigureAwait(false).WithCancellation(token)) // Compliant — ConfigureAwait precedes WithCancellation
        {
        }
    }

    public async Task Compliant_EnumeratorCancellationUnderConfigureAwait(CancellationToken token)
    {
        await foreach (var item in GetItemsAsyncEc(token).ConfigureAwait(false)) // Compliant — [EnumeratorCancellation] parameter wired underneath ConfigureAwait
        {
        }
    }

    public async Task Compliant_ConfigureAwaitStoredInVariable(CancellationToken token)
    {
        var items = Producer.GetItemsAsync().ConfigureAwait(false); // Compliant — ConfiguredCancelableAsyncEnumerable<T> may already carry a token applied upstream, unlike IAsyncEnumerable<T>
        await foreach (var item in items)
        {
        }
    }
}

// ─── Lookalike 'WithCancellation'/'ConfigureAwait' methods on an unrelated type must not be mistaken for the BCL ones ──

public class LookalikeMethodCases
{
    public class FakeConfiguredAsyncEnumerable : IAsyncEnumerable<int>
    {
        public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken token = default) => null;
        public FakeConfiguredAsyncEnumerable WithCancellation(CancellationToken token) => this; // does not actually wire the token
        public FakeConfiguredAsyncEnumerable ConfigureAwait(bool continueOnCapturedContext) => this;
    }

    public async Task Noncompliant_LookalikeWithCancellation(CancellationToken token) // Secondary
    {
        await foreach (var item in new FakeConfiguredAsyncEnumerable().WithCancellation(token)) // Noncompliant — not the real TaskAsyncEnumerableExtensions.WithCancellation
        {
        }
    }

    public async Task Noncompliant_LookalikeConfigureAwait(CancellationToken token) // Secondary
    {
        await foreach (var item in new FakeConfiguredAsyncEnumerable().ConfigureAwait(false)) // Noncompliant — not the real TaskAsyncEnumerableExtensions.ConfigureAwait
        {
        }
    }
}
