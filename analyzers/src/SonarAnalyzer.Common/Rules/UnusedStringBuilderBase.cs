/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Rules;

public abstract class UnusedStringBuilderBase<TSyntaxKind, TVariableDeclarator, TInvocationExpression, TReturnStatement, TInterpolatedString> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TVariableDeclarator : SyntaxNode
    where TInvocationExpression : SyntaxNode
    where TReturnStatement : SyntaxNode
    where TInterpolatedString : SyntaxNode
{
    private const string DiagnosticId = "S3063";
    protected override string MessageFormat => """Remove this "StringBuilder"; ".ToString()" is never called.""";

    internal readonly string[] StringBuilderAccessMethods = { "ToString", "CopyTo", "GetChunks" };

    protected abstract ISymbol GetSymbol(TVariableDeclarator declaration, SemanticModel semanticModel);
    protected abstract bool NeedsToTrack(TVariableDeclarator declaration, SemanticModel semanticModel);
    protected abstract IList<TInvocationExpression> GetInvocations(TVariableDeclarator declaration);
    protected abstract IList<TReturnStatement> GetReturnStatements(TVariableDeclarator declaration);
    protected abstract IList<TInterpolatedString> GetInterpolatedStrings(TVariableDeclarator declaration);
    protected abstract bool IsStringBuilderContentRead(IList<TInvocationExpression> invocations, ISymbol variableSymbol, SemanticModel semanticModel);
    protected abstract bool IsPassedToMethod(IList<TInvocationExpression> invocations, ISymbol variableSymbol, SemanticModel semanticModel);
    protected abstract bool IsReturned(IList<TReturnStatement> returnStatements, ISymbol variableSymbol, SemanticModel semanticModel);
    protected abstract bool IsWithinInterpolatedString(IList<TInterpolatedString> interpolations, ISymbol variableSymbol, SemanticModel semanticModel);
    protected abstract bool IsPropertyReferenced(TVariableDeclarator declaration, IList<TInvocationExpression> invocations, ISymbol variableSymbol, SemanticModel semanticModel);

    protected UnusedStringBuilderBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var variableDeclaration = (TVariableDeclarator)c.Node;
            if (!NeedsToTrack(variableDeclaration, c.SemanticModel))
            {
                return;
            }
            var variableSymbol = GetSymbol(variableDeclaration, c.SemanticModel);
            var invocations = GetInvocations(variableDeclaration);
            if (IsStringBuilderContentRead(invocations, variableSymbol, c.SemanticModel)
                || IsPassedToMethod(invocations, variableSymbol, c.SemanticModel)
                || IsReturned(GetReturnStatements(variableDeclaration), variableSymbol, c.SemanticModel)
                || IsWithinInterpolatedString(GetInterpolatedStrings(variableDeclaration), variableSymbol, c.SemanticModel)
                || IsPropertyReferenced(variableDeclaration, invocations, variableSymbol, c.SemanticModel))
            {
                return;
            }
            c.ReportIssue(Diagnostic.Create(Rule, variableDeclaration.GetLocation()));
        }, Language.SyntaxKind.VariableDeclarator);
}
