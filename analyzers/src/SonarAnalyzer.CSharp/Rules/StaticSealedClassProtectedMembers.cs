/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StaticSealedClassProtectedMembers : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2156";
        private const string MessageFormat = "Remove this 'protected' modifier.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
            {
                ReportDiagnostics(c, c.Node, ((BaseMethodDeclarationSyntax)c.Node).Modifiers);
            },
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration);

            context.RegisterNodeAction(c =>
            {
                ReportDiagnostics(c, c.Node, ((BasePropertyDeclarationSyntax)c.Node).Modifiers);
            },
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.EventDeclaration);

            context.RegisterNodeAction(c =>
            {
                var fieldDeclaration = (BaseFieldDeclarationSyntax)c.Node;

                ReportDiagnostics(c, fieldDeclaration.Declaration.Variables.FirstOrDefault(), fieldDeclaration.Modifiers);
            },
            SyntaxKind.FieldDeclaration,
            SyntaxKind.EventFieldDeclaration);
        }

        private static void ReportDiagnostics(SonarSyntaxNodeReportingContext context, SyntaxNode declaration, IEnumerable<SyntaxToken> modifiers)
        {
            var symbol = context.Model.GetDeclaredSymbol(declaration);
            if (symbol == null || symbol.IsOverride || !symbol.ContainingType.IsSealed)
            {
                return;
            }

            modifiers
                .Where(m => m.IsKind(SyntaxKind.ProtectedKeyword))
                .Select(m => Diagnostic.Create(rule, m.GetLocation()))
                .ToList()
                .ForEach(d => context.ReportIssue(d));
        }
    }
}
