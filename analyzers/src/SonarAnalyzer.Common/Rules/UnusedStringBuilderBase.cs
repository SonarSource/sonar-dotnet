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

public abstract class UnusedStringBuilderBase<TSyntaxKind, TVariableDeclarator, TInvocationExpression, TReturnStatement> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TVariableDeclarator : SyntaxNode
    where TInvocationExpression : SyntaxNode
    where TReturnStatement : SyntaxNode
{
    private const string DiagnosticId = "S3063";
    protected override string MessageFormat => """Remove this "StringBuilder"; ".ToString()" is never called.""";

    protected abstract string GetVariableName(TVariableDeclarator declaration);
    protected abstract bool NeedsToTrack(TVariableDeclarator declaration, SemanticModel semanticModel);
    protected abstract SyntaxNode GetAncestorBlock(TVariableDeclarator declaration);
    protected abstract bool IsIsStringInvoked(string variableName, IList<TInvocationExpression> invocations);
    protected abstract bool IsPassedToMethod(string variableName, IList<TInvocationExpression> invocations);
    protected abstract bool IsReturned(string variableName, IList<TReturnStatement> returnStatements);

    protected UnusedStringBuilderBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var variableDeclaration = (TVariableDeclarator)c.Node;
            if (!NeedsToTrack(variableDeclaration, c.SemanticModel))
            {
                return;
            }
            var variableName = GetVariableName(variableDeclaration);
            var block = GetAncestorBlock(variableDeclaration);
            if (block == null || string.IsNullOrEmpty(variableName))
            {
                return;
            }
            var invocations = block.DescendantNodes().OfType<TInvocationExpression>().ToList();
            if (IsIsStringInvoked(variableName, invocations)
                || IsPassedToMethod(variableName, invocations)
                || IsReturned(variableName, block.DescendantNodes().OfType<TReturnStatement>().ToList()))
            {
                return;
            }
            c.ReportIssue(Diagnostic.Create(Rule, variableDeclaration.GetLocation()));
        }, Language.SyntaxKind.VariableDeclarator);
}
