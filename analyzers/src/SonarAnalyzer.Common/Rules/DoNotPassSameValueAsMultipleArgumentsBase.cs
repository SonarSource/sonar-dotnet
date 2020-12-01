/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotPassSameValueAsMultipleArgumentsBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S4142";

        protected const string MessageFormat =
            "Verify that this is the intended value; it is the same as the {0} argument.";
    }

    public abstract class DoNotPassSameValueAsMultipleArgumentsBase<TSyntaxKind, TInvocationSyntax, TArgumentSyntax>
        : DoNotPassSameValueAsMultipleArgumentsBase
        where TSyntaxKind : struct
        where TInvocationSyntax : SyntaxNode
        where TArgumentSyntax : SyntaxNode
    {
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract TSyntaxKind InvocationExpressionSyntaxKind { get; }

        protected abstract IReadOnlyList<TArgumentSyntax> GetArguments(TInvocationSyntax invocation);

        protected abstract bool IsLiteral(TArgumentSyntax argument);

        protected abstract ITypeSymbol GetConvertedType(TArgumentSyntax argument, SemanticModel semanticModel);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(GeneratedCodeRecognizer,
                c =>
                {
                    var invocation = (TInvocationSyntax)c.Node;

                    var arguments = GetArguments(invocation);

                    if (arguments.Count < 2)
                    {
                        return;
                    }

                    var argConvertedTypes = arguments
                        .Select(arg => GetConvertedType(arg, c.SemanticModel))
                        .ToList();

                    for (var i = 1; i < arguments.Count; i++)
                    {
                        var parameterUsage = new HashSet<ISymbol> { argConvertedTypes[i] };

                        for (var j = 0; j < i; j++)
                        {
                            if (!IsLiteral(arguments[i]) &&
                                arguments[i].IsEquivalentTo(arguments[j]) &&
                                IsDuplicateUsage(parameterUsage, argConvertedTypes[j]))
                            {
                                c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0],
                                    arguments[i].GetLocation(),
                                    additionalLocations: new[] { arguments[j].GetLocation() },
                                    messageArgs: ToOrdinalNumberString(j + 1)));
                                break;
                            }
                        }
                    }
                },
                InvocationExpressionSyntaxKind);
        }

        private static bool IsDuplicateUsage(ISet<ISymbol> parameterUsage, ISymbol typeSymbol)
        {
            return typeSymbol != null && !parameterUsage.Add(typeSymbol);
        }

        private static string ToOrdinalNumberString(int number)
        {
            switch (number)
            {
                case 1:
                    return number + "st";

                case 2:
                    return number + "nd";

                case 3:
                    return number + "rd";

                default:
                    return number + "th";
            }
        }
    }
}
