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
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
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
    public sealed class ClearTextProtocolsAreSensitive : HotspotDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5332";
        private const string MessageFormat = "Using {0} protocol is insecure. Use {1} instead.";
        private const string HttpPattern = @"^http:\/\/(?!localhost|127.0.0.1).+";
        private const string FtpPattern = @"^ftp:\/\/.*@(?!localhost|127.0.0.1)";
        private const string TelnetPattern = @"^telnet:\/\/.*@(?!localhost|127.0.0.1)";

        private readonly Dictionary<string, string> recommendedProtocols = new Dictionary<string, string>
        {
            {"telnet", "ssh"},
            {"ftp", "sftp, scp or ftps"},
            {"http", "https"},
            {"clear-text SMTP", "SMTP over SSL/TLS or SMTP with STARTTLS" }
        };

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public ClearTextProtocolsAreSensitive() : base(AnalyzerConfiguration.Hotspot) { }

        public ClearTextProtocolsAreSensitive(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
               {
                   var text = GetText(c.Node);
                   if (TryGetUnsafeProtocol(text, out var unsafeProtocol))
                   {
                       c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, c.Node.GetLocation(), unsafeProtocol, recommendedProtocols[unsafeProtocol]));
                   }
               },
               SyntaxKind.StringLiteralExpression,
               SyntaxKind.InterpolatedStringExpression);

        private static bool TryGetUnsafeProtocol(string text, out string unsafeProtocol)
        {
            unsafeProtocol = null;

            if (Regex.IsMatch(text, HttpPattern))
            {
                unsafeProtocol = "http";
            }
            else if (Regex.IsMatch(text, FtpPattern))
            {
                unsafeProtocol = "ftp";
            }
            else if (Regex.IsMatch(text, TelnetPattern))
            {
                unsafeProtocol = "telnet";
            }

            return unsafeProtocol != null;
        }

        private static string GetText(SyntaxNode node) =>
            node switch
            {
                InterpolatedStringExpressionSyntax interpolatedStringExpression => interpolatedStringExpression.GetContentsText(),
                LiteralExpressionSyntax literalExpression => literalExpression.Token.ValueText,
                _ => string.Empty
            };
    }
}

