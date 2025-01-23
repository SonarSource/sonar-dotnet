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

using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using SonarAnalyzer.Core.AnalysisContext;
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.TestFramework.Build;

public class SnippetCompiler
{
    public Compilation Compilation { get; }
    public SyntaxTree SyntaxTree { get; }
    public SemanticModel SemanticModel { get; }

    public SnippetCompiler(string code, params MetadataReference[] additionalReferences) : this(code, false, AnalyzerLanguage.CSharp, additionalReferences) { }

    public SnippetCompiler(string code, IEnumerable<MetadataReference> additionalReferences) : this(code, false, AnalyzerLanguage.CSharp, additionalReferences) { }

    public SnippetCompiler(string code, bool ignoreErrors, AnalyzerLanguage language, IEnumerable<MetadataReference> additionalReferences = null, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, ParseOptions parseOptions = null)
    {
        Compilation = SolutionBuilder
            .Create()
            .AddProject(language, outputKind)
            .AddSnippet(code)
            .AddReferences(additionalReferences ?? Enumerable.Empty<MetadataReference>())
            .GetCompilation(parseOptions);

        if (!ignoreErrors && HasCompilationErrors(Compilation))
        {
            DumpCompilationErrors(Compilation);
            throw new InvalidOperationException("Test setup error: test code snippet did not compile. See output window for details.");
        }

        SyntaxTree = Compilation.SyntaxTrees.First();
        SemanticModel = Compilation.GetSemanticModel(SyntaxTree);
    }

    public bool IsCSharp() =>
        Compilation.Language == LanguageNames.CSharp;

    public IEnumerable<TSyntaxNodeType> GetNodes<TSyntaxNodeType>() where TSyntaxNodeType : SyntaxNode =>
        SyntaxTree.GetRoot().DescendantNodes().OfType<TSyntaxNodeType>();

    public TSymbolType GetSymbol<TSymbolType>(SyntaxNode node) where TSymbolType : class, ISymbol =>
        SemanticModel.GetSymbolInfo(node).Symbol as TSymbolType;

    public SyntaxNode GetMethodDeclaration(string typeDotMethodName)
    {
        var nameParts = typeDotMethodName.Split('.');
        SyntaxNode method = null;

        if (IsCSharp())
        {
            var type = GetNodes<CS.TypeDeclarationSyntax>().First(m => m.Identifier.ValueText == nameParts[0]);
            method = type.DescendantNodes().OfType<CS.MethodDeclarationSyntax>().First(m => m.Identifier.ValueText == nameParts[1]);
        }
        else
        {
            var type = GetNodes<VB.TypeStatementSyntax>().First(m => m.Identifier.ValueText == nameParts[0]);
            method = type.Parent.DescendantNodes().OfType<VB.MethodStatementSyntax>().First(m => m.Identifier.ValueText == nameParts[1]);
        }

        method.Should().NotBeNull("Test setup error: could not find method declaration in code snippet: Type: {nameParts[0]}, Method: {nameParts[1]}");
        return method;
    }

    public INamespaceSymbol GetNamespaceSymbol(string name)
    {
        var symbol = GetNodes<CS.BaseNamespaceDeclarationSyntax>()
            .Concat<SyntaxNode>(GetNodes<VB.NamespaceStatementSyntax>())
            .Select(s => SemanticModel.GetDeclaredSymbol(s))
            .First(s => s.Name == name) as INamespaceSymbol;

        symbol.Should().NotBeNull($"Test setup error: could not find namespace in code snippet: {name}");
        return symbol;
    }

    public ITypeSymbol GetTypeSymbol(string typeName)
    {
        var type = (SyntaxNode)GetNodes<CS.TypeDeclarationSyntax>().FirstOrDefault(m => m.Identifier.ValueText == typeName)
            ?? GetNodes<VB.TypeStatementSyntax>().FirstOrDefault(m => m.Identifier.ValueText == typeName);

        var symbol = SemanticModel.GetDeclaredSymbol(type) as ITypeSymbol;
        symbol.Should().NotBeNull($"Test setup error: could not find type in code snippet: {type}");
        return symbol;
    }

    public IMethodSymbol GetMethodSymbol(string typeDotMethodName)
    {
        var method = GetMethodDeclaration(typeDotMethodName);
        return SemanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
    }

    public IPropertySymbol GetPropertySymbol(string typeDotMethodName)
    {
        var nameParts = typeDotMethodName.Split('.');
        SyntaxNode property = null;

        if (IsCSharp())
        {
            var type = SyntaxTree.GetRoot().DescendantNodes().OfType<CS.TypeDeclarationSyntax>().First(m => m.Identifier.ValueText == nameParts[0]);
            property = type.DescendantNodes().OfType<CS.PropertyDeclarationSyntax>().First(m => m.Identifier.ValueText == nameParts[1]);
        }
        else
        {
            var type = SyntaxTree.GetRoot().DescendantNodes().OfType<VB.TypeStatementSyntax>().First(m => m.Identifier.ValueText == nameParts[0]);
            property = type.DescendantNodes().OfType<VB.PropertyStatementSyntax>().First(m => m.Identifier.ValueText == nameParts[1]);
        }

        var symbol = SemanticModel.GetDeclaredSymbol(property) as IPropertySymbol;
        symbol.Should().NotBeNull("Test setup error: could not find property in code snippet: Type: {nameParts[0]}, Method: {nameParts[1]}");
        return symbol;
    }

    public INamedTypeSymbol GetTypeByMetadataName(string metadataName) =>
        SemanticModel.Compilation.GetTypeByMetadataName(metadataName);

    public SonarSyntaxNodeReportingContext CreateAnalysisContext(SyntaxNode node)
    {
        var nodeContext = new SyntaxNodeAnalysisContext(node, SemanticModel, null, null, null, default);
        return new(AnalysisScaffolding.CreateSonarAnalysisContext(), nodeContext);
    }

    public Assembly EmitAssembly()
    {
        using var memoryStream = new MemoryStream();
        Compilation.Emit(memoryStream).Success.Should().BeTrue("The provided snippet should emit assembly.");
        return Assembly.Load(memoryStream.ToArray());
    }

    private static bool HasCompilationErrors(Compilation compilation) =>
        compilation.GetDiagnostics().Any(IsCompilationError);

    private static bool IsCompilationError(Diagnostic diagnostic) =>
        diagnostic.Severity == DiagnosticSeverity.Error && (diagnostic.Id.StartsWith("CS") || diagnostic.Id.StartsWith("BC"));

    private static void DumpCompilationErrors(Compilation compilation)
    {
        Console.WriteLine("Diagnostic errors:");
        foreach (var d in compilation.GetDiagnostics().Where(IsCompilationError))
        {
            Console.WriteLine($"  {d.Id} Line: {d.Location.GetMappedLineSpan().StartLinePosition.Line}: {d.GetMessage()}");
        }
    }
}
