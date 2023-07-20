/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisposableMemberInNonDisposableClass : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2931";
        private const string MessageFormat = "Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on {0}.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var namedType = (INamedTypeSymbol)c.Symbol;
                    if (ShouldExclude(namedType))
                    {
                        return;
                    }

                    var message = GetMessage(c, namedType);
                    if (string.IsNullOrEmpty(message))
                    {
                        return;
                    }

                    var typeDeclarations = new CSharpRemovableDeclarationCollector(namedType, c.Compilation).TypeDeclarations;
                    foreach (var classDeclaration in typeDeclarations)
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, classDeclaration.Node.Identifier.GetLocation(), message));
                    }
                },
                SymbolKind.NamedType);

        private static bool ShouldExclude(ITypeSymbol typeSymbol) =>
            !typeSymbol.IsClass()
            || typeSymbol.Implements(KnownType.System_IDisposable)
            || typeSymbol.Implements(KnownType.System_IAsyncDisposable);

        private static string GetMessage(SonarSymbolReportingContext context, INamespaceOrTypeSymbol namedType)
        {
            var disposableFields = namedType.GetMembers()
                                            .OfType<IFieldSymbol>()
                                            .Where(fs => fs.IsNonStaticNonPublicDisposableField(context.Compilation.GetLanguageVersion()))
                                            .ToHashSet();

            if (disposableFields.Count == 0)
            {
                return string.Empty;
            }

            var otherInitializationsOfFields = namedType.GetMembers()
                                                        .OfType<IMethodSymbol>()
                                                        .SelectMany(m => GetAssignmentsToFieldsIn(m, context.Compilation))
                                                        .Where(f => disposableFields.Contains(f));

            return disposableFields.Where(IsOwnerSinceDeclaration)
                                   .Union(otherInitializationsOfFields)
                                   .Distinct()
                                   .Select(symbol => $"'{symbol.Name}'")
                                   .OrderBy(name => name)
                                   .JoinAnd();
        }

        private static IEnumerable<IFieldSymbol> GetAssignmentsToFieldsIn(ISymbol m, Compilation compilation)
        {
            if (m.DeclaringSyntaxReferences.Length != 1
                || !(m.DeclaringSyntaxReferences[0].GetSyntax() is BaseMethodDeclarationSyntax method)
                || !method.HasBodyOrExpressionBody())
            {
                return Enumerable.Empty<IFieldSymbol>();
            }

            return method.GetBodyDescendantNodes()
                         .OfType<AssignmentExpressionSyntax>()
                         .Where(n => n.IsKind(SyntaxKind.SimpleAssignmentExpression) && n.Right is ObjectCreationExpressionSyntax)
                         .Select(n => compilation.GetSemanticModel(method.SyntaxTree).GetSymbolInfo(n.Left).Symbol)
                         .OfType<IFieldSymbol>();
        }

        private static bool IsOwnerSinceDeclaration(ISymbol symbol) =>
            symbol.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax() is VariableDeclaratorSyntax varDeclarator
            && varDeclarator.Initializer?.Value is ObjectCreationExpressionSyntax;
    }
}
