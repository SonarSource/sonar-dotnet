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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LooseFilePermissions : LooseFilePermissionsBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    public LooseFilePermissions() : this(AnalyzerConfiguration.Hotspot) { }

    internal LooseFilePermissions(IAnalyzerConfiguration configuration) : base(configuration) { }

    protected override void VisitAssignments(SonarSyntaxNodeReportingContext context)
    {
        var node = context.Node;
        if (IsFileAccessPermissions(node, context.Model) && !node.IsPartOfBinaryNegationOrCondition())
        {
            context.ReportIssue(Rule, node);
        }
    }

    protected override void VisitInvocations(SonarSyntaxNodeReportingContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if ((IsSetAccessRule(invocation, context.Model) || IsAddAccessRule(invocation, context.Model))
            && (GetObjectCreation(invocation, context.Model) is { } objectCreation))
        {
            var invocationLocation = invocation.GetLocation();
            var secondaryLocation = objectCreation.Expression.GetLocation();
            context.ReportIssue(Rule, invocationLocation, invocationLocation.StartLine() == secondaryLocation.StartLine() ? [] : [secondaryLocation.ToSecondary(MessageFormat)]);
        }
    }

    private static bool IsSetAccessRule(InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation.IsMemberAccessOnKnownType("SetAccessRule", KnownType.System_Security_AccessControl_FileSystemSecurity, model);

    private static bool IsAddAccessRule(InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation.IsMemberAccessOnKnownType("AddAccessRule", KnownType.System_Security_AccessControl_FileSystemSecurity, model);

    private static IObjectCreation GetObjectCreation(InvocationExpressionSyntax invocation, SemanticModel model)
    {
        if (GetVulnerableFileSystemAccessRule(invocation.DescendantNodes()) is { } accessRuleSyntaxNode)
        {
            return accessRuleSyntaxNode;
        }
        else if (invocation.GetArgumentSymbolsOfKnownType(KnownType.System_Security_AccessControl_FileSystemAccessRule, model).FirstOrDefault() is { } accessRuleSymbol
            && accessRuleSymbol is not IMethodSymbol)
        {
            return GetVulnerableFileSystemAccessRule(accessRuleSymbol.GetLocationNodes(invocation));
        }
        else
        {
            return null;
        }

        IObjectCreation GetVulnerableFileSystemAccessRule(IEnumerable<SyntaxNode> nodes) =>
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
        nodes.Where(x => x.Kind() is SyntaxKind.ObjectCreationExpression or SyntaxKindEx.ImplicitObjectCreationExpression)
             .Select(ObjectCreationFactory.Create);
}
