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

namespace SonarAnalyzer.Core.Rules;

public abstract class UnusedStringBuilderBase<TSyntaxKind, TVariableDeclarator, TIdentifierName> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TVariableDeclarator : SyntaxNode
    where TIdentifierName : SyntaxNode
{
    private const string DiagnosticId = "S3063";

    private readonly string[] stringBuilderAccessInvocations = ["ToString", "CopyTo", "GetChunks"];
    private readonly string[] stringBuilderAccessExpressions = ["Length"];
    protected abstract SyntaxNode Scope(TVariableDeclarator declarator);
    protected abstract ILocalSymbol RetrieveStringBuilderObject(SemanticModel model, TVariableDeclarator declaration);
    protected abstract SyntaxNode StringBuilderReadExpression(SemanticModel model, SyntaxNode node);
    protected abstract bool DescendIntoChildren(SyntaxNode node);

    protected override string MessageFormat => """Remove this "StringBuilder"; ".ToString()" is never called.""";

    protected UnusedStringBuilderBase() : base(DiagnosticId) { }

    internal bool IsAccessInvocation(string invocation) =>
        stringBuilderAccessInvocations.Any(x => x.Equals(invocation, Language.NameComparison));

    internal bool IsAccessExpression(string expression) =>
        stringBuilderAccessExpressions.Any(x => x.Equals(expression, Language.NameComparison));

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                var variableDeclarator = (TVariableDeclarator)c.Node;

                if (RetrieveStringBuilderObject(c.Model, variableDeclarator) is { } symbol
                    && Scope(variableDeclarator) is { } scope
                    && !IsStringBuilderAccessed(c.Model, symbol, scope))
                {
                    c.ReportIssue(Rule, variableDeclarator);
                }
            },
            Language.SyntaxKind.VariableDeclarator);

    private bool IsSameVariable(SemanticModel model, ILocalSymbol symbol, SyntaxNode identifier) =>
        Language.GetName(identifier).Equals(symbol.Name, Language.NameComparison) && symbol.Equals(model.GetSymbolInfo(identifier).Symbol);

    private static IEnumerable<TIdentifierName> LocalReferences(SyntaxNode node) =>
        node.DescendantNodesAndSelf().OfType<TIdentifierName>();

    private bool IsStringBuilderAccessed(SemanticModel model, ILocalSymbol symbol, SyntaxNode scope) =>
        scope.DescendantNodes(DescendIntoChildren).Any(x => StringBuilderReadExpression(model, x) is { } expression && LocalReferences(expression).Any(x => IsSameVariable(model, symbol, x)));
}
