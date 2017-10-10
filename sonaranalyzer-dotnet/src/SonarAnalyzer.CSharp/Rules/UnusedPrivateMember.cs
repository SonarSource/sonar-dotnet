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

using System;
using System.Collections.Concurrent;
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
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class UnusedPrivateMember : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1144";
        private const string MessageFormat = "Remove the unused {0} {1} '{2}'.";
        private const IdeVisibility ideVisibility = IdeVisibility.Hidden;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, ideVisibility, RspecStrings.ResourceManager)
                                       .WithSeverity(Severity.Info);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly Accessibility maxAccessibility = Accessibility.Private;

        private ConcurrentBag<Diagnostic> possibleUnusedInternalMembers;

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    var shouldRaise = true;
                    possibleUnusedInternalMembers = new ConcurrentBag<Diagnostic>();

                    c.RegisterSemanticModelAction(
                        cc =>
                        {
                            var isInternalsVisibleToAttributeFound = cc.SemanticModel.SyntaxTree.GetRoot()
                                .DescendantNodes()
                                .OfType<AttributeListSyntax>()
                                .SelectMany(list => list.Attributes)
                                .Any(a => IsInternalVisibleToAttribute(a, cc.SemanticModel));
                            if (isInternalsVisibleToAttributeFound)
                            {
                                shouldRaise = false;
                            }
                        });

                    c.RegisterSymbolAction(
                        cc =>
                        {
                            var namedType = (INamedTypeSymbol)cc.Symbol;
                            if (!namedType.IsClassOrStruct() ||
                                namedType.ContainingType != null)
                            {
                                return;
                            }

                            var declarationCollector = new RemovableDeclarationCollector(namedType, cc.Compilation);

                            var declaredPrivateSymbols = new HashSet<ISymbol>();
                            var fieldLikeSymbols = new BidirectionalDictionary<ISymbol, SyntaxNode>();

                            CollectRemovableNamedTypes(declarationCollector, declaredPrivateSymbols);
                            CollectRemovableFieldLikeDeclarations(declarationCollector, declaredPrivateSymbols, fieldLikeSymbols);
                            CollectRemovableEventsAndProperties(declarationCollector, declaredPrivateSymbols);
                            CollectRemovableMethods(declarationCollector, declaredPrivateSymbols);

                            if (!declaredPrivateSymbols.Any())
                            {
                                return;
                            }

                            var usedSymbols = new HashSet<ISymbol>();
                            var emptyConstructors = new HashSet<ISymbol>();

                            var propertyAccessorAccess = new Dictionary<IPropertySymbol, AccessorAccess>();

                            CollectUsedSymbols(declarationCollector, usedSymbols, declaredPrivateSymbols, propertyAccessorAccess);
                            CollectUsedSymbolsFromCtorInitializerAndCollectEmptyCtors(declarationCollector,
                                usedSymbols, emptyConstructors);

                            ReportIssues(cc, usedSymbols, declaredPrivateSymbols, emptyConstructors, fieldLikeSymbols);
                            ReportUnusedPropertyAccessors(cc, usedSymbols, declaredPrivateSymbols, propertyAccessorAccess);
                        },
                        SymbolKind.NamedType);

                    c.RegisterCompilationEndAction(
                       cc =>
                       {
                           if (!shouldRaise)
                           {
                               return;
                           }

                           foreach (var diagnostic in possibleUnusedInternalMembers)
                           {
                               cc.ReportDiagnosticIfNonGenerated(diagnostic, cc.Compilation);
                           }
                       });
                });
        }

        private static bool IsInternalVisibleToAttribute(AttributeSyntax attribute, SemanticModel semanticModel)
        {
            var attributeConstructor = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;

            return attributeConstructor != null &&
                attributeConstructor.ContainingType.Is(KnownType.System_Runtime_CompilerServices_InternalsVisibleToAttribute);
        }

        private static void ReportUnusedPropertyAccessors(SymbolAnalysisContext context, HashSet<ISymbol> usedSymbols,
            HashSet<ISymbol> declaredPrivateSymbols,
            Dictionary<IPropertySymbol, AccessorAccess> propertyAccessorAccess)
        {
            var usedPrivatePropertis = declaredPrivateSymbols.Intersect(usedSymbols).OfType<IPropertySymbol>();
            var onlyOneAccessorAccessed = usedPrivatePropertis.Where(p => propertyAccessorAccess.ContainsKey(p));

            foreach (var property in onlyOneAccessorAccessed)
            {
                var access = propertyAccessorAccess[property];
                if (access == AccessorAccess.None || access == AccessorAccess.Both)
                {
                    continue;
                }

                if (access == AccessorAccess.Get && property.SetMethod != null)
                {
                    var accessorSyntax = GetAccessorSyntax(property.SetMethod);
                    if (accessorSyntax != null)
                    {
                        context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, accessorSyntax.GetLocation(),
                            "private", "set accessor in property", property.Name), context.Compilation);
                    }
                    continue;
                }

                if (access == AccessorAccess.Set && property.GetMethod != null)
                {
                    var accessorSyntax = GetAccessorSyntax(property.GetMethod);
                    if (accessorSyntax != null && accessorSyntax.Body != null)
                    {
                        context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, accessorSyntax.GetLocation(),
                            "private", "get accessor in property", property.Name), context.Compilation);
                    }
                }
            }
        }

        private static AccessorDeclarationSyntax GetAccessorSyntax(IMethodSymbol methodSymbol)
        {
            return methodSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as AccessorDeclarationSyntax;
        }

        private void ReportIssues(SymbolAnalysisContext context, HashSet<ISymbol> usedSymbols,
            HashSet<ISymbol> declaredPrivateSymbols, HashSet<ISymbol> emptyConstructors,
            BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            var unusedSymbols = declaredPrivateSymbols
                .Except(usedSymbols.Union(emptyConstructors))
                .ToList();

            var alreadyReportedFieldLikeSymbols = new HashSet<ISymbol>();

            var unusedSymbolSyntaxPairs = unusedSymbols
                .SelectMany(unusedSymbol => unusedSymbol.DeclaringSyntaxReferences
                    .Select(r =>
                        new
                        {
                            Syntax = r.GetSyntax(),
                            Symbol = unusedSymbol
                        }));

            foreach (var unused in unusedSymbolSyntaxPairs)
            {
                var location = unused.Syntax.GetLocation();

                var canBeFieldLike = unused.Symbol is IFieldSymbol || unused.Symbol is IEventSymbol;
                if (canBeFieldLike)
                {
                    if (alreadyReportedFieldLikeSymbols.Contains(unused.Symbol))
                    {
                        continue;
                    }

                    var variableDeclaration = GetVariableDeclaration(unused.Syntax);
                    if (variableDeclaration == null)
                    {
                        continue;
                    }

                    var declarations = variableDeclaration.Variables
                        .Select(v => fieldLikeSymbols.GetByB(v))
                        .ToList();

                    if (declarations.All(d => unusedSymbols.Contains(d)))
                    {
                        location = unused.Syntax.Parent.Parent.GetLocation();
                        alreadyReportedFieldLikeSymbols.UnionWith(declarations);
                    }
                }

                var memberKind = GetMemberType(unused.Symbol);
                var memberName = GetMemberName(unused.Symbol);
                var effectiveAccessibility = unused.Symbol.GetEffectiveAccessibility();

                if (effectiveAccessibility == Accessibility.Internal)
                {
                    // We can't report internal members directly because they might be used through another assembly.
                    possibleUnusedInternalMembers.Add(
                        Diagnostic.Create(rule, location, "internal", memberKind, memberName));
                    continue;
                }

                if (effectiveAccessibility == Accessibility.Private)
                {
                    context.ReportDiagnosticIfNonGenerated(
                        Diagnostic.Create(rule, location, "private", memberKind, memberName), context.Compilation);
                }
            }
        }

        private static void CollectRemovableMethods(RemovableDeclarationCollector declarationCollector,
            HashSet<ISymbol> declaredPrivateSymbols)
        {
            var methodSymbols = declarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes(RemovableDeclarationCollector.IsNodeContainerTypeDeclaration)
                    .Where(node =>
                        node.IsKind(SyntaxKind.MethodDeclaration) ||
                        node.IsKind(SyntaxKind.ConstructorDeclaration))
                    .Select(node =>
                        new SyntaxNodeSemanticModelTuple<SyntaxNode>
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel
                        }))
                    .Select(node => node.SemanticModel.GetDeclaredSymbol(node.SyntaxNode) as IMethodSymbol)
                    .Where(method => RemovableDeclarationCollector.IsRemovable(method, maxAccessibility));

            declaredPrivateSymbols.UnionWith(methodSymbols);
        }

        private static void CollectRemovableEventsAndProperties(RemovableDeclarationCollector helper,
            HashSet<ISymbol> declaredPrivateSymbols)
        {
            var declarationKinds = ImmutableHashSet.Create(SyntaxKind.EventDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration);
            var declarations = helper.GetRemovableDeclarations(declarationKinds, maxAccessibility);
            declaredPrivateSymbols.UnionWith(declarations.Select(d => d.Symbol));
        }

        private static void CollectRemovableFieldLikeDeclarations(RemovableDeclarationCollector declarationCollector,
            HashSet<ISymbol> declaredPrivateSymbols, BidirectionalDictionary<ISymbol, SyntaxNode> fieldLikeSymbols)
        {
            var declarationKinds = ImmutableHashSet.Create(SyntaxKind.FieldDeclaration, SyntaxKind.EventFieldDeclaration);
            var removableFieldsDefinitions = declarationCollector.GetRemovableFieldLikeDeclarations(declarationKinds, maxAccessibility);

            foreach (var fieldsDefinitions in removableFieldsDefinitions)
            {
                declaredPrivateSymbols.Add(fieldsDefinitions.Symbol);
                fieldLikeSymbols.Add(fieldsDefinitions.Symbol, fieldsDefinitions.SyntaxNode);
            }
        }

        private static void CollectRemovableNamedTypes(RemovableDeclarationCollector declarationCollector,
            HashSet<ISymbol> declaredPrivateSymbols)
        {
            var symbols = declarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes(RemovableDeclarationCollector.IsNodeContainerTypeDeclaration)
                    .Where(node =>
                        node.IsKind(SyntaxKind.ClassDeclaration) ||
                        node.IsKind(SyntaxKind.InterfaceDeclaration) ||
                        node.IsKind(SyntaxKind.StructDeclaration) ||
                        node.IsKind(SyntaxKind.DelegateDeclaration))
                    .Select(node =>
                        new SyntaxNodeSemanticModelTuple<SyntaxNode>
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel
                        }))
                    .Select(node => node.SemanticModel.GetDeclaredSymbol(node.SyntaxNode))
                    .Where(symbol => RemovableDeclarationCollector.IsRemovable(symbol, Accessibility.Internal));

            declaredPrivateSymbols.UnionWith(symbols);
        }

        private static void CollectUsedSymbolsFromCtorInitializerAndCollectEmptyCtors(
            RemovableDeclarationCollector declarationCollector,
            HashSet<ISymbol> usedSymbols, HashSet<ISymbol> emptyConstructors)
        {
            var ctors = declarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes(RemovableDeclarationCollector.IsNodeStructOrClassDeclaration)
                    .Where(node => node.IsKind(SyntaxKind.ConstructorDeclaration))
                    .Select(node =>
                        new SyntaxNodeSemanticModelTuple<ConstructorDeclarationSyntax>
                        {
                            SyntaxNode = (ConstructorDeclarationSyntax)node,
                            SemanticModel = container.SemanticModel
                        }));

            foreach (var ctor in ctors)
            {
                if (ctor.SyntaxNode.Body == null ||
                    !ctor.SyntaxNode.Body.Statements.Any())
                {
                    var ctorSymbol = ctor.SemanticModel.GetDeclaredSymbol(ctor.SyntaxNode);
                    if (ctorSymbol != null &&
                        !ctorSymbol.Parameters.Any())
                    {
                        emptyConstructors.Add(ctorSymbol.OriginalDefinition);
                    }
                }

                if (ctor.SyntaxNode.Initializer == null)
                {
                    continue;
                }

                usedSymbols.UnionWith(GetAllCandidateSymbols(ctor.SemanticModel.GetSymbolInfo(ctor.SyntaxNode.Initializer)));
            }
        }

        private static void CollectUsedSymbols(RemovableDeclarationCollector declarationCollector,
            HashSet<ISymbol> usedSymbols, HashSet<ISymbol> declaredPrivateSymbols,
            Dictionary<IPropertySymbol, AccessorAccess> propertyAccessorAccess)
        {
            var symbolNames = declaredPrivateSymbols.Select(s => s.Name).ToImmutableHashSet();
            var anyRemovableIndexers = declaredPrivateSymbols
                .OfType<IPropertySymbol>()
                .Any(p => p.IsIndexer);
            var anyRemovableCtors = declaredPrivateSymbols
                .OfType<IMethodSymbol>()
                .Any(m => m.MethodKind == MethodKind.Constructor);

            var identifiers = declarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .Where(node => node.IsKind(SyntaxKind.IdentifierName))
                    .Cast<IdentifierNameSyntax>()
                    .Where(node => symbolNames.Contains(node.Identifier.ValueText))
                    .Select(node =>
                        new SyntaxNodeSemanticModelTuple<ExpressionSyntax>
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel
                        }));

            var generic = declarationCollector.TypeDeclarations
                .SelectMany(container => container.SyntaxNode.DescendantNodes()
                    .Where(node => node.IsKind(SyntaxKind.GenericName))
                    .Cast<GenericNameSyntax>()
                    .Where(node => symbolNames.Contains(node.Identifier.ValueText))
                    .Select(node =>
                        new SyntaxNodeSemanticModelTuple<ExpressionSyntax>
                        {
                            SyntaxNode = node,
                            SemanticModel = container.SemanticModel
                        }));

            var allNodes = identifiers.Concat(generic);

            if (anyRemovableIndexers)
            {
                var nodes = declarationCollector.TypeDeclarations
                    .SelectMany(container => container.SyntaxNode.DescendantNodes()
                        .Where(node => node.IsKind(SyntaxKind.ElementAccessExpression))
                        .Cast<ElementAccessExpressionSyntax>()
                        .Select(node =>
                            new SyntaxNodeSemanticModelTuple<ExpressionSyntax>
                            {
                                SyntaxNode = node,
                                SemanticModel = container.SemanticModel
                            }));

                allNodes = allNodes.Concat(nodes);
            }

            if (anyRemovableCtors)
            {
                var nodes = declarationCollector.TypeDeclarations
                    .SelectMany(container => container.SyntaxNode.DescendantNodes()
                        .Where(node => node.IsKind(SyntaxKind.ObjectCreationExpression))
                        .Cast<ObjectCreationExpressionSyntax>()
                        .Select(node =>
                            new SyntaxNodeSemanticModelTuple<ExpressionSyntax>
                            {
                                SyntaxNode = node,
                                SemanticModel = container.SemanticModel
                            }));

                allNodes = allNodes.Concat(nodes);
            }

            var candidateSymbols = allNodes
                .Select(n => new { Syntax = n.SyntaxNode, SemanticModel = n.SemanticModel, Symbol = n.SemanticModel.GetSymbolInfo(n.SyntaxNode) })
                .Select(n => new
                {
                    Syntax = n.Syntax,
                    SemanticModel = n.SemanticModel,
                    Symbols = GetAllCandidateSymbols(n.Symbol)
                        .Select(s => GetOriginalSymbol(s))
                        .Where(s => s != null)
                });

            foreach (var candidateSymbol in candidateSymbols)
            {
                usedSymbols.UnionWith(candidateSymbol.Symbols);

                var accessorKind = GetAccessorAccessKind(candidateSymbol.Syntax, candidateSymbol.SemanticModel);

                foreach (var propertySymbol in candidateSymbol.Symbols.OfType<IPropertySymbol>())
                {
                    if (propertyAccessorAccess.ContainsKey(propertySymbol))
                    {
                        propertyAccessorAccess[propertySymbol] |= accessorKind;
                    }
                    else
                    {
                        propertyAccessorAccess[propertySymbol] = accessorKind;
                    }
                }
            }
        }

        private static AccessorAccess GetAccessorAccessKind(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var accessExpression = GetParentMemberAccess(expression).GetSelfOrTopParenthesizedExpression();

            var assignment = accessExpression.Parent as AssignmentExpressionSyntax;
            if (assignment != null &&
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return assignment.Left == accessExpression
                    ? AccessorAccess.Set
                    : AccessorAccess.Get;
            }

            if (assignment != null ||
                IncrementKinds.Contains(accessExpression.Parent.Kind()) ||
                accessExpression.IsInNameofCall(semanticModel))
            {
                return AccessorAccess.Both;
            }

            return AccessorAccess.Get;
        }

        private static ExpressionSyntax GetParentMemberAccess(ExpressionSyntax expression)
        {
            var parent = expression.Parent as MemberAccessExpressionSyntax;
            if (parent == null)
            {
                return expression;
            }

            if (parent.Expression == expression)
            {
                return expression;
            }

            return parent;
        }

        private static readonly ISet<SyntaxKind> IncrementKinds = ImmutableHashSet.Create(
            SyntaxKind.PostIncrementExpression,
            SyntaxKind.PreIncrementExpression,
            SyntaxKind.PostDecrementExpression,
            SyntaxKind.PreDecrementExpression);

        [Flags]
        private enum AccessorAccess
        {
            None = 0,
            Get = 1,
            Set = 2,
            Both = Get | Set
        }

        private static ISymbol GetOriginalSymbol(ISymbol candidateSymbol)
        {
            var symbol = candidateSymbol;
            var methodSymbol = symbol as IMethodSymbol;

            if (methodSymbol?.MethodKind == MethodKind.ReducedExtension)
            {
                symbol = methodSymbol.ReducedFrom;
            }

            return symbol?.OriginalDefinition;
        }

        private static IEnumerable<ISymbol> GetAllCandidateSymbols(SymbolInfo symbolInfo)
        {
            return new[] { symbolInfo.Symbol }
                .Concat(symbolInfo.CandidateSymbols)
                .Where(s => s != null);
        }

        private static VariableDeclarationSyntax GetVariableDeclaration(SyntaxNode syntax)
        {
            var fieldDeclaration = syntax.Parent.Parent as FieldDeclarationSyntax;
            if (fieldDeclaration != null)
            {
                return fieldDeclaration.Declaration;
            }

            var eventFieldDeclaration = syntax.Parent.Parent as EventFieldDeclarationSyntax;
            return eventFieldDeclaration?.Declaration;
        }

        private static string GetMemberType(ISymbol symbol)
        {
            if (symbol.IsConstructor())
            {
                return "constructor";
            }

            switch (symbol.Kind)
            {
                case SymbolKind.Event:
                case SymbolKind.Field:
                case SymbolKind.Method:
                case SymbolKind.Property:
                    return symbol.Kind.ToString().ToLowerInvariant();
                case SymbolKind.NamedType:
                    return "type";
                default:
                    return "member";
            }
        }

        private static string GetMemberName(ISymbol symbol)
        {
            return symbol.IsConstructor() ? symbol.ContainingType.Name : symbol.Name;
        }
    }
}