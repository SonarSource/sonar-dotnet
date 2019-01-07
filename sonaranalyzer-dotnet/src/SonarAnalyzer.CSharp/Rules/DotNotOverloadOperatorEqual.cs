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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DotNotOverloadOperatorEqual : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3875";
        private const string MessageFormat = "Remove this overload of 'operator =='.";

        private static readonly ImmutableArray<KnownType> InterfacesRelyingOnOperatorEqualOverload =
            ImmutableArray.Create(
                KnownType.System_IComparable,
                KnownType.System_IComparable_T,
                KnownType.System_IEquatable_T
            );

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForIssue, SyntaxKind.OperatorDeclaration);
        }

        private void CheckForIssue(SyntaxNodeAnalysisContext analysisContext)
        {
            var declaration = (OperatorDeclarationSyntax)analysisContext.Node;

            if (!declaration.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
            {
                return;
            }

            if (declaration.Parent is StructDeclarationSyntax)
            {
                return;
            }

            if (!(declaration.Parent is ClassDeclarationSyntax classDeclaration))
            {
                return;
            }

            var hasAdditionOrSubstractionOverload =
                classDeclaration.ChildNodes()
                                .OfType<OperatorDeclarationSyntax>()
                                .Any(op => op.OperatorToken.IsKind(SyntaxKind.PlusToken) ||
                                           op.OperatorToken.IsKind(SyntaxKind.MinusToken));
            if (hasAdditionOrSubstractionOverload)
            {
                return;
            }

            var namedTypeSymbol = analysisContext.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (namedTypeSymbol == null ||
                namedTypeSymbol.ImplementsAny(InterfacesRelyingOnOperatorEqualOverload))
            {
                return;
            }

            analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, declaration.OperatorToken.GetLocation()));
        }
    }
}
