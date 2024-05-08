/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

extern alias csharp;
using csharp::SonarAnalyzer.Rules.CSharp;
using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class Sandbox_2259
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksCS.NullPointerDereference.S2259);

    [TestMethod]
    public void S2259_1() => builderCS.AddSnippet("""
        using System;
        class Test
        {
            string Method(string s)
            {
                return s?.Length switch
                {
                    1 => s.ToString(),  // Noncompliant FP
                    _ => s.ToString()   // Noncompliant
                };
            }
        }
        """).WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

    [TestMethod]
    public void S2259_2() => builderCS.AddSnippet("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        class Sample
        {
            string Method(Sample s) =>
                s?.Thingy switch
                {
                    E.A => s.ToString(),    // Noncompliant FP
                    _ => s.ToString()       // Noncompliant
                };

            E Thingy { get; }

            enum E { A, B }
        }
        """).WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

    [TestMethod]
    public void S2259_3() => builderCS.AddSnippet("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        class Test
        {
            private void Foo(List<string> values)
            {
                string x = null;
                if (values.Count > 0)
                {
                    x = "";
                }
                foreach (var value in values)
                {
                    _ = x.Length;   // Noncompliant FP
                }
            }
        }
        """).WithOptions(ParseOptionsHelper.FromCSharp12).Verify();
}

[TestClass]
public class Sandbox_2583_2589
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksCS.ConditionEvaluatesToConstant.S2583, ChecksCS.ConditionEvaluatesToConstant.S2589, ChecksCS.EmptyCollectionsShouldNotBeEnumerated.S4158);

    [TestMethod]
    public void S2583_2589_1() => builderCS.AddSnippet("""
        using System;
        using System.Diagnostics.CodeAnalysis;
        class Test
        {
            string Method(string s)
            {
                return s?.Length switch
                {
                    1 => s.ToString(),
                    _ => s.ToString()
                };
            }
        }
        """).WithOptions(ParseOptionsHelper.FromCSharp12).VerifyNoIssues();

    [TestMethod]
    public void S2583_2589_2() => builderCS.AddSnippet("""
        using System;
        class Test
        {
            public static bool IsGreaterThanATenth(double n) => n switch
            {
                > 0.1 => true,
                _ => false
            };
        }
        """).WithOptions(ParseOptionsHelper.FromCSharp12).VerifyNoIssues();
}

[TestClass]
public class Sandbox_3949
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksCS.CalculationsShouldNotOverflow.S3949);

    [TestMethod]
    public void S3949_1() => builderCS.AddSnippet("""
        class Test
        {
            void Method(int numberOf4Kbs)
            {
                int posBy4Kbs = 0;
                while (posBy4Kbs < numberOf4Kbs)
                {
                    var roundTo4Kb = numberOf4Kbs / -numberOf4Kbs;
                    if (roundTo4Kb > int.MaxValue)
                    {
                        _ = "Math failure";
                    }
                    posBy4Kbs += (int)roundTo4Kb;   // Noncompliant
                }
            }
        }
        """).WithOptions(ParseOptionsHelper.FromCSharp12).Verify();
}

[TestClass]
public class Sandbox_S3220
{
    private readonly VerifierBuilder builder = new VerifierBuilder<InvocationResolvesToOverrideWithParams>();

    [TestMethod]
    public void S3220_1() => builder.AddSnippet("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        class Test
        {
            void Method(int[] numbers)
            {
                Print(numbers.Append(1));    // Compliant
                Print([.. numbers, 1]);      // Noncompliant
            }
            void Print(params int[] numbers) => Print(numbers.AsEnumerable());
            void Print(IEnumerable<int> numbers) => throw new NotImplementedException();
        }
        """).WithOptions(ParseOptionsHelper.FromCSharp12).Verify();
}

[TestClass]
public class Sandbox_S3878
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ArrayPassedAsParams>();

    [TestMethod]
    public void S3878_1() => builder.AddSnippet("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        class Test
        {
            void Method(int i, int[] numbers)
            {
                Print(numbers.Append(1).ToArray()); // Compliant
                Print([
                    1,
                    2,
                    .. i switch
                    {
                        1 => [1, 2],
                        2 => [2, 3],
                        _ => Array.Empty<int>()
                    }]);
            }
            void Print(params int[] numbers) => throw new NotImplementedException();
        }
        """).WithOptions(ParseOptionsHelper.FromCSharp12).VerifyNoIssues();
}
