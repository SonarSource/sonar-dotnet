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

namespace SonarAnalyzer.Core.Rules;

public abstract class ShiftDynamicNotIntegerBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind> where TSyntaxKind : struct
{
    private const string DiagnosticId = "S3449";

    protected abstract bool CanBeConvertedTo(SyntaxNode expression, ITypeSymbol type, SemanticModel model);

    protected abstract bool ShouldRaise(SemanticModel model, SyntaxNode left, SyntaxNode right);

    protected override string MessageFormat => "Remove this erroneous shift, it will fail because '{0}' can't be implicitly converted to 'int'.";

    protected ShiftDynamicNotIntegerBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c => CheckExpressionWithTwoParts(c, b => Language.Syntax.BinaryExpressionLeft(b), b => Language.Syntax.BinaryExpressionRight(b)),
            Language.SyntaxKind.LeftShiftExpression,
            Language.SyntaxKind.RightShiftExpression);

        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c => CheckExpressionWithTwoParts(c, b => Language.Syntax.AssignmentLeft(b), b => Language.Syntax.AssignmentRight(b)),
            Language.SyntaxKind.LeftShiftAssignmentStatement,
            Language.SyntaxKind.RightShiftAssignmentStatement);
    }

    protected bool IsConvertibleToInt(SyntaxNode expression, SemanticModel model) =>
        model.Compilation.GetTypeByMetadataName(KnownType.System_Int32) is { } intType
        && CanBeConvertedTo(expression, intType, model);

    private void CheckExpressionWithTwoParts(SonarSyntaxNodeReportingContext context, Func<SyntaxNode, SyntaxNode> getLeft, Func<SyntaxNode, SyntaxNode> getRight)
    {
        var expression = context.Node;
        var left = getLeft(expression);
        var right = getRight(expression);

        if (!IsErrorType(right, context.Model, out var typeOfRight)
            && ShouldRaise(context.Model, left, right))
        {
            var typeInMessage = TypeNameForMessage(right, typeOfRight, context.Model);
            context.ReportIssue(Rule, right, typeInMessage);
        }
    }

    private static string TypeNameForMessage(SyntaxNode expression, ITypeSymbol typeOfRight, SemanticModel model) =>
        model.GetConstantValue(expression) is { HasValue: true, Value: null }
            ? "null"
            : typeOfRight.ToMinimalDisplayString(model, expression.SpanStart);

    private static bool IsErrorType(SyntaxNode expression, SemanticModel model, out ITypeSymbol type)
    {
        type = model.GetTypeInfo(expression).Type;
        return type.Is(TypeKind.Error);
    }
}
