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
public sealed class LooseFilePermissions : LooseFilePermissionsBase<SyntaxKind, MemberAccessExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override void VisitInvocations(SonarSyntaxNodeReportingContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if ((IsSetAccessRule(invocation, context.Model) || IsAddAccessRule(invocation, context.Model))
            && (ObjectCreation(invocation, context.Model) is { } objectCreation))
        {
            var invocationLocation = invocation.GetLocation();
            var secondaryLocation = objectCreation.Expression.GetLocation();
            context.ReportIssue(rule, invocationLocation, invocationLocation.StartLine() == secondaryLocation.StartLine() ? [] : [secondaryLocation.ToSecondary(MessageFormat)]);
        }
    }

    private static IObjectCreation ObjectCreation(InvocationExpressionSyntax invocation, SemanticModel model)
    {
        if (VulnerableFileSystemAccessRule(invocation.DescendantNodes()) is { } accessRuleSyntaxNode)
        {
            return accessRuleSyntaxNode;
        }
        else if (invocation.GetArgumentSymbolsOfKnownType(KnownType.System_Security_AccessControl_FileSystemAccessRule, model).FirstOrDefault() is { } accessRuleSymbol
            && accessRuleSymbol is not IMethodSymbol)
        {
            return VulnerableFileSystemAccessRule(accessRuleSymbol.LocationNodes(invocation));
        }
        else
        {
            return null;
        }

        IObjectCreation VulnerableFileSystemAccessRule(IEnumerable<SyntaxNode> nodes) =>
            FilterObjectCreations(nodes).FirstOrDefault(x => IsFileSystemAccessRuleForEveryoneWithAllow(x, model));
    }

    private static bool IsFileSystemAccessRuleForEveryoneWithAllow(IObjectCreation objectCreation, SemanticModel model) =>
        objectCreation.IsKnownType(KnownType.System_Security_AccessControl_FileSystemAccessRule, model)
        && objectCreation.ArgumentList is { } argumentList
        && IsEveryone(argumentList.Arguments.First().Expression, model)
        && model.GetConstantValue(argumentList.Arguments.Last().Expression) is { HasValue: true, Value: 0 };

    private static bool IsEveryone(SyntaxNode node, SemanticModel model) =>
        model.GetConstantValue(node) is { HasValue: true, Value: Everyone }
        || FilterObjectCreations(node.DescendantNodesAndSelf()).Any(x => IsNTAccountWithEveryone(x, model) || IsSecurityIdentifierWithEveryone(x, model));

    private static bool IsNTAccountWithEveryone(IObjectCreation objectCreation, SemanticModel model) =>
        objectCreation.IsKnownType(KnownType.System_Security_Principal_NTAccount, model)
        && objectCreation.ArgumentList is { } argumentList
        && model.GetConstantValue(argumentList.Arguments.Last().Expression) is { HasValue: true, Value: Everyone };

    private static bool IsSecurityIdentifierWithEveryone(IObjectCreation objectCreation, SemanticModel model) =>
        objectCreation.IsKnownType(KnownType.System_Security_Principal_SecurityIdentifier, model)
        && objectCreation.ArgumentList is { } argumentList
        && model.GetConstantValue(argumentList.Arguments.First().Expression) is { HasValue: true, Value: 1 };

    private static IEnumerable<IObjectCreation> FilterObjectCreations(IEnumerable<SyntaxNode> nodes) =>
        nodes.Where(x => x.Kind() is SyntaxKind.ObjectCreationExpression or SyntaxKindEx.ImplicitObjectCreationExpression).Select(ObjectCreationFactory.Create);
}
