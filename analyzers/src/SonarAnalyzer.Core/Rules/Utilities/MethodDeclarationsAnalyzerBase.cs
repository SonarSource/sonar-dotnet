/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.Core.Rules.Utilities;

/// <summary>
/// This class is responsible for exporting the method declarations to a protobuf file which will later be
/// used in the plugin to map the content of the test reports to the actual files.
/// </summary>
/// <typeparam name="TSyntaxKind">Discriminator for C#/VB.NET.</typeparam>
public abstract class MethodDeclarationsAnalyzerBase<TSyntaxKind>() : UtilityAnalyzerBase<TSyntaxKind, MethodDeclarationsInfo>(DiagnosticId, Title)
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S9999-methodDeclaration";
    private const string Title = "Method declarations generator";

    protected abstract IEnumerable<SyntaxNode> GetMethodDeclarations(SyntaxNode node);

    protected sealed override string FileName => "method-declarations.pb";

    protected override bool ShouldGenerateMetrics(UtilityAnalyzerParameters parameters, SyntaxTree tree) =>
        // In this analyzer, we want to always generate the metrics for the test projects (contrary to the base class implementation).
        parameters.IsTestProject;

    protected sealed override MethodDeclarationsInfo CreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model)
    {
        var declarations = new HashSet<MethodDeclarationInfo>(new MethodDeclarationInfoComparer());
        var symbols = GetMethodDeclarations(tree.GetRoot()).Select(x => model.GetDeclaredSymbol(x)).Where(x => x is not null);
        foreach (var declarationSymbol in symbols)
        {
            declarations.Add(new MethodDeclarationInfo
            {
                TypeName = declarationSymbol.ContainingType.ToDisplayString(),
                MethodName = declarationSymbol.Name
            });
        }

        if (declarations.Count == 0)
        {
            // Optimization to reduce report size. Null values are excluded before serialization.
            return null;
        }

        return new MethodDeclarationsInfo
        {
            FilePath = tree.FilePath,
            AssemblyName = model.Compilation.AssemblyName,
            MethodDeclarations = { declarations }
        };
    }
}
