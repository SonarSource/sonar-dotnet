/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private const string DiagnosticId = "S1313";
        private const string MessageFormat = "Make sure using this hardcoded IP address '{0}' is safe here.";
        private const int IPv4AddressParts  = 4;
        private const string IPv4Broadcast = "255.255.255.255";

        private readonly string[] ignoredVariableNames =
        {
            "VERSION",
            "ASSEMBLY",
        };

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract string GetAssignedVariableName(SyntaxNode stringLiteral);
        protected abstract string GetValueText(TLiteralExpression literalExpression);
        protected abstract bool HasAttributes(SyntaxNode literalExpression);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected DiagnosticDescriptor Rule { get; init; }

        protected HardcodedIpAddressBase(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) =>
            Rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, CheckForHardcodedIpAddresses, Language.SyntaxKind.StringLiteralExpression);

        protected bool IsHardcodedIp(string literalValue, SyntaxNode node) =>
            literalValue != IPv4Broadcast
            && !literalValue.StartsWith("2.5.")                                  // Looks like OID
            && IsRoutableNonLoopbackIPAddress(literalValue, out var address)
            && (address.AddressFamily != AddressFamily.InterNetwork
                || literalValue.Count(x => x == '.') == IPv4AddressParts - 1)
            && (!(GetAssignedVariableName(node) is { } variableName)
                || !ignoredVariableNames.Any(x => variableName.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0))
            && !HasAttributes(node);

        private void CheckForHardcodedIpAddresses(SyntaxNodeAnalysisContext context)
        {
            if (IsEnabled(context.Options)
                && (TLiteralExpression)context.Node is var stringLiteral
                && GetValueText(stringLiteral) is var literalValue
                && IsHardcodedIp(literalValue, stringLiteral))
            {
                context.ReportIssue(Diagnostic.Create(Rule, stringLiteral.GetLocation(), literalValue));
            }
        }

        private static bool IsRoutableNonLoopbackIPAddress(string literalValue, out IPAddress ipAddress) =>
            IPAddress.TryParse(literalValue, out ipAddress)
            && !IPAddress.IsLoopback(ipAddress)
            && !ipAddress.GetAddressBytes().All(x => x == 0); // Nonroutable 0.0.0.0 or 0::0
    }
}
