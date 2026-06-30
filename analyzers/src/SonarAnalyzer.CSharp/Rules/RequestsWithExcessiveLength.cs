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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RequestsWithExcessiveLength : RequestsWithExcessiveLengthBase<SyntaxKind, AttributeSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override void Initialize(SonarParametrizedAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                var body = methodDeclaration.BodyOrExpressionBody;

                if (body is not null
                    && body.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(x => x.IsMethodInvocation(KnownType.Microsoft_AspNetCore_Components_Forms_IBrowserFile, "OpenReadStream", c.Model)))
                {
                    var walker = new StreamReadSizeCheck(c.Model, FileUploadSizeLimit);
                    if (walker.SafeVisit(body))
                    {
                        foreach (var location in walker.Locations)
                        {
                            c.ReportIssue(rule, location);
                        }
                    }
                }
            },
            SyntaxKind.MethodDeclaration);

        base.Initialize(context);
    }

    protected override AttributeSyntax IsInvalidRequestFormLimits(AttributeSyntax attribute, SemanticModel model) =>
        IsRequestFormLimits(attribute.Name.ToString())
        && attribute.ArgumentList?.Arguments.FirstOrDefault(IsMultipartBodyLengthLimit) is { } firstArgument
        && model.GetConstantValue(firstArgument.Expression) is { HasValue: true } constantValue
        && constantValue.Value is int intValue
        && intValue > FileUploadSizeLimit
        && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestFormLimitsAttribute, model)
            ? attribute
            : null;

    protected override AttributeSyntax IsInvalidRequestSizeLimit(AttributeSyntax attribute, SemanticModel model) =>
        IsRequestSizeLimit(attribute.Name.ToString())
        && attribute.ArgumentList?.Arguments.FirstOrDefault() is { } firstArgument
        && model.GetConstantValue(firstArgument.Expression) is { HasValue: true } constantValue
        && constantValue.Value is int intValue
        && intValue > FileUploadSizeLimit
        && attribute.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_RequestSizeLimitAttribute, model)
            ? attribute
            : null;

    protected override SyntaxNode MethodLocalFunctionOrClassDeclaration(AttributeSyntax attribute) =>
        attribute.FirstAncestorOrSelf<SyntaxNode>(x => x is MemberDeclarationSyntax || LocalFunctionStatementSyntaxWrapper.IsInstance(x));

    protected override string AttributeName(AttributeSyntax attribute) =>
        attribute.Name.ToString();

    private static bool IsMultipartBodyLengthLimit(AttributeArgumentSyntax argument) =>
        argument.NameEquals is { } nameEquals
        && nameEquals.Name.Identifier.ValueText.Equals(MultipartBodyLengthLimit);

    private sealed class StreamReadSizeCheck : SafeCSharpSyntaxWalker
    {
        private const int GetMultipleFilesMaximumFileCount = 10; // Default value for `maximumFileCount` in `InputFileChangeEventArgs.GetMultipleFiles` is 10

        private readonly SemanticModel model;
        private readonly int fileUploadSizeLimit;

        private int numberOfFiles = 1;

        public List<Location> Locations { get; } = [];

        public StreamReadSizeCheck(SemanticModel model, int fileUploadSizeLimit)
        {
            this.model = model;
            this.fileUploadSizeLimit = fileUploadSizeLimit;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.IsMethodInvocation(KnownType.Microsoft_AspNetCore_Components_Forms_InputFileChangeEventArgs, "GetMultipleFiles", model))
            {
                numberOfFiles = node.ArgumentList.Arguments.FirstOrDefault() is { } firstArgument
                                && model.GetConstantValue(firstArgument.Expression) is { HasValue: true } constantValue
                                && Convert.ToInt32(constantValue.Value) is var count
                                    ? count
                                    : GetMultipleFilesMaximumFileCount;
            }
            if (node.IsMethodInvocation(KnownType.Microsoft_AspNetCore_Components_Forms_IBrowserFile, "OpenReadStream", model))
            {
                var size = OpenReadStreamInvocationSize(node, model);
                if (numberOfFiles * size > fileUploadSizeLimit)
                {
                    Locations.Add(node.GetLocation());
                }
            }

            base.VisitInvocationExpression(node);
        }

        private static long OpenReadStreamInvocationSize(InvocationExpressionSyntax invocation, SemanticModel model) =>
            invocation.ArgumentList.Arguments.FirstOrDefault() is { } firstArgument
            && model.GetConstantValue(firstArgument.Expression) is { HasValue: true } constantValue
            && Convert.ToInt64(constantValue.Value) is var size
                ? size
                : 500 * 1024; // Default `maxAllowedSize` in `IBrowserFile.OpenReadStream` is 500 KB
    }
}
