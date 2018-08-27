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

using System.Collections.Generic;
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
    public sealed class StaticSealedClassProtectedMembers : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2156";
        private const string MessageFormat = "Remove this 'protected' modifier.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                ReportDiagnostics(c, c.Node, ((BaseMethodDeclarationSyntax)c.Node).Modifiers);
            },
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                ReportDiagnostics(c, c.Node, ((BasePropertyDeclarationSyntax)c.Node).Modifiers);
            },
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.EventDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var fieldDeclaration = (BaseFieldDeclarationSyntax)c.Node;

                ReportDiagnostics(c, fieldDeclaration.Declaration.Variables.FirstOrDefault(), fieldDeclaration.Modifiers);
            },
            SyntaxKind.FieldDeclaration,
            SyntaxKind.EventFieldDeclaration);
        }

        private static void ReportDiagnostics(SyntaxNodeAnalysisContext context, SyntaxNode declaration, IEnumerable<SyntaxToken> modifiers)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(declaration);
            if (symbol == null || symbol.IsOverride || !symbol.ContainingType.IsSealed)
            {
                return;
            }

            modifiers
                .Where(m => m.IsKind(SyntaxKind.ProtectedKeyword))
                .Select(m => Diagnostic.Create(rule, m.GetLocation()))
                .ToList()
                .ForEach(d => context.ReportDiagnosticWhenActive(d));
        }
    }
}
