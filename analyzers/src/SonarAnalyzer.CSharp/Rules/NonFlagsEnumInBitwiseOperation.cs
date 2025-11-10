/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NonFlagsEnumInBitwiseOperation : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3265";

        private const string MessageFormat = "{0}";
        private const string MessageRemove = "Remove this bitwise operation; the enum '{0}' is not marked with 'Flags' attribute.";
        private const string MessageChangeOrRemove = "Mark enum '{0}' with 'Flags' attribute or remove this bitwise operation.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => CheckExpressionWithOperator<BinaryExpressionSyntax>(c, b => b.OperatorToken),
                SyntaxKind.BitwiseOrExpression,
                SyntaxKind.BitwiseAndExpression,
                SyntaxKind.ExclusiveOrExpression);

            context.RegisterNodeAction(
                c => CheckExpressionWithOperator<AssignmentExpressionSyntax>(c, a => a.OperatorToken),
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression);
        }

        private static void CheckExpressionWithOperator<T>(SonarSyntaxNodeReportingContext context, Func<T, SyntaxToken> operatorSelector)
            where T : SyntaxNode
        {
            if (context.Model.GetSymbolInfo(context.Node).Symbol is not IMethodSymbol { MethodKind: MethodKind.BuiltinOperator, ReturnType.TypeKind: TypeKind.Enum } operation
                || operation.ReturnType.HasAttribute(KnownType.System_FlagsAttribute))
            {
                return;
            }

            var friendlyTypeName = operation.ReturnType.ToMinimalDisplayString(context.Model, context.Node.SpanStart);
            var messageFormat = operation.ReturnType.DeclaringSyntaxReferences.Any()
                ? MessageChangeOrRemove
                : MessageRemove;

            var message = string.Format(messageFormat, friendlyTypeName);

            var op = operatorSelector((T)context.Node);
            context.ReportIssue(Rule, op, message);
        }
    }
}
