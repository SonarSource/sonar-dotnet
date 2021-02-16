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
        private const string MessageFormat = "Make sure the content length limit is safe here.";
        private const int DefaultFileUploadSizeLimit = 8_000_000;
        private readonly IAnalyzerConfiguration analyzerConfiguration;
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract bool IsInvalidRequestFormLimits(TAttributeSyntax attribute, SemanticModel semanticModel);
        protected abstract bool IsInvalidRequestSizeLimit(TAttributeSyntax attribute, SemanticModel semanticModel);
        protected abstract SyntaxNode GetMethodOrClassDeclaration(TAttributeSyntax attribute, SemanticModel semanticModel);

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
                        cc =>
                        {
                            if (!IsEnabled(c.Options))
                            {
                                return;
                            }

                            var attribute = (TAttributeSyntax)cc.Node;

                            if (attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_DisableRequestSizeLimitAttribute, cc.SemanticModel))
                            {
                                cc.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attribute.GetLocation()));
                            }

                            if ((IsInvalidRequestSizeLimit(attribute, cc.SemanticModel)
                                 || IsInvalidRequestFormLimits(attribute, cc.SemanticModel))
                                && GetMethodOrClassDeclaration(attribute, cc.SemanticModel) is { } delcaration)
                            {
                                var isRequestFormLimits = attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestFormLimitsAttribute, cc.SemanticModel);

                                attributesOverTheLimit.AddOrUpdate(
                                    delcaration,
                                    new Attributes
                                    {
                                        InvalidRequestForm = isRequestFormLimits ? attribute : null,
                                        InvalidRequestSize = isRequestFormLimits ? null : attribute
                                    },
                                    (declaration, attributes) =>
                                    new Attributes
                                    {
                                        InvalidRequestForm = isRequestFormLimits ? attribute : attributes.InvalidRequestForm,
                                        InvalidRequestSize = isRequestFormLimits ? attributes.InvalidRequestSize : attribute
                                    });
                            }
                        },
                        Language.SyntaxKind.Attribute);

                    c.RegisterCompilationEndAction(
                        cc =>
                        {
                            foreach (var attrsByDec in attributesOverTheLimit)
                            {
                                var mainAttribute = attrsByDec.Value.InvalidRequestForm ?? attrsByDec.Value.InvalidRequestSize;

                                cc.ReportDiagnosticWhenActive(
                                    attrsByDec.Value.InvalidRequestForm != null
                                    && attrsByDec.Value.InvalidRequestSize != null
                                        ? Diagnostic.Create(rule, mainAttribute.GetLocation(), new List<Location> { attrsByDec.Value.InvalidRequestSize.GetLocation() })
                                        : Diagnostic.Create(rule, mainAttribute.GetLocation()));
                            }
                        });
                });

        private bool IsEnabled(AnalyzerOptions options)
        {
            analyzerConfiguration.Initialize(options);
            return SupportedDiagnostics.Any(d => analyzerConfiguration.IsEnabled(d.Id));
        }

        private struct Attributes
        {
            public SyntaxNode InvalidRequestForm { get; set; }

            public SyntaxNode InvalidRequestSize { get; set; }
        }
    }
}
