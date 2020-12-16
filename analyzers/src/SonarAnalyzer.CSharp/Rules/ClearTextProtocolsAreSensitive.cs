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
        private const string MessageFormat = "Using {0} protocol is insecure. Use {1} instead";

        private readonly ImmutableArray<string> unsafeProtocols = ImmutableArray.Create("http://", "ftp://", "telnet://");
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
                   if (ContainsUnsafeProtocol(text))
                   {
                       c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, c.Node.GetLocation()));
                   }
               },
               SyntaxKind.StringLiteralExpression,
               SyntaxKind.InterpolatedStringText);

        private bool ContainsUnsafeProtocol(string text) =>
            unsafeProtocols.Any(text.Contains);

        private static string GetText(SyntaxNode node) =>
            node switch
            {
                InterpolatedStringTextSyntax interpolatedStringText => interpolatedStringText.TextToken.Text,
                LiteralExpressionSyntax literalExpression => literalExpression.Token.Text,
                _ => string.Empty
            };
    }
}

