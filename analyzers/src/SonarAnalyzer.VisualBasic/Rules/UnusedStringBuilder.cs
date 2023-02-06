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

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class UnusedStringBuilder : UnusedStringBuilderBase<SyntaxKind, VariableDeclaratorSyntax, InvocationExpressionSyntax, ReturnStatementSyntax, InterpolationSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override string GetVariableName(VariableDeclaratorSyntax declaration) => declaration.Names.FirstOrDefault().ToString();

    protected override bool NeedsToTrack(VariableDeclaratorSyntax expression, SemanticModel semanticModel) =>
        expression.Initializer is not null
        && expression.Initializer.Value is ObjectCreationExpressionSyntax objectCreation
        && objectCreation.Type.IsKnownType(KnownType.System_Text_StringBuilder, semanticModel);

    protected override IList<InvocationExpressionSyntax> GetInvocations(VariableDeclaratorSyntax declaration) =>
        declaration.Parent.Parent.Parent.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

    protected override IList<ReturnStatementSyntax> GetReturnStatements(VariableDeclaratorSyntax declaration) =>
        declaration.Parent.Parent.Parent.DescendantNodes().OfType<ReturnStatementSyntax>().ToList();

    protected override IList<InterpolationSyntax> GetInterpolatedStrings(VariableDeclaratorSyntax declaration) =>
        declaration.Parent.Parent.Parent.DescendantNodes().OfType<InterpolationSyntax>().ToList();

    protected override bool IsStringBuilderAccessed(string variableName, IList<InvocationExpressionSyntax> invocations) =>
        invocations.Any(x => IsSameVariable(x.Expression, variableName) && StringBuilderAccessMethods.Contains(x.GetName()));

    protected override bool IsPassedToMethod(string variableName, IList<InvocationExpressionSyntax> invocations) =>
        invocations.Any(x => x.ArgumentList.Arguments.Any(y => IsSameVariable(y.GetExpression(), variableName)));

    protected override bool IsReturned(string variableName, IList<ReturnStatementSyntax> returnStatements) =>
        returnStatements.Any(x => IsSameVariable(x.Expression, variableName));

    protected override bool IsWithinInterpolatedString(string variableName, IList<InterpolationSyntax> interpolations) =>
        interpolations.Any(x => IsSameVariable(x.Expression, variableName));

    private static bool IsSameVariable(ExpressionSyntax expression, string variableName) =>
        expression.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(p => p.NameIs(variableName))
        || (expression.Ancestors().OfType<ConditionalAccessExpressionSyntax>().Any() && expression.Ancestors().OfType<ConditionalAccessExpressionSyntax>().First()
            .DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(p => p.NameIs(variableName)));
}
