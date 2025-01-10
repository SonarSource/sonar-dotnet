/*
* SonarAnalyzer for .NET
* Copyright (C) 2014-2025 SonarSource SA
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

extern alias common;

using System.Collections.Concurrent;
using System.Text;
using common::SonarAnalyzer.AnalysisContext;
using JetBrains.dotMemoryUnit;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Core.Facade;
using SonarAnalyzer.CSharp.Core.Facade;

namespace SonarAnalyzer.Memory.Test.Registrations;

[TestClass]
public class SonarSyntaxNodeReportingContextMemoryTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void SonarSyntaxNodeReportingContextAllocationsPerNodeKind()
    {
        const int nodeCount = 20_000;
        var compilation = CSharpCompilation.Create(assemblyName: null, [CSharpSyntaxTree.ParseText(Enumerable.Range(0, nodeCount).Aggregate(new StringBuilder(),
            (x, i) => x.AppendLine($$"""namespace A{{i}} { }"""), x => x.ToString()))]);
        var keepAlive = new ConcurrentStack<SonarSyntaxNodeReportingContext>();
        var withAnalyzer = compilation.WithAnalyzers([new TestAnalyzerCS((c, g) => c.RegisterNodeAction(g, context =>
        {
            keepAlive.Push(context);
            context.ReportIssue(TestAnalyzer.Rule, context.Node);
        }, SyntaxKind.NamespaceDeclaration))]);
        var result = withAnalyzer.GetAllDiagnosticsAsync().GetAwaiter().GetResult();
        keepAlive.Should().HaveCount(nodeCount);
        dotMemory.Check(x =>
        {
            TestContext.WriteLine(x.GroupByType().OrderByDescending(x => x.SizeInBytes).Aggregate(new StringBuilder(), (builder, x) => builder.AppendLine(x.ToString()), builder => builder.ToString()));
            var contextCount = x.GetObjects(where => where.Type.Is<SonarSyntaxNodeReportingContext>()).ObjectsCount;
            contextCount.Should().Be(0); // SonarSyntaxNodeReportingContext is a struct and should never be boxed
        });
        GC.KeepAlive(keepAlive);
    }
}
