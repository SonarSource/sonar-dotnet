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

using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class UnusedStringBuilder : UnusedStringBuilderBase<SyntaxKind, VariableDeclaratorSyntax, InvocationExpressionSyntax, ReturnStatementSyntax, InterpolationSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override ILocalSymbol GetSymbol(VariableDeclaratorSyntax declaration, SemanticModel semanticModel) => (ILocalSymbol)semanticModel.GetDeclaredSymbol(declaration.Names.First());

    protected override bool NeedsToTrack(VariableDeclaratorSyntax declaration, SemanticModel semanticModel) =>
        declaration is
        {
            Parent: LocalDeclarationStatementSyntax,
            Initializer.Value: ObjectCreationExpressionSyntax { } objectCreation,
        }
        && objectCreation.Type.IsKnownType(KnownType.System_Text_StringBuilder, semanticModel);

    protected override bool IsStringBuilderRead(SemanticModel model, ILocalSymbol local, SyntaxNode node) =>
        throw new NotImplementedException();

    protected override IList<InvocationExpressionSyntax> GetInvocations(VariableDeclaratorSyntax declaration) =>
        declaration.Parent.Parent.Parent.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

    protected override IList<ReturnStatementSyntax> GetReturnStatements(VariableDeclaratorSyntax declaration) =>
        declaration.Parent.Parent.Parent.DescendantNodes().OfType<ReturnStatementSyntax>().ToList();

    protected override IList<InterpolationSyntax> GetInterpolatedStrings(VariableDeclaratorSyntax declaration) =>
        declaration.Parent.Parent.Parent.DescendantNodes().OfType<InterpolationSyntax>().ToList();

    protected override bool IsStringBuilderContentRead(IList<InvocationExpressionSyntax> invocations, ILocalSymbol variableSymbol, SemanticModel semanticModel) =>
        invocations.Any(x => IsSameVariable(x.Expression, variableSymbol, semanticModel) && StringBuilderAccessMethods.Contains(x.GetName()));

    protected override bool IsPassedToMethod(IList<InvocationExpressionSyntax> invocations, ILocalSymbol variableSymbol, SemanticModel semanticModel) =>
        invocations.Any(x => x.ArgumentList.Arguments.Any(y => IsSameVariable(y.GetExpression(), variableSymbol, semanticModel)));

    protected override bool IsReturned(IList<ReturnStatementSyntax> returnStatements, ILocalSymbol variableSymbol, SemanticModel semanticModel) =>
        returnStatements.Any(x => IsSameVariable(x.Expression, variableSymbol, semanticModel));

    protected override bool IsWithinInterpolatedString(IList<InterpolationSyntax> interpolations, ILocalSymbol variableSymbol, SemanticModel semanticModel) =>
        interpolations.Any(x => IsSameVariable(x.Expression, variableSymbol, semanticModel));

    protected override bool IsPropertyReferenced(VariableDeclaratorSyntax declaration, IList<InvocationExpressionSyntax> invocations, ILocalSymbol variableSymbol, SemanticModel semanticModel) =>
        invocations.Any(x => IsSameVariable(x.Expression, variableSymbol, semanticModel) && semanticModel.GetOperation(x).Kind is OperationKindEx.PropertyReference);

    private static bool IsSameVariable(ExpressionSyntax expression, ILocalSymbol variableSymbol, SemanticModel semanticModel)
    {
        var references = GetLocalReferences(expression, semanticModel);
        if (!references.Any() && expression.Ancestors().OfType<ConditionalAccessExpressionSyntax>().Any())
        {
            references = GetLocalReferences(expression.Ancestors().OfType<ConditionalAccessExpressionSyntax>().First(), semanticModel);
        }
        return references.Any(x => IsSameVariable(x, variableSymbol, semanticModel));
    }

    private static IEnumerable<IdentifierNameSyntax> GetLocalReferences(SyntaxNode node, SemanticModel semanticModel) =>
        node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Where(x => IsLocalReference(x, semanticModel));

    private static bool IsLocalReference(IdentifierNameSyntax identifier, SemanticModel semanticModel) =>
        semanticModel.GetOperation(identifier) is { Kind: OperationKindEx.LocalReference };

    private static bool IsSameVariable(IdentifierNameSyntax identifier, ILocalSymbol variableSymbol, SemanticModel semanticModel) =>
        variableSymbol.Equals(semanticModel.GetSymbolInfo(identifier).Symbol);
}
