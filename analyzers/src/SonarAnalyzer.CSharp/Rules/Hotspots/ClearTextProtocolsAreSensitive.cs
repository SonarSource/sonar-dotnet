/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.Trackers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ClearTextProtocolsAreSensitive : HotspotDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5332";
        private const string MessageFormat = "Using {0} protocol is insecure. Use {1} instead.";
        private const string EnableSslMessage = "EnableSsl should be set to true.";

        private const string TelnetKey = "telnet";
        private const string EnableSslName = "EnableSsl";

        private static readonly DiagnosticDescriptor DefaultRule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager).WithNotConfigurable();
        private static readonly DiagnosticDescriptor EnableSslRule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, EnableSslMessage, RspecStrings.ResourceManager).WithNotConfigurable();

        private readonly Dictionary<string, string> recommendedProtocols = new Dictionary<string, string>
        {
            {"telnet", "ssh"},
            {"ftp", "sftp, scp or ftps"},
            {"http", "https"},
            {"clear-text SMTP", "SMTP over SSL/TLS or SMTP with STARTTLS" }
        };

        private readonly string[] commonlyUsedXmlDomains =
        {
            "www.w3.org",
            "xml.apache.org",
            "schemas.xmlsoap.org",
            "schemas.openxmlformats.org",
            "rdfs.org",
            "purl.org",
            "xmlns.com",
            "schemas.google.com",
            "a9.com",
            "ns.adobe.com",
            "ltsc.ieee.org",
            "docbook.org",
            "graphml.graphdrawing.org",
            "json-schema.org"
        };

        private readonly string[] commonlyUsedExampleDomains = {"example.com", "example.org", "test.com"};
        private readonly string[] localhostAddresses = {"localhost", "127.0.0.1", "::1"};

        private readonly CSharpObjectInitializationTracker objectInitializationTracker =
            new CSharpObjectInitializationTracker(constantValue => constantValue is bool value && value,
                                                  ImmutableArray.Create(KnownType.System_Net_Mail_SmtpClient, KnownType.System_Net_FtpWebRequest),
                                                  propertyName => propertyName == EnableSslName);

        private readonly Regex httpRegex;
        private readonly Regex ftpRegex;
        private readonly Regex telnetRegex;
        private readonly Regex telnetRegexForIdentifier;
        private readonly Regex validServerRegex;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, EnableSslRule);

        public ClearTextProtocolsAreSensitive() : this(AnalyzerConfiguration.Hotspot) { }

        public ClearTextProtocolsAreSensitive(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration)
        {
            const string allSubdomainsPattern = @"([^/?#]+\.)?";
            var domainsList = localhostAddresses
                .Concat(commonlyUsedXmlDomains)
                    .Select(Regex.Escape)
                    .Concat(commonlyUsedExampleDomains.Select(x => allSubdomainsPattern + Regex.Escape(x)));
            var validServerPattern = domainsList.JoinStr("|");

            httpRegex = CompileRegex(@$"^http:\/\/(?!{validServerPattern}).");
            ftpRegex = CompileRegex(@$"^ftp:\/\/.*@(?!{validServerPattern})");
            telnetRegex = CompileRegex(@$"^telnet:\/\/.*@(?!{validServerPattern})");
            telnetRegexForIdentifier = CompileRegex(@"Telnet(?![a-z])", false);
            validServerRegex = CompileRegex($"^({validServerPattern})$");
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(c =>
            {
                if (!IsEnabled(c.Options))
                {
                    return;
                }

                context.RegisterSyntaxNodeActionInNonGenerated(VisitStringExpressions, SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression);
                context.RegisterSyntaxNodeActionInNonGenerated(VisitObjectCreation, SyntaxKind.ObjectCreationExpression);
                context.RegisterSyntaxNodeActionInNonGenerated(VisitInvocationExpression, SyntaxKind.InvocationExpression);
                context.RegisterSyntaxNodeActionInNonGenerated(VisitAssignments, SyntaxKind.SimpleAssignmentExpression);
            });

        private void VisitObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

            if (telnetRegexForIdentifier.IsMatch(objectCreation.Type.ToString()))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(DefaultRule, objectCreation.GetLocation(), TelnetKey, recommendedProtocols[TelnetKey]));
            }
            else if (!IsServerSafe(objectCreation) && objectInitializationTracker.ShouldBeReported(objectCreation, context.SemanticModel, false))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(EnableSslRule, objectCreation.GetLocation()));
            }
        }

        private void VisitInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (telnetRegexForIdentifier.IsMatch(invocation.Expression.ToString()))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(DefaultRule, invocation.GetLocation(), TelnetKey, recommendedProtocols[TelnetKey]));
            }
        }

        private static void VisitAssignments(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            if (assignment.Left is MemberAccessExpressionSyntax memberAccess
                && memberAccess.IsMemberAccessOnKnownType(EnableSslName, KnownType.System_Net_FtpWebRequest, context.SemanticModel)
                && assignment.Right.FindConstantValue(context.SemanticModel) is bool enableSslValue
                && !enableSslValue)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(EnableSslRule, assignment.GetLocation()));
            }
        }

        private void VisitStringExpressions(SyntaxNodeAnalysisContext c)
        {
            if (GetUnsafeProtocol(c.Node) is {} unsafeProtocol)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(DefaultRule, c.Node.GetLocation(), unsafeProtocol, recommendedProtocols[unsafeProtocol]));
            }
        }

        private bool IsServerSafe(ObjectCreationExpressionSyntax objectCreation) =>
            objectCreation.ArgumentList?.Arguments.Count > 0
            && validServerRegex.IsMatch(GetText(objectCreation.ArgumentList.Arguments[0].Expression));

        private string GetUnsafeProtocol(SyntaxNode node)
        {
            var text = GetText(node);
            if (httpRegex.IsMatch(text) && !IsNamespace(node.Parent))
            {
                return "http";
            }
            else if (ftpRegex.IsMatch(text))
            {
                return "ftp";
            }
            else if (telnetRegex.IsMatch(text))
            {
                return "telnet";
            }
            else
            {
                return null;
            }
        }

        private static string GetText(SyntaxNode node) =>
            node switch
            {
                InterpolatedStringExpressionSyntax interpolatedStringExpression => interpolatedStringExpression.GetContentsText(),
                LiteralExpressionSyntax literalExpression => literalExpression.Token.ValueText,
                _ => string.Empty
            };

        private static bool IsNamespace(SyntaxNode node) =>
            node switch
            {
                AttributeArgumentSyntax attributeArgument =>
                    attributeArgument.NameEquals is { } nameEquals && TokenContainsNamespace(nameEquals.Name.Identifier),
                EqualsValueClauseSyntax equalsValueClause =>
                    (equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator && TokenContainsNamespace(variableDeclarator.Identifier))
                    || (equalsValueClause.Parent is ParameterSyntax parameter && TokenContainsNamespace(parameter.Identifier)),
                AssignmentExpressionSyntax assignmentExpression =>
                    assignmentExpression.Left.RemoveParentheses() is IdentifierNameSyntax identifierName && TokenContainsNamespace(identifierName.Identifier),
                _ => false
            };

        private static bool TokenContainsNamespace(SyntaxToken token) =>
            token.Text.IndexOf("Namespace", StringComparison.OrdinalIgnoreCase) != -1;

        private static Regex CompileRegex(string pattern, bool ignoreCase = true) =>
            new Regex(pattern, ignoreCase
                          ? RegexOptions.Compiled | RegexOptions.IgnoreCase
                          : RegexOptions.Compiled);
    }
}
