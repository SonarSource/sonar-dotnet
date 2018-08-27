/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class MutableFieldsShouldNotBe : SonarDiagnosticAnalyzer
    {
        protected const string MessageFormat = "Use an immutable collection or reduce the accessibility of the field(s) {0}.";

        private static readonly ISet<KnownType> MutableBaseTypes =
            new HashSet<KnownType>
            {
                KnownType.System_Collections_Generic_ICollection_T,
                KnownType.System_Array
            };

        private static readonly ISet<KnownType> ImmutableBaseTypes =
            new HashSet<KnownType>
            {
                KnownType.System_Collections_ObjectModel_ReadOnlyCollection_T,
                KnownType.System_Collections_ObjectModel_ReadOnlyDictionary_TKey_TValue,
                KnownType.System_Collections_Immutable_IImmutableArray_T,
                KnownType.System_Collections_Immutable_IImmutableDictionary_TKey_TValue,
                KnownType.System_Collections_Immutable_IImmutableList_T,
                KnownType.System_Collections_Immutable_IImmutableSet_T,
                KnownType.System_Collections_Immutable_IImmutableStack_T,
                KnownType.System_Collections_Immutable_IImmutableQueue_T
            };

        protected abstract ISet<SyntaxKind> InvalidModifiers { get; }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForIssue,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration);
        }

        private void CheckForIssue(SyntaxNodeAnalysisContext analysisContext)
        {
            var typeDeclaration = (TypeDeclarationSyntax)analysisContext.Node;
            var fieldDeclarations = typeDeclaration.Members.OfType<FieldDeclarationSyntax>();

            var assignmentsImmutability = GetFieldAssignmentImmutability(typeDeclaration, fieldDeclarations,
                analysisContext.SemanticModel);

            foreach (var fieldDeclaration in fieldDeclarations)
            {
                if (!HasAllInvalidModifiers(fieldDeclaration) ||
                    fieldDeclaration.Declaration.Variables.Count == 0)
                {
                    return;
                }

                if (!(analysisContext.SemanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables[0]) is IFieldSymbol fieldSymbol) ||
                    fieldSymbol.Type == null ||
                    fieldSymbol.GetEffectiveAccessibility() != Accessibility.Public ||
                    IsImmutableOrValidMutableType(fieldSymbol.Type))
                {
                    return;
                }

                // The field seems to be violating the rule but we should exclude the cases where the field is read-only
                // and all initializations to this field are immutable
                var incorrectFieldVariables = CollectInvalidFieldVariables(fieldDeclaration, assignmentsImmutability,
                        analysisContext.SemanticModel)
                    .ToSentence(quoteWords: true);

                if (incorrectFieldVariables != null)
                {
                    analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0],
                        fieldDeclaration.Declaration.Type.GetLocation(), incorrectFieldVariables));
                }
            }
        }

        private bool HasAllInvalidModifiers(FieldDeclarationSyntax fieldDeclaration)
        {
            return fieldDeclaration.Modifiers.Count(m => InvalidModifiers.Contains(m.Kind())) == InvalidModifiers.Count;
        }

        private Dictionary<string, bool?> GetFieldAssignmentImmutability(TypeDeclarationSyntax typeDeclaration,
            IEnumerable<FieldDeclarationSyntax> fieldDeclarations, SemanticModel semanticModel)
        {
            var variableNames = fieldDeclarations.SelectMany(x => x.Declaration.Variables)
                .Select(x => x.Identifier.ValueText)
                .ToHashSet();

            var ctorAssignments = typeDeclaration.Members.OfType<ConstructorDeclarationSyntax>()
                .SelectMany(x => x.DescendantNodes())
                .OfType<AssignmentExpressionSyntax>();

            var variableToImmutability = variableNames.ToDictionary(x => x, x => (bool?)null);

            foreach (var assignment in ctorAssignments)
            {
                if (!(assignment.Left is IdentifierNameSyntax identifierName) ||
                    !variableNames.Contains(identifierName.Identifier.ValueText) ||
                    variableToImmutability[identifierName.Identifier.ValueText] == false)
                {
                    continue;
                }

                variableToImmutability[identifierName.Identifier.ValueText] =
                    IsImmutableOrValidMutableType(semanticModel.GetTypeInfo(assignment.Right).Type, assignment.Right);
            }

            return variableToImmutability;
        }

        private IEnumerable<string> CollectInvalidFieldVariables(FieldDeclarationSyntax fieldDeclaration,
            Dictionary<string, bool?> assignmentsInCtors, SemanticModel semanticModel)
        {
            if (!fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            {
                return fieldDeclaration.Declaration.Variables.Select(x => x.Identifier.ValueText);
            }

            return CollectReadonlyInvalidFieldVariables(fieldDeclaration, assignmentsInCtors, semanticModel);
        }

        private IEnumerable<string> CollectReadonlyInvalidFieldVariables(FieldDeclarationSyntax fieldDeclaration,
            Dictionary<string, bool?> assignmentsInCtors, SemanticModel semanticModel)
        {
            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                var onlyInitializedWithImmutablesInCtor = assignmentsInCtors[variable.Identifier.ValueText];

                if (onlyInitializedWithImmutablesInCtor == false)
                {
                    yield return variable.Identifier.ValueText;
                }

                if (variable.Initializer == null)
                {
                    continue;
                }

                if (!(semanticModel.GetSymbolInfo(variable.Initializer.Value).Symbol is IMethodSymbol methodSymbol))
                {
                    continue;
                }

                var typeSymbol = methodSymbol.MethodKind == MethodKind.Constructor
                    ? methodSymbol.ContainingType
                    : methodSymbol.ReturnType;

                if (!IsImmutableOrValidMutableType(typeSymbol, variable.Initializer.Value))
                {
                    yield return variable.Identifier.ValueText;
                }
            }
        }

        private bool IsImmutableOrValidMutableType(ITypeSymbol typeSymbol, ExpressionSyntax value = null)
        {
            if (value != null &&
                value.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return true;
            }

            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                typeSymbol = namedTypeSymbol.ConstructedFrom;
            }

            return !typeSymbol.DerivesOrImplementsAny(MutableBaseTypes) ||
                typeSymbol.DerivesOrImplementsAny(ImmutableBaseTypes);
        }
    }
}
