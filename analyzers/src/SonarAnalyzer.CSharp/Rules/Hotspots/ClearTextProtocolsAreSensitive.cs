/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Text.RegularExpressions;
using SonarAnalyzer.CSharp.Core.Trackers;

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ClearTextProtocolsAreSensitive : HotspotDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5332";
        private const string MessageFormat = "Using {0} protocol is insecure. Use {1} instead.";
        private const string EnableSslMessage = "EnableSsl should be set to true.";

        private const string TelnetKey = "telnet";
        private const string EnableSslName = "EnableSsl";

        private static readonly DiagnosticDescriptor DefaultRule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly DiagnosticDescriptor EnableSslRule = DescriptorFactory.Create(DiagnosticId, EnableSslMessage);

        private static readonly Dictionary<string, string> RecommendedProtocols = new()
        {
            {"telnet", "ssh"},
            {"ftp", "sftp, scp or ftps"},
            {"http", "https"},
            {"clear-text SMTP", "SMTP over SSL/TLS or SMTP with STARTTLS" }
        };

        private static readonly string[] CommonlyUsedXmlDomains =
        {
            "www.w3.org",
            "xml.apache.org",
            "maven.apache.org",
            "schemas.xmlsoap.org",
            "schemas.openxmlformats.org",
            "rdfs.org",
            "purl.org",
            "xmlns.com",
            "schemas.google.com",
            "schemas.microsoft.com",
            "collations.microsoft.com",
            "a9.com",
            "ns.adobe.com",
            "ltsc.ieee.org",
            "docbook.org",
            "graphml.graphdrawing.org",
            "json-schema.org",
            "www.sitemaps.org",
            "exslt.org",
            "docs.oasis-open.org",
            "ws-i.org",
            "schemas.android.com",
            "www.omg.org",
            "www.opengis.net",
            "www.itunes.com",
        };

        private static readonly string[] CommonlyUsedExampleDomains = { "example.com", "example.org", "test.com" };
        private static readonly string[] LocalhostAddresses = { "localhost", "127.0.0.1", "::1" };
        private static readonly KnownType[] AttributesWithNamespaceParameter = new[]
        {
            KnownType.System_Windows_Markup_XmlnsPrefixAttribute,
            KnownType.System_Windows_Markup_XmlnsDefinitionAttribute,
            KnownType.System_Windows_Markup_XmlnsCompatibleWithAttribute,
        };

        private static readonly CSharpObjectInitializationTracker ObjectInitializationTracker =
            new(constantValue => constantValue is bool value && value,
                                                  ImmutableArray.Create(KnownType.System_Net_Mail_SmtpClient, KnownType.System_Net_FtpWebRequest),
                                                  propertyName => propertyName == EnableSslName);

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);
        private static readonly Regex HttpRegex;
        private static readonly Regex FtpRegex;
        private static readonly Regex TelnetRegex;
        private static readonly Regex TelnetRegexForIdentifier;
        private static readonly Regex ValidServerRegex;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DefaultRule, EnableSslRule);

        public ClearTextProtocolsAreSensitive() : this(AnalyzerConfiguration.Hotspot) { }

        public ClearTextProtocolsAreSensitive(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) { }

        static ClearTextProtocolsAreSensitive()
        {
            const string allSubdomainsPattern = @"([^/?#]+\.)?";

            var domainsList = LocalhostAddresses
                .Concat(CommonlyUsedXmlDomains)
                .Select(Regex.Escape)
                .Concat(CommonlyUsedExampleDomains.Select(x => allSubdomainsPattern + Regex.Escape(x)));

            var validServerPattern = domainsList.JoinStr("|");

            HttpRegex = CompileRegex(@$"^http:\/\/(?!{validServerPattern}).");
            FtpRegex = CompileRegex(@$"^ftp:\/\/.*@(?!{validServerPattern})");
            TelnetRegex = CompileRegex(@$"^telnet:\/\/.*@(?!{validServerPattern})");
            TelnetRegexForIdentifier = CompileRegex(@"Telnet(?![a-z])", false);
            ValidServerRegex = CompileRegex($"^({validServerPattern})$");
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(c =>
            {
                if (!IsEnabled(c.Options))
                {
                    return;
                }

                c.RegisterNodeAction(
                    VisitStringExpressions,
                    SyntaxKind.StringLiteralExpression,
                    SyntaxKind.InterpolatedStringExpression,
                    SyntaxKindEx.Utf8StringLiteralExpression);

                c.RegisterNodeAction(VisitObjectCreation, SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);
                c.RegisterNodeAction(VisitInvocationExpression, SyntaxKind.InvocationExpression);
                c.RegisterNodeAction(VisitAssignments, SyntaxKind.SimpleAssignmentExpression);
            });

        private static void VisitObjectCreation(SonarSyntaxNodeReportingContext context)
        {
            var objectCreation = ObjectCreationFactory.Create(context.Node);

            if (!IsServerSafe(objectCreation, context.Model) && ObjectInitializationTracker.ShouldBeReported(objectCreation, context.Model, false))
            {
                context.ReportIssue(EnableSslRule, objectCreation.Expression);
            }
            else if (objectCreation.TypeAsString(context.Model) is { } typeAsString && TelnetRegexForIdentifier.SafeIsMatch(typeAsString))
            {
                context.ReportIssue(DefaultRule, objectCreation.Expression, TelnetKey, RecommendedProtocols[TelnetKey]);
            }
        }

        private static void VisitInvocationExpression(SonarSyntaxNodeReportingContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (TelnetRegexForIdentifier.SafeIsMatch(invocation.Expression.ToString()))
            {
                context.ReportIssue(DefaultRule, invocation, TelnetKey, RecommendedProtocols[TelnetKey]);
            }
        }

        private static void VisitAssignments(SonarSyntaxNodeReportingContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            if (assignment.Left is MemberAccessExpressionSyntax memberAccess
                && memberAccess.IsMemberAccessOnKnownType(EnableSslName, KnownType.System_Net_FtpWebRequest, context.Model)
                && assignment.Right.FindConstantValue(context.Model) is bool enableSslValue
                && !enableSslValue)
            {
                context.ReportIssue(EnableSslRule, assignment);
            }
        }

        private static void VisitStringExpressions(SonarSyntaxNodeReportingContext c)
        {
            if (GetUnsafeProtocol(c.Node, c.Model) is { } unsafeProtocol)
            {
                c.ReportIssue(DefaultRule, c.Node, unsafeProtocol, RecommendedProtocols[unsafeProtocol]);
            }
        }

        private static bool IsServerSafe(IObjectCreation objectCreation, SemanticModel semanticModel) =>
            objectCreation.ArgumentList?.Arguments.Count > 0
            && ValidServerRegex.SafeIsMatch(GetText(objectCreation.ArgumentList.Arguments[0].Expression, semanticModel));

        private static string GetUnsafeProtocol(SyntaxNode node, SemanticModel semanticModel)
        {
            var text = GetText(node, semanticModel);
            if (HttpRegex.SafeIsMatch(text) && !IsNamespace(semanticModel, node.Parent))
            {
                return "http";
            }
            else if (FtpRegex.SafeIsMatch(text))
            {
                return "ftp";
            }
            else if (TelnetRegex.SafeIsMatch(text))
            {
                return "telnet";
            }
            else
            {
                return null;
            }
        }

        private static string GetText(SyntaxNode node, SemanticModel model)
        {
            if (node is InterpolatedStringExpressionSyntax interpolatedStringExpression)
            {
                return interpolatedStringExpression.InterpolatedTextValue(model) ?? interpolatedStringExpression.ContentsText();
            }
            else
            {
                return node is LiteralExpressionSyntax literalExpression ? literalExpression.Token.ValueText : string.Empty;
            }
        }

        private static bool IsNamespace(SemanticModel model, SyntaxNode node) =>
            node switch
            {
                AttributeArgumentSyntax attributeArgument when attributeArgument.NameEquals is { } nameEquals && TokenContainsNamespace(nameEquals.Name.Identifier) => true,
                AttributeArgumentSyntax { Parent.Parent: AttributeSyntax attribute } => IsAttributeWithNamespaceParameter(model, attribute),
                EqualsValueClauseSyntax equalsValueClause =>
                    (equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator && TokenContainsNamespace(variableDeclarator.Identifier))
                    || (equalsValueClause.Parent is ParameterSyntax parameter && TokenContainsNamespace(parameter.Identifier)),
                AssignmentExpressionSyntax assignmentExpression =>
                    assignmentExpression.Left.RemoveParentheses() is IdentifierNameSyntax identifierName && TokenContainsNamespace(identifierName.Identifier),
                ArgumentSyntax { Parent: ArgumentListSyntax { Parent: { } invocationOrCreation } } argument =>
                    CSharpFacade.Instance.MethodParameterLookup(invocationOrCreation, model).TryGetSymbol(argument, out var symbol)
                        && symbol switch
                        {
                            { Name: "ns", ContainingNamespace: { } ns } when ns.Is("System.Xml.Serialization") => true,
                            { Name: "ns" or "uri" or "namespaceURI", ContainingNamespace: { } ns } when ns.Is("System.Xml") => true,
                            { Name: "xmlNamespace", ContainingType.Name: "XmlnsDictionary", ContainingNamespace: { } ns } when ns.Is("System.Windows.Markup") => true,
                            { Name: "namespaceName", ContainingSymbol.Name: "Get", ContainingType.Name: "XNamespace", ContainingNamespace: { } ns } when ns.Is("System.Xml.Linq") => true,
                            _ => false,
                        },
                _ => false
            };

        private static bool IsAttributeWithNamespaceParameter(SemanticModel model, AttributeSyntax attribute) =>
            model.GetSymbolInfo(attribute).Symbol is IMethodSymbol { ContainingType: { } attributeSymbol } && Array.Exists(AttributesWithNamespaceParameter, x => x.Matches(attributeSymbol));

        private static bool TokenContainsNamespace(SyntaxToken token) =>
            token.Text.IndexOf("Namespace", StringComparison.OrdinalIgnoreCase) != -1;

        private static Regex CompileRegex(string pattern, bool ignoreCase = true) =>
            new(pattern, ignoreCase
                          ? RegexOptions.Compiled | RegexOptions.IgnoreCase
                          : RegexOptions.Compiled, RegexTimeout);
    }
}
