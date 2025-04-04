﻿/*
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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Core.Trackers;

public class CSharpMethodDeclarationTracker : MethodDeclarationTracker<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    public override Condition ParameterAtIndexIsUsed(int index) =>
        context =>
        {
            var parameterSymbol = context.MethodSymbol.Parameters.ElementAtOrDefault(0);
            if (parameterSymbol == null)
            {
                return false;
            }

            var methodInfo = GetMethodInfo(context);
            if (methodInfo?.DescendantNodes == null)
            {
                return false;
            }

            return methodInfo.DescendantNodes.Any(
                node =>
                    node.IsKind(SyntaxKind.IdentifierName)
                    && ((IdentifierNameSyntax)node).Identifier.ValueText == parameterSymbol.Name
                    && parameterSymbol.Equals(methodInfo.SemanticModel.GetSymbolInfo(node).Symbol));
        };

    private static MethodInfo GetMethodInfo(MethodDeclarationContext context)
    {
        if (context.MethodSymbol.IsTopLevelMain())
        {
            var declaration = context.MethodSymbol
                                     .DeclaringSyntaxReferences
                                     .Select(r => r.GetSyntax())
                                     .OfType<CompilationUnitSyntax>()
                                     .First();

            return new MethodInfo(context.GetSemanticModel(declaration), declaration.GetTopLevelMainBody().SelectMany(x => x.DescendantNodes()));
        }
        else
        {
            var declaration = context.MethodSymbol
                                     .DeclaringSyntaxReferences
                                     .Select(r => r.GetSyntax())
                                     .OfType<BaseMethodDeclarationSyntax>()
                                     .FirstOrDefault(declaration => declaration.HasBodyOrExpressionBody());
            if (declaration == null)
            {
                return null;
            }

            return new MethodInfo(
                context.GetSemanticModel(declaration),
                declaration.Body?.DescendantNodes() ?? declaration.ExpressionBody()?.DescendantNodes() ?? Enumerable.Empty<SyntaxNode>());
        }
    }

    protected override SyntaxToken? GetMethodIdentifier(SyntaxNode methodDeclaration) =>
        methodDeclaration switch
        {
            MethodDeclarationSyntax method => method.Identifier,
            ConstructorDeclarationSyntax constructor => constructor.Identifier,
            DestructorDeclarationSyntax destructor => destructor.Identifier,
            OperatorDeclarationSyntax op => op.OperatorToken,
            _ => methodDeclaration?.Parent.Parent switch // Accessors
            {
                EventDeclarationSyntax e => e.Identifier,
                PropertyDeclarationSyntax p => p.Identifier,
                IndexerDeclarationSyntax i => i.ThisKeyword,
                _ => null
            }
        };

    private sealed class MethodInfo
    {
        public SemanticModel SemanticModel { get; }

        public IEnumerable<SyntaxNode> DescendantNodes { get; }

        public MethodInfo(SemanticModel model, IEnumerable<SyntaxNode> descendantNodes)
        {
            SemanticModel = model;
            DescendantNodes = descendantNodes;
        }
    }
}
