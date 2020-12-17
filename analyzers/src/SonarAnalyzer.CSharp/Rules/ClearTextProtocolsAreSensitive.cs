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
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SyntaxTrackers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ClearTextProtocolsAreSensitive : HotspotDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5332";
        private const string MessageFormat = "Using {0} protocol is insecure. Use {1} instead.";
        private const string EnableSslMessage = "EnableSsl should be set to true.";
        private const string HttpPattern = @"^http:\/\/(?!localhost|127.0.0.1).+";
        private const string FtpPattern = @"^ftp:\/\/.*@(?!localhost|127.0.0.1)";
        private const string Telnet = "telnet";
        private const string TelnetPattern = @"^telnet:\/\/.*@(?!localhost|127.0.0.1)";
        private const string TelnetPatternForIdentifier = @"(Telnet)(?![a-z])";
        private const string LocalHost = "localhost";
        private const string LocalHostIp = "127.0.0.1";
        private const string EnableSslName = "EnableSsl";

        private static readonly DiagnosticDescriptor DefaultRule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager).WithNotConfigurable();

        private static readonly DiagnosticDescriptor EnableSslRule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, EnableSslMessage, RspecStrings.ResourceManager).WithNotConfigurable();

        private readonly Dictionary<string, string> recommendedProtocols = new Dictionary<string, string>
        {
            {"telnet", "ssh"},
            {"ftp", "sftp, scp or ftps"},
            {"http", "https"},
            {"clear-text SMTP", "SMTP over SSL/TLS or SMTP with STARTTLS" }
        };

        private readonly ImmutableArray<string> validServerValues = ImmutableArray.Create(LocalHost, LocalHostIp);

        private readonly CSharpObjectInitializationTracker objectInitializationTracker;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, EnableSslRule);

        public ClearTextProtocolsAreSensitive() : base(AnalyzerConfiguration.Hotspot) { }

        public ClearTextProtocolsAreSensitive(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) =>
            objectInitializationTracker = new CSharpObjectInitializationTracker(constantValue => constantValue is bool value && value,
                                                                                ImmutableArray.Create(KnownType.System_Net_Mail_SmtpClient, KnownType.System_Net_FtpWebRequest),
                                                                                propertyName => EnableSslName == propertyName);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(ccc =>
            {
                if (!IsEnabled(ccc.Options))
                {
                    return;
                }

                context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var text = GetText(c.Node);
                    if (TryGetUnsafeProtocol(text, out var unsafeProtocol))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(DefaultRule, c.Node.GetLocation(), unsafeProtocol, recommendedProtocols[unsafeProtocol]));
                    }
                },
                SyntaxKind.StringLiteralExpression,
                SyntaxKind.InterpolatedStringExpression);

                context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var objectCreation = (ObjectCreationExpressionSyntax)c.Node;
                    if (!IsServerSafe(objectCreation) && objectInitializationTracker.ShouldBeReported(objectCreation, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(EnableSslRule, objectCreation.GetLocation()));
                    }

                    if (Regex.IsMatch(objectCreation.Type.ToString(), TelnetPatternForIdentifier))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(DefaultRule, objectCreation.GetLocation(), Telnet, recommendedProtocols[Telnet]));
                    }
                },
                SyntaxKind.ObjectCreationExpression);

                context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (Regex.IsMatch(invocation.Expression.ToString(), TelnetPatternForIdentifier))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(DefaultRule, invocation.GetLocation(), Telnet, recommendedProtocols[Telnet]));
                    }
                },
                SyntaxKind.InvocationExpression);

                context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;
                    if (assignment.Left is MemberAccessExpressionSyntax memberAccess
                        && memberAccess.IsMemberAccessOnKnownType(EnableSslName, KnownType.System_Net_FtpWebRequest, c.SemanticModel)
                        && c.SemanticModel.GetConstantValue(assignment.Right) is { HasValue: true, Value: bool enableSslValue }
                        && !enableSslValue)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(EnableSslRule, assignment.GetLocation()));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression);
            });

        private bool IsServerSafe(ObjectCreationExpressionSyntax objectCreation) =>
            objectCreation.ArgumentList.Arguments.Count > 0
            && validServerValues.Contains(GetText(objectCreation.ArgumentList.Arguments[0].Expression));

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
