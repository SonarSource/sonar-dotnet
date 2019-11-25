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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DisposableMemberInNonDisposableClass : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2931";
        private const string MessageFormat = "Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);


        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                analysisContext =>
                {
                    var namedType = analysisContext.Symbol as INamedTypeSymbol;
                    if (!namedType.IsClass() || namedType.Implements(KnownType.System_IDisposable))
                    {
                        return;
                    }
                    var disposableFields = namedType.GetMembers().OfType<IFieldSymbol>()
                        .Where(IsNonStaticNonPublicDisposableField).ToHashSet();

                    var disposableFieldsWithInitializer = disposableFields
                        .Where(f => IsOwnerSinceDeclaration(f));

                    var otherInitializationsOfFields = namedType.GetMembers().OfType<IMethodSymbol>().
                        SelectMany(m => GetAssignementsToFieldsIn(m, analysisContext.Compilation)).
                        Where(f => disposableFields.Contains(f));

                    var message = string.Join(", ",
                        disposableFieldsWithInitializer.
                        Union(otherInitializationsOfFields).
                        Distinct().
                        Select(symbol => $"'{symbol.Name}'").
                        OrderBy(s => s));

                    if (!string.IsNullOrEmpty(message))
                    {
                        var typeDeclarations = new CSharpRemovableDeclarationCollector(namedType, analysisContext.Compilation).TypeDeclarations;
                        foreach (var classDeclaration in typeDeclarations)
                        {
                            analysisContext.ReportDiagnosticIfNonGenerated(
                                Diagnostic.Create(rule, classDeclaration.SyntaxNode.Identifier.GetLocation(), message));
                        }
                    }
                },
                SymbolKind.NamedType);
        }

        private IEnumerable<IFieldSymbol> GetAssignementsToFieldsIn(IMethodSymbol m, Compilation compilation)
        {
            var empty = Enumerable.Empty<IFieldSymbol>();
            if (m.DeclaringSyntaxReferences.Length != 1)
            {
                return empty;
            }
            if (m.DeclaringSyntaxReferences[0].GetSyntax() is BaseMethodDeclarationSyntax method)
            {
                if (!method.HasBodyOrExpressionBody())
                {
                    return empty;
                }
                var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
                var methodNodes = method.Body == null ?
                    method.ExpressionBody().DescendantNodes() :
                    method.Body.DescendantNodes();
                return methodNodes.
                    OfType<AssignmentExpressionSyntax>().
                    Where(n => n.IsKind(SyntaxKind.SimpleAssignmentExpression) && n.Right is ObjectCreationExpressionSyntax).
                    Select(n => semanticModel.GetSymbolInfo(n.Left).Symbol).
                    OfType<IFieldSymbol>();
            }
            return empty;
        }

        private bool IsOwnerSinceDeclaration(IFieldSymbol f)
        {
            var syntaxRef = f.DeclaringSyntaxReferences.SingleOrDefault();
            if (syntaxRef == null)
            {
                return false;
            }
            if (syntaxRef.GetSyntax() is VariableDeclaratorSyntax varDeclarator)
            {
                return varDeclarator.Initializer != null && varDeclarator.Initializer.Value is ObjectCreationExpressionSyntax;
            }
            return false;
        }

        internal static bool IsNonStaticNonPublicDisposableField(IFieldSymbol fieldSymbol)
        {
            return fieldSymbol != null &&
                   !fieldSymbol.IsStatic &&
                   (fieldSymbol.DeclaredAccessibility == Accessibility.Protected || fieldSymbol.DeclaredAccessibility == Accessibility.Private) &&
                   fieldSymbol.Type.Implements(KnownType.System_IDisposable);
        }
    }
}
