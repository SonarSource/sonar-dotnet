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
    public sealed class DoNotOverloadOperatorEqual : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3875";
        private const string MessageFormat = "Remove this overload of 'operator =='.";

        private static readonly ImmutableArray<KnownType> InterfacesRelyingOnOperatorEqualOverload =
            ImmutableArray.Create(
                KnownType.System_IComparable,
                KnownType.System_IComparable_T,
                KnownType.System_IEquatable_T,
                KnownType.System_Numerics_IEqualityOperators_TSelf_TOther_TResult);

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(CheckForIssue, SyntaxKind.OperatorDeclaration);

        private static void CheckForIssue(SonarSyntaxNodeReportingContext analysisContext)
        {
            var declaration = (OperatorDeclarationSyntax)analysisContext.Node;

            if (declaration.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken)
               && declaration.Parent is ClassDeclarationSyntax classDeclaration
               && !classDeclaration.ChildNodes()
                      .OfType<OperatorDeclarationSyntax>()
                      .Any(op => op.OperatorToken.IsKind(SyntaxKind.PlusToken)
                                 || op.OperatorToken.IsKind(SyntaxKind.MinusToken))
               && analysisContext.Model.GetDeclaredSymbol(classDeclaration) is { } namedTypeSymbol
               && !namedTypeSymbol.ImplementsAny(InterfacesRelyingOnOperatorEqualOverload))
            {
                analysisContext.ReportIssue(Rule, declaration.OperatorToken);
            }
        }
    }
}
