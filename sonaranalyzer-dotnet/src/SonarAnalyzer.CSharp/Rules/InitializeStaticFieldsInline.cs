/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
    public sealed class InitializeStaticFieldsInline : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3963";
        private const string MessageFormat = "Initialize all 'static fields' inline and remove the 'static constructor'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var constructorDeclaration = (ConstructorDeclarationSyntax)c.Node;
                    if (!constructorDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                        (constructorDeclaration.Body == null && constructorDeclaration.ExpressionBody() == null))
                    {
                        return;
                    }

                    var currentType = c.SemanticModel.GetDeclaredSymbol(constructorDeclaration).ContainingType;
                    if (currentType == null)
                    {
                        return;
                    }

                    var bodyDescendantNodes = constructorDeclaration.Body?.DescendantNodes()
                        ?? constructorDeclaration.ExpressionBody()?.DescendantNodes()
                        ?? Enumerable.Empty<SyntaxNode>();

                    var hasFieldAssignment = bodyDescendantNodes
                        .OfType<AssignmentExpressionSyntax>()
                        .Select(x => c.SemanticModel.GetSymbolInfo(x.Left).Symbol)
                        .OfType<IFieldSymbol>()
                        .Any(fs => fs.ContainingType.Equals(currentType));
                    if (hasFieldAssignment)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, constructorDeclaration.GetLocation()));
                    }
                }, SyntaxKind.ConstructorDeclaration);
        }
    }
}
