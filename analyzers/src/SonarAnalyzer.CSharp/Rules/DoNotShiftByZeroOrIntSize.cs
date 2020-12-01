/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotShiftByZeroOrIntSize : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2183";

        private const string MessageFormat_UseLargerTypeOrPromote
            = "Either promote shift target to a larger integer type or shift by {0} instead.";
        private const string MessageFormat_ShiftTooLarge = "Correct this shift; shift by {0} instead.";
        private const string MessageFormat_RightShiftTooLarge = "Correct this shift; '{0}' is larger than the type size.";
        private const string MessageFormat_UselessShift = "Remove this useless shift by {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, "{0}", RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static ImmutableDictionary<KnownType, int> mapKnownTypesToIntegerBitSize
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

        private enum Shift { Left, Right };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
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
                            c.ReportDiagnosticWhenActive(shift.Diagnostic);
                        }
                    }

                    zeroShiftIssues
                        .Where(sh => !ContainsShiftExpressionWithinTwoLines(linesWithShiftOperations, sh.Line))
                        .ToList()
                        .ForEach(sh => c.ReportDiagnosticWhenActive(sh.Diagnostic));
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration);
        }

        private static bool ContainsShiftExpressionWithinTwoLines(HashSet<int> linesWithShiftOperations,
            int lineNumber)
        {
            return linesWithShiftOperations.Contains(lineNumber - 2) ||
                   linesWithShiftOperations.Contains(lineNumber - 1) ||
                   linesWithShiftOperations.Contains(lineNumber)     ||
                   linesWithShiftOperations.Contains(lineNumber + 1) ||
                   linesWithShiftOperations.Contains(lineNumber + 2);
        }

        private static Tuple<Shift, ExpressionSyntax> GetRhsArgumentOfShiftNode(SyntaxNode node)
        {
            var binaryExpression = node as BinaryExpressionSyntax;
            if (binaryExpression?.OperatorToken.IsKind(SyntaxKind.LessThanLessThanToken) ?? false)
            {
                return new Tuple<Shift, ExpressionSyntax>(Shift.Left, binaryExpression.Right);
            }

            if (binaryExpression?.OperatorToken.IsKind(SyntaxKind.GreaterThanGreaterThanToken) ?? false)
            {
                return new Tuple<Shift, ExpressionSyntax>(Shift.Right, binaryExpression.Right);
            }

            var assignmentExpession = node as AssignmentExpressionSyntax;
            if (assignmentExpession?.OperatorToken.IsKind(SyntaxKind.LessThanLessThanEqualsToken) ?? false)
            {
                return new Tuple<Shift, ExpressionSyntax>(Shift.Left, assignmentExpession.Right);
            }

            if (assignmentExpession?.OperatorToken.IsKind(SyntaxKind.GreaterThanGreaterThanEqualsToken) ?? false)
            {
                return new Tuple<Shift, ExpressionSyntax>(Shift.Right, assignmentExpession.Right);
            }

            return null;
        }

        private static bool TryGetConstantValue(ExpressionSyntax expression, out int value)
        {
            value = 0;
            return expression?.RemoveParentheses() is LiteralExpressionSyntax literalExpression &&
                int.TryParse(literalExpression?.Token.ValueText, out value);
        }

        private static ShiftInstance FindShiftInstance(SyntaxNode node, SemanticModel semanticModel)
        {
            var tuple = GetRhsArgumentOfShiftNode(node);
            if (tuple == null)
            {
                return null;
            }

            if (!TryGetConstantValue(tuple.Item2, out var shiftByCount))
            {
                return new ShiftInstance(node);
            }

            var typeSymbol = semanticModel.GetTypeInfo(node).ConvertedType;
            if (typeSymbol == null)
            {
                return new ShiftInstance(node);
            }

            var variableBitLength = FindTypeSizeOrDefault(typeSymbol);
            if (variableBitLength == 0)
            {
                return new ShiftInstance(node);
            }

            var issueDescription = FindProblemDescription(variableBitLength, shiftByCount, tuple.Item1, out bool isLiteralZero);
            return issueDescription == null ? new ShiftInstance(node) : new ShiftInstance(issueDescription, isLiteralZero, node);
        }

        private static int FindTypeSizeOrDefault(ITypeSymbol typeSymbol)
        {
            return mapKnownTypesToIntegerBitSize
                .Where(kv => typeSymbol.Is(kv.Key))
                .Select(kv => kv.Value)
                .FirstOrDefault();
        }

        private static string FindProblemDescription(int typeSizeInBits, int shiftBy, Shift shiftDirection, out bool isLiteralZero)
        {
            if (shiftBy == 0)
            {
                isLiteralZero = true;
                return string.Format(MessageFormat_UselessShift, 0);
            }

            isLiteralZero = false;

            if (shiftBy < typeSizeInBits)
            {
                return null;
            }

            if (shiftDirection == Shift.Right)
            {
                return string.Format(MessageFormat_RightShiftTooLarge, shiftBy);
            }

            var shiftSuggestion = shiftBy % typeSizeInBits;

            if (typeSizeInBits == 64)
            {
                return shiftSuggestion == 0
                    ? string.Format(MessageFormat_UselessShift, shiftBy)
                    : string.Format(MessageFormat_ShiftTooLarge, shiftSuggestion);
            }

            if (shiftSuggestion == 0)
            {
                return string.Format(MessageFormat_UseLargerTypeOrPromote,
                    "less than " + typeSizeInBits);
            }

            return string.Format(MessageFormat_UseLargerTypeOrPromote, shiftSuggestion);
        }

        private class ShiftInstance
        {
            public Diagnostic Diagnostic { get; }
            public bool IsLiteralZero { get; }
            public int Line { get; }

            public ShiftInstance(SyntaxNode node)
            {
                Line = node.GetLineNumberToReport();
            }

            public ShiftInstance(string description, bool isLieralZero, SyntaxNode node)
                : this(node)
            {
                Diagnostic = Diagnostic.Create(rule, node.GetLocation(), description);
                IsLiteralZero = isLieralZero;
            }
        }
    }
}
