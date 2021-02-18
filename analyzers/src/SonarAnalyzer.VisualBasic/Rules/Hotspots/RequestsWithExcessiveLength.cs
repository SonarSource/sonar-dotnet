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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class RequestsWithExcessiveLength : RequestsWithExcessiveLengthBase<SyntaxKind, AttributeSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        public RequestsWithExcessiveLength() : this(AnalyzerConfiguration.Hotspot) { }

        public RequestsWithExcessiveLength(IAnalyzerConfiguration analyzerConfiguration) : base(RspecStrings.ResourceManager, analyzerConfiguration) { }

        protected override AttributeSyntax IsInvalidRequestFormLimits(AttributeSyntax attribute, SemanticModel semanticModel) =>
            IsRequestFormLimits(attribute.Name.ToString())
            && attribute.ArgumentList?.Arguments.FirstOrDefault(arg => IsMultipartBodyLengthLimit(arg)) is { } firstArgument
            && semanticModel.GetConstantValue(firstArgument.GetExpression()) is { HasValue: true } constantValue
            && constantValue.Value is int intValue
            && intValue > FileUploadSizeLimit
            && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestFormLimitsAttribute, semanticModel)
                ? attribute
                : null;

        protected override AttributeSyntax IsInvalidRequestSizeLimit(AttributeSyntax attribute, SemanticModel semanticModel) =>
            IsRequestSizeLimit(attribute.Name.ToString())
            && attribute.ArgumentList?.Arguments.FirstOrDefault() is { } firstArgument
            && semanticModel.GetConstantValue(firstArgument.GetExpression()) is { HasValue: true } constantValue
            && constantValue.Value is int intValue
            && intValue > FileUploadSizeLimit
            && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestSizeLimitAttribute, semanticModel)
                ? attribute
                : null;

        protected override SyntaxNode GetMethodLocalFunctionOrClassDeclaration(AttributeSyntax attribute, SemanticModel semanticModel) =>
            attribute.FirstAncestorOrSelf<DeclarationStatementSyntax>();

        protected override string AttributeName(AttributeSyntax attribute) =>
            attribute.Name.ToString();

        private bool IsMultipartBodyLengthLimit(ArgumentSyntax argument) =>
            argument is SimpleArgumentSyntax { NameColonEquals: { } nameColonEquals }
            && nameColonEquals.Name.Identifier.ValueText.Equals(MultipartBodyLengthLimit, Language.NameComparison);
    }
}
