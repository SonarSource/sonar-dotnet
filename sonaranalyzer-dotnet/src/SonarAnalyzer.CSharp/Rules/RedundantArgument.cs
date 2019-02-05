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
    public sealed class RedundantArgument : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3254";
        private const string MessageFormat = "Remove this default value assigned to parameter '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    // Can't use optional arguments in expression trees (CS0584), so skip those
                    if (c.Node.IsInExpressionTree(c.SemanticModel))
                    {
                        return;
                    }

                    var methodCall = (InvocationExpressionSyntax) c.Node;
                    var methodParameterLookup = new CSharpMethodParameterLookup(methodCall, c.SemanticModel);
                    var argumentMappings = methodParameterLookup.GetAllArgumentParameterMappings()
                        .ToList();

                    var methodSymbol = methodParameterLookup.MethodSymbol;
                    if (methodSymbol == null)
                    {
                        return;
                    }

                    foreach (var argumentMapping in argumentMappings.Where(argumentMapping => ArgumentHasDefaultValue(argumentMapping, c.SemanticModel)))
                    {
                        var argument = argumentMapping.SyntaxNode;
                        var parameter = argumentMapping.Symbol;
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, argument.GetLocation(), parameter.Name));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        internal static bool ArgumentHasDefaultValue(SyntaxNodeSymbolSemanticModelTuple<ArgumentSyntax, IParameterSymbol> argumentMapping,
            SemanticModel semanticModel)
        {
            var argument = argumentMapping.SyntaxNode;
            var parameter = argumentMapping.Symbol;

            if (!parameter.HasExplicitDefaultValue)
            {
                return false;
            }

            var defaultValue = parameter.ExplicitDefaultValue;
            var argumentValue = semanticModel.GetConstantValue(argument.Expression);
            return argumentValue.HasValue &&
                object.Equals(argumentValue.Value, defaultValue);
        }
    }
}
