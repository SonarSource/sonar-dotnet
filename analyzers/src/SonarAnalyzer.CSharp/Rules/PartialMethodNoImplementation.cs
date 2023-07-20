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
                && context.SemanticModel.GetDeclaredSymbol(declaration) is { } methodSymbol
                && methodSymbol.PartialImplementationPart == null)
            {
                context.ReportIssue(CreateDiagnostic(Rule, partialKeyword.GetLocation(), "this", string.Empty));
            }
        }

        private static void CheckForCandidatePartialInvocation(SonarSyntaxNodeReportingContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.Parent is StatementSyntax statement
                && context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                && methodSymbol.PartialImplementationPart == null
                && PartialMethodsWithoutAccessModifier(methodSymbol).Any())
            {
                context.ReportIssue(CreateDiagnostic(Rule, statement.GetLocation(), "the", MessageAdditional));
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
