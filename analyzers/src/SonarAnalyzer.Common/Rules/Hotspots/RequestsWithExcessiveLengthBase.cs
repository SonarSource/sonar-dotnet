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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class RequestsWithExcessiveLengthBase<TSyntaxKind, TAttributeSyntax> : ParameterLoadingDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TAttributeSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S5693";
        protected const string MultipartBodyLengthLimit = "MultipartBodyLengthLimit";
        private const string RequestSizeLimit = "RequestSizeLimit";
        private const string RequestSizeLimitAttribute = RequestSizeLimit + Attribute;
        private const string DisableRequestSizeLimit = "DisableRequestSizeLimit";
        private const string DisableRequestSizeLimitAttribute = DisableRequestSizeLimit + Attribute;
        private const string RequestFormLimits = "RequestFormLimits";
        private const string RequestFormLimitsAttribute = RequestFormLimits + Attribute;
        private const string MessageFormat = "Make sure the content length limit is safe here.";
        private const string Attribute = "Attribute";
        private const int DefaultFileUploadSizeLimit = 8_000_000;
        private const int MaxAllowedRequestLength = 8192;
        private const int MaxAllowedContentLength = 8_388_608;
        private readonly IAnalyzerConfiguration analyzerConfiguration;
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract TAttributeSyntax IsInvalidRequestFormLimits(TAttributeSyntax attribute, SemanticModel semanticModel);
        protected abstract TAttributeSyntax IsInvalidRequestSizeLimit(TAttributeSyntax attribute, SemanticModel semanticModel);
        protected abstract SyntaxNode GetMethodLocalFunctionOrClassDeclaration(TAttributeSyntax attribute, SemanticModel semanticModel);
        protected abstract string AttributeName(TAttributeSyntax attribute);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        [RuleParameter("fileUploadSizeLimit", PropertyType.Integer, "The maximum size of HTTP requests handling file uploads (in bytes).", DefaultFileUploadSizeLimit)]
        public int FileUploadSizeLimit { get; set; } = DefaultFileUploadSizeLimit;
        protected override bool EnableConcurrentExecution => false;

        protected RequestsWithExcessiveLengthBase(IAnalyzerConfiguration analyzerConfiguration)
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources).WithNotConfigurable();
            this.analyzerConfiguration = analyzerConfiguration;
        }

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    var attributesOverTheLimit = new ConcurrentDictionary<SyntaxNode, Attributes>();

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        Language.GeneratedCodeRecognizer,
                        cc => CollectAttributesOverTheLimit(cc, attributesOverTheLimit),
                        Language.SyntaxKind.Attribute);

                    c.RegisterCompilationEndAction(cc => ReportOnCollectedAttributes(cc, attributesOverTheLimit));
                });
            context.GetInnerContext().RegisterCompilationAction(c => CheckWebConfig(context.GetInnerContext(), c));
        }

        protected bool IsRequestFormLimits(string attributeName) =>
            attributeName.Equals(RequestFormLimits, Language.NameComparison)
            || attributeName.Equals(RequestFormLimitsAttribute, Language.NameComparison);

        protected bool IsRequestSizeLimit(string attributeName) =>
            attributeName.Equals(RequestSizeLimit, Language.NameComparison)
            || attributeName.Equals(RequestSizeLimitAttribute, Language.NameComparison);

        private void CollectAttributesOverTheLimit(SyntaxNodeAnalysisContext context, ConcurrentDictionary<SyntaxNode, Attributes> attributesOverTheLimit)
        {
            if (!IsEnabled(context.Options))
            {
                return;
            }

            var attribute = (TAttributeSyntax)context.Node;

            if (IsDisableRequestSizeLimit(AttributeName(attribute))
                && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_DisableRequestSizeLimitAttribute, context.SemanticModel))
            {
                context.ReportIssue(Diagnostic.Create(rule, attribute.GetLocation()));
                return;
            }

            var requestSizeLimit = IsInvalidRequestSizeLimit(attribute, context.SemanticModel);
            var requestFormLimits = IsInvalidRequestFormLimits(attribute, context.SemanticModel);

            if ((requestSizeLimit != null || requestFormLimits != null)
                && GetMethodLocalFunctionOrClassDeclaration(attribute, context.SemanticModel) is { } declaration)
            {
                attributesOverTheLimit.AddOrUpdate(
                    declaration,
                    new Attributes(requestFormLimits, requestSizeLimit),
                    (declaration, attributes) => new Attributes(requestFormLimits, requestSizeLimit, attributes));
            }
        }

        private void ReportOnCollectedAttributes(CompilationAnalysisContext context, ConcurrentDictionary<SyntaxNode, Attributes> attributesOverTheLimit)
        {
            foreach (var invalidAttributes in attributesOverTheLimit.Values)
            {
                context.ReportIssue(
                    invalidAttributes.SecondaryAttribute != null
                        ? Diagnostic.Create(rule, invalidAttributes.MainAttribute.GetLocation(), new List<Location> { invalidAttributes.SecondaryAttribute.GetLocation() })
                        : Diagnostic.Create(rule, invalidAttributes.MainAttribute.GetLocation()));
            }
        }

        private bool IsDisableRequestSizeLimit(string attributeName) =>
            attributeName.Equals(DisableRequestSizeLimit, Language.NameComparison)
            || attributeName.Equals(DisableRequestSizeLimitAttribute, Language.NameComparison);

        private bool IsEnabled(AnalyzerOptions options)
        {
            analyzerConfiguration.Initialize(options);
            return SupportedDiagnostics.Any(d => analyzerConfiguration.IsEnabled(d.Id));
        }

        private void CheckWebConfig(SonarAnalysisContext context, CompilationAnalysisContext c)
        {
            foreach (var fullPath in context.GetWebConfig(c))
            {
                var webConfig = File.ReadAllText(fullPath);
                if (webConfig.Contains("<system.web") && XmlHelper.ParseXDocument(webConfig) is { } doc)
                {
                    ReportRequestLengthViolation(doc, fullPath, c);
                }
            }
        }

        private void ReportRequestLengthViolation(XDocument doc, string webConfigPath, CompilationAnalysisContext c)
        {
            foreach (var httpRuntime in doc.XPathSelectElements("configuration/system.web/httpRuntime"))
            {
                if (httpRuntime.Attribute("maxRequestLength") is { } maxRequestLength
                    && IsVulnerable(maxRequestLength.Value, MaxAllowedRequestLength)
                    && maxRequestLength.CreateLocation(webConfigPath) is { } location)
                {
                    c.ReportIssue(Diagnostic.Create(rule, location));
                }
            }
            foreach (var requestLimit in doc.XPathSelectElements("configuration/system.webServer/security/requestFiltering/requestLimits"))
            {
                if (requestLimit.Attribute("maxAllowedContentLength") is { } maxAllowedContentLength
                    && IsVulnerable(maxAllowedContentLength.Value, MaxAllowedContentLength)
                    && maxAllowedContentLength.CreateLocation(webConfigPath) is { } location)
                {
                    c.ReportIssue(Diagnostic.Create(rule, location));
                }
            }
        }

        private static bool IsVulnerable(string value, int limit) =>
            int.TryParse(value, out var val) && val > limit;

        // This struct is used as the same attributes can not be applied multiple times to the same declaration.
        private struct Attributes
        {
            private readonly TAttributeSyntax requestForm;
            private readonly TAttributeSyntax requestSize;

            public SyntaxNode MainAttribute =>
                requestForm ?? requestSize;

            public SyntaxNode SecondaryAttribute =>
                requestForm != null && requestSize != null ? requestSize : null;

            public Attributes(TAttributeSyntax requestForm, TAttributeSyntax requestSize)
            {
                this.requestForm = requestForm;
                this.requestSize = requestSize;
            }

            public Attributes(TAttributeSyntax requestForm, TAttributeSyntax requestSize, Attributes oldAttributes)
            {
                this.requestForm = requestForm ?? oldAttributes.requestForm;
                this.requestSize = requestSize ?? oldAttributes.requestSize;
            }
        }
    }
}
