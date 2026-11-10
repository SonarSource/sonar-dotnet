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

using System.Threading;

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
