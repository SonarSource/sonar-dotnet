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

using Roslyn.Utilities;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoopsAndLinq : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3267";
    private const string MessageFormat = "{0}";
    private const string LinqMethodMessageFormat = @"Loops should be simplified using the ""{0}"" LINQ method";
    private const string SelectMessageFormat = "Loop should be simplified by calling Select({0} => {0}.{1})";
    private const string WhereMethod = "Where";
    private const string AnyMethod = "Any";
    private const string AllMethod = "All";
    private const string FirstOrDefaultMethod = "FirstOrDefault";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var forEachStatementSyntax = (ForEachStatementSyntax)c.Node;
                if (!IsOrImplementsIEnumerable(c.Model, forEachStatementSyntax)
                    || IsOrImplementsIQueryable(c.Model, forEachStatementSyntax)
                    || IsInPerformanceSensitiveContext(forEachStatementSyntax, c.Model))
                {
                    return;
                }

                if (SimplifiableIf(forEachStatementSyntax.Statement, c) is { } ifStatement)
                {
                    c.ReportIssue(
                        Rule,
                        forEachStatementSyntax.Expression,
                        [ifStatement.Condition.ToSecondaryLocation()],
                        string.Format(LinqMethodMessageFormat, SuggestedMethod(forEachStatementSyntax, ifStatement, c)));
                }
                else
                {
                    CheckIfCanBeSimplifiedUsingSelect(c, forEachStatementSyntax);
                }
            },
            SyntaxKind.ForEachStatement);

    private static bool IsInPerformanceSensitiveContext(ForEachStatementSyntax node, SemanticModel model) =>
        node.PerformanceSensitiveAttribute(model) is { } attribute
        // If AllowGenericEnumeration property is configured to true, in the context of the rule, we are not in a performance sensitive context
        && (!attribute.TryGetAttributeValue<bool>(nameof(PerformanceSensitiveAttribute.AllowGenericEnumeration), out var allow) || allow);

    private static IfStatementSyntax SimplifiableIf(SyntaxNode statement, SonarSyntaxNodeReportingContext context) =>
        IfStatement(statement) is { } ifStatementSyntax
        && CanIfStatementBeMoved(ifStatementSyntax)
        && !ContainsRefStructReference(ifStatementSyntax, context.Model)
        && !HasNullableConversion(ifStatementSyntax, context)
            ? ifStatementSyntax
            : null;

    // A single return, or an assignment followed by a break, is not expressible in LINQ when it is a nullable conversion.
    // also see https://sonarsource.atlassian.net/browse/NET-1222
    private static bool HasNullableConversion(IfStatementSyntax ifStatementSyntax, SonarSyntaxNodeReportingContext context) =>
        SingleReturnOrBreakingAssignment(ifStatementSyntax) is { } returnOrAssignment
        && RequiresNullableConversion(returnOrAssignment, context);

    private static string SuggestedMethod(ForEachStatementSyntax forEachStatementSyntax, IfStatementSyntax ifStatementSyntax, SonarSyntaxNodeReportingContext context) =>
        !IsOrImplementsIAsyncEnumerable(context.Model, forEachStatementSyntax)
        // A down-casting foreach would additionally need a Cast<T>/OfType<T>, so no specific method fits.
        && context.Model.GetForEachStatementInfo(forEachStatementSyntax).ElementConversion.IsIdentity
            ? SingleReturnOrBreakingAssignment(ifStatementSyntax) switch
            {
                ReturnStatementSyntax returnStatement => SuggestedMethodForReturn(forEachStatementSyntax, returnStatement, context),
                // The whole assignment is checked: "Any" returns a bool and cannot supply the matched element, so neither
                // the assigned value nor the assignment target may depend on it.
                AssignmentExpressionSyntax assignment when !ReferencesLoopVariable(assignment, forEachStatementSyntax) => AnyMethod,
                _ => WhereMethod
            }
            : WhereMethod;

    /// <remarks>
    /// "FirstOrDefault" needs the element type and the returned type to share the same default value.
    /// <see cref="RequiresNullableConversion"/> already excluded the nullable conversions, see
    /// https://sonarsource.atlassian.net/browse/NET-1222. Boxing and user-defined conversions are excluded here
    /// because they can change default(T) where the loop returns null or default.
    /// </remarks>
    private static string SuggestedMethodForReturn(ForEachStatementSyntax forEachStatementSyntax, ReturnStatementSyntax returnStatement, SonarSyntaxNodeReportingContext context) =>
        returnStatement.Expression?.WithoutEnclosingParentheses switch
        {
            null => AnyMethod,
            { } returned when !ReferencesLoopVariable(returned, forEachStatementSyntax) =>
                returned.IsFalse() && ReturnsTrue(forEachStatementSyntax.FollowingStatement) ? AllMethod : AnyMethod,
            IdentifierNameSyntax identifier when identifier.Identifier.ValueText == forEachStatementSyntax.Identifier.ValueText
                && !RequiresBoxingOrUserDefinedConversion(identifier, context)
                && ReturnsNullOrDefault(forEachStatementSyntax.FollowingStatement) => FirstOrDefaultMethod,
            _ => WhereMethod
        };

    private static bool RequiresBoxingOrUserDefinedConversion(ExpressionSyntax expression, SonarSyntaxNodeReportingContext context) =>
        context.Model.GetTypeInfo(expression, context.Cancel) is { Type: { } type, ConvertedType: { } convertedType }
        && context.Compilation.ClassifyConversion(type, convertedType) is { IsBoxing: true } or { IsUserDefined: true };

    private static bool ReturnsTrue(StatementSyntax statement) =>
        statement is ReturnStatementSyntax { Expression: { } expression } && expression.IsTrue();

    private static bool ReturnsNullOrDefault(StatementSyntax statement) =>
        statement is ReturnStatementSyntax { Expression: { } expression }
        && (expression.IsDefaultLiteral
            || expression.WithoutEnclosingParentheses.IsNullLiteral()
            || expression.WithoutEnclosingParentheses.Kind() is SyntaxKind.DefaultExpression);

    private static bool ReferencesLoopVariable(SyntaxNode node, ForEachStatementSyntax forEachStatementSyntax) =>
        node.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
            .Any(x => x.Identifier.ValueText == forEachStatementSyntax.Identifier.ValueText);

    private static bool ContainsRefStructReference(IfStatementSyntax ifStatement, SemanticModel model) =>
        ifStatement.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Any(x => model.GetTypeInfo(x).Type is { IsRefStruct: true });

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
        if (ifStatementSyntax.Statement is { FirstNonBlockStatement: ReturnStatementSyntax returnStatement })
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

    private static IfStatementSyntax IfStatement(SyntaxNode node) =>
        node switch
        {
            IfStatementSyntax ifStatementSyntax => ifStatementSyntax,
            BlockSyntax blockSyntax when blockSyntax.ChildNodes().Count() == 1 => IfStatement(blockSyntax.ChildNodes().Single()),
            _ => null
        };

    private static bool CanIfStatementBeMoved(IfStatementSyntax ifStatementSyntax) =>
        ifStatementSyntax.Else is null && IsValidCondition(ifStatementSyntax.Condition);

    private static bool IsValidCondition(ExpressionSyntax condition) =>
        condition switch
        {
            _ when condition.Kind() is SyntaxKind.IsExpression or SyntaxKindEx.IsPatternExpression => IsValidIsPattern(condition),
            InvocationExpressionSyntax invocation => IsValidInvocation(invocation),
            BinaryExpressionSyntax binary => IsValidBinaryExpression(binary),
            PrefixUnaryExpressionSyntax unary => IsValidCondition(unary.Operand),
            IdentifierNameSyntax => true,
            LiteralExpressionSyntax => true,
            _ => false
        };

    private static bool IsValidBinaryExpression(BinaryExpressionSyntax expression) =>
        IsValidCondition(expression.Left) && IsValidCondition(expression.Right);

    private static bool IsValidInvocation(InvocationExpressionSyntax invocationExpressionSyntax) =>
        !invocationExpressionSyntax.DescendantNodes()
            .OfType<ArgumentSyntax>()
            .Any(x => x.RefOrOutKeyword.Kind() is SyntaxKind.OutKeyword or SyntaxKind.RefKeyword);

    private static bool IsValidIsPattern(SyntaxNode isPattern) =>
        !isPattern.DescendantNodes()
            .Any(x => x.Kind() is SyntaxKindEx.VarPattern
                                    or SyntaxKindEx.SingleVariableDesignation
                                    or SyntaxKindEx.ParenthesizedVariableDesignation);

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
                && c.Model.GetSymbolInfo(identifierSyntax).Symbol is { } identifierSymbol
                && identifierSymbol.Equals(declaredSymbol.Value)
                && c.Model.GetSymbolInfo(memberAccessExpressionSyntax.Name).Symbol is { SymbolType.IsRefStruct: false } symbol)
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
        IsOrImplements(model, forEachStatementSyntax, KnownType.System_Collections_Generic_IEnumerable_T)
        || IsOrImplementsIAsyncEnumerable(model, forEachStatementSyntax);

    // On IAsyncEnumerable the terminal operators are named "AnyAsync", "AllAsync" and "FirstOrDefaultAsync", so those
    // suggestions would be wrong. "Where" is deferred and keeps its name, which makes it the correct fallback.
    private static bool IsOrImplementsIAsyncEnumerable(SemanticModel model, ForEachStatementSyntax forEachStatementSyntax) =>
        IsOrImplements(model, forEachStatementSyntax, KnownType.System_Collections_Generic_IAsyncEnumerable_T);

    // For IQueryable the "Where"/"Select" rewrite is translated by the query provider (e.g. EF Core -> SQL) and executed
    // server-side, unlike the in-memory foreach+if. The rewrite is therefore not equivalent and can throw at runtime for a
    // predicate the provider cannot translate, so the rule should not raise on IQueryable sources.
    private static bool IsOrImplementsIQueryable(SemanticModel model, ForEachStatementSyntax forEachStatementSyntax) =>
        IsOrImplements(model, forEachStatementSyntax, KnownType.System_Linq_IQueryable);

    private static bool IsOrImplements(SemanticModel model, ForEachStatementSyntax forEachStatementSyntax, KnownType type) =>
        model.GetTypeInfo(forEachStatementSyntax.Expression).Type is { } expressionType
        && (expressionType.Is(type) || expressionType.Implements(type));

    private sealed class UsageStats
    {
        public int Count { get; set; }

        public bool IsInVarDeclarator { get; set; }
    }
}
