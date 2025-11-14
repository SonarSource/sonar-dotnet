/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Core.Rules;

/// <summary>
/// This class is responsible for exporting the method declarations to a protobuf file which will later be
/// used in the plugin to map the content of the test reports to the actual files.
/// </summary>
/// <typeparam name="TSyntaxKind">Discriminator for C#/VB.NET.</typeparam>
public abstract class TestMethodDeclarationsAnalyzerBase<TSyntaxKind>() : UtilityAnalyzerBase<TSyntaxKind, MethodDeclarationsInfo>(DiagnosticId, Title)
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S9999-testMethodDeclaration";
    private const string Title = "Test method declarations generator";

    protected abstract IEnumerable<SyntaxNode> GetTypeDeclarations(SyntaxNode node);

    protected abstract IEnumerable<SyntaxNode> GetMethodDeclarations(SyntaxNode node);

    protected sealed override string FileName => "test-method-declarations.pb";

    protected override bool ShouldGenerateMetrics(UtilityAnalyzerParameters parameters, SyntaxTree tree) =>
        // In this analyzer, we want to always generate the metrics for the test projects (contrary to the base class implementation).
        parameters.IsTestProject && !Language.GeneratedCodeRecognizer.IsGenerated(tree);

    protected sealed override MethodDeclarationsInfo CreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model)
    {
        // Test method declarations found in the file by starting with the syntax tree.
        var fileDeclarations = GetMethodDeclarations(tree.GetRoot())
            .Select(x => GetTestMethodSymbol(x, model))
            .WhereNotNull()
            .Select(GetDeclarationInfo);

        // Test method declarations pulled from the base types.
        var baseTypeDeclarations = GetTypeDeclarations(tree.GetRoot())
            .Select(x => (ITypeSymbol)model.GetDeclaredSymbol(x))
            .WhereNotNull()
            .SelectMany(x => GetAllTestMethods(x, x.BaseType));

        var declarations = new HashSet<MethodDeclarationInfo>(fileDeclarations, new MethodDeclarationInfoComparer());
        declarations.UnionWith(baseTypeDeclarations);
        return declarations.Count == 0
            ? null
            : new MethodDeclarationsInfo { FilePath = tree.FilePath, AssemblyName = model.Compilation.AssemblyName, MethodDeclarations = { declarations } };
    }

    private static MethodDeclarationInfo GetDeclarationInfo(IMethodSymbol methodSymbol) =>
        new()
        {
            TypeName = Name(methodSymbol.ContainingType),
            MethodName = methodSymbol.Name
        };

    private static MethodDeclarationInfo GetDeclarationInfo(ITypeSymbol derivedType, IMethodSymbol methodSymbol) =>
        new()
        {
            TypeName = Name(derivedType),
            MethodName = methodSymbol.Name
        };

    private static string Name(ITypeSymbol typeSymbol)
    {
        const string separator = ".";
        var nameParts = new Stack<string>();
        var currentType = typeSymbol;
        while (currentType is not null)
        {
            nameParts.Push(currentType.Name);
            currentType = currentType.ContainingType;
        }
        var currentNamespace = typeSymbol.ContainingNamespace;
        while (currentNamespace is not null && !currentNamespace.IsGlobalNamespace)
        {
            nameParts.Push(currentNamespace.Name);
            currentNamespace = currentNamespace.ContainingNamespace;
        }
        return string.Join(separator, nameParts);
    }

    private static IEnumerable<MethodDeclarationInfo> GetAllTestMethods(ITypeSymbol derivedType, ITypeSymbol typeSymbol)
    {
        if (typeSymbol is null)
        {
            return [];
        }
        var members = new HashSet<MethodDeclarationInfo>(GetTestMethodSymbols(typeSymbol).Select(x => GetDeclarationInfo(derivedType, x)));
        var baseType = typeSymbol.BaseType;
        while (baseType is not null)
        {
            members.UnionWith(GetAllTestMethods(derivedType, baseType));
            baseType = baseType.BaseType;
        }

        return members;
    }

    private static IEnumerable<IMethodSymbol> GetTestMethodSymbols(ITypeSymbol typeSymbol) =>
        typeSymbol.GetMembers().OfType<IMethodSymbol>().Where(IsTestMethod);

    private static IMethodSymbol GetTestMethodSymbol(SyntaxNode methodDeclarationSyntax, SemanticModel model) =>
        model.GetDeclaredSymbol(methodDeclarationSyntax) is IMethodSymbol methodSymbol && IsTestMethod(methodSymbol)
            ? methodSymbol
            : null;

    private static bool IsTestMethod(IMethodSymbol methodSymbol) =>
        methodSymbol is not null && !methodSymbol.IsImplicitlyDeclared && methodSymbol.IsTestMethod();
}
