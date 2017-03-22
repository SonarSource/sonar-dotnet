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
using System.Globalization;
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
    public class StringFormatWithNoArgument : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3457";
        private const string MessageFormat = "Remove this formatting call and simply use the input string.";
        private const IdeVisibility ideVisibility = IdeVisibility.Hidden;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, ideVisibility, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        internal const string FormatStringIndexKey = "formatStringIndex";

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                    if (!methodSymbol.IsInType(KnownType.System_String) ||
                        methodSymbol.Name != "Format" ||
                        invocation.HasExactlyNArguments(0))
                    {
                        return;
                    }

                    var lookup = new MethodParameterLookup(invocation, c.SemanticModel);
                    if (InvocationHasFormatArgument(invocation, lookup))
                    {
                        return;
                    }

                    var formatArgument = invocation.ArgumentList.Arguments
                        .FirstOrDefault(arg =>
                        {
                            IParameterSymbol parameter;
                            return lookup.TryGetParameterSymbol(arg, out parameter) &&
                                   parameter.Name == "format";
                        });
                    if (formatArgument == null)
                    {
                        return;
                    }

                    var constValue = c.SemanticModel.GetConstantValue(formatArgument.Expression);
                    if (!constValue.HasValue)
                    {
                        // we don't report on non-contant format strings
                        return;
                    }

                    var formatString = constValue.Value as string;
                    if (formatString == null)
                    {
                        return;
                    }

                    if (!StringFormatArgumentNumberMismatch.IsFormatValid(formatString, 0))
                    {
                        ///A more severe issue is already reported by <see cref="StringFormatArgumentNumberMismatch"/>
                        return;
                    }

                    c.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Expression.GetLocation(),
                        ImmutableDictionary<string, string>.Empty.Add(
                            FormatStringIndexKey,
                            invocation.ArgumentList.Arguments.IndexOf(formatArgument).ToString(CultureInfo.InvariantCulture))));
                },
                SyntaxKind.InvocationExpression);
        }

        private static bool InvocationHasFormatArgument(InvocationExpressionSyntax invocation, MethodParameterLookup lookup)
        {
            return invocation.ArgumentList.Arguments.Any(arg =>
            {
                IParameterSymbol parameter;
                return lookup.TryGetParameterSymbol(arg, out parameter) &&
                    parameter.Name.StartsWith("arg", System.StringComparison.Ordinal);
            });
        }
    }
}
