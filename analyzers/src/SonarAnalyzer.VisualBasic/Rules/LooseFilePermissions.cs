/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class LooseFilePermissions : LooseFilePermissionsBase<SyntaxKind, MemberAccessExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override void VisitInvocations(SonarSyntaxNodeReportingContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if ((IsSetAccessRule(invocation, context.Model) || IsAddAccessRule(invocation, context.Model))
            && (ObjectCreation(invocation, context.Model) is { } objectCreation))
        {
            var invocationLocation = invocation.GetLocation();
            var secondaryLocation = objectCreation.GetLocation();
            context.ReportIssue(rule, invocationLocation, invocationLocation.StartLine() == secondaryLocation.StartLine() ? [] : [secondaryLocation.ToSecondary(MessageFormat)]);
        }
    }

    private static ObjectCreationExpressionSyntax ObjectCreation(InvocationExpressionSyntax invocation, SemanticModel model)
    {
        var accessRuleSyntaxNode = VulnerableFileSystemAccessRule(invocation.DescendantNodes());
        if (accessRuleSyntaxNode is not null)
        {
            return accessRuleSyntaxNode;
        }

        var accessRuleSymbol = invocation.GetArgumentSymbolsOfKnownType(KnownType.System_Security_AccessControl_FileSystemAccessRule, model).FirstOrDefault();
        return accessRuleSymbol is null or IMethodSymbol
            ? null
            : VulnerableFileSystemAccessRule(accessRuleSymbol.LocationNodes(invocation));

        ObjectCreationExpressionSyntax VulnerableFileSystemAccessRule(IEnumerable<SyntaxNode> nodes) =>
            nodes.OfType<ObjectCreationExpressionSyntax>()
                .FirstOrDefault(x => IsFileSystemAccessRuleForEveryoneWithAllow(x, model));
    }

    private static bool IsFileSystemAccessRuleForEveryoneWithAllow(ObjectCreationExpressionSyntax objectCreation, SemanticModel model) =>
        objectCreation.IsKnownType(KnownType.System_Security_AccessControl_FileSystemAccessRule, model)
        && objectCreation.ArgumentList is { } argumentList
        && IsEveryone(argumentList.Arguments.First().GetExpression(), model)
        && model.GetConstantValue(argumentList.Arguments.Last().GetExpression()) is {HasValue: true, Value: 0};

    private static bool IsEveryone(SyntaxNode node, SemanticModel model) =>
        model.GetConstantValue(node) is {HasValue: true, Value: Everyone}
        || node.DescendantNodesAndSelf()
            .OfType<ObjectCreationExpressionSyntax>()
            .Any(x => IsNTAccountWithEveryone(x, model) || IsSecurityIdentifierWithEveryone(x, model));

    private static bool IsNTAccountWithEveryone(ObjectCreationExpressionSyntax objectCreation, SemanticModel model) =>
        objectCreation.IsKnownType(KnownType.System_Security_Principal_NTAccount, model)
        && objectCreation.ArgumentList is { } argumentList
        && model.GetConstantValue(argumentList.Arguments.Last().GetExpression()) is { HasValue: true, Value: Everyone };

    private static bool IsSecurityIdentifierWithEveryone(ObjectCreationExpressionSyntax objectCreation, SemanticModel model) =>
        objectCreation.IsKnownType(KnownType.System_Security_Principal_SecurityIdentifier, model)
        && objectCreation.ArgumentList is { } argumentList
        && model.GetConstantValue(argumentList.Arguments.First().GetExpression()) is { HasValue: true, Value: 1 };
}
