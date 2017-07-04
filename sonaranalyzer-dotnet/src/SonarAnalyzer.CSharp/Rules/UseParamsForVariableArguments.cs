/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public sealed class UseParamsForVariableArguments : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4061";
        private const string MessageFormat = "Use the 'params' keyword instead of '__arglist'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);

                    if (!methodDeclaration.Identifier.IsMissing &&
                        methodSymbol != null &&
                        IsPubliclyAccessible(methodSymbol) &&
                        HasAnyArgListParameter(methodDeclaration))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, methodDeclaration.Identifier.GetLocation()));
                    }
                }, SyntaxKind.MethodDeclaration);
        }

        private static bool IsPubliclyAccessible(ISymbol symbol)
        {
            var effectiveAccessibility = symbol.GetEffectiveAccessibility();

            return effectiveAccessibility == Accessibility.Public ||
                effectiveAccessibility == Accessibility.Protected ||
                effectiveAccessibility == Accessibility.ProtectedOrInternal;
        }

        private static bool HasAnyArgListParameter(MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.ParameterList.Parameters
                .Any(p => p.Identifier.IsKind(SyntaxKind.ArgListKeyword));
        }
    }
}
