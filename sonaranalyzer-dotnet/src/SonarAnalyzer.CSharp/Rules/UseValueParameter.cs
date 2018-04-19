/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class UseValueParameter : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3237";
        private const string MessageFormat = "Use the 'value' parameter in this {0} accessor declaration.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCodeBlockStartActionInNonGenerated<SyntaxKind>(
                cbc =>
                {
                    var accessorDeclaration = cbc.CodeBlock as AccessorDeclarationSyntax;
                    if (accessorDeclaration == null ||
                        accessorDeclaration.IsKind(SyntaxKind.GetAccessorDeclaration) ||
                        accessorDeclaration.Body == null)
                    {
                        return;
                    }

                    if (accessorDeclaration.Body.Statements.Count == 1 &&
                        accessorDeclaration.Body.Statements[0] is ThrowStatementSyntax)
                    {
                        return;
                    }

                    var interfaceMember = cbc.SemanticModel.GetDeclaredSymbol(accessorDeclaration)
                        .GetInterfaceMember();
                    if (interfaceMember != null &&
                        accessorDeclaration.Body.Statements.Count == 0)
                    {
                        return;
                    }

                    var foundValueReference = false;
                    cbc.RegisterSyntaxNodeAction(
                        c =>
                        {
                            var identifier = (IdentifierNameSyntax)c.Node;
                            var parameter = c.SemanticModel.GetSymbolInfo(identifier).Symbol as IParameterSymbol;

                            if (identifier.Identifier.ValueText == "value" &&
                                parameter != null &&
                                parameter.IsImplicitlyDeclared)
                            {
                                foundValueReference = true;
                            }
                        },
                        SyntaxKind.IdentifierName);

                    cbc.RegisterCodeBlockEndAction(
                        c =>
                        {
                            if (!foundValueReference)
                            {
                                var accessorType = GetAccessorType(accessorDeclaration);
                                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, accessorDeclaration.Keyword.GetLocation(), accessorType));
                            }
                        });
                });
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
