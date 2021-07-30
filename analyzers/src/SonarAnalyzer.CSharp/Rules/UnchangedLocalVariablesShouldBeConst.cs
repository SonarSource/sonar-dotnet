/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UnchangedLocalVariablesShouldBeConst : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3353";
        private const string MessageFormat = "Add the 'const' modifier to '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private enum DeclarationType
        {
            CannotBeConst,
            Value,
            Reference,
            String
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var localDeclaration = (LocalDeclarationStatementSyntax)c.Node;
                    if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
                    {
                        return;
                    }

                    var declaredType = FindDeclarationType(localDeclaration, c.SemanticModel);
                    if (declaredType == DeclarationType.CannotBeConst)
                    {
                        return;
                    }

                    localDeclaration?.Declaration?.Variables
                        .Where(v => v?.Identifier != null)
                        .Select(v => new { Syntax = v, Symbol = c.SemanticModel.GetDeclaredSymbol(v) })
                        .Where(ss => ss.Symbol != null)
                        .Where(ss => IsInitializedWithCompatibleConstant(ss.Syntax, c.SemanticModel, declaredType))
                        .Where(ss => !HasMutableUsagesInMethod(ss.Syntax, ss.Symbol, c.SemanticModel))
                        .ToList()
                        .ForEach(ss => Report(ss.Syntax, c));
                },
                SyntaxKind.LocalDeclarationStatement);

        private static DeclarationType FindDeclarationType(LocalDeclarationStatementSyntax localDeclaration, SemanticModel semanticModel)
        {
            var declaredTypeSyntax = localDeclaration.Declaration?.Type;
            if (declaredTypeSyntax == null)
            {
                return DeclarationType.CannotBeConst;
            }

            var declaredType = semanticModel.GetTypeInfo(declaredTypeSyntax).Type;
            if (declaredType == null)
            {
                return DeclarationType.CannotBeConst;
            }
            else if (declaredType.Is(KnownType.System_String))
            {
                return DeclarationType.String;
            }
            else if (declaredType.OriginalDefinition?.DerivesFrom(KnownType.System_Nullable_T) ?? false)
            {
                // Defining nullable as const raises error CS0283.
                return DeclarationType.CannotBeConst;
            }
            else if (declaredType.IsStruct() && declaredType.SpecialType == SpecialType.None && declaredType.GetMembers("op_Implicit").Any(x => !x.IsImplicitlyDeclared))
            {
                // Struct with explicitly declared "implicit operator"
                return DeclarationType.CannotBeConst;
            }
            else
            {
                return declaredType.IsValueType ? DeclarationType.Value : DeclarationType.Reference;
            }
        }

        private static bool IsInitializedWithCompatibleConstant(VariableDeclaratorSyntax variableDeclarator, SemanticModel semanticModel, DeclarationType declarationType)
        {
            var initializer = variableDeclarator?.Initializer?.Value;
            if (initializer == null)
            {
                return false;
            }
            var constantValueContainer = semanticModel.GetConstantValue(initializer);
            if (!constantValueContainer.HasValue)
            {
                return false;
            }

            var constantValue = constantValueContainer.Value;
            if (constantValue is string)
            {
                return declarationType == DeclarationType.String;
            }
            else if (constantValue is ValueType)
            {
                return declarationType == DeclarationType.Value;
            }
            else
            {
                return declarationType == DeclarationType.Reference || declarationType == DeclarationType.String;
            }
        }

        private static bool HasMutableUsagesInMethod(VariableDeclaratorSyntax variable, ISymbol variableSymbol, SemanticModel semanticModel)
        {
            var parentSyntax = variable.Ancestors().FirstOrDefault(IsMethodLike);
            if (parentSyntax == null)
            {
                return false;
            }
            else if (parentSyntax is GlobalStatementSyntax)
            {
                // If the variable is declared in a top level statement we should search inside the compilation unit.
                parentSyntax = parentSyntax.Parent;
            }

            return parentSyntax
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(MatchesIdentifier)
                .Any(x => IsMutatingUse(semanticModel, x));

            static bool IsMethodLike(SyntaxNode arg) =>
                arg is BaseMethodDeclarationSyntax
                || arg is IndexerDeclarationSyntax
                || arg is AccessorDeclarationSyntax
                || arg is LambdaExpressionSyntax
                || arg is GlobalStatementSyntax;

            bool MatchesIdentifier(IdentifierNameSyntax id) =>
                Equals(variableSymbol, semanticModel.GetSymbolInfo(id).Symbol);
        }

        private static bool IsMutatingUse(SemanticModel semanticModel, IdentifierNameSyntax identifier) =>
            identifier.Parent switch
            {
                AssignmentExpressionSyntax assignmentExpression => Equals(identifier, assignmentExpression?.Left),
                ArgumentSyntax argumentSyntax => argumentSyntax.IsInTupleAssignmentTarget() || !argumentSyntax.RefOrOutKeyword.IsKind(SyntaxKind.None),
                PostfixUnaryExpressionSyntax _ => true,
                PrefixUnaryExpressionSyntax _ => true,
                _ => IsUsedAsLambdaExpression(semanticModel, identifier)
            };

        private static bool IsUsedAsLambdaExpression(SemanticModel semanticModel, IdentifierNameSyntax identifier)
        {
            if (identifier.FirstAncestorOrSelf<LambdaExpressionSyntax>().GetSelfOrTopParenthesizedExpression() is { } lambda)
            {
                if (lambda.Parent is ArgumentSyntax argument
                    && argument.FirstAncestorOrSelf<InvocationExpressionSyntax>() is { } invocation)
                {
                    var lookup = new CSharpMethodParameterLookup(invocation, semanticModel);
                    return lookup.TryGetSymbol(argument, out var parameter) && parameter.IsType(KnownType.System_Linq_Expressions_Expression_T);
                }
                else if (lambda.Parent is AssignmentExpressionSyntax assignment)
                {
                    // Lambda cannot be on the left side, we don't need to check it
                    return assignment.Left.IsKnownType(KnownType.System_Linq_Expressions_Expression_T, semanticModel);
                }
            }
            return false;
        }

        private static void Report(VariableDeclaratorSyntax declaratorSyntax, SyntaxNodeAnalysisContext c) =>
            c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule,
                declaratorSyntax.Identifier.GetLocation(),
                declaratorSyntax.Identifier.ValueText));
    }
}
