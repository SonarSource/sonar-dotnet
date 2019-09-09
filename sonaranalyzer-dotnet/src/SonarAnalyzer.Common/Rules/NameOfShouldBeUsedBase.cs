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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class NameOfShouldBeUsedBase<TMethodSyntax> : SonarDiagnosticAnalyzer
            where TMethodSyntax : SyntaxNode
    {
        internal const string DiagnosticId = "S2302";
        protected const string MessageFormat = "Replace the string '{0}' with 'nameof({0})'.";

        protected abstract DiagnosticDescriptor Rule { get; }

        protected abstract StringComparison CaseSensitivity { get; }

        // Is string literal or interpolated string
        protected abstract bool IsStringLiteral(SyntaxToken t);

        // handle parameters with the same name (in the IDE it can happen) - get groups of parameters
        protected abstract IEnumerable<string> GetParameterNames(TMethodSyntax method);

        protected void ReportIssues<TThrowSyntax>(SyntaxNodeAnalysisContext context)
            where TThrowSyntax : SyntaxNode
        {
            var methodSyntax = (TMethodSyntax)context.Node;
            var paramLookup = GetParameterNames(methodSyntax);
            // either no parameters, or duplicated parameters
            if (!paramLookup.Any())
            {
                return;
            }

            methodSyntax
                .DescendantNodes()
                .OfType<TThrowSyntax>()
                .SelectMany(th => th.DescendantTokens())
                .Where(IsStringLiteral)
                .Where(t => paramLookup.Any(param => param.Equals(t.ValueText, CaseSensitivity)))
                .ToList()
                .ForEach(t => ReportIssue(t, context));
        }

        protected void ReportIssue(SyntaxToken stringLiteralToken,
            SyntaxNodeAnalysisContext context)
        {
            context.ReportDiagnosticWhenActive(Diagnostic.Create(
                    descriptor: Rule,
                    location: stringLiteralToken.GetLocation(),
                    messageArgs: stringLiteralToken.ValueText));
        }
    }
}

