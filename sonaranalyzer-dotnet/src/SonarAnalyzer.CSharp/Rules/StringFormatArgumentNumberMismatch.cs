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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class StringFormatArgumentNumberMismatch : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2275";
        private const string MessageFormat = "The passed arguments do not match the format string.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                    if (methodSymbol == null ||
                        !methodSymbol.IsInType(KnownType.System_String) ||
                        methodSymbol.Name != "Format")
                    {
                        return;
                    }

                    var formatIndex = 0;
                    if (!methodSymbol.Parameters[0].IsType(KnownType.System_String))
                    {
                        formatIndex = 1;
                    }

                    if (invocation.ArgumentList.Arguments.Count == formatIndex + 2)
                    {
                        var argType = c.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[formatIndex + 1].Expression).Type;
                        if (argType.Is(TypeKind.Array))
                        {
                            // can't statically check the override that supplies args in an array
                            return;
                        }
                    }

                    var formatExpression = invocation.ArgumentList.Arguments[formatIndex];
                    var constValue = c.SemanticModel.GetConstantValue(formatExpression.Expression);
                    if (!constValue.HasValue)
                    {
                        // can't check non-constant format strings
                        return;
                    }

                    var formatString = (string)constValue.Value;
                    if (formatString == null ||
                        !IsFormatValid(formatString))
                    {
                        return;
                    }

                    // Check the format with the supplied number of args
                    var formatArgCount = invocation.ArgumentList.Arguments.Count - formatIndex - 1;
                    if (!FormatterAcceptsArgumentCount(formatString, formatArgCount))
                    {
                        // Must be insufficient arguments
                        c.ReportDiagnostic(Diagnostic.Create(Rule, formatExpression.Expression.GetLocation()));
                        return;
                    }

                    int removableArgumentCount;
                    if (!HasAdditionalArguments(formatString, formatArgCount, out removableArgumentCount))
                    {
                        return;
                    }

                    var argument = invocation.ArgumentList.Arguments
                        .Skip(invocation.ArgumentList.Arguments.Count - removableArgumentCount)
                        .FirstOrDefault();

                    if (argument == null)
                    {
                        return;
                    }

                    var spanStart = argument.SpanStart;
                    var location = Location.Create(invocation.SyntaxTree, new TextSpan(
                        spanStart,
                        invocation.ArgumentList.Arguments.Last().Span.End - spanStart));
                    c.ReportDiagnostic(Diagnostic.Create(Rule, location));
                },
                SyntaxKind.InvocationExpression);
        }

        private static bool IsFormatValid(string format)
        {
            const int maxArgCount = 100;
            return FormatterAcceptsArgumentCount(format, maxArgCount);
        }

        private static bool HasAdditionalArguments(string format, int expectedArgCount, out int removableArgumentCount)
        {
            for (int i = expectedArgCount - 1; i >= 0; i--)
            {
                var canBeRemoved = FormatterAcceptsArgumentCount(format, i);
                if (!canBeRemoved && i == expectedArgCount - 1)
                {
                    removableArgumentCount = -1;
                    return false;
                }

                if (!canBeRemoved)
                {
                    removableArgumentCount = expectedArgCount - i - 1;
                    return true;
                }
            }

            removableArgumentCount = expectedArgCount;
            return true;
        }

        internal static bool FormatterAcceptsArgumentCount(string format, int expectedArgCount)
        {
            try
            {
                string.Format(format, new object[expectedArgCount]);
                return true;
            }
            catch (System.FormatException)
            {
                return false;
            }
        }
    }
}
