/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InitializeStaticFieldsInline : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3963";
    private const string MessageFormat = "Initialize all 'static fields' inline and remove the 'static constructor'.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var constructor = (ConstructorDeclarationSyntax)c.Node;
                if (!constructor.Modifiers.Any(SyntaxKind.StaticKeyword)
                    || (constructor.Body is null && constructor.ExpressionBody() is null))
                {
                    return;
                }
                if (c.Model.GetDeclaredSymbol(constructor).ContainingType is { } currentType)
                {
                    var bodyDescendantNodes = constructor.Body?.DescendantNodes().ToArray()
                                              ?? constructor.ExpressionBody()?.DescendantNodes().ToArray()
                                              ?? [];

                    var assignedFieldCount = bodyDescendantNodes
                        .OfType<AssignmentExpressionSyntax>()
                        .SelectMany(x => FieldSymbolsFromLeftSide(x, c.Model, currentType))
                        .Select(x => x.Name)
                        .Distinct()
                        .Count();
                    var hasIfOrSwitch = Array.Exists(bodyDescendantNodes, x => x.Kind() is SyntaxKind.IfStatement or SyntaxKind.SwitchStatement);
                    if (((hasIfOrSwitch && assignedFieldCount == 1) || (!hasIfOrSwitch && assignedFieldCount > 0))
                        && !HasTupleAssignmentForMultipleFields(bodyDescendantNodes, c.Model, currentType))
                    {
                        c.ReportIssue(Rule, constructor.Identifier);
                    }
                }
            },
            SyntaxKind.ConstructorDeclaration);

    private static bool HasTupleAssignmentForMultipleFields(SyntaxNode[] nodes, SemanticModel model, INamedTypeSymbol currentType) =>
        nodes.OfType<AssignmentExpressionSyntax>()
            .Where(x => x.Left.Kind() is SyntaxKindEx.TupleExpression)
            .Select(x => FieldSymbolsFromLeftSide(x, model, currentType))
            .Any(x => x.Count() > 1); // if more than one field is assigned in a tuple then we assume that the static constructor is needed

    private static IEnumerable<ISymbol> FieldSymbolsFromLeftSide(AssignmentExpressionSyntax assignment, SemanticModel model, INamedTypeSymbol currentType) =>
        ExtractSymbols(assignment.Left, model)
            .OfType<IFieldSymbol>()
            .Distinct()
            .Where(x => x.ContainingType.Equals(currentType));

    private static ISymbol[] ExtractSymbols(SyntaxNode node, SemanticModel model) =>
        TupleExpressionSyntaxWrapper.IsInstance(node)
            ? ((TupleExpressionSyntaxWrapper)node).Arguments
                .SelectMany(x => ExtractSymbols(x.Expression, model))
                .ToArray()
            : [model.GetSymbolInfo(node).Symbol];
}
