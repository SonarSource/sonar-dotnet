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
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DisposableMemberInNonDisposableClass : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2931";
        private const string MessageFormat = "Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on {0}.";

        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                analysisContext =>
                {
                    var namedType = (INamedTypeSymbol)analysisContext.Symbol;
                    if (ShouldExclude(namedType))
                    {
                        return;
                    }

                    var message = GetMessage(namedType, analysisContext);
                    if (string.IsNullOrEmpty(message))
                    {
                        return;
                    }

                    var typeDeclarations = new CSharpRemovableDeclarationCollector(namedType, analysisContext.Compilation).TypeDeclarations;
                    foreach (var classDeclaration in typeDeclarations)
                    {
                        analysisContext.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, classDeclaration.SyntaxNode.Identifier.GetLocation(), message));
                    }
                },
                SymbolKind.NamedType);

        private static bool ShouldExclude(ITypeSymbol typeSymbol) =>
            !typeSymbol.IsClass()
            || typeSymbol.Implements(KnownType.System_IDisposable)
            || typeSymbol.Implements(KnownType.System_IAsyncDisposable);

        private static string GetMessage(INamespaceOrTypeSymbol namedType, SymbolAnalysisContext analysisContext)
        {
            var disposableFields = namedType.GetMembers()
                                            .OfType<IFieldSymbol>()
                                            .Where(fs => fs.IsNonStaticNonPublicDisposableField(analysisContext.Compilation.GetLanguageVersion())).ToHashSet();

            var disposableFieldsWithInitializer = disposableFields.Where(IsOwnerSinceDeclaration);

            var otherInitializationsOfFields = namedType.GetMembers()
                                                        .OfType<IMethodSymbol>()
                                                        .SelectMany(m => GetAssignmentsToFieldsIn(m, analysisContext.Compilation))
                                                        .Where(f => disposableFields.Contains(f));

            return string.Join(", ", disposableFieldsWithInitializer.Union(otherInitializationsOfFields)
                                                                    .Distinct()
                                                                    .Select(symbol => $"'{symbol.Name}'")
                                                                    .OrderBy(name => name));
        }

        private static IEnumerable<IFieldSymbol> GetAssignmentsToFieldsIn(ISymbol m, Compilation compilation)
        {
            if (m.DeclaringSyntaxReferences.Length != 1
                || !(m.DeclaringSyntaxReferences[0].GetSyntax() is BaseMethodDeclarationSyntax method)
                || !method.HasBodyOrExpressionBody())
            {
                return Enumerable.Empty<IFieldSymbol>();
            }

            var methodNodes = method.Body == null
                                  ? method.ExpressionBody().DescendantNodes()
                                  : method.Body.DescendantNodes();

            return methodNodes
                   .OfType<AssignmentExpressionSyntax>()
                   .Where(n => n.IsKind(SyntaxKind.SimpleAssignmentExpression) && n.Right is ObjectCreationExpressionSyntax)
                   .Select(n => compilation.GetSemanticModel(method.SyntaxTree).GetSymbolInfo(n.Left).Symbol)
                   .OfType<IFieldSymbol>();
        }

        private static bool IsOwnerSinceDeclaration(IFieldSymbol field) =>
            field.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax() is VariableDeclaratorSyntax varDeclarator
            && varDeclarator.Initializer?.Value is ObjectCreationExpressionSyntax;
    }
}
