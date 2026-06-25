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

namespace SonarAnalyzer.Core.Trackers;

public abstract class ConstantValueFinder<TIdentifierNameSyntax, TVariableDeclaratorSyntax>
    where TIdentifierNameSyntax : SyntaxNode
    where TVariableDeclaratorSyntax : SyntaxNode
{
    protected readonly SemanticModel model;

    private readonly AssignmentFinder assignmentFinder;
    private readonly int nullLiteralExpressionSyntaxKind;
    private readonly bool strict;

    protected abstract string IdentifierName(TIdentifierNameSyntax node);
    protected abstract SyntaxNode InitializerValue(TVariableDeclaratorSyntax node);
    protected abstract TVariableDeclaratorSyntax VariableDeclarator(SyntaxNode node);
    protected abstract bool IsPtrZero(SyntaxNode node);

    /// <param name="strict">If true, result derived from field initializers and parameter default values will be omitted. Use it when you need certainty about the value.</param>
    protected ConstantValueFinder(SemanticModel model, AssignmentFinder assignmentFinder, int nullLiteralExpressionSyntaxKind, bool strict)
    {
        this.model = model;
        this.assignmentFinder = assignmentFinder;
        this.nullLiteralExpressionSyntaxKind = nullLiteralExpressionSyntaxKind;
        this.strict = strict;
    }

    public object FindConstant(SyntaxNode node) =>
        FindConstant(node, null);

    private object FindConstant(SyntaxNode node, HashSet<SyntaxNode> visitedVariables)
    {
        if (node is null || node.RawKind == nullLiteralExpressionSyntaxKind) // Performance shortcut
        {
            return null;
        }

        if (IsPtrZero(node))
        {
            return 0;
        }

        return node.EnsureCorrectSemanticModelOrDefault(model) is { } nodeModel
            ? nodeModel.GetConstantValue(node).Value ?? FindAssignedConstant(node, nodeModel, visitedVariables)
            : null;
    }

    private object FindAssignedConstant(SyntaxNode node, SemanticModel model, HashSet<SyntaxNode> visitedVariables)
    {
        if (node is TIdentifierNameSyntax identifier)
        {
            return assignmentFinder.FindLinearPrecedingAssignment(IdentifierName(identifier), node) switch
            {
                PrecedingAssignment.Found found => FindConstant(found.Assignment, visitedVariables),
                PrecedingAssignment.None when !strict => FindDefaultValue(model, visitedVariables, identifier),
                _ => null, // None in strict mode, or Uncertain
            };
        }
        else
        {
            return null;
        }
    }

    private object FindDefaultValue(SemanticModel model, HashSet<SyntaxNode> visitedVariables, TIdentifierNameSyntax identifier)
    {
        var symbol = model.GetSymbolInfo(identifier).Symbol;
        if (symbol is IFieldSymbol fieldSymbol
            && VariableDeclarator(fieldSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()) is { } variable
            && (visitedVariables is null || !visitedVariables.Contains(variable)))
        {
            visitedVariables ??= [];
            visitedVariables.Add(variable);
            return FindConstant(InitializerValue(variable), visitedVariables);
        }
        else if (symbol is IParameterSymbol parameter)
        {
            return ParameterDefaultValue(parameter);
        }
        return null;
    }

    private static object ParameterDefaultValue(IParameterSymbol parameter)
    {
        // For a partial method the defining declaration's default is authoritative; a default on the implementing declaration is ignored by the compiler (CS1066).
        var authoritative = parameter.ContainingSymbol is IMethodSymbol { PartialDefinitionPart: { } definition }
            ? definition.Parameters[parameter.Ordinal]
            : parameter;
        return authoritative.HasExplicitDefaultValue ? authoritative.ExplicitDefaultValue : null;
    }
}
