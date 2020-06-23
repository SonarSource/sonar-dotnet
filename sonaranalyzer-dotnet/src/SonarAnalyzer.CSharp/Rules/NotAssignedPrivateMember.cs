/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    using MemberUsage = SyntaxNodeSymbolSemanticModelTuple<SimpleNameSyntax, ISymbol>;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class NotAssignedPrivateMember : SonarDiagnosticAnalyzer
    {
        /*
         CS0649 reports the same on internal fields. So that's wider in scope, but that's not a live Roslyn analyzer,
         the issue only shows up at build time and not during editing.
        */

        internal const string DiagnosticId = "S3459";
        private const string MessageFormat = "Remove unassigned {0} '{1}', or set its value.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly Accessibility maxAccessibility = Accessibility.Private;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    var namedType = (INamedTypeSymbol)c.Symbol;
                    if (!namedType.IsClassOrStruct() ||
                        HasStructLayoutAttribute(namedType) ||
                        namedType.ContainingType != null)
                    {
                        return;
                    }

                    var removableDeclarationCollector = new CSharpRemovableDeclarationCollector(namedType, c.Compilation);

                    var candidateFields = removableDeclarationCollector.GetRemovableFieldLikeDeclarations(
                        new HashSet<SyntaxKind> { SyntaxKind.FieldDeclaration }, maxAccessibility)
                        .Where(tuple => !IsInitializedOrFixed((VariableDeclaratorSyntax)tuple.SyntaxNode) &&
                                        !HasStructLayoutAttribute(tuple.Symbol.ContainingType));

                    var candidateProperties = removableDeclarationCollector.GetRemovableDeclarations(
                        new HashSet<SyntaxKind> { SyntaxKind.PropertyDeclaration }, maxAccessibility)
                        .Where(tuple => IsAutoPropertyWithNoInitializer((PropertyDeclarationSyntax)tuple.SyntaxNode) &&
                                        !HasStructLayoutAttribute(tuple.Symbol.ContainingType));

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

        private static bool HasStructLayoutAttribute(ISymbol namedTypeSymbol) =>
            namedTypeSymbol.HasAttribute(KnownType.System_Runtime_InteropServices_StructLayoutAttribute);

        private static bool IsInitializedOrFixed(VariableDeclaratorSyntax declarator)
        {
            if (declarator.Parent.Parent is BaseFieldDeclarationSyntax fieldDeclaration &&
                fieldDeclaration.Modifiers.Any(SyntaxKind.FixedKeyword))
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

        private static IList<MemberUsage> GetMemberUsages(CSharpRemovableDeclarationCollector removableDeclarationCollector,
            HashSet<ISymbol> declaredPrivateSymbols)
        {
            var symbolNames = declaredPrivateSymbols.Select(s => s.Name).ToHashSet();

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
                if (node.Parent is MemberAccessExpressionSyntax simpleMemberAccess &&
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

                if (parentNode is AssignmentExpressionSyntax assignment)
                {
                    if (assignment.Left == node)
                    {
                        assignedMembers.Add(memberSymbol);
                    }

                    continue;
                }

                if (parentNode is ArgumentSyntax argument &&
                    (!argument.RefOrOutKeyword.IsKind(SyntaxKind.None) || TupleExpressionSyntaxWrapper.IsInstance(argument.Parent)))
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
            if (symbol is IFieldSymbol field)
            {
                return field.Type.IsValueType;
            }

            if (symbol is IPropertySymbol property)
            {
                return property.Type.IsValueType;
            }

            return false;
        }

        private static readonly ISet<SyntaxKind> PreOrPostfixOpSyntaxKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.PostDecrementExpression,
            SyntaxKind.PostIncrementExpression,
            SyntaxKind.PreDecrementExpression,
            SyntaxKind.PreIncrementExpression
        };
    }
}
