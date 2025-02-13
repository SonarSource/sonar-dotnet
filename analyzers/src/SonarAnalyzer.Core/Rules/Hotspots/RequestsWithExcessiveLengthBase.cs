/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SonarAnalyzer.Core.Rules
{
    public abstract class RequestsWithExcessiveLengthBase<TSyntaxKind, TAttributeSyntax> : ParametrizedDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TAttributeSyntax : SyntaxNode
    {
        protected const string MultipartBodyLengthLimit = "MultipartBodyLengthLimit";
        private const string DiagnosticId = "S5693";
        private const string RequestSizeLimit = "RequestSizeLimit";
        private const string RequestSizeLimitAttribute = RequestSizeLimit + Attribute;
        private const string DisableRequestSizeLimit = "DisableRequestSizeLimit";
        private const string DisableRequestSizeLimitAttribute = DisableRequestSizeLimit + Attribute;
        private const string RequestFormLimits = "RequestFormLimits";
        private const string RequestFormLimitsAttribute = RequestFormLimits + Attribute;
        private const string MessageFormat = "Make sure the content length limit is safe here.";
        private const string Attribute = "Attribute";
        private const int DefaultFileUploadSizeLimit = 8_388_608;   // 8 MB (in bytes)
        private const int OneKilobyte = 1024; // 1 KB = 1024 bytes

        protected readonly DiagnosticDescriptor Rule;
        protected readonly IAnalyzerConfiguration AnalyzerConfiguration;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract TAttributeSyntax IsInvalidRequestFormLimits(TAttributeSyntax attribute, SemanticModel semanticModel);
        protected abstract TAttributeSyntax IsInvalidRequestSizeLimit(TAttributeSyntax attribute, SemanticModel semanticModel);
        protected abstract SyntaxNode GetMethodLocalFunctionOrClassDeclaration(TAttributeSyntax attribute);
        protected abstract string AttributeName(TAttributeSyntax attribute);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [RuleParameter("fileUploadSizeLimit", PropertyType.Integer, "The maximum size of HTTP requests handling file uploads (in bytes).", DefaultFileUploadSizeLimit)]
        public int FileUploadSizeLimit { get; set; } = DefaultFileUploadSizeLimit;
        protected override bool EnableConcurrentExecution => false;

        protected RequestsWithExcessiveLengthBase(IAnalyzerConfiguration analyzerConfiguration)
        {
            AnalyzerConfiguration = analyzerConfiguration;
            Rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);
        }

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    var attributesOverTheLimit = new Dictionary<SyntaxNode, Attributes>();

                    c.RegisterNodeAction(
                        Language.GeneratedCodeRecognizer,
                        cc => CollectAttributesOverTheLimit(cc, attributesOverTheLimit),
                        Language.SyntaxKind.Attribute);

                    c.RegisterCompilationEndAction(cc => ReportOnCollectedAttributes(cc, attributesOverTheLimit));
                });
            context.RegisterCompilationAction(CheckWebConfig);
        }

        protected bool IsRequestFormLimits(string attributeName) =>
            attributeName.Equals(RequestFormLimits, Language.NameComparison)
            || attributeName.Equals(RequestFormLimitsAttribute, Language.NameComparison);

        protected bool IsRequestSizeLimit(string attributeName) =>
            attributeName.Equals(RequestSizeLimit, Language.NameComparison)
            || attributeName.Equals(RequestSizeLimitAttribute, Language.NameComparison);

        private void CollectAttributesOverTheLimit(SonarSyntaxNodeReportingContext context, IDictionary<SyntaxNode, Attributes> attributesOverTheLimit)
        {
            if (!IsEnabled(context.Options))
            {
                return;
            }

            var attribute = (TAttributeSyntax)context.Node;

            if (IsDisableRequestSizeLimit(AttributeName(attribute))
                && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_DisableRequestSizeLimitAttribute, context.Model))
            {
                context.ReportIssue(Rule, attribute);
                return;
            }

            var requestSizeLimit = IsInvalidRequestSizeLimit(attribute, context.Model);
            var requestFormLimits = IsInvalidRequestFormLimits(attribute, context.Model);

            if ((requestSizeLimit != null || requestFormLimits != null)
                && GetMethodLocalFunctionOrClassDeclaration(attribute) is { } declaration)
            {
                attributesOverTheLimit[declaration] = attributesOverTheLimit.TryGetValue(declaration, out var existingAttribute)
                    ? new Attributes(requestFormLimits, requestSizeLimit, existingAttribute)
                    : new Attributes(requestFormLimits, requestSizeLimit);
            }
        }

        private void ReportOnCollectedAttributes(SonarCompilationReportingContext context, IDictionary<SyntaxNode, Attributes> attributesOverTheLimit)
        {
            foreach (var invalidAttributes in attributesOverTheLimit.Values)
            {
                context.ReportIssue(
                    Language.GeneratedCodeRecognizer,
                    Rule,
                    invalidAttributes.MainAttribute.GetLocation(),
                    invalidAttributes.SecondaryAttribute is null ? [] : [invalidAttributes.SecondaryAttribute.ToSecondaryLocation(MessageFormat)]);
            }
        }

        private bool IsDisableRequestSizeLimit(string attributeName) =>
            attributeName.Equals(DisableRequestSizeLimit, Language.NameComparison)
            || attributeName.Equals(DisableRequestSizeLimitAttribute, Language.NameComparison);

        private bool IsEnabled(AnalyzerOptions options)
        {
            AnalyzerConfiguration.Initialize(options);
            return SupportedDiagnostics.Any(x => AnalyzerConfiguration.IsEnabled(x.Id));
        }

        private void CheckWebConfig(SonarCompilationReportingContext c)
        {
            foreach (var fullPath in c.WebConfigFiles())
            {
                var webConfig = File.ReadAllText(fullPath);
                if (webConfig.Contains("<system.web") && webConfig.ParseXDocument() is { } doc)
                {
                    ReportRequestLengthViolation(c, doc, fullPath);
                }
            }
        }

        private void ReportRequestLengthViolation(SonarCompilationReportingContext c, XDocument doc, string webConfigPath)
        {
            foreach (var httpRuntime in doc.XPathSelectElements("configuration/system.web/httpRuntime"))
            {
                if (httpRuntime.Attribute("maxRequestLength") is { } maxRequestLength
                    && IsVulnerable(maxRequestLength.Value, FileUploadSizeLimit / OneKilobyte)
                    && maxRequestLength.CreateLocation(webConfigPath) is { } location)
                {
                    c.ReportIssue(Language.GeneratedCodeRecognizer, Rule, location);
                }
            }
            foreach (var requestLimit in doc.XPathSelectElements("configuration/system.webServer/security/requestFiltering/requestLimits"))
            {
                if (requestLimit.Attribute("maxAllowedContentLength") is { } maxAllowedContentLength
                    && IsVulnerable(maxAllowedContentLength.Value, FileUploadSizeLimit)
                    && maxAllowedContentLength.CreateLocation(webConfigPath) is { } location)
                {
                    c.ReportIssue(Language.GeneratedCodeRecognizer, Rule, location);
                }
            }
        }

        private static bool IsVulnerable(string value, int limit) =>
            int.TryParse(value, out var val) && val > limit;

        // This struct is used as the same attributes can not be applied multiple times to the same declaration.
        private readonly struct Attributes
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
