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

public abstract class DoNotOverwriteCollectionElementsBase<TSyntaxKind, TStatementSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TStatementSyntax : SyntaxNode
{
    protected const string DiagnosticId = "S4143";

    /// <summary>
    /// Returns the index or key from the provided InvocationExpression or SimpleAssignmentExpression.
    /// Returns null if the provided SyntaxNode is not an InvocationExpression or SimpleAssignmentExpression.
    /// </summary>
    protected abstract SyntaxNode GetIndexOrKey(TStatementSyntax statement);

    /// <summary>
    /// Returns the identifier of a collection that is modified in the provided InvocationExpression
    /// or SimpleAssignmentExpression. Returns null if the provided SyntaxNode is not an
    /// InvocationExpression or SimpleAssignmentExpression.
    /// </summary>
    protected abstract SyntaxNode GetCollectionIdentifier(TStatementSyntax statement);

    /// <summary>
    /// Returns a value specifying whether the provided SyntaxNode is an identifier or
    /// a literal (string, numeric, bool, etc.).
    /// </summary>
    protected abstract bool IsIdentifierOrLiteral(SyntaxNode node);

    protected override string MessageFormat => "Verify this is the index/key that was intended; " +
        "a value has already been set for it.";

    private static string SecondaryMessage => "The index/key set here gets set again later.";

    protected DoNotOverwriteCollectionElementsBase() : base(DiagnosticId) { }

    protected void AnalysisAction(SonarSyntaxNodeReportingContext context)
    {
        var statement = (TStatementSyntax)context.Node;
        var collectionIdentifier = GetCollectionIdentifier(statement);
        var indexOrKey = GetIndexOrKey(statement);

        if (collectionIdentifier is null
            || indexOrKey is null
            || !IsIdentifierOrLiteral(indexOrKey)
            || !IsDictionaryOrCollection(collectionIdentifier, context.Model))
        {
            return;
        }

        var previousSet = GetPreviousStatements(statement)
            .TakeWhile(IsSameCollection(collectionIdentifier))
            .FirstOrDefault(IsSameIndexOrKey(indexOrKey));

        if (previousSet is not null)
        {
            context.ReportIssue(Rule, context.Node, [previousSet.ToSecondaryLocation(SecondaryMessage)]);
        }
    }

    private Func<TStatementSyntax, bool> IsSameCollection(SyntaxNode collectionIdentifier) =>
        x =>
            GetCollectionIdentifier(x) is { } identifier &&
            identifier.ToString() == collectionIdentifier.ToString();

    private Func<TStatementSyntax, bool> IsSameIndexOrKey(SyntaxNode indexOrKey) =>
        x => GetIndexOrKey(x)?.ToString() == indexOrKey.ToString();

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
    private static IEnumerable<TStatementSyntax> GetPreviousStatements(TStatementSyntax statement)
    {
        var previousStatements = statement.Parent.ChildNodes()
            .OfType<TStatementSyntax>()
            .TakeWhile(x => x != statement)
            .Reverse();

        return statement.Parent is TStatementSyntax parentStatement
            ? previousStatements.Union(GetPreviousStatements(parentStatement))
            : previousStatements;
    }
}
