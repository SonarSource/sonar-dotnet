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
    public sealed class RequestsWithExcessiveLength : ParameterLoadingDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5693";
        private const string MessageFormat = "Make sure the content length limit is safe here.";
        private const int DefaultFileUploadSizeLimit = 8000000;
        private const int DefaultStandardSizeLimit = 2000000;
        private const string MultipartBodyLengthLimit = "MultipartBodyLengthLimit";

        private CSharpExpressionNumericConverter numericConverter = new CSharpExpressionNumericConverter();

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [RuleParameter("standardSizeLimit", PropertyType.Integer, "The maximum size of regular HTTP requests (in bytes).", DefaultStandardSizeLimit)]
        public int StandardSizeLimit { get; set; } = DefaultStandardSizeLimit;

        [RuleParameter("fileUploadSizeLimit", PropertyType.Integer, "The maximum size of HTTP requests handling file uploads (in bytes).", DefaultFileUploadSizeLimit)]
        public int FileUploadSizeLimit { get; set; } = DefaultFileUploadSizeLimit;

        protected override void Initialize(ParameterLoadingAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    if (c.Node is AttributeSyntax attribute
                        && (attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_DisableRequestSizeLimitAttribute, c.SemanticModel)
                            || IsInvalidRequestSizeLimitAttribute(attribute, c.SemanticModel)
                            || IsInvalidRequestFormLimitsAttribut(attribute, c.SemanticModel)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, c.Node.GetLocation()));
                    }
                },
                SyntaxKind.Attribute);

        private bool IsInvalidRequestFormLimitsAttribut(AttributeSyntax attribute, SemanticModel semanticModel)
        {
          return  attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestFormLimitsAttribute, semanticModel)
            && attribute.ArgumentList != null
            && attribute.ArgumentList.Arguments.First() is { } firstArgument
            && firstArgument.NameEquals.Name.Identifier.ValueText.Equals(MultipartBodyLengthLimit)
            && numericConverter.TryGetConstantIntValue(firstArgument.Expression, out var intValue)
            && intValue > FileUploadSizeLimit;
        }

        private bool IsInvalidRequestSizeLimitAttribute(AttributeSyntax attribute, SemanticModel semanticModel) =>
            attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestSizeLimitAttribute, semanticModel)
            && attribute.ArgumentList != null
            && attribute.ArgumentList.Arguments.First() is { } firstArgument
            && numericConverter.TryGetConstantIntValue(firstArgument.Expression, out var intValue)
            && intValue > StandardSizeLimit;
    }
}
