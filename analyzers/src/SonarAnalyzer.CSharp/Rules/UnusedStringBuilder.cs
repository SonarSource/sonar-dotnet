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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnusedStringBuilder : UnusedStringBuilderBase<SyntaxKind, VariableDeclaratorSyntax, InvocationExpressionSyntax, ReturnStatementSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    private const string StringBuilderToString = "System.Text.StringBuilder.ToString()";

    protected override string GetVariableName(VariableDeclaratorSyntax declaration) =>
        declaration.GetIdentifier().ToString();

    protected override bool NeedsToTrack(VariableDeclaratorSyntax declaration, SemanticModel semanticModel) =>
        declaration.Initializer is not null
        && declaration.Initializer.Value is { } expression
        && expression switch
        {
            ObjectCreationExpressionSyntax => ObjectCreationFactory.Create(expression).IsKnownType(KnownType.System_Text_StringBuilder, semanticModel),
            _ when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(expression) => ObjectCreationFactory.Create(expression).IsKnownType(KnownType.System_Text_StringBuilder, semanticModel),
            _ => false
        };

    protected override SyntaxNode GetAncestorBlock(VariableDeclaratorSyntax declaration) =>
        declaration.Ancestors().OfType<BlockSyntax>().FirstOrDefault();

    protected override bool IsIsStringInvoked(string variableName, List<InvocationExpressionSyntax> invocations, SemanticModel semanticModel) =>
        invocations.Where(x => x.Expression is MemberAccessExpressionSyntax { } member
            && IsSameVariable(member.Expression, variableName)
            && member.IsMemberAccessOnKnownType(nameof(ToString), KnownType.System_Text_StringBuilder, semanticModel)
            && semanticModel.GetSymbolInfo(x).Symbol is IMethodSymbol symbol
            && symbol.OriginalDefinition.ToString().Equals(StringBuilderToString)).Any();

    protected override bool IsPassedToMethod(string variableName, List<InvocationExpressionSyntax> invocations) =>
        invocations.Where(x => x.ArgumentList.Arguments.Where(y => IsSameVariable(y.Expression, variableName)).Any()).Any();

    protected override bool IsReturned(string variableName, List<ReturnStatementSyntax> returnStatements) =>
        returnStatements.Where(x => IsSameVariable(x.Expression, variableName)).Any();

    private static bool IsSameVariable(ExpressionSyntax expression, string variableName) =>
        expression is IdentifierNameSyntax identifierName
        && identifierName.GetIdentifier().Value is SyntaxToken { } token
        && token.ToString().Equals(variableName);
}
