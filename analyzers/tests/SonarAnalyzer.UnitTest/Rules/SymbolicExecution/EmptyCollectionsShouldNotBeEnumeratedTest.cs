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

using System.Collections.Generic;
using SonarAnalyzer.SymbolicExecution.Sonar.Analyzers;
using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using ChecksVB = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class EmptyCollectionsShouldNotBeEnumeratedTest
{
    private readonly VerifierBuilder sonar = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
        .WithBasePath(@"SymbolicExecution\Sonar")
        .WithOnlyDiagnostics(EmptyCollectionsShouldNotBeEnumerated.S4158)
        .WithConcurrentAnalysis(false);

    private readonly VerifierBuilder roslynCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksCS.EmptyCollectionsShouldNotBeEnumerated.S4158);

    private readonly VerifierBuilder roslynVB = new VerifierBuilder<VB.SymbolicExecutionRunner>()
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksVB.EmptyCollectionsShouldNotBeEnumerated.S4158);

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void EmptyCollectionsShouldNotBeEnumerated_Sonar(ProjectType projectType) =>
        sonar.AddReferences(TestHelper.ProjectTypeReference(projectType))
            .AddPaths("EmptyCollectionsShouldNotBeEnumerated.cs")
            .Verify();

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void EmptyCollectionsShouldNotBeEnumerated_Roslyn_CS(ProjectType projectType) =>
        roslynCS.AddReferences(TestHelper.ProjectTypeReference(projectType))
            .AddPaths("EmptyCollectionsShouldNotBeEnumerated.cs")
            .Verify();

    [DataTestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void EmptyCollectionsShouldNotBeEnumerated_Roslyn_VB(ProjectType projectType) =>
        roslynVB.AddReferences(TestHelper.ProjectTypeReference(projectType))
            .AddPaths("EmptyCollectionsShouldNotBeEnumerated.vb")
            .Verify();

#if NET

    [DataTestMethod]
    [DataRow("list.Aggregate((x, y) => x)", false)]
    [DataRow("list.All(x => true)", false)]
    [DataRow("list.AsEnumerable()", true)]
    [DataRow("list.AsQueryable()", true)]
    [DataRow("list.AsReadOnly()", true)]
    [DataRow("list.Average()", false)]
    [DataRow("list.Cast<byte>()", true)] // FN
    [DataRow("list.Concat(list)", true)]
    [DataRow("list.Contains(1)", false)]
    [DataRow("list.Count()", true)]
    [DataRow("list.DefaultIfEmpty()", true)]
    [DataRow("list.Distinct()", false)]
    [DataRow("list.ElementAt(0)", false)]
    [DataRow("list.ElementAtOrDefault(0)", false)]
    [DataRow("list.Except(list)", false)]
    [DataRow("list.First()", false)]
    [DataRow("list.FirstOrDefault()", false)]
    [DataRow("list.GroupBy(x => x)", false)]
    [DataRow("list.GroupJoin(list, x => x, x => x, (x, y) => x)", false)]
    [DataRow("list.Intersect(list)", false)]
    [DataRow("list.Join(list, x => x, x => x, (x, y) => x)", true)]
    [DataRow("list.Last()", false)]
    [DataRow("list.LastOrDefault()", false)]
    [DataRow("list.LongCount()", false)]
    [DataRow("list.Max()", false)]
    [DataRow("list.Min()", false)]
    [DataRow("list.OfType<int>()", true)] // FN
    [DataRow("list.OrderBy(x => x)", false)]
    [DataRow("list.OrderByDescending(x => x)", false)]
    [DataRow("list.Reverse()", false)]
    [DataRow("list.Select(x => x)", false)]
    [DataRow("list.SelectMany(x => list)", false)]
    [DataRow("list.SequenceEqual(list)", false)]
    [DataRow("list.Single()", false)]
    [DataRow("list.SingleOrDefault()", false)]
    [DataRow("list.Skip(0)", false)]
    [DataRow("list.SkipWhile(x => true)", false)]
    [DataRow("list.Sum()", false)]
    [DataRow("list.Take(0)", false)]
    [DataRow("list.TakeWhile(x => true)", false)]
    [DataRow("list.ToArray()", true)]
    [DataRow("list.ToDictionary(x => x)", true)]
    [DataRow("list.ToList()", true)]
    [DataRow("list.ToLookup(x => x)", true)]
    [DataRow("list.Union(list)", true)]
    [DataRow("list.Where(x => true)", false)]
    [DataRow("list.Zip(list, (x, y) => x)", false)]
    [DataRow("Enumerable.Reverse(list)", false)]
    public void EmptyCollectionsShouldNotBeEnumerated_Roslyn_ExtensionMethods(string invocation, bool compliant) =>
        roslynCS.AddSnippet($$"""
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M()
                {
                    var list = new List<int>() { };
                    {{invocation}}; // {{(compliant ? "Compliant" : "Noncompliant")}}
                }
            }
            """).Verify();

    [TestMethod]
    public void EmptyCollectionsShouldNotBeEnumerated_Sonar_CSharp8() =>
        sonar.AddPaths("EmptyCollectionsShouldNotBeEnumerated.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

    [TestMethod]
    public void EmptyCollectionsShouldNotBeEnumerated_Roslyn_CSharp8() =>
        roslynCS.AddPaths("EmptyCollectionsShouldNotBeEnumerated.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

    [TestMethod]
    public void EmptyCollectionsShouldNotBeEnumerated_Sonar_CSharp9() =>
        sonar.AddPaths("EmptyCollectionsShouldNotBeEnumerated.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void EmptyCollectionsShouldNotBeEnumerated_Roslyn_CSharp9() =>
        roslynCS.AddPaths("EmptyCollectionsShouldNotBeEnumerated.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void EmptyCollectionsShouldNotBeEnumerated_Sonar_CSharp10() =>
        sonar.AddPaths("EmptyCollectionsShouldNotBeEnumerated.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

    [TestMethod]
    public void EmptyCollectionsShouldNotBeEnumerated_Roslyn_CSharp10() =>
        roslynCS.AddPaths("EmptyCollectionsShouldNotBeEnumerated.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

    [TestMethod]
    public void EmptyCollectionsShouldNotBeEnumerated_Sonar_CSharp11() =>
        sonar.AddPaths("EmptyCollectionsShouldNotBeEnumerated.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

    [TestMethod]
    public void EmptyCollectionsShouldNotBeEnumerated_Roslyn_CSharp11() =>
        roslynCS.AddPaths("EmptyCollectionsShouldNotBeEnumerated.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

#endif

}
