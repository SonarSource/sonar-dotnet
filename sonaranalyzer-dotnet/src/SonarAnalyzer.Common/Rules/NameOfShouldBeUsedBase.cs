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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class NameOfShouldBeUsedBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2302";
        protected const string MessageFormat = "Replace the string '{0}' with 'nameof({0})'.";

        protected static readonly char[] Separators = { ' ', '.', ',', ';', '!', '?' };

        // when the parameter name is inside a bigger string, we want to avoid common English words like
        // "a", "then", "he", "of", "have" etc, to avoid false positives
        protected const int MIN_STRING_LENGTH = 5;
    }

    public abstract class NameOfShouldBeUsedBase<TMethodSyntax> : NameOfShouldBeUsedBase
            where TMethodSyntax : SyntaxNode
    {
        protected abstract DiagnosticDescriptor Rule { get; }

        protected abstract bool IsCaseSensitive { get; }

        protected StringComparison CaseSensitivity
        {
            get => IsCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
        }

        // Is string literal or interpolated string
        protected abstract bool IsStringLiteral(SyntaxToken t);

        // handle parameters with the same name (in the IDE it can happen) - get groups of parameters
        protected abstract IEnumerable<string> GetParameterNames(TMethodSyntax method);

        protected void ReportIssues<TThrowSyntax>(SyntaxNodeAnalysisContext context)
            where TThrowSyntax : SyntaxNode
        {
            var methodSyntax = (TMethodSyntax)context.Node;
            var parameterNames = GetParameterNames(methodSyntax);
            // either no parameters, or duplicated parameters
            if (!parameterNames.Any())
            {
                return;
            }

            var stringTokensInsideThrowExpressions = methodSyntax
                .DescendantNodes()
                .OfType<TThrowSyntax>()
                .SelectMany(th => th.DescendantTokens())
                .Where(IsStringLiteral);

            var stringTokenAndParameterPairs = GetStringTokenAndParamNamePairs(stringTokensInsideThrowExpressions, parameterNames);

            foreach (var stringTokenAndParam in stringTokenAndParameterPairs)
            {
                ReportIssue(stringTokenAndParam.Key, stringTokenAndParam.Value, context);
            }
        }

        protected void ReportIssue(SyntaxToken stringLiteralToken, string parameterName, SyntaxNodeAnalysisContext context) =>
            context.ReportDiagnosticWhenActive(Diagnostic.Create(
                    descriptor: Rule,
                    location: stringLiteralToken.GetLocation(),
                    messageArgs: parameterName));

        /// <summary>
        /// Iterates over the string tokens (either from simple strings or from interpolated strings)
        /// and returns pairs where
        /// - the key is the string SyntaxToken which contains the verbatim parameter name
        /// - the value is the name of the parameter which is present in the string token
        /// </summary>
        private Dictionary<SyntaxToken, string> GetStringTokenAndParamNamePairs(IEnumerable<SyntaxToken> tokens, IEnumerable<string> parameterNames)
        {
            var result = new Dictionary<SyntaxToken, string>();
            foreach (var stringToken in tokens)
            {
                var stringTokenText = stringToken.ValueText;
                foreach (var parameterName in parameterNames)
                {
                    if (parameterName.Equals(stringTokenText, CaseSensitivity))
                    {
                        // given it's exact equality, there can be only one stringToken key in the dictionary
                        result.Add(stringToken, parameterName);
                    }
                    else if (parameterName.Length > MIN_STRING_LENGTH &&
                        // we are looking at the words inside the string, so there can be multiple parameters matching inside the token
                        // stop after the first one is found
                            !result.ContainsKey(stringToken) &&
                            stringTokenText
                                .Split(Separators, StringSplitOptions.RemoveEmptyEntries)
                                .Any(word => word.Equals(parameterName, CaseSensitivity)))
                    {
                        result.Add(stringToken, parameterName);
                    }
                }
            }
            return result;
        }
    }
}

