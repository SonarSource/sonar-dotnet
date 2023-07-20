/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Net;
using System.Net.Sockets;

namespace SonarAnalyzer.Rules
{
    public abstract class HardcodedIpAddressBase<TSyntaxKind, TLiteralExpression> : HotspotDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TLiteralExpression : SyntaxNode
    {
        private const string DiagnosticId = "S1313";
        private const string MessageFormat = "Make sure using this hardcoded IP address '{0}' is safe here.";
        private const int IPv4AddressParts = 4;
        private const string IPv4Broadcast = "255.255.255.255";
        private const string OIDPrefix = "2.5.";

        private readonly string[] ignoredVariableNames =
        {
            "VERSION",
            "ASSEMBLY",
        };

        // https://datatracker.ietf.org/doc/html/rfc5737#section-3
        private readonly byte[][] interNetworkDocumentationRanges =
        {
            new byte[] { 192, 0, 2 },
            new byte[] { 198, 51, 100 },
            new byte[] { 203, 0, 113 }
        };

        // https://datatracker.ietf.org/doc/html/rfc3849#section-2
        private readonly byte[] interNetwork6DocumentationRange = { 0x20, 0x01, 0x0d, 0xb8 }; // 2001:0DB8::/32

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract string GetAssignedVariableName(SyntaxNode stringLiteral);
        protected abstract string GetValueText(TLiteralExpression literalExpression);
        protected abstract bool HasAttributes(SyntaxNode literalExpression);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected DiagnosticDescriptor Rule { get; init; }

        protected HardcodedIpAddressBase(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) =>
            Rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckForHardcodedIpAddressesInStringLiteral, Language.SyntaxKind.StringLiteralExpressions);
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckForHardcodedIpAddressesInStringInterpolation, Language.SyntaxKind.InterpolatedStringExpression);
        }

        protected bool IsHardcodedIp(string literalValue, SyntaxNode node) =>
            literalValue != IPv4Broadcast
            && !IsObjectIdentifier(literalValue)
            && IsRoutableNonLoopbackIPAddress(literalValue, out var address)
            && (address.AddressFamily != AddressFamily.InterNetwork
                || literalValue.Count(x => x == '.') == IPv4AddressParts - 1)
            && !IsInDocumentationBlock(address)
            && !IsIgnoredVariableName(node)
            && !HasAttributes(node);

        private void CheckForHardcodedIpAddressesInStringLiteral(SonarSyntaxNodeReportingContext context)
        {
            if (IsEnabled(context.Options)
                && (TLiteralExpression)context.Node is var stringLiteral
                && GetValueText(stringLiteral) is var literalValue
                && IsHardcodedIp(literalValue, stringLiteral))
            {
                context.ReportIssue(CreateDiagnostic(Rule, stringLiteral.GetLocation(), literalValue));
            }
        }

        private void CheckForHardcodedIpAddressesInStringInterpolation(SonarSyntaxNodeReportingContext context)
        {
            if (IsEnabled(context.Options)
                && Language.Syntax.TryGetInterpolatedTextValue(context.Node, context.SemanticModel, out var stringContent)
                && IsHardcodedIp(stringContent, context.Node))
            {
                context.ReportIssue(CreateDiagnostic(Rule, context.Node.GetLocation(), stringContent));
            }
        }

        private static bool IsRoutableNonLoopbackIPAddress(string literalValue, out IPAddress ipAddress) =>
            IPAddress.TryParse(literalValue, out ipAddress)
            && !IPAddress.IsLoopback(ipAddress)
            && !(ipAddress.IsIPv4MappedToIPv6 && IPAddress.IsLoopback(ipAddress.MapToIPv4()))
            && !ipAddress.GetAddressBytes().All(x => x == 0); // Nonroutable 0.0.0.0 or 0::0

        private bool IsInDocumentationBlock(IPAddress address)
        {
            var ip = address.GetAddressBytes();
            return address.AddressFamily switch
            {
                AddressFamily.InterNetwork => interNetworkDocumentationRanges.Any(x => SequenceStartsWith(ip, x)),
                AddressFamily.InterNetworkV6 => SequenceStartsWith(ip, interNetwork6DocumentationRange),
                _ => false,
            };

            static bool SequenceStartsWith(byte[] sequence, byte[] startsWith) =>
                sequence.Take(startsWith.Length).SequenceEqual(startsWith);
        }

        private static bool IsObjectIdentifier(string literalValue) =>
            literalValue.StartsWith(OIDPrefix);   // Looks like OID

        private bool IsIgnoredVariableName(SyntaxNode node) =>
            GetAssignedVariableName(node) is { } variableName
            && ignoredVariableNames.Any(x => variableName.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0);
    }
}
