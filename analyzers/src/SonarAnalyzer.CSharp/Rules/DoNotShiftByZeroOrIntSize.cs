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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotShiftByZeroOrIntSize : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2183";
        private const string MessageFormatUseLargerTypeOrPromote = "Either promote shift target to a larger integer type or shift by {0} instead.";
        private const string MessageFormatShiftTooLarge = "Correct this shift; shift by {0} instead.";
        private const string MessageFormatRightShiftTooLarge = "Correct this shift; '{0}' is larger than the type size.";
        private const string MessageFormatUselessShift = "Remove this useless shift by {0}.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, "{0}");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableDictionary<KnownType, int> MapKnownTypesToIntegerBitSize
            = new Dictionary<KnownType, int>
            {
                [KnownType.System_Int64] = 64,
                [KnownType.System_UInt64] = 64,

                [KnownType.System_Int32] = 32,
                [KnownType.System_UInt32] = 32,

                [KnownType.System_Int16] = 32,
                [KnownType.System_UInt16] = 32,

                [KnownType.System_Byte] = 32,
                [KnownType.System_SByte] = 32
            }.ToImmutableDictionary();

        private enum Shift
        {
            Left,
            Right
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var shiftInstances = ((MemberDeclarationSyntax)c.Node)
                        .DescendantNodes()
                        .Select(n => FindShiftInstance(n, c.SemanticModel))
                        .WhereNotNull();

                    var zeroShiftIssues = new List<ShiftInstance>();
                    var linesWithShiftOperations = new HashSet<int>();

                    foreach (var shift in shiftInstances)
                    {
                        if (shift.IsLiteralZero)
                        {
                            zeroShiftIssues.Add(shift);
                            continue;
                        }

                        linesWithShiftOperations.Add(shift.Line);
                        if (shift.Diagnostic != null)
                        {
                            c.ReportIssue(shift.Diagnostic);
                        }
                    }

                    zeroShiftIssues
                        .Where(sh => !ContainsShiftExpressionWithinTwoLines(linesWithShiftOperations, sh.Line))
                        .ToList()
                        .ForEach(sh => c.ReportIssue(sh.Diagnostic));
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration);

        private static bool ContainsShiftExpressionWithinTwoLines(HashSet<int> linesWithShiftOperations, int lineNumber) =>
            linesWithShiftOperations.Contains(lineNumber - 2)
            || linesWithShiftOperations.Contains(lineNumber - 1)
            || linesWithShiftOperations.Contains(lineNumber)
            || linesWithShiftOperations.Contains(lineNumber + 1)
            || linesWithShiftOperations.Contains(lineNumber + 2);

        private static Tuple<Shift, ExpressionSyntax> GetRhsArgumentOfShiftNode(SyntaxNode node)
        {
            var binaryExpression = node as BinaryExpressionSyntax;
            if (binaryExpression?.OperatorToken.IsKind(SyntaxKind.LessThanLessThanToken) ?? false)
            {
                return new Tuple<Shift, ExpressionSyntax>(Shift.Left, binaryExpression.Right);
            }

            if (binaryExpression?.OperatorToken.IsAnyKind(SyntaxKind.GreaterThanGreaterThanToken, SyntaxKindEx.GreaterThanGreaterThanGreaterThanToken) ?? false)
            {
                return new Tuple<Shift, ExpressionSyntax>(Shift.Right, binaryExpression.Right);
            }

            var assignmentExpession = node as AssignmentExpressionSyntax;
            if (assignmentExpession?.OperatorToken.IsKind(SyntaxKind.LessThanLessThanEqualsToken) ?? false)
            {
                return new Tuple<Shift, ExpressionSyntax>(Shift.Left, assignmentExpession.Right);
            }

            if (assignmentExpession?.OperatorToken.IsAnyKind(SyntaxKind.GreaterThanGreaterThanEqualsToken, SyntaxKindEx.GreaterThanGreaterThanGreaterThanEqualsToken) ?? false)
            {
                return new Tuple<Shift, ExpressionSyntax>(Shift.Right, assignmentExpession.Right);
            }

            return null;
        }

        private static bool TryGetConstantValue(ExpressionSyntax expression, out int value)
        {
            value = 0;
            return expression.RemoveParentheses() is LiteralExpressionSyntax literalExpression
                && int.TryParse(literalExpression.Token.ValueText, out value);
        }

        private static ShiftInstance FindShiftInstance(SyntaxNode node, SemanticModel semanticModel)
        {
            var tuple = GetRhsArgumentOfShiftNode(node);
            if (tuple == null)
            {
                return null;
            }

            if (!TryGetConstantValue(tuple.Item2, out var shiftByCount)
                || semanticModel.GetTypeInfo(node).ConvertedType is not { } typeSymbol
                || (FindTypeSizeOrDefault(typeSymbol) is var variableBitLength && variableBitLength == 0))
            {
                return new ShiftInstance(node);
            }

            var issueDescription = FindProblemDescription(variableBitLength, shiftByCount, tuple.Item1, out var isLiteralZero);
            return issueDescription == null ? new ShiftInstance(node) : new ShiftInstance(issueDescription, isLiteralZero, node);
        }

        private static int FindTypeSizeOrDefault(ITypeSymbol typeSymbol) =>
            MapKnownTypesToIntegerBitSize
                .Where(kv => typeSymbol.Is(kv.Key))
                .Select(kv => kv.Value)
                .FirstOrDefault();

        private static string FindProblemDescription(int typeSizeInBits, int shiftBy, Shift shiftDirection, out bool isLiteralZero)
        {
            if (shiftBy == 0)
            {
                isLiteralZero = true;
                return string.Format(MessageFormatUselessShift, 0);
            }

            isLiteralZero = false;

            if (shiftBy < typeSizeInBits)
            {
                return null;
            }

            if (shiftDirection == Shift.Right)
            {
                return string.Format(MessageFormatRightShiftTooLarge, shiftBy);
            }

            var shiftSuggestion = shiftBy % typeSizeInBits;

            if (typeSizeInBits == 64)
            {
                return shiftSuggestion == 0
                    ? string.Format(MessageFormatUselessShift, shiftBy)
                    : string.Format(MessageFormatShiftTooLarge, shiftSuggestion);
            }

            if (shiftSuggestion == 0)
            {
                return string.Format(MessageFormatUseLargerTypeOrPromote,
                    "less than " + typeSizeInBits);
            }

            return string.Format(MessageFormatUseLargerTypeOrPromote, shiftSuggestion);
        }

        private sealed class ShiftInstance
        {
            public Diagnostic Diagnostic { get; }
            public bool IsLiteralZero { get; }
            public int Line { get; }

            public ShiftInstance(SyntaxNode node) =>
                Line = node.GetLineNumberToReport();

            public ShiftInstance(string description, bool isLieralZero, SyntaxNode node)
                : this(node)
            {
                Diagnostic = CreateDiagnostic(Rule, node.GetLocation(), description);
                IsLiteralZero = isLieralZero;
            }
        }
    }
}
