/*
* SonarAnalyzer for .NET
* Copyright (C) 2014-2025 SonarSource Sàrl
* mailto:info AT sonarsource DOT com
* This program is free software; you can redistribute it and/or
* modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
* See the Sonar Source-Available License for more details.
*
* You should have received a copy of the Sonar Source-Available License
* along with this program; if not, see https://sonarsource.com/license/ssal/
*/

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.dotMemoryUnit;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Core.AnalysisContext;

namespace SonarAnalyzer.Memory.Test.Registrations;

[TestClass]
public class SonarSyntaxNodeReportingContextMemoryTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void SonarSyntaxNodeReportingContextAllocationsPerNodeKind()
    {
        const int namespaceCount = 20_000;
        var compilation = CreateCompilationWithNamespaceDeclarations(namespaceCount);
        var keepAlive = new ConcurrentStack<SonarSyntaxNodeReportingContext>();
        var withAnalyzer = compilation.WithAnalyzers([new TestAnalyzerCS((c, g) =>
            c.RegisterNodeAction(
                g,
                context =>
                {
                    keepAlive.Push(context);
                    context.ReportIssue(TestAnalyzer.Rule, context.Node);
                },
                SyntaxKind.NamespaceDeclaration))]);
        var result = withAnalyzer.GetAllDiagnosticsAsync().GetAwaiter().GetResult();
        keepAlive.Should().HaveCount(namespaceCount);
        dotMemory.Check(x =>
            {
                PrintObjectStatistics(x.GroupByType());
                var contextCount = x.GetObjects(where => where.Type.Is<SonarSyntaxNodeReportingContext>()).ObjectsCount;
                contextCount.Should().Be(0); // SonarSyntaxNodeReportingContext is a struct and should never be boxed
            });
        result.Should().HaveCount(namespaceCount);
        GC.KeepAlive(keepAlive);
    }

    [TestMethod]
    [DotMemoryUnit(CollectAllocations = true)]
    public void SonarSyntaxNodeReportingContextOverallAllocations()
    {
        const int namespaceCount = 20_000;
        var warmupCompilation = CSharpCompilation.Create(assemblyName: null, [CreateNamespaceDeclarations(1)], options: new(OutputKind.DynamicallyLinkedLibrary));
        ImmutableArray<DiagnosticAnalyzer> analyzers = [new TestAnalyzerCS((c, g) =>
            c.RegisterNodeAction(
                g,
                context =>
                {
                    new MarkerObject().Reset(); // Make sure we don't get optimized away by inlining
                    context.ReportIssue(TestAnalyzer.Rule, context.Node);
                },
                SyntaxKind.NamespaceDeclaration))];
        var withAnalyzer = warmupCompilation.WithAnalyzers(analyzers);
        // Warmup
        withAnalyzer.GetAllDiagnosticsAsync().GetAwaiter().GetResult();

        var compilation = warmupCompilation.RemoveAllSyntaxTrees().AddSyntaxTrees(CreateNamespaceDeclarations(namespaceCount));
        withAnalyzer = compilation.WithAnalyzers(analyzers);
        var checkpoint = dotMemory.Check();
        var result = withAnalyzer.GetAllDiagnosticsAsync().GetAwaiter().GetResult();
        dotMemory.Check(x =>
            {
                var traffic = x.GetTrafficFrom(checkpoint).GroupByType();
                PrintObjectStatistics(traffic);
                var allocated = new { AllocatedMemoryInfo = new { ObjectsCount = namespaceCount }, CollectedMemoryInfo = new { ObjectsCount = namespaceCount } };
                var allocatedAndCollected = new { AllocatedMemoryInfo = new { ObjectsCount = namespaceCount }, CollectedMemoryInfo = new { ObjectsCount = 0 } };
                traffic.Should().ContainEquivalentOf(new { Type = typeof(MarkerObject) }).Which.Should().BeEquivalentTo(allocated);
                traffic.Should().ContainEquivalentOf(new { Type = typeof(SonarSyntaxNodeReportingContext) }).Which.Should().BeEquivalentTo(
                    allocated,
                    because: "ReportIssue captures and boxes SonarSyntaxNodeReportingContext. This is not the hotpath, though and therefore okay.");
                traffic.Should().ContainEquivalentOf(new { Type = typeof(SyntaxNodeAnalysisContext) }).Which.Should().BeEquivalentTo(allocated, because: "Boxed by ReportIssue");
                traffic.Should().ContainEquivalentOf(new { Type = typeof(NamespaceDeclarationSyntax) }).Which.Should().BeEquivalentTo(allocatedAndCollected);
                traffic.Should().ContainEquivalentOf(new { Type = typeof(ReportingContext) }).Which.Should().BeEquivalentTo(allocated);
                var stringAllocations = traffic.Should().ContainEquivalentOf(new { Type = typeof(string) }).Subject;
                stringAllocations.AllocatedMemoryInfo.ObjectsCount.Should().BeLessThan(100);
                stringAllocations.AllocatedMemoryInfo.SizeInBytes.Should().BeLessThan(5_000);
            });
        result.Should().HaveCount(namespaceCount);
        GC.KeepAlive(result);
        GC.KeepAlive(withAnalyzer);
    }

    [TestMethod]
    [DotMemoryUnit(CollectAllocations = true)]
    public void SonarSyntaxNodeReportingContextOverallAllocationsWithoutReporting()
    {
        const int namespaceCount = 20_000;
        var compilation = CSharpCompilation.Create(assemblyName: null, [CreateNamespaceDeclarations(namespaceCount)], options: new(OutputKind.DynamicallyLinkedLibrary));
        var withAnalyzer = compilation.WithAnalyzers([new TestAnalyzerCS((c, g) => c.RegisterNodeAction(g, _ => new MarkerObject().Reset(), SyntaxKind.NamespaceDeclaration))]);
        var checkpoint = dotMemory.Check();
        var result = withAnalyzer.GetAllDiagnosticsAsync().GetAwaiter().GetResult();
        dotMemory.Check(x =>
            {
                var traffic = x.GetTrafficFrom(checkpoint).GroupByType();
                PrintObjectStatistics(traffic);
                traffic.Should().ContainEquivalentOf(new { Type = typeof(MarkerObject) }).Which.Should().BeEquivalentTo(new { AllocatedMemoryInfo = new { ObjectsCount = namespaceCount } });
                traffic.Should().NotContainEquivalentOf(new { Type = typeof(SonarSyntaxNodeReportingContext) }, because: "The action invocation is allocation free");
                traffic.Should().NotContainEquivalentOf(new { Type = typeof(SyntaxNodeAnalysisContext) }, because: "The action invocation is allocation free");
                traffic.Where(x => x.Type.Namespace.StartsWith(nameof(SonarAnalyzer)) && x.Type != typeof(MarkerObject)).Should().NotBeEmpty().And.AllSatisfy(x =>
                    x.AllocatedMemoryInfo.ObjectsCount.Should().BeLessThan(50));
                var stringAllocations = traffic.Should().ContainEquivalentOf(new { Type = typeof(string) }).Subject;
                stringAllocations.AllocatedMemoryInfo.ObjectsCount.Should().BeLessThan(1_000);
                stringAllocations.AllocatedMemoryInfo.SizeInBytes.Should().BeLessThan(60_000);
            });
        result.Should().HaveCount(0);
        GC.KeepAlive(result);
        GC.KeepAlive(withAnalyzer);
    }

    private void PrintObjectStatistics(IEnumerable<TypeTrafficInfo> typeTrafficInfos) =>
        TestContext.WriteLine(typeTrafficInfos.OrderByDescending(x => x.AllocatedMemoryInfo.SizeInBytes).Aggregate(
            new StringBuilder(),
            (builder, x) => builder.AppendLine(x.ToString()),
            x => x.ToString()));

    private void PrintObjectStatistics(IEnumerable<TypeMemoryInfo> typeMemoryInfos) =>
        TestContext.WriteLine(typeMemoryInfos.OrderByDescending(x => x.SizeInBytes).Aggregate(
            new StringBuilder(),
            (builder, x) => builder.AppendLine(x.ToString()),
            x => x.ToString()));

    private static CSharpCompilation CreateCompilationWithNamespaceDeclarations(int namespaceCount) =>
        CSharpCompilation.Create(assemblyName: null, [CreateNamespaceDeclarations(namespaceCount)], options: new(OutputKind.DynamicallyLinkedLibrary));

    private static SyntaxTree CreateNamespaceDeclarations(int namespaceCount) =>
        CSharpSyntaxTree.ParseText(Enumerable.Range(0, namespaceCount).Aggregate(
            new StringBuilder(),
            (x, i) => x.AppendLine($$"""namespace A{{i}} { }"""),
            x => x.ToString()));
}

public class MarkerObject
{
    public int Value { get; set; } = 1;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Reset() => Value = 0;
}
