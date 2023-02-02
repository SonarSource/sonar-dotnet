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

using System.Numerics;
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
            if (ShouldRaise(context.SemanticModel, binary.Left, binary.Right, binary.Kind(), false))
            {
                context.ReportIssue(Diagnostic.Create(BitwiseRule, binary.GetLocation()));
            }

            if (ShouldRaise(context.SemanticModel, binary.Right, binary.Left, binary.Kind(), true))
            {
                context.ReportIssue(Diagnostic.Create(BitwiseRule, binary.GetLocation()));
            }
        }

        private bool ShouldRaise(SemanticModel semanticModel, SyntaxNode value, SyntaxNode expectedConstant, SyntaxKind kind, bool constantIsLeft)
        {
            if (FindConstant(semanticModel, expectedConstant, Convert.ToDouble) is { } constant
                && semanticModel.GetSymbolInfo(value).Symbol.GetSymbolType() is { } symbol
                && Ranges.FirstOrDefault(x => symbol.Is(x.Type)) is { } range)
            {
                // Implement out-of-range checks for the types that CS0652 does not.
                if (symbol.IsAny(FullyImplementedTypes) && (constant < range.MinValue || constant > range.MaxValue))
                {
                    return true;
                }

                // Implement threshold checks for every type.
                return GetThreshold(kind, constantIsLeft, range) is { } threshold && constant == threshold;
            }

            return false;
        }

        private static double? GetThreshold(SyntaxKind operation, bool constantIsLeft, TypeRange range)
        {
            if (constantIsLeft)
            {
                return operation switch
                {
                    SyntaxKind.LessThanExpression => range.MaxValue, // T.MaxValue < x
                    SyntaxKind.LessThanOrEqualExpression => range.MinValue, // T.MinValue <= x
                    SyntaxKind.GreaterThanExpression => range.MinValue, // T.MinValue > x
                    SyntaxKind.GreaterThanOrEqualExpression => range.MaxValue, //  T.MaxValue >= x
                    _ => null
                };
            }
            else
            {

                return operation switch
                {
                    SyntaxKind.LessThanExpression => range.MinValue, // x < T.MinValue
                    SyntaxKind.LessThanOrEqualExpression => range.MaxValue, // x <= T.MaxValue
                    SyntaxKind.GreaterThanExpression => range.MaxValue, // x > T.MaxValue
                    SyntaxKind.GreaterThanOrEqualExpression => range.MinValue, // x >= T.MinValue
                    _ => null
                };
            }
        }

        private static readonly KnownType[] FullyImplementedTypes = new[]
        {
            KnownType.System_Half,
            KnownType.System_Single,
        };

        private static readonly TypeRange[] Ranges = new[]
        {
            new TypeRange(KnownType.System_SByte, sbyte.MinValue, sbyte.MaxValue),
            new TypeRange(KnownType.System_Byte, byte.MinValue, byte.MaxValue),
            new TypeRange(KnownType.System_Int16, short.MinValue, short.MaxValue),
            new TypeRange(KnownType.System_UInt16, ushort.MinValue, ushort.MaxValue),
            new TypeRange(KnownType.System_Int32, int.MinValue, int.MaxValue),
            new TypeRange(KnownType.System_UInt32, uint.MinValue, uint.MaxValue),
            new TypeRange(KnownType.System_Int64, long.MinValue, long.MaxValue),
            new TypeRange(KnownType.System_UInt64, ulong.MinValue, ulong.MaxValue),
            new TypeRange(KnownType.System_Single, float.MinValue, float.MaxValue),
            //new TypeRange(KnownType.System_Half, half.MinValue, half.MaxValue),
        };

        private class TypeRange
        {
            public KnownType Type { get; init; }
            public double MinValue { get; init; }
            public double MaxValue { get; init; }

            public TypeRange(KnownType type, double minValue, double maxValue)
            {
                Type = type;
                MinValue = minValue;
                MaxValue = maxValue;
            }
        }
    }
}
