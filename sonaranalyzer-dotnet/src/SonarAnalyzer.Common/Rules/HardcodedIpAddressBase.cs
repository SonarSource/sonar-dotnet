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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class HardcodedIpAddressBase<TLiteralExpression> : HotspotDiagnosticAnalyzer
        where TLiteralExpression : SyntaxNode
    {
        protected const string DiagnosticId = "S1313";
        protected const string MessageFormat = "Make sure using this hardcoded IP address '{0}' is safe here.";

        private static readonly ISet<string> IgnoredVariableNames =
            new HashSet<string>
            {
                "VERSION",
                "ASSEMBLY",
            };

        protected abstract string GetAssignedVariableName(TLiteralExpression stringLiteral);
        protected abstract string GetValueText(TLiteralExpression literalExpression);
        protected abstract bool HasAttributes(TLiteralExpression literalExpression);

        protected HardcodedIpAddressBase(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration)
        {
        }

        protected Action<SyntaxNodeAnalysisContext> GetAnalysisAction(DiagnosticDescriptor rule) =>
            c =>
            {
                var stringLiteral = (TLiteralExpression)c.Node;
                var literalValue = GetValueText(stringLiteral);

                if (literalValue == "::" ||
                    literalValue == "127.0.0.1" ||
                    !IPAddress.TryParse(literalValue, out var address))
                {
                    return;
                }

                if (address.AddressFamily == AddressFamily.InterNetwork &&
                    literalValue.Split('.').Length != 4)
                {
                    return;
                }

                var variableName = GetAssignedVariableName(stringLiteral);
                if (variableName != null &&
                    IgnoredVariableNames.Any(variableName.Contains))
                {
                    return;
                }

                if (HasAttributes(stringLiteral))
                {
                    return;
                }

                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, stringLiteral.GetLocation(), literalValue));
            };
    }
}
