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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class StaticFieldWrittenFromInstanceMemberTest
{
    private readonly VerifierBuilder<StaticFieldWrittenFromInstanceMember> builder = new();

    [TestMethod]
    public void StaticFieldWrittenFromInstanceMember() =>
        builder.AddPaths(@"StaticFieldWrittenFromInstanceMember.cs").WithOptions(LanguageOptions.FromCSharp8).AddReferences(MetadataReferenceFacade.NetStandard21).Verify();

    [TestMethod]
    public async Task SecondaryIssueInReferencedCompilation()
    {
        const string firstClass =
            @"
public class Foo
{
    public static int Count = 0; // Secondary
}
";

        const string secondClass =
            @"
public class Bar
{
    public int Increment() => Foo.Count++;
}
";

        var analyzers = ImmutableArray<DiagnosticAnalyzer>.Empty.Add(new StaticFieldWrittenFromInstanceMember());
        var firstCompilation = CreateCompilation(CSharpSyntaxTree.ParseText(firstClass), "First").WithAnalyzers(analyzers).Compilation;
        var secondCompilation = CreateCompilation(CSharpSyntaxTree.ParseText(secondClass), "Second")
                                .AddReferences(firstCompilation.ToMetadataReference())
                                .WithAnalyzers(analyzers);

        var result = await secondCompilation.GetAnalyzerDiagnosticsAsync();

        firstCompilation.GetDiagnostics().Should().BeEmpty();
        result.Should().BeEquivalentTo(new[] { new { Id = "S2696", AdditionalLocations = Array.Empty<Location>() } });
        result.Single().GetMessage().Should().StartWith("Make the enclosing instance method 'static' or remove this set on the 'static' field.");
    }

#if NET

    [TestMethod]
    public void StaticFieldWrittenFromInstanceMember_Latest() =>
        builder.AddPaths(@"StaticFieldWrittenFromInstanceMember.Latest.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#endif

    private static CSharpCompilation CreateCompilation(SyntaxTree tree, string name) =>
        CSharpCompilation
            .Create(name, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
            .AddSyntaxTrees(tree);
}
