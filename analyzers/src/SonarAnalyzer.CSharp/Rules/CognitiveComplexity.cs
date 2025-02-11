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

using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.CSharp.Metrics;

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CognitiveComplexity : CognitiveComplexityBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
                {
                    if (c.IsTopLevelMain())
                    {
                        CheckComplexity<CompilationUnitSyntax>(
                            c,
                            compilationUnit => compilationUnit,
                            _ => Location.Create(c.Node.SyntaxTree, TextSpan.FromBounds(0, 0)),
                            node => CSharpCognitiveComplexityMetric.GetComplexity(node, true),
                            "top-level file",
                            Threshold);
                    }
                },
                SyntaxKind.CompilationUnit);

            context.RegisterNodeAction(
                c => CheckComplexity<MethodDeclarationSyntax>(
                    c,
                    m => m,
                    m => m.Identifier.GetLocation(),
                    CSharpCognitiveComplexityMetric.GetComplexity,
                    "method",
                    Threshold),
                SyntaxKind.MethodDeclaration);

            // Here, we only care about arrowed properties, others will be handled by the accessor.
            context.RegisterNodeAction(
                c => CheckComplexity<PropertyDeclarationSyntax>(
                    c,
                    p => p.ExpressionBody,
                    p => p.Identifier.GetLocation(),
                    CSharpCognitiveComplexityMetric.GetComplexity,
                    "property",
                    PropertyThreshold),
                SyntaxKind.PropertyDeclaration);

            context.RegisterNodeAction(
                c => CheckComplexity<ConstructorDeclarationSyntax>(
                    c,
                    co => co,
                    co => co.Identifier.GetLocation(),
                    CSharpCognitiveComplexityMetric.GetComplexity,
                    "constructor",
                    Threshold),
                SyntaxKind.ConstructorDeclaration);

            context.RegisterNodeAction(
                c => CheckComplexity<DestructorDeclarationSyntax>(
                    c,
                    d => d,
                    d => d.Identifier.GetLocation(),
                    CSharpCognitiveComplexityMetric.GetComplexity,
                    "destructor",
                    Threshold),
                SyntaxKind.DestructorDeclaration);

            context.RegisterNodeAction(
                c => CheckComplexity<OperatorDeclarationSyntax>(
                    c,
                    o => o,
                    o => o.OperatorToken.GetLocation(),
                    CSharpCognitiveComplexityMetric.GetComplexity,
                    "operator",
                    Threshold),
                SyntaxKind.OperatorDeclaration);

            context.RegisterNodeAction(
                c => CheckComplexity<AccessorDeclarationSyntax>(
                    c,
                    a => a,
                    a => a.Keyword.GetLocation(),
                    CSharpCognitiveComplexityMetric.GetComplexity,
                    "accessor",
                    PropertyThreshold),
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKindEx.InitAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterNodeAction(
               c => CheckComplexity<FieldDeclarationSyntax>(
                   c,
                   f => f,
                   f => f.Declaration.Variables[0].Identifier.GetLocation(),
                   CSharpCognitiveComplexityMetric.GetComplexity,
                   "field",
                   Threshold),
               SyntaxKind.FieldDeclaration);

            context.RegisterNodeAction(c =>
            {
                if (((LocalFunctionStatementSyntaxWrapper)c.Node).Modifiers.Any(SyntaxKind.StaticKeyword))
                {
                   CheckComplexity<SyntaxNode>(
                       c,
                       m => m,
                       m => ((LocalFunctionStatementSyntaxWrapper)m).Identifier.GetLocation(),
                       CSharpCognitiveComplexityMetric.GetComplexity,
                       "static local function",
                       Threshold);
                }
            },
            SyntaxKindEx.LocalFunctionStatement);
        }
    }
}
