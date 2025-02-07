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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class RequestsWithExcessiveLength : RequestsWithExcessiveLengthBase<SyntaxKind, AttributeSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        public RequestsWithExcessiveLength() : this(Core.Common.AnalyzerConfiguration.Hotspot) { }

        public RequestsWithExcessiveLength(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) { }

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

        protected override SyntaxNode GetMethodLocalFunctionOrClassDeclaration(AttributeSyntax attribute) =>
            attribute.FirstAncestorOrSelf<DeclarationStatementSyntax>();

        protected override string AttributeName(AttributeSyntax attribute) =>
            attribute.Name.ToString();

        private bool IsMultipartBodyLengthLimit(ArgumentSyntax argument) =>
            argument is SimpleArgumentSyntax { NameColonEquals: { } nameColonEquals }
            && nameColonEquals.Name.Identifier.ValueText.Equals(MultipartBodyLengthLimit, Language.NameComparison);
    }
}
