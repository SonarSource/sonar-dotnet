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
               && analysisContext.SemanticModel.GetDeclaredSymbol(classDeclaration) is { } namedTypeSymbol
               && !namedTypeSymbol.ImplementsAny(InterfacesRelyingOnOperatorEqualOverload))
            {
                analysisContext.ReportIssue(CreateDiagnostic(Rule, declaration.OperatorToken.GetLocation()));
            }
        }
    }
}
