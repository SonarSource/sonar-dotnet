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

    protected override ISymbol GetSymbol(VariableDeclaratorSyntax declaration, SemanticModel semanticModel) => semanticModel.GetDeclaredSymbol(declaration.Names.First());

    protected override bool NeedsToTrack(VariableDeclaratorSyntax declaration, SemanticModel semanticModel) =>
        declaration.Initializer is not null
        && declaration.Parent is LocalDeclarationStatementSyntax
        && declaration.Initializer.Value is ObjectCreationExpressionSyntax objectCreation
        && objectCreation.Type.IsKnownType(KnownType.System_Text_StringBuilder, semanticModel);

    protected override IList<InvocationExpressionSyntax> GetInvocations(VariableDeclaratorSyntax declaration) =>
        declaration.Parent.Parent.Parent.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

    protected override IList<ReturnStatementSyntax> GetReturnStatements(VariableDeclaratorSyntax declaration) =>
        declaration.Parent.Parent.Parent.DescendantNodes().OfType<ReturnStatementSyntax>().ToList();

    protected override IList<InterpolationSyntax> GetInterpolatedStrings(VariableDeclaratorSyntax declaration) =>
        declaration.Parent.Parent.Parent.DescendantNodes().OfType<InterpolationSyntax>().ToList();

    protected override bool IsStringBuilderContentRead(IList<InvocationExpressionSyntax> invocations, ISymbol variableSymbol, SemanticModel semanticModel) =>
        invocations.Any(x => IsSameVariable(x.Expression, variableSymbol, semanticModel) && StringBuilderAccessMethods.Contains(x.GetName()));

    protected override bool IsPassedToMethod(IList<InvocationExpressionSyntax> invocations, ISymbol variableSymbol, SemanticModel semanticModel) =>
        invocations.Any(x => x.ArgumentList.Arguments.Any(y => IsSameVariable(y.GetExpression(), variableSymbol, semanticModel)));

    protected override bool IsReturned(IList<ReturnStatementSyntax> returnStatements, ISymbol variableSymbol, SemanticModel semanticModel) =>
        returnStatements.Any(x => IsSameVariable(x.Expression, variableSymbol, semanticModel));

    protected override bool IsWithinInterpolatedString(IList<InterpolationSyntax> interpolations, ISymbol variableSymbol, SemanticModel semanticModel) =>
        interpolations.Any(x => IsSameVariable(x.Expression, variableSymbol, semanticModel));

    private static bool IsSameVariable(ExpressionSyntax expression, ISymbol variableSymbol, SemanticModel semanticModel) =>
        expression.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(p => IsSameVariable(p, variableSymbol, semanticModel))
        || (expression.Ancestors().OfType<ConditionalAccessExpressionSyntax>().Any() && expression.Ancestors().OfType<ConditionalAccessExpressionSyntax>().First()
            .DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(p => IsSameVariable(p, variableSymbol, semanticModel)));

    private static bool IsSameVariable(IdentifierNameSyntax identifier, ISymbol variableSymbol, SemanticModel semanticModel) =>
        variableSymbol.Equals(semanticModel.GetSymbolInfo(identifier).Symbol);
}
