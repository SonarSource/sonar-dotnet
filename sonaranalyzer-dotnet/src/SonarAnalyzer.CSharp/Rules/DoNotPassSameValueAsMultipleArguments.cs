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
    public sealed class DoNotPassSameValueAsMultipleArguments : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4142";
        private const string MessageFormat =
            "Verify that this is the intended value; it is the same as the {0} argument.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;

                if (invocation.ArgumentList?.Arguments.Count < 2)
                {
                    return;
                }

                var arguments = invocation.ArgumentList.Arguments;
                for (int i = 1; i < arguments.Count; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (ShouldReportOnArgument(arguments[i]) &&
                            arguments[i].IsEquivalentTo(arguments[j]))
                        {
                            c.ReportDiagnostic(Diagnostic.Create(rule,
                                arguments[i].GetLocation(),
                                additionalLocations: new[] { arguments[j].GetLocation() },
                                messageArgs: ToOrdinalNumberString(j + 1)));
                            break;
                        }
                    }
                }

            },
            SyntaxKind.InvocationExpression);
        }

        private static bool ShouldReportOnArgument(ArgumentSyntax argument)
        {
            bool isLiteral = argument.Expression is LiteralExpressionSyntax ||
                (argument.Expression as PrefixUnaryExpressionSyntax)?.Operand is LiteralExpressionSyntax;

            var variableName = (argument.Expression as IdentifierNameSyntax)?.Identifier.ValueText;

            return !isLiteral &&
                   variableName != "self" &&
                   variableName != "Self";
        }

        private static string ToOrdinalNumberString(int number)
        {
            switch (number % 10)
            {
                case  1: return number + "st";
                case  2: return number + "nd";
                case  3: return number + "rd";
                default: return number + "th";
            }
        }
    }
}
