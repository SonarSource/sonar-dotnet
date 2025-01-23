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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AzureFunctionsStateless : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6419";
        private const string MessageFormat = "Do not modify a static state from Azure Function.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => CheckTarget(c, ((AssignmentExpressionSyntax)c.Node).Left),
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression,
                SyntaxKind.DivideAssignmentExpression,
                SyntaxKind.ModuloAssignmentExpression,
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression,
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.LeftShiftAssignmentExpression,
                SyntaxKind.RightShiftAssignmentExpression,
                SyntaxKindEx.CoalesceAssignmentExpression,
                SyntaxKindEx.UnsignedRightShiftAssignmentExpression);

            context.RegisterNodeAction(
                c => CheckTarget(c, ((PrefixUnaryExpressionSyntax)c.Node).Operand),
                SyntaxKind.PreDecrementExpression,
                SyntaxKind.PreIncrementExpression);

            context.RegisterNodeAction(
                c => CheckTarget(c, ((PostfixUnaryExpressionSyntax)c.Node).Operand),
                SyntaxKind.PostDecrementExpression,
                SyntaxKind.PostIncrementExpression);

            context.RegisterNodeAction(c =>
                {
                    var argument = (ArgumentSyntax)c.Node;
                    if (argument.RefOrOutKeyword != default)
                    {
                        CheckTarget(c, argument.Expression);
                    }
                },
                SyntaxKind.Argument);
        }

        private static void CheckTarget(SonarSyntaxNodeReportingContext context, ExpressionSyntax target)
        {
            if (context.IsAzureFunction()
                && context.Model.GetSymbolInfo((target as ElementAccessExpressionSyntax)?.Expression ?? target).Symbol is { } symbol
                && symbol.IsStatic)
            {
                context.ReportIssue(Rule, target);
            }
        }
    }
}
