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

namespace SonarAnalyzer.CSharp.Rules
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
                        || accessor.DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => IsAccessorValue(x, c.Model)))
                    {
                        return;
                    }

                    var interfaceMember = c.Model.GetDeclaredSymbol(accessor).InterfaceMembers();
                    if (interfaceMember.Any() && accessor.Body?.Statements.Count == 0) // No need to check ExpressionBody, it can't be empty
                    {
                        return;
                    }

                    c.ReportIssue(Rule, accessor.Keyword, GetAccessorType(accessor));
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
