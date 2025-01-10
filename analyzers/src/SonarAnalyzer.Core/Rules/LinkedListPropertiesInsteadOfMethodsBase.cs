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

namespace SonarAnalyzer.Rules;

public abstract class LinkedListPropertiesInsteadOfMethodsBase<TSyntaxKind, TInvocationExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocationExpression : SyntaxNode
{
    internal const string DiagnosticId = "S6613";

    protected override string MessageFormat => "'{0}' property of 'LinkedList' should be used instead of the '{0}()' extension method.";

    protected abstract bool IsRelevantCallAndType(TInvocationExpression invocation, SemanticModel model);

    protected LinkedListPropertiesInsteadOfMethodsBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var invocation = (TInvocationExpression)c.Node;
            var methodName = Language.GetName(invocation);

            if (IsFirstOrLast(methodName) && IsRelevantCallAndType(invocation, c.SemanticModel))
            {
                c.ReportIssue(Rule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation(), methodName);
            }
        }, Language.SyntaxKind.InvocationExpression);

    protected static bool IsRelevantType(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);

    private bool IsFirstOrLast(string methodName) =>
        methodName.Equals("First", Language.NameComparison)
        || methodName.Equals("Last", Language.NameComparison);
}
