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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        [RuleParameter("fileUploadSizeLimit", PropertyType.Integer, "The maximum size of HTTP requests handling file uploads (in bytes).", DefaultFileUploadSizeLimit)]
        public int FileUploadSizeLimit { get; set; } = DefaultFileUploadSizeLimit;

        protected RequestsWithExcessiveLengthBase(System.Resources.ResourceManager rspecResources, IAnalyzerConfiguration analyzerConfiguration)
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();
            this.analyzerConfiguration = analyzerConfiguration;
        }

        protected override void Initialize(ParameterLoadingAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (!IsEnabled(c.Options))
                    {
                        return;
                    }

                    if (c.Node is TAttributeSyntax attribute
                        && (attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_DisableRequestSizeLimitAttribute, c.SemanticModel)
                            || IsInvalidRequestSizeLimit(attribute, c.SemanticModel)
                            || IsInvalidRequestFormLimits(attribute, c.SemanticModel)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, c.Node.GetLocation()));
                    }
                },
                Language.SyntaxKind.Attribute);

        private bool IsEnabled(AnalyzerOptions options)
        {
            analyzerConfiguration.Initialize(options);
            return SupportedDiagnostics.Any(d => analyzerConfiguration.IsEnabled(d.Id));
        }
    }
}
