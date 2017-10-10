/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    using MemberUsage = SyntaxNodeSymbolSemanticModelTuple<SimpleNameSyntax, ISymbol>;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class NotAssignedPrivateMember : SonarDiagnosticAnalyzer
    {
        /*
         CS0649 reports the same on internal fields. So that's wider in scope, but that's not a live Roslyn analyzer,
         the issue only shows up at build time and not during editing.
        */

        internal const string DiagnosticId = "S3459";
        private const string MessageFormat = "Remove unassigned {0} '{1}', or set its value.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly Accessibility maxAccessibility = Accessibility.Private;

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    var namedType = (INamedTypeSymbol)c.Symbol;
                    if (!namedType.IsClassOrStruct() ||
                        namedType.ContainingType != null)
                    {
                        return;
                    }

                    var removableDeclarationCollector = new RemovableDeclarationCollector(namedType, c.Compilation);

                    var candidateFields = removableDeclarationCollector.GetRemovableFieldLikeDeclarations(
                        ImmutableHashSet.Create(SyntaxKind.FieldDeclaration), maxAccessibility)
                        .Where(tuple => !IsInitializedOrFixed(((VariableDeclaratorSyntax)tuple.SyntaxNode)));

                    var candidateProperties = removableDeclarationCollector.GetRemovableDeclarations(
                        ImmutableHashSet.Create(SyntaxKind.PropertyDeclaration), maxAccessibility)
                        .Where(tuple => IsAutoPropertyWithNoInitializer((PropertyDeclarationSyntax)tuple.SyntaxNode));

                    var allCandidateMembers = candidateFields.Concat(candidateProperties).ToList();
                    if (!allCandidateMembers.Any())
                    {
                        return;
                    }

                    var usedMembers = GetMemberUsages(removableDeclarationCollector, new HashSet<ISymbol>(allCandidateMembers.Select(t => t.Symbol)));
                    var usedMemberSymbols = new HashSet<ISymbol>(usedMembers.Select(tuple => tuple.Symbol));

                    var assignedMemberSymbols = GetAssignedMemberSymbols(usedMembers);

                    foreach (var candidateMember in allCandidateMembers)
                    {
                        if (!usedMemberSymbols.Contains(candidateMember.Symbol))
                        {
                            /// reported by <see cref="UnusedPrivateMember"/>
                            continue;
                        }

                        if (!assignedMemberSymbols.Contains(candidateMember.Symbol))
                        {
                            var field = candidateMember.SyntaxNode as VariableDeclaratorSyntax;
                            var property = candidateMember.SyntaxNode as PropertyDeclarationSyntax;

                            var memberType = field != null ? "field" : "auto-property";

                            var location = field != null
                                ? field.Identifier.GetLocation()
                                : property.Identifier.GetLocation();

                            c.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, location, memberType, candidateMember.Symbol.Name));
                        }
                    }
                },
                SymbolKind.NamedType);
        }

        private static bool IsInitializedOrFixed(VariableDeclaratorSyntax declarator)
        {
            var fieldDeclaration = declarator.Parent.Parent as BaseFieldDeclarationSyntax;
            if (fieldDeclaration != null &&
                fieldDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.FixedKeyword)))
            {
                return true;
            }

            return declarator.Initializer != null;
        }

        private static bool IsAutoPropertyWithNoInitializer(PropertyDeclarationSyntax declaration)
        {
            return declaration.Initializer == null &&
                declaration.AccessorList != null &&
                declaration.AccessorList.Accessors.All(acc => acc.Body == null);
        }

        private static IList<MemberUsage> GetMemberUsages(RemovableDeclarationCollector removableDeclarationCollector,
            HashSet<ISymbol> declaredPrivateSymbols)
        {
            var symbolNames = declaredPrivateSymbols.Select(s => s.Name).ToImmutableHashSet();

            var identifiers = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .Where(node => node.IsKind(SyntaxKind.IdentifierName))
                    .Cast<IdentifierNameSyntax>()
                    .Where(node => symbolNames.Contains(node.Identifier.ValueText))
                    .Select(node =>
                        new MemberUsage
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel,
                            Symbol = container.SemanticModel.GetSymbolInfo(node).Symbol
                        }));

            var generic = removableDeclarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .Where(node => node.IsKind(SyntaxKind.GenericName))
                    .Cast<GenericNameSyntax>()
                    .Where(node => symbolNames.Contains(node.Identifier.ValueText))
                    .Select(node =>
                        new MemberUsage
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel,
                            Symbol = container.SemanticModel.GetSymbolInfo(node).Symbol
                        }));

            return identifiers.Concat(generic)
                .Where(tuple => tuple.Symbol is IFieldSymbol || tuple.Symbol is IPropertySymbol)
                .ToList();
        }

        private static ISet<ISymbol> GetAssignedMemberSymbols(IList<MemberUsage> memberUsages)
        {
            var assignedMembers = new HashSet<ISymbol>();

            foreach (var memberUsage in memberUsages)
            {
                ExpressionSyntax node = memberUsage.SyntaxNode;
                var memberSymbol = memberUsage.Symbol;

                // Handle "expr.FieldName"
                var simpleMemberAccess = node.Parent as MemberAccessExpressionSyntax;
                if (simpleMemberAccess != null &&
                    simpleMemberAccess.Name == node)
                {
                    node = simpleMemberAccess;
                }

                // Handle "((expr.FieldName))"
                node = node.GetSelfOrTopParenthesizedExpression();

                if (IsValueType(memberSymbol))
                {
                    // Handle (((exp.FieldName)).Member1).Member2
                    var parentMemberAccess = node.Parent as MemberAccessExpressionSyntax;
                    while (IsParentMemberAccess(parentMemberAccess, node))
                    {
                        node = parentMemberAccess.GetSelfOrTopParenthesizedExpression();
                        parentMemberAccess = node.Parent as MemberAccessExpressionSyntax;
                    }

                    node = node.GetSelfOrTopParenthesizedExpression();
                }

                var parentNode = node.Parent;

                if (PreOrPostfixOpSyntaxKinds.Contains(parentNode.Kind()))
                {
                    assignedMembers.Add(memberSymbol);
                    continue;
                }

                var assignment = parentNode as AssignmentExpressionSyntax;
                if (assignment != null)
                {
                    if (assignment.Left == node)
                    {
                        assignedMembers.Add(memberSymbol);
                    }

                    continue;
                }

                var argument = parentNode as ArgumentSyntax;
                if (argument != null &&
                    !argument.RefOrOutKeyword.IsKind(SyntaxKind.None))
                {
                    assignedMembers.Add(memberSymbol);
                }
            }

            return assignedMembers;
        }

        private static bool IsParentMemberAccess(MemberAccessExpressionSyntax parent, ExpressionSyntax node)
        {
            return parent != null &&
                parent.Expression == node;
        }

        private static bool IsValueType(ISymbol symbol)
        {
            var field = symbol as IFieldSymbol;
            if (field != null)
            {
                return field.Type.IsValueType;
            }

            var property = symbol as IPropertySymbol;
            if (property != null)
            {
                return property.Type.IsValueType;
            }

            return false;
        }

        private static readonly ISet<SyntaxKind> PreOrPostfixOpSyntaxKinds = ImmutableHashSet.Create(
            SyntaxKind.PostDecrementExpression,
            SyntaxKind.PostIncrementExpression,
            SyntaxKind.PreDecrementExpression,
            SyntaxKind.PreIncrementExpression);
    }
}
