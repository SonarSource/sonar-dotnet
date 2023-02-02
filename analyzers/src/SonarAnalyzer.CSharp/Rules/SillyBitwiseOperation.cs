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

using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SillyBitwiseOperation : SillyBitwiseOperationBase
    {
        protected override ILanguageFacade Language => CSharpFacade.Instance;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => CheckBinary(c, -1),
                SyntaxKind.BitwiseAndExpression);

            context.RegisterNodeAction(
                c => CheckBinary(c, 0),
                SyntaxKind.BitwiseOrExpression,
                SyntaxKind.ExclusiveOrExpression);

            context.RegisterNodeAction(
                c => CheckAssignment(c, -1),
                SyntaxKind.AndAssignmentExpression);

            context.RegisterNodeAction(
                c => CheckAssignment(c, 0),
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression);

            context.RegisterNodeAction(
                CheckComparison,
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression);
        }

        private void CheckAssignment(SonarSyntaxNodeReportingContext context, int constValueToLookFor)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            if (FindIntConstant(context.SemanticModel, assignment.Right) is { } constValue
                && constValue == constValueToLookFor)
            {
                var location = assignment.Parent is StatementSyntax
                    ? assignment.Parent.GetLocation()
                    : assignment.OperatorToken.CreateLocation(assignment.Right);
                context.ReportIssue(Diagnostic.Create(BitwiseRule, location));
            }
        }

        private void CheckBinary(SonarSyntaxNodeReportingContext context, int constValueToLookFor)
        {
            var binary = (BinaryExpressionSyntax)context.Node;
            CheckBinary(context, binary.Left, binary.OperatorToken, binary.Right, constValueToLookFor);
        }

        private void CheckComparison(SonarSyntaxNodeReportingContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;

            var normalComparer = GetNormalComparer(binary.Kind());
            var inverseComparer = GetReverseComparer(binary.Kind());

            if (ShouldRaise(context.SemanticModel, binary.Left, binary.Right, normalComparer)) // x >= constant
            {
                context.ReportIssue(Diagnostic.Create(ComparisonRule, binary.GetLocation()));
            }
            else if (ShouldRaise(context.SemanticModel, binary.Right, binary.Left, inverseComparer))  // constant >= x inversed: x <= constant
            {
                context.ReportIssue(Diagnostic.Create(ComparisonRule, binary.GetLocation()));
            }
        }

        private Func<TypeMetadata, double, bool> GetNormalComparer(SyntaxKind kind) =>
            kind switch
            {
                SyntaxKind.LessThanExpression => (tm, constant) => tm.Less(constant),
                SyntaxKind.LessThanOrEqualExpression => (tm, constant) => tm.LessOrEqual(constant),
                SyntaxKind.GreaterThanExpression => (tm, constant) => tm.Greater(constant),
                SyntaxKind.GreaterThanOrEqualExpression => (tm, constant) => tm.GreaterOrEqual(constant),
            };

        private Func<TypeMetadata, double, bool> GetReverseComparer(SyntaxKind kind) =>
            kind switch
            {
                SyntaxKind.LessThanExpression => (tm, constant) => tm.Greater(constant),
                SyntaxKind.LessThanOrEqualExpression => (tm, constant) => tm.GreaterOrEqual(constant),
                SyntaxKind.GreaterThanExpression => (tm, constant) => tm.Less(constant),
                SyntaxKind.GreaterThanOrEqualExpression => (tm, constant) => tm.LessOrEqual(constant),
            };

        private bool ShouldRaise(SemanticModel semanticModel, SyntaxNode value, SyntaxNode expectedConstant, Func<TypeMetadata, double, bool> checkConstant) =>
            FindConstant(semanticModel, expectedConstant, Convert.ToDouble) is { } constant
            && semanticModel.GetSymbolInfo(value).Symbol.GetSymbolType() is { } symbol
            && Ranges.FirstOrDefault(x => symbol.Is(x.Type)) is { } typeMetadata
            && checkConstant(typeMetadata, constant);

        private static readonly TypeMetadata[] Ranges = new TypeMetadata[] // FIXME: Add more types here
        {
            new LimitedFunctionalityTypeMetadata(KnownType.System_Byte, byte.MinValue, byte.MaxValue),
            new LimitedFunctionalityTypeMetadata(KnownType.System_Int32, int.MinValue, int.MaxValue),
            new LimitedFunctionalityTypeMetadata(KnownType.System_Int64, long.MinValue, long.MaxValue),
            new FullFunctionalityTypeMetadata(KnownType.System_Single, long.MinValue, long.MaxValue),
        };

        private abstract class TypeMetadata
        {
            public KnownType Type { get; init; }
            public double MinValue { get; init; }
            public double MaxValue { get; init; }

            protected TypeMetadata(KnownType type, double minValue, double maxValue)
            {
                Type = type;
                MinValue = minValue;
                MaxValue = maxValue;
            }

            public virtual bool Less(double constant) => false;
            public virtual bool Greater(double constant) => false;

            public abstract bool LessOrEqual(double constant);
            public abstract bool GreaterOrEqual(double constant);
        }

        private class LimitedFunctionalityTypeMetadata : TypeMetadata // For types that are already semi-covered by CS0652
        {
            public LimitedFunctionalityTypeMetadata(KnownType type, double minValue, double maxValue)
                : base(type, minValue, maxValue)
            { }

            public override bool GreaterOrEqual(double constant) => // something >= constant
                constant == MinValue;

            public override bool LessOrEqual(double constant) => // something <= constant
                constant == MaxValue;
        }

        private class FullFunctionalityTypeMetadata : TypeMetadata // For types that are not covered by CS0652
        {
            public FullFunctionalityTypeMetadata(KnownType type, double minValue, double maxValue)
                : base(type, minValue, maxValue)
            { }

            public override bool Greater(double constant) => // something > constant
                constant < MinValue;

            public override bool Less(double constant) => // something < constant
                constant > MaxValue;

            public override bool GreaterOrEqual(double constant) => // something >= constant
                constant <= MinValue;

            public override bool LessOrEqual(double constant) => // something <= constant
                constant >= MaxValue;
        }
    }
}
