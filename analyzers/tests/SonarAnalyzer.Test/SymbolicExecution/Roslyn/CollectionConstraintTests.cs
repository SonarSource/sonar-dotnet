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

using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class CollectionConstraintTests
{
    private readonly VerifierBuilder builder = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithOnlyDiagnostics(ChecksCS.ConditionEvaluatesToConstant.S2589, ChecksCS.EmptyCollectionsShouldNotBeEnumerated.S4158);

    [DataTestMethod]
    [DataRow("List", "Add", "Remove(1)")]
    [DataRow("List", "Add", "RemoveAll(x => true)")]
    [DataRow("List", "Add", "RemoveAt(1)")]
    [DataRow("List", "Add", "RemoveRange(1, 1)")]
    [DataRow("HashSet", "Add", "ExceptWith(null)")]
    [DataRow("HashSet", "Add", "IntersectWith(null)")]
    [DataRow("HashSet", "Add", "RemoveWhere(x => true)")]
    [DataRow("Queue", "Enqueue", "Dequeue()")]
    [DataRow("Stack", "Push", "Pop()")]
    public void CollectionConstraint_RemoveMethods_RemoveNotNull(string type, string add, string remove) =>
        builder.AddSnippet($$"""
            using System;
            using System.Collections.Generic;
            class Sample
            {
                void Foo({{type}}<int> items)
                {
                    items.{{add}}(1);       // Adds NotNull
                    if (items.Count == 0)   // Noncompliant 2589 always false
                        return;
                    items.{{remove}};       // Removes NotNull
                    if (items.Count == 0)   // Compliant
                        return;
                    Console.WriteLine();
                }
            }
            """)
            .Verify();

    [TestMethod]
    public void CollectionConstraint_RemoveOnNonTrackedSymbol_DoesNotThrow() =>
        builder.AddSnippet($$"""
            using System;
            using System.Collections.Generic;
            class Sample
            {
                public List<int> Items { get; }

                void Foo(Sample sample)
                {
                    sample.Items.Remove(1);
                }
            }
            """)
            .VerifyNoIssues();

    [TestMethod]
    public void CollectionConstraint_ShouldNotLearnConstraintForNonCollectionProperties() =>
        builder.AddSnippet($$$"""
            ﻿using System.Collections.Generic;

            class Tests
            {
                int Length { get; }

                void Test(List<string> foo)
                {
                    bool result = foo.Contains("foo"); // This is needed to trigger S4158

                    // This FP used to trigger due to S4158, when checking for the Length property.
                    if (this.Length < 0) // Compliant
                    { }
                }
            }
            """)
        .VerifyNoIssues();

    [TestMethod]
    public void CollectionConstraint_ShouldNotLearnConstraintFromBinariesForNonTrackedTypes() =>
        builder.AddSnippet("""
            using System;
            ﻿using System.Collections.Generic;
            class NotACollection
            {
                List<int> items;
                int Count => items.Count;
            
                void Remove(int i) => items.Remove(i);
            
                void Foo(NotACollection notACollection)
                {
                    if (notACollection.Count > 0)
                    {
                        notACollection.Remove(5);
                        if (notACollection.Count > 0)   // Compliant
                        {
                            Console.WriteLine();
                        }
                    }
                }
            }
            """)
        .VerifyNoIssues();
}
