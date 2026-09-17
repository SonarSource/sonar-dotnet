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

namespace SonarAnalyzer.Core.Rules;

public abstract class DoNotOverwriteCollectionElementsBase<TSyntaxKind, TStatementSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TStatementSyntax : SyntaxNode
{
    protected const string DiagnosticId = "S4143";

    /// <summary>
    /// Returns the index or key from the provided InvocationExpression or SimpleAssignmentExpression.
    /// Returns null if the provided SyntaxNode is not an InvocationExpression or SimpleAssignmentExpression.
    /// </summary>
    protected abstract SyntaxNode IndexOrKey(TStatementSyntax statement);

    /// <summary>
    /// Returns the identifier of a collection that is modified in the provided InvocationExpression
    /// or SimpleAssignmentExpression. Returns null if the provided SyntaxNode is not an
    /// InvocationExpression or SimpleAssignmentExpression.
    /// </summary>
    protected abstract SyntaxNode CollectionIdentifier(TStatementSyntax statement, bool includeCompoundAssignments);

    /// <summary>
    /// Returns a value specifying whether the provided SyntaxNode is an identifier or
    /// a literal (string, numeric, bool, etc.).
    /// </summary>
    protected abstract bool IsIdentifierOrLiteral(SyntaxNode node);

    protected override string MessageFormat => "Verify this is the index/key that was intended; a value has already been set for it.";

    private static string SecondaryMessage => "The index/key set here gets set again later.";

    protected DoNotOverwriteCollectionElementsBase() : base(DiagnosticId) { }

    protected void AnalysisAction(SonarSyntaxNodeReportingContext context)
    {
        var statement = (TStatementSyntax)context.Node;
        if (CollectionIdentifier(statement, false) is { } collectionIdentifier
            && IndexOrKey(statement) is { } indexOrKey
            && IsIdentifierOrLiteral(indexOrKey)
            && IsDictionaryOrCollection(collectionIdentifier, context.Model)
            && PreviousStatements(statement).TakeWhile(IsSameCollection(collectionIdentifier)).FirstOrDefault(IsSameIndexOrKey(indexOrKey)) is { } previousSet)
        {
            context.ReportIssue(Rule, context.Node, [previousSet.ToSecondaryLocation(SecondaryMessage)]);
        }
    }

    private Func<TStatementSyntax, bool> IsSameCollection(SyntaxNode collectionIdentifier) =>
        x => CollectionIdentifier(x, true) is { } identifier && identifier.ToString() == collectionIdentifier.ToString();

    private Func<TStatementSyntax, bool> IsSameIndexOrKey(SyntaxNode indexOrKey) =>
        x => IndexOrKey(x)?.ToString() == indexOrKey.ToString();

    private static bool IsDictionaryOrCollection(SyntaxNode identifier, SemanticModel model)
    {
        var identifierType = model.GetTypeInfo(identifier).Type;
        return identifierType.DerivesOrImplements(KnownType.System_Collections_Generic_IDictionary_TKey_TValue)
            || identifierType.DerivesOrImplements(KnownType.System_Collections_Generic_ICollection_T);
    }

    /// <summary>
    /// Returns all statements before the specified statement within the containing method.
    /// This method recursively traverses all parent blocks of the provided statement.
    /// </summary>
    private static IEnumerable<TStatementSyntax> PreviousStatements(TStatementSyntax statement)
    {
        var previousStatements = statement.Parent.ChildNodes().OfType<TStatementSyntax>().TakeWhile(x => x != statement).Reverse();
        return statement.Parent is TStatementSyntax parentStatement
            ? previousStatements.Union(PreviousStatements(parentStatement))
            : previousStatements;
    }
}
