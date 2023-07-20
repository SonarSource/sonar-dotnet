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
    public sealed class UseValueParameter : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3237";
        private const string MessageFormat = "Use the 'value' contextual keyword in this {0} accessor declaration.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var accessor = (AccessorDeclarationSyntax)c.Node;

                    if ((accessor.Body == null && accessor.ExpressionBody() == null)
                        || OnlyThrows(accessor)
                        || accessor.DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => IsAccessorValue(x, c.SemanticModel)))
                    {
                        return;
                    }

                    var interfaceMember = c.SemanticModel.GetDeclaredSymbol(accessor).GetInterfaceMember();
                    if (interfaceMember != null && accessor.Body?.Statements.Count == 0) // No need to check ExpressionBody, it can't be empty
                    {
                        return;
                    }

                    c.ReportIssue(CreateDiagnostic(Rule, accessor.Keyword.GetLocation(), GetAccessorType(accessor)));
                },
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKindEx.InitAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration);

        private static bool OnlyThrows(AccessorDeclarationSyntax accessor) =>
            (accessor.Body?.Statements.Count == 1 && accessor.Body.Statements[0] is ThrowStatementSyntax)
            || ThrowExpressionSyntaxWrapper.IsInstance(accessor.ExpressionBody()?.Expression);

        private static bool IsAccessorValue(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            if (identifier.Identifier.ValueText != "value")
            {
                return false;
            }

            return semanticModel.GetSymbolInfo(identifier).Symbol is IParameterSymbol { IsImplicitlyDeclared: true };
        }

        private static string GetAccessorType(AccessorDeclarationSyntax accessorDeclaration) =>
            accessorDeclaration.Parent.Parent switch
            {
                IndexerDeclarationSyntax _ => "indexer set",
                PropertyDeclarationSyntax _ => GetPropertyAccessorKind(accessorDeclaration),
                EventDeclarationSyntax _ => "event",
                _ => null
            };

        private static string GetPropertyAccessorKind(AccessorDeclarationSyntax accessorDeclaration) =>
            accessorDeclaration.IsKind(SyntaxKind.SetAccessorDeclaration) ? "property set" : "property init";
    }
}
