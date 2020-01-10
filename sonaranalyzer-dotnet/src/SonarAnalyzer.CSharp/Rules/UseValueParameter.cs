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
    public sealed class UseValueParameter : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3237";
        private const string MessageFormat = "Use the 'value' parameter in this {0} accessor declaration.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var accessor = (AccessorDeclarationSyntax)c.Node;

                    if ((accessor.Body == null && accessor.ExpressionBody() == null) ||
                        OnlyThrows(accessor) ||
                        accessor.DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => IsAccessorValue(x, c.SemanticModel)))
                    {
                        return;
                    }

                    var interfaceMember = c.SemanticModel.GetDeclaredSymbol(accessor).GetInterfaceMember();
                    if (interfaceMember != null &&
                        accessor.Body?.Statements.Count == 0) // No need to check ExpressionBody, it can't be empty
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, accessor.Keyword.GetLocation(),
                        GetAccessorType(accessor)));
                },
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration);
        }

        private static bool OnlyThrows(AccessorDeclarationSyntax accessor) =>
            (accessor.Body?.Statements.Count == 1 &&
            accessor.Body.Statements[0] is ThrowStatementSyntax) ||
            ThrowExpressionSyntaxWrapper.IsInstance(accessor.ExpressionBody()?.Expression);

        private static bool IsAccessorValue(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            if (identifier.Identifier.ValueText != "value")
            {
                return false;
            }

            return semanticModel.GetSymbolInfo(identifier).Symbol is IParameterSymbol parameter &&
                parameter.IsImplicitlyDeclared;
        }

        private static string GetAccessorType(AccessorDeclarationSyntax accessorDeclaration)
        {
            if (accessorDeclaration.IsKind(SyntaxKind.AddAccessorDeclaration) ||
                accessorDeclaration.IsKind(SyntaxKind.RemoveAccessorDeclaration))
            {
                return "event";
            }

            var accessorList = accessorDeclaration.Parent;
            if (accessorList == null)
            {
                return null;
            }

            var indexerOrProperty = accessorList.Parent;
            if (indexerOrProperty is IndexerDeclarationSyntax)
            {
                return "indexer set";
            }
            else if (indexerOrProperty is PropertyDeclarationSyntax)
            {
                return "property set";
            }
            else
            {
                return null;
            }
        }
    }
}
