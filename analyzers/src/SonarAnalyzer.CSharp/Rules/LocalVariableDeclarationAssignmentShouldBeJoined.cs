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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LocalVariableDeclarationAssignmentShouldBeJoined : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S9414";
    private const string MessageFormat = "Join the declaration and the assignment of this local variable.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var localDeclaration = (LocalDeclarationStatementSyntax)c.Node;
                if (UninitializedSingleVariable(localDeclaration) is { } variable
                    && localDeclaration.FollowingStatement is ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment }
                    && c.Model.GetDeclaredSymbol(variable) is { } declaredSymbol
                    && IsJoinableAssignment(assignment, declaredSymbol, c.Model))
                {
                    c.ReportIssue(Rule, variable.Identifier);
                }
            },
            SyntaxKind.LocalDeclarationStatement);

    private static VariableDeclaratorSyntax UninitializedSingleVariable(LocalDeclarationStatementSyntax localDeclaration) =>
        localDeclaration.Declaration.Variables is { Count: 1 } variables
        && variables[0] is { Initializer: null } variable
        && !localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword)
        && !localDeclaration.UsingKeyword.IsKind(SyntaxKind.UsingKeyword)
        && !RefTypeSyntaxWrapper.IsInstance(localDeclaration.Declaration.Type)
        && localDeclaration.Declaration.Type is not PointerTypeSyntax
            ? variable
            : null;

    private static bool IsJoinableAssignment(AssignmentExpressionSyntax assignment, ISymbol declaredSymbol, SemanticModel model) =>
        assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
        && !ReferencesSymbol(assignment.Right, declaredSymbol, model)
        && assignment.AssignmentTargets.Select(x => x is IdentifierNameSyntax ? model.GetSymbolInfo(x).Symbol : null).ToArray() is var targetSymbols
        && targetSymbols.Any(declaredSymbol.Equals)
        // Mixing a declaration with existing variables in a deconstruction requires C# 10
        && (model.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp10)
            || targetSymbols.All(x => declaredSymbol.Equals(x) || x is { Kind: SymbolKindEx.Discard }));

    private static bool ReferencesSymbol(SyntaxNode node, ISymbol symbol, SemanticModel model) =>
        node.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
            .Any(x => x.Identifier.ValueText == symbol.Name && symbol.Equals(model.GetSymbolInfo(x).Symbol));
}
