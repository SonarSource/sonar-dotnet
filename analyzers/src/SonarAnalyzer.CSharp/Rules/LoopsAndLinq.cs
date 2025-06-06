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
public sealed class LoopsAndLinq : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3267";
    private const string MessageFormat = "{0}";
    private const string WhereMessageFormat = @"Loops should be simplified using the ""Where"" LINQ method";
    private const string SelectMessageFormat = "Loop should be simplified by calling Select({0} => {0}.{1}))";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var forEachStatementSyntax = (ForEachStatementSyntax)c.Node;
                if (!IsOrImplementsIEnumerable(c.Model, forEachStatementSyntax))
                {
                    return;
                }

                if (CanBeSimplifiedUsingWhere(forEachStatementSyntax.Statement, c, out var ifConditionLocation))
                {
                    c.ReportIssue(Rule, forEachStatementSyntax.Expression, [ifConditionLocation], WhereMessageFormat);
                }
                else
                {
                    CheckIfCanBeSimplifiedUsingSelect(c, forEachStatementSyntax);
                }
            },
            SyntaxKind.ForEachStatement);

    private static bool CanBeSimplifiedUsingWhere(SyntaxNode statement, SonarSyntaxNodeReportingContext context, out SecondaryLocation ifConditionLocation)
    {
        if (GetIfStatement(statement) is { } ifStatementSyntax && CanIfStatementBeMoved(ifStatementSyntax))
        {
            ifConditionLocation = ifStatementSyntax.Condition.ToSecondaryLocation();
            // If the 'if' block contains a single return or assignment with a break,
            // we cannot simplify the loop using LINQ if the return or assignment is a nullable conversion.
            // also see https://sonarsource.atlassian.net/browse/NET-1222
            return SingleReturnOrBreakingAssignment(ifStatementSyntax) is not { } returnOrAssignment
                || !RequiresNullableConversion(returnOrAssignment, context);
        }

        ifConditionLocation = null;
        return false;
    }

    private static bool RequiresNullableConversion(SyntaxNode returnOrAssignment, SonarSyntaxNodeReportingContext context)
    {
        var expression = returnOrAssignment switch
        {
            ReturnStatementSyntax returnStatement => returnStatement.Expression,
            AssignmentExpressionSyntax assignment => assignment.Right,
            _ => throw new InvalidOperationException("Unreachable")
        };

        // expression can be null if the return statement is empty
        return expression is not null
            && context.Model.GetTypeInfo(expression) is { Type: { } type, ConvertedType: { } convertedType }
            && context.Compilation.ClassifyConversion(type, convertedType).IsNullable;
    }

    private static SyntaxNode SingleReturnOrBreakingAssignment(IfStatementSyntax ifStatementSyntax)
    {
        // Check if the first statement of the block is a return
        if (ifStatementSyntax.Statement.FirstNonBlockStatement() is ReturnStatementSyntax returnStatement)
        {
            return returnStatement;
        }

        // Check if the statement is a block with a single assignment followed by a break
        if (ifStatementSyntax.Statement is BlockSyntax { Statements: { Count: 2 } statements }
            && statements[0] is ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment }
            && statements[1] is BreakStatementSyntax)
        {
            return assignment;
        }
        return null;
    }

    private static IfStatementSyntax GetIfStatement(SyntaxNode node) =>
        node switch
        {
            IfStatementSyntax ifStatementSyntax => ifStatementSyntax,
            BlockSyntax blockSyntax when blockSyntax.ChildNodes().Count() == 1 => GetIfStatement(blockSyntax.ChildNodes().Single()),
            _ => null
        };

    private static bool CanIfStatementBeMoved(IfStatementSyntax ifStatementSyntax)
    {
        return ifStatementSyntax.Else is null && (ConditionValidIsPattern() || ConditionValidInvocation());

        bool ConditionValidIsPattern() => ifStatementSyntax.Condition?.Kind() is SyntaxKind.IsExpression or SyntaxKindEx.IsPatternExpression
            && !ifStatementSyntax.Condition.DescendantNodes()
                .Any(x => x.Kind() is SyntaxKindEx.VarPattern or
                    SyntaxKindEx.SingleVariableDesignation or
                    SyntaxKindEx.ParenthesizedVariableDesignation);

        bool ConditionValidInvocation() => ifStatementSyntax.Condition is InvocationExpressionSyntax invocationExpressionSyntax
            && !invocationExpressionSyntax.DescendantNodes()
                .OfType<ArgumentSyntax>()
                .Any(x => x.RefOrOutKeyword.Kind() is SyntaxKind.OutKeyword or SyntaxKind.RefKeyword);
    }

    /// <remarks>
    /// There are multiple scenarios where the code can be simplified using LINQ.
    /// For simplicity, we consider that Select() can be used
    /// only when a single property from the foreach variable is used.
    /// We skip checking method invocations since depending on the method being called, moving it can make the code harder to read.
    /// The issue is raised if:
    ///  - the property is used more than once
    ///  - the property is the right side of a variable declaration.
    /// </remarks>
    private static void CheckIfCanBeSimplifiedUsingSelect(SonarSyntaxNodeReportingContext c, ForEachStatementSyntax forEachStatementSyntax)
    {
        var declaredSymbol = new Lazy<ILocalSymbol>(() => c.Model.GetDeclaredSymbol(forEachStatementSyntax));

        var accessedProperties = new Dictionary<ISymbol, UsageStats>();

        foreach (var identifierSyntax in GetStatementIdentifiers(forEachStatementSyntax))
        {
            if (identifierSyntax.Parent is MemberAccessExpressionSyntax { Parent: not InvocationExpressionSyntax } memberAccessExpressionSyntax
                && IsNotLeftSideOfAssignment(memberAccessExpressionSyntax)
                && c.Model.GetSymbolInfo(identifierSyntax).Symbol.Equals(declaredSymbol.Value)
                && c.Model.GetSymbolInfo(memberAccessExpressionSyntax.Name).Symbol is { } symbol
                && !symbol.GetSymbolType().IsRefStruct())
            {
                var usageStats = accessedProperties.GetOrAdd(symbol, _ => new UsageStats());

                usageStats.IsInVarDeclarator = memberAccessExpressionSyntax.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax };
                usageStats.Count++;
            }
            else
            {
                return;
            }
        }

        if (accessedProperties.Count == 1
            && accessedProperties.First().Value is var stats
            && (stats.IsInVarDeclarator || stats.Count > 1))
        {
            c.ReportIssue(Rule, forEachStatementSyntax.Expression, string.Format(SelectMessageFormat, forEachStatementSyntax.Identifier.ValueText, accessedProperties.Single().Key.Name));
        }

        static IEnumerable<IdentifierNameSyntax> GetStatementIdentifiers(ForEachStatementSyntax forEachStatementSyntax) =>
            forEachStatementSyntax.Statement
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(x => x.Identifier.ValueText == forEachStatementSyntax.Identifier.ValueText);

        static bool IsNotLeftSideOfAssignment(MemberAccessExpressionSyntax memberAccess) =>
            !(memberAccess.Parent is AssignmentExpressionSyntax assignment && assignment.Left == memberAccess);
    }

    private static bool IsOrImplementsIEnumerable(SemanticModel model, ForEachStatementSyntax forEachStatementSyntax) =>
        model.GetTypeInfo(forEachStatementSyntax.Expression).Type is var expressionType
        && (expressionType.Is(KnownType.System_Collections_Generic_IEnumerable_T)
            || expressionType.Implements(KnownType.System_Collections_Generic_IEnumerable_T)
            || expressionType.Is(KnownType.System_Collections_Generic_IAsyncEnumerable_T)
            || expressionType.Implements(KnownType.System_Collections_Generic_IAsyncEnumerable_T));

    private sealed class UsageStats
    {
        public int Count { get; set; }

        public bool IsInVarDeclarator { get; set; }
    }
}
