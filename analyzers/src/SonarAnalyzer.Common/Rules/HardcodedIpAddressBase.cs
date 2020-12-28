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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class HardcodedIpAddressBase<TSyntaxKind, TLiteralExpression> : HotspotDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TLiteralExpression : SyntaxNode
    {
        protected const string DiagnosticId = "S1313";
        protected const string MessageFormat = "Make sure using this hardcoded IP address '{0}' is safe here.";
        private const int NumOfIPv4AddressParts = 4;
        private const string IPv4Broadcast = "255.255.255.255";
        private const string IPv4NonRoutable = "0.0.0.0";

        private readonly ISet<string> ignoredVariableNames =
            new HashSet<string>
            {
                "VERSION",
                "ASSEMBLY",
            };

        private static readonly Regex IPv6LoopbackPattern = new Regex("^(?:0*:)*?:?0*1$", RegexOptions.Compiled);

        private static readonly Regex IPv6NonRoutablePattern = new Regex("^(?:0*:)*?:?0*$", RegexOptions.Compiled);

        private ImmutableArray<DiagnosticDescriptor> supportedDiagnostics;

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract TSyntaxKind SyntaxKind { get; }

        protected abstract DiagnosticDescriptor Rule { get; }

        protected abstract string GetAssignedVariableName(TLiteralExpression stringLiteral);

        protected abstract string GetValueText(TLiteralExpression literalExpression);

        protected abstract bool HasAttributes(TLiteralExpression literalExpression);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (supportedDiagnostics == null)
                {
                    supportedDiagnostics = ImmutableArray.Create(Rule);
                }

                return supportedDiagnostics;
            }
        }

        protected HardcodedIpAddressBase(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration)
        {
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(GeneratedCodeRecognizer, AnalyzeLiteral, SyntaxKind);

        private void AnalyzeLiteral(SyntaxNodeAnalysisContext context)
        {
            if (!IsEnabled(context.Options))
            {
                return;
            }

            CheckForHardcodedIpAddresses(context);
        }

        private void CheckForHardcodedIpAddresses(SyntaxNodeAnalysisContext context)
        {
            var stringLiteral = (TLiteralExpression)context.Node;
            var variableName = GetAssignedVariableName(stringLiteral);

            if (variableName != null
                && ignoredVariableNames.Any(variableName.Contains))
            {
                return;
            }

            if (HasAttributes(stringLiteral))
            {
                return;
            }

            var literalValue = GetValueText(stringLiteral);

            if (!IPAddress.TryParse(literalValue, out var address))
            {
                return;
            }

            if (IsLoopbackAddress(literalValue) || IsNonRoutableAddress(literalValue) || literalValue == IPv4Broadcast || LooksLikeOid(literalValue))
            {
                return;
            }

            if (address.AddressFamily == AddressFamily.InterNetwork
                && literalValue.Count(x => x == '.') != NumOfIPv4AddressParts - 1)
            {
                return;
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, stringLiteral.GetLocation(), literalValue));
        }

        private static bool IsLoopbackAddress(string ip) =>
            ip.StartsWith("127.") || IPv6LoopbackPattern.IsMatch(ip);

        private static bool IsNonRoutableAddress(string ip) =>
             ip == IPv4NonRoutable || IPv6NonRoutablePattern.IsMatch(ip);

        private static bool LooksLikeOid(string s) =>
            s.StartsWith("2.5.");
    }
}
