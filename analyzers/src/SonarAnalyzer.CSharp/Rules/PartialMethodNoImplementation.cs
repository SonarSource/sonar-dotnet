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
    public sealed class PartialMethodNoImplementation : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3251";
        private const string MessageFormat = "Supply an implementation for {0} partial method{1}.";
        private const string MessageAdditional = ", otherwise this call will be ignored";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(CheckForCandidatePartialDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterNodeAction(CheckForCandidatePartialInvocation, SyntaxKind.InvocationExpression);
        }

        private static void CheckForCandidatePartialDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var declaration = (MethodDeclarationSyntax)context.Node;
            var partialKeyword = declaration.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.PartialKeyword));

            if (partialKeyword != default
                && !declaration.HasBodyOrExpressionBody()
                && !declaration.Modifiers.Any(HasAccessModifier)
                && context.Model.GetDeclaredSymbol(declaration) is { } methodSymbol
                && methodSymbol.PartialImplementationPart == null)
            {
                context.ReportIssue(Rule, partialKeyword, "this", string.Empty);
            }
        }

        private static void CheckForCandidatePartialInvocation(SonarSyntaxNodeReportingContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.Parent is StatementSyntax statement
                && context.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                && methodSymbol.PartialImplementationPart == null
                && PartialMethodsWithoutAccessModifier(methodSymbol).Any())
            {
                context.ReportIssue(Rule, statement, "the", MessageAdditional);
            }
        }

        // from the method symbol it's not possible to tell if it's a partial method or not.
        // https://github.com/dotnet/roslyn/issues/48
        private static IEnumerable<MethodDeclarationSyntax> PartialMethodsWithoutAccessModifier(IMethodSymbol methodSymbol) =>
            methodSymbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Modifiers.Any(SyntaxKind.PartialKeyword)
                                 && !method.Modifiers.Any(HasAccessModifier)
                                 && method.Body == null
                                 && method.ExpressionBody == null);

        private static bool HasAccessModifier(SyntaxToken token) =>
            token.IsAnyKind(SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.PrivateKeyword);
    }
}
