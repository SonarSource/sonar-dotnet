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
    public sealed class DoNotNestTypesInArguments : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4017";
        private const string MessageFormat = "Refactor this method to remove the nested type argument.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
            c =>
            {
                var methodDeclaration = c.Node as MethodDeclarationSyntax;

                var argumentTypeSymbols = methodDeclaration
                        .ParameterList
                        .Parameters
                        .Where(p => GetNestingDepth(p, c) > 1);

                foreach (var argument in argumentTypeSymbols)
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, argument.GetLocation()));
                }
            },
            SyntaxKind.MethodDeclaration);
        }

        private static int GetNestingDepth(ParameterSyntax parameterSyntax, SyntaxNodeAnalysisContext context)
        {
            return GetNestingDepth(context.SemanticModel.GetDeclaredSymbol(parameterSyntax)?.Type, 0 , true);
        }

        private static readonly ImmutableArray<KnownType> expressionFuncActionTypes =
            KnownType.SystemFuncVariants
            .Union(KnownType.SystemActionVariants)
            .Union(new[] { KnownType.System_Linq_Expressions_Expression_T })
            .ToImmutableArray();

        private static int GetNestingDepth(ITypeSymbol argumentSymbol, int depth, bool ignoreExpressions)
        {
            var currentOrNestedArgument = argumentSymbol;
            if (currentOrNestedArgument is IArrayTypeSymbol arrayTypeSymbol)
            {
                currentOrNestedArgument = arrayTypeSymbol.ElementType;
            }

            if (!(currentOrNestedArgument is INamedTypeSymbol namedTypeSymbol))
            {
                return depth;
            }

            var nextDepth = ignoreExpressions && namedTypeSymbol.ConstructedFrom.IsAny(expressionFuncActionTypes) ? depth : depth + 1;

            var typeArguments = namedTypeSymbol.TypeArguments;
            if (typeArguments != null && typeArguments.Any())
            {
                return typeArguments.Max(p => GetNestingDepth(p, nextDepth, false));
            }

            return depth;
        }
    }
}
