/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.Rules;

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
}
