/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class RequestsWithExcessiveLength : RequestsWithExcessiveLengthBase<SyntaxKind, AttributeSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override AttributeSyntax IsInvalidRequestFormLimits(AttributeSyntax attribute, SemanticModel model) =>
        IsRequestFormLimits(attribute.Name.ToString())
        && attribute.ArgumentList?.Arguments.FirstOrDefault(IsMultipartBodyLengthLimit) is { } firstArgument
        && model.GetConstantValue(firstArgument.GetExpression()) is { HasValue: true } constantValue
        && constantValue.Value is int intValue
        && intValue > FileUploadSizeLimit
        && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestFormLimitsAttribute, model)
            ? attribute
            : null;

    protected override AttributeSyntax IsInvalidRequestSizeLimit(AttributeSyntax attribute, SemanticModel model) =>
        IsRequestSizeLimit(attribute.Name.ToString())
        && attribute.ArgumentList?.Arguments.FirstOrDefault() is { } firstArgument
        && model.GetConstantValue(firstArgument.GetExpression()) is { HasValue: true } constantValue
        && constantValue.Value is int intValue
        && intValue > FileUploadSizeLimit
        && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestSizeLimitAttribute, model)
            ? attribute
            : null;

    protected override SyntaxNode MethodLocalFunctionOrClassDeclaration(AttributeSyntax attribute) =>
        attribute.FirstAncestorOrSelf<DeclarationStatementSyntax>();

    protected override string AttributeName(AttributeSyntax attribute) =>
        attribute.Name.ToString();

    private bool IsMultipartBodyLengthLimit(ArgumentSyntax argument) =>
        argument is SimpleArgumentSyntax { NameColonEquals: { } nameColonEquals }
        && nameColonEquals.Name.Identifier.ValueText.Equals(MultipartBodyLengthLimit, Language.NameComparison);
}
