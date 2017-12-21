/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer;
using SonarAnalyzer.Helpers;
using System.Collections.Generic;

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
        private const string MessageFormat_UselessShift = "Remove this useless shift by {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, "{0}", RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static Dictionary<KnownType, int> mapKnownTypesToIntegerBitSize
            = new Dictionary<KnownType, int>
            {
                [KnownType.System_Int64] = 64,
                [KnownType.System_UInt64] = 64,

                [KnownType.System_Int32] = 32,
                [KnownType.System_UInt32] = 32,

                [KnownType.System_Int16] = 16,
                [KnownType.System_UInt16] = 16,

                [KnownType.System_Byte] = 8,
                [KnownType.System_SByte] = 8
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var right = (c.Node as BinaryExpressionSyntax)?.Right
                                ?? (c.Node as AssignmentExpressionSyntax)?.Right;

                    if (!TryGetConstantValue(right, out var shiftByCount))
                    {
                        return;
                    }

                    var typeInfo = c.SemanticModel.GetTypeInfo(c.Node);
                    ReportProblems(typeInfo.ConvertedType, shiftByCount, c.Node.GetLocation(), c);
                },
                SyntaxKind.LeftShiftExpression,
                SyntaxKind.LeftShiftAssignmentExpression);
        }

        private static bool TryGetConstantValue(ExpressionSyntax expression, out int value)
        {
            value = 0;
            var literalExpression = expression?.RemoveParentheses() as LiteralExpressionSyntax;
            return literalExpression != null &&
                int.TryParse(literalExpression.Token.ValueText, out value);
        }

        private static void ReportProblems(ITypeSymbol typeSymbol, int shiftByCount,
            Location location, SyntaxNodeAnalysisContext context)
        {
            if (typeSymbol == null)
            {
                return;
            }

            var variableBitLength = FindTypeSizeOrDefault(typeSymbol);
            if (variableBitLength == 0)
            {
                return;
            }

            var issueDescription = FindProblemDescription(variableBitLength, shiftByCount);
            if (issueDescription != null)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, issueDescription));
            }
        }

        private static int FindTypeSizeOrDefault(ITypeSymbol typeSymbol)
        {
            return mapKnownTypesToIntegerBitSize
                .Where(kv => typeSymbol.Is(kv.Key))
                .Select(kv => kv.Value)
                .FirstOrDefault();
        }

        private static string FindProblemDescription(int typeSizeInBits, int shiftBy)
        {
            if (shiftBy == 0)
            {
                return string.Format(MessageFormat_UselessShift, 0);
            }

            if (shiftBy < typeSizeInBits)
            {
                return null;
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
    }
}
