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
using System.Linq;
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
        protected const string RequestSizeLimit = "RequestSizeLimit";
        protected const string RequestSizeLimitAttribute = RequestSizeLimit + Attribute;
        protected const string DisableRequestSizeLimit = "DisableRequestSizeLimit";
        protected const string DisableRequestSizeLimitAttribute = DisableRequestSizeLimit + Attribute;
        protected const string RequestFormLimits = "RequestFormLimits";
        protected const string RequestFormLimitsAttribute = RequestFormLimits + Attribute;
        private const string MessageFormat = "Make sure the content length limit is safe here.";
        private const string Attribute = "Attribute";
        private const int DefaultFileUploadSizeLimit = 8_000_000;
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

        protected RequestsWithExcessiveLengthBase(System.Resources.ResourceManager rspecResources, IAnalyzerConfiguration analyzerConfiguration)
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();
            this.analyzerConfiguration = analyzerConfiguration;
        }

        protected override void Initialize(ParameterLoadingAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                c =>
                {
                    var attributesOverTheLimit = new ConcurrentDictionary<SyntaxNode, Attributes>();

                    c.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer,
                        cc => CollectAttributesOverTheLimit(cc, attributesOverTheLimit),
                        Language.SyntaxKind.Attribute);

                    c.RegisterCompilationEndAction(cc => ReportOnCollectedAttributes(cc, attributesOverTheLimit));
                });

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
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attribute.GetLocation()));
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
                context.ReportDiagnosticWhenActive(
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
            analyzerConfiguration.Initialize(options, AnalyzerConfiguration.RuleLoader);
            return SupportedDiagnostics.Any(d => analyzerConfiguration.IsEnabled(d.Id));
        }

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
