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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class RequestsWithExcessiveLength : RequestsWithExcessiveLengthBase<SyntaxKind, AttributeSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public RequestsWithExcessiveLength() : this(AnalyzerConfiguration.Hotspot) { }

        internal RequestsWithExcessiveLength(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) { }

        protected override AttributeSyntax IsInvalidRequestFormLimits(AttributeSyntax attribute, SemanticModel semanticModel) =>
            IsRequestFormLimits(attribute.Name.ToString())
            && attribute.ArgumentList?.Arguments.FirstOrDefault(arg => IsMultipartBodyLengthLimit(arg)) is { } firstArgument
            && semanticModel.GetConstantValue(firstArgument.Expression) is { HasValue: true } constantValue
            && constantValue.Value is int intValue
            && intValue > FileUploadSizeLimit
            && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestFormLimitsAttribute, semanticModel)
                ? attribute
                : null;

        protected override AttributeSyntax IsInvalidRequestSizeLimit(AttributeSyntax attribute, SemanticModel semanticModel) =>
            IsRequestSizeLimit(attribute.Name.ToString())
            && attribute.ArgumentList?.Arguments.FirstOrDefault() is { } firstArgument
            && semanticModel.GetConstantValue(firstArgument.Expression) is { HasValue: true } constantValue
            && constantValue.Value is int intValue
            && intValue > FileUploadSizeLimit
            && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestSizeLimitAttribute, semanticModel)
                ? attribute
                : null;

        protected override SyntaxNode GetMethodLocalFunctionOrClassDeclaration(AttributeSyntax attribute, SemanticModel semanticModel) =>
            attribute.FirstAncestorOrSelf<SyntaxNode>(node => node is MemberDeclarationSyntax || LocalFunctionStatementSyntaxWrapper.IsInstance(node));

        protected override string AttributeName(AttributeSyntax attribute) =>
            attribute.Name.ToString();

        private static bool IsMultipartBodyLengthLimit(AttributeArgumentSyntax argument) =>
            argument.NameEquals is { } nameEquals
            && nameEquals.Name.Identifier.ValueText.Equals(MultipartBodyLengthLimit);
    }
}
