/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class MethodParameterUnused : MethodParameterUnusedBase
    {
        private const string MessageFormat = "Remove this unused procedure parameter '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodBlock = (MethodBlockBaseSyntax)c.Node;

                    // Bail-out if this is not a method we want to report on
                    // (only based on syntax checks)
                    if (methodBlock.BlockStatement == null ||
                        !HasAnyParameter(methodBlock) ||
                        IsEmptyMethod(methodBlock) ||
                        IsVirtualOrOverride(methodBlock) ||
                        IsInterfaceImplementation(methodBlock) ||
                        HasAnyAttribute(methodBlock) ||
                        OnlyThrowsNotImplementedException(methodBlock, c.SemanticModel))
                    {
                        return;
                    }

                    var unusedParameters = GetUnusedParameters(methodBlock);
                    if (unusedParameters.Count == 0)
                    {
                        return;
                    }

                    // Bail-out if this is not a method we want to report on
                    // (only based on symbols checks)
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodBlock);
                    if (methodSymbol == null ||
                        methodSymbol.IsAbstract ||
                        methodSymbol.IsMainMethod() ||
                        methodSymbol.IsEventHandler() ||
                        methodSymbol.GetEffectiveAccessibility() != Accessibility.Private)
                    {
                        return;
                    }

                    foreach (var parameter in unusedParameters)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, parameter.GetLocation(),
                            parameter.Identifier.Identifier.ValueText));
                    }
                },
                SyntaxKind.SubBlock,
                SyntaxKind.FunctionBlock);
        }

        private static bool HasAnyParameter(MethodBlockBaseSyntax method) =>
            method.BlockStatement.ParameterList != null
            && method.BlockStatement.ParameterList.Parameters.Count > 0;

        private static bool IsEmptyMethod(MethodBlockBaseSyntax method) =>
            method.Statements.Count == 0;

        private static bool IsVirtualOrOverride(MethodBlockBaseSyntax method) =>
             method.BlockStatement.Modifiers.Any(m =>
                m.IsKind(SyntaxKind.OverridesKeyword) ||
                m.IsKind(SyntaxKind.OverridableKeyword));

        private static bool IsInterfaceImplementation(MethodBlockBaseSyntax method) =>
            (method.BlockStatement as MethodStatementSyntax)?.ImplementsClause != null;

        private static bool HasAnyAttribute(MethodBlockBaseSyntax method) =>
            method.BlockStatement.AttributeLists.Count > 0;

        private static bool OnlyThrowsNotImplementedException(MethodBlockBaseSyntax method, SemanticModel semanticModel) =>
            method.Statements.Count == 1
            && method.Statements
                .OfType<ThrowStatementSyntax>()
                .Select(tss => tss.Expression)
                .OfType<ObjectCreationExpressionSyntax>()
                .Select(oces => semanticModel.GetSymbolInfo(oces).Symbol)
                .OfType<IMethodSymbol>()
                .Any(s => s != null && s.ContainingType.Is(KnownType.System_NotImplementedException));

        private static List<ParameterSyntax> GetUnusedParameters(MethodBlockBaseSyntax methodBlock)
        {
            var usedIdentifiers = GetAllUsedVarOrParameterIdentifierNames();

            return methodBlock.BlockStatement
                .ParameterList
                .Parameters
                .Where(p => p.Identifier?.Identifier.ValueText != null
                    && !usedIdentifiers.Contains(p.Identifier.Identifier.ValueText))
                .ToList();

            HashSet<string> GetAllUsedVarOrParameterIdentifierNames() =>
                methodBlock.Statements
                    .SelectMany(statement => statement.DescendantNodes())
                    .Where(node => node.IsKind(SyntaxKind.IdentifierName)
                        && IsVarOrParameter(node))
                    .Cast<IdentifierNameSyntax>()
                    .Select(ins => ins.Identifier.ValueText)
                    .WhereNotNull()
                    .ToHashSet();

            bool IsVarOrParameter(SyntaxNode node)
            {
                if (node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    return ((MemberAccessExpressionSyntax)node.Parent).Expression == node;
                }

                if (node.Parent.IsKind(SyntaxKind.ConditionalAccessExpression))
                {
                    return ((ConditionalAccessExpressionSyntax)node.Parent).Expression == node;
                }

                return true;
            }
        }
    }
}

