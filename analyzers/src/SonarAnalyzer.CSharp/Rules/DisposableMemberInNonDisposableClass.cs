/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
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
                        c.ReportIssue(Rule, classDeclaration.Node.Identifier, message);
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
