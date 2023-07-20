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

using System.Text.RegularExpressions;
using SonarAnalyzer.Helpers.Trackers;

namespace SonarAnalyzer.Rules.CSharp
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
        };

        private static readonly string[] CommonlyUsedExampleDomains = { "example.com", "example.org", "test.com" };
        private static readonly string[] LocalhostAddresses = { "localhost", "127.0.0.1", "::1" };
        private static readonly KnownType[] AttributesWithNamespaceParameter = new[]
        {
            KnownType.System_Windows_Markup_XmlnsPrefixAttribute,
            KnownType.System_Windows_Markup_XmlnsDefinitionAttribute,
            KnownType.System_Windows_Markup_XmlnsCompatibleWithAttribute,
        };

        private readonly CSharpObjectInitializationTracker objectInitializationTracker =
            new(constantValue => constantValue is bool value && value,
                                                  ImmutableArray.Create(KnownType.System_Net_Mail_SmtpClient, KnownType.System_Net_FtpWebRequest),
                                                  propertyName => propertyName == EnableSslName);

        private readonly Regex httpRegex;
        private readonly Regex ftpRegex;
        private readonly Regex telnetRegex;
        private readonly Regex telnetRegexForIdentifier;
        private readonly Regex validServerRegex;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DefaultRule, EnableSslRule);

        public ClearTextProtocolsAreSensitive() : this(AnalyzerConfiguration.Hotspot) { }

        public ClearTextProtocolsAreSensitive(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration)
        {
            const string allSubdomainsPattern = @"([^/?#]+\.)?";

            var domainsList = LocalhostAddresses
                .Concat(CommonlyUsedXmlDomains)
                .Select(Regex.Escape)
                .Concat(CommonlyUsedExampleDomains.Select(x => allSubdomainsPattern + Regex.Escape(x)));

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

                context.RegisterNodeAction(
                    VisitStringExpressions,
                    SyntaxKind.StringLiteralExpression,
                    SyntaxKind.InterpolatedStringExpression,
                    SyntaxKindEx.Utf8StringLiteralExpression);

                context.RegisterNodeAction(VisitObjectCreation, SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);
                context.RegisterNodeAction(VisitInvocationExpression, SyntaxKind.InvocationExpression);
                context.RegisterNodeAction(VisitAssignments, SyntaxKind.SimpleAssignmentExpression);
            });

        private void VisitObjectCreation(SonarSyntaxNodeReportingContext context)
        {
            var objectCreation = ObjectCreationFactory.Create(context.Node);

            if (!IsServerSafe(objectCreation, context.SemanticModel) && objectInitializationTracker.ShouldBeReported(objectCreation, context.SemanticModel, false))
            {
                context.ReportIssue(CreateDiagnostic(EnableSslRule, objectCreation.Expression.GetLocation()));
            }
            else if (telnetRegexForIdentifier.IsMatch(objectCreation.TypeAsString(context.SemanticModel)))
            {
                context.ReportIssue(CreateDiagnostic(DefaultRule, objectCreation.Expression.GetLocation(), TelnetKey, RecommendedProtocols[TelnetKey]));
            }
        }

        private void VisitInvocationExpression(SonarSyntaxNodeReportingContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (telnetRegexForIdentifier.IsMatch(invocation.Expression.ToString()))
            {
                context.ReportIssue(CreateDiagnostic(DefaultRule, invocation.GetLocation(), TelnetKey, RecommendedProtocols[TelnetKey]));
            }
        }

        private static void VisitAssignments(SonarSyntaxNodeReportingContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            if (assignment.Left is MemberAccessExpressionSyntax memberAccess
                && memberAccess.IsMemberAccessOnKnownType(EnableSslName, KnownType.System_Net_FtpWebRequest, context.SemanticModel)
                && assignment.Right.FindConstantValue(context.SemanticModel) is bool enableSslValue
                && !enableSslValue)
            {
                context.ReportIssue(CreateDiagnostic(EnableSslRule, assignment.GetLocation()));
            }
        }

        private void VisitStringExpressions(SonarSyntaxNodeReportingContext c)
        {
            if (GetUnsafeProtocol(c.Node, c.SemanticModel) is { } unsafeProtocol)
            {
                c.ReportIssue(CreateDiagnostic(DefaultRule, c.Node.GetLocation(), unsafeProtocol, RecommendedProtocols[unsafeProtocol]));
            }
        }

        private bool IsServerSafe(IObjectCreation objectCreation, SemanticModel semanticModel) =>
            objectCreation.ArgumentList?.Arguments.Count > 0
            && validServerRegex.IsMatch(GetText(objectCreation.ArgumentList.Arguments[0].Expression, semanticModel));

        private string GetUnsafeProtocol(SyntaxNode node, SemanticModel semanticModel)
        {
            var text = GetText(node, semanticModel);
            if (httpRegex.IsMatch(text) && !IsNamespace(semanticModel, node.Parent))
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

        private static string GetText(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node is InterpolatedStringExpressionSyntax interpolatedStringExpression)
            {
                interpolatedStringExpression.TryGetInterpolatedTextValue(semanticModel, out var interpolatedValue);
                return interpolatedValue ?? interpolatedStringExpression.GetContentsText();
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
            model.GetSymbolInfo(attribute).Symbol is IMethodSymbol { ContainingType: { } attributeSymbol } && AttributesWithNamespaceParameter.Any(x => x.Matches(attributeSymbol));

        private static bool TokenContainsNamespace(SyntaxToken token) =>
            token.Text.IndexOf("Namespace", StringComparison.OrdinalIgnoreCase) != -1;

        private static Regex CompileRegex(string pattern, bool ignoreCase = true) =>
            new(pattern, ignoreCase
                          ? RegexOptions.Compiled | RegexOptions.IgnoreCase
                          : RegexOptions.Compiled);
    }
}
