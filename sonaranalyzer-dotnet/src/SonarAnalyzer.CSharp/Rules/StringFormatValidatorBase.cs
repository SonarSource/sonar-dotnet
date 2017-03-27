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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class StringFormatValidatorBase : SonarDiagnosticAnalyzer
    {
        protected const string MessageFormat = "{0}";

        private static readonly ISet<MethodSignature> HandledFormatMethods = new HashSet<MethodSignature>
        {
            new MethodSignature(KnownType.System_String, "Format"),
            new MethodSignature(KnownType.System_Console, "Write"),
            new MethodSignature(KnownType.System_Console, "WriteLine"),
            new MethodSignature(KnownType.System_Text_StringBuilder, "AppendFormat"),
            new MethodSignature(KnownType.System_IO_TextWriter, "Write"),
            new MethodSignature(KnownType.System_IO_TextWriter, "WriteLine"),
            new MethodSignature(KnownType.System_Diagnostics_Debug, "WriteLine"),
            new MethodSignature(KnownType.System_Diagnostics_Trace, "TraceError"),
            new MethodSignature(KnownType.System_Diagnostics_Trace, "TraceInformation"),
            new MethodSignature(KnownType.System_Diagnostics_Trace, "TraceWarning"),
            new MethodSignature(KnownType.System_Diagnostics_TraceSource, "TraceInformation")
        };

        protected abstract ISet<ValidationFailure.FailureKind> FailuresToReportOn { get; }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                    if (methodSymbol == null || !methodSymbol.Parameters.Any())
                    {
                        return;
                    }

                    var currentMethod = HandledFormatMethods
                        .Where(hfm => methodSymbol.ContainingType.Is(hfm.ContainingType))
                        .FirstOrDefault(method => method.Name == methodSymbol.Name);
                    if (currentMethod == null)
                    {
                        return;
                    }

                    var formatArgumentIndex = methodSymbol.Parameters[0].IsType(KnownType.System_IFormatProvider)
                        ? 1 : 0;
                    var formatStringExpression = invocation.ArgumentList.Arguments[formatArgumentIndex];

                    var constValue = c.SemanticModel.GetConstantValue(formatStringExpression.Expression);
                    if (!constValue.HasValue)
                    {
                        // can't check non-constant format strings
                        return;
                    }

                    var failure = CheckForIssues((string)constValue.Value, invocation.ArgumentList,
                        formatArgumentIndex, c.SemanticModel);
                    if (ShouldNotReport(failure, currentMethod))
                    {
                        return;
                    }

                    c.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Expression.GetLocation(),
                            ProcessErrorMessage(failure)));
                }, SyntaxKind.InvocationExpression);
        }

        private bool ShouldNotReport(ValidationFailure failure, MethodSignature currentMethod)
        {
            return failure == null ||
                !FailuresToReportOn.Contains(failure.Kind) ||
                (failure.Kind == ValidationFailure.FailureKind.SimpleString &&
                !currentMethod.Name.Contains("Format"));
        }

        private static ValidationFailure CheckForIssues(string formatStringText, ArgumentListSyntax argumentList,
            int formatArgumentIndex, SemanticModel semanticModel)
        {
            if (formatStringText == null)
            {
                return new ValidationFailure(ValidationFailure.FailureKind.NullFormatString);
            }

            List<FormatStringItem> formatStringItems;
            return ExtractFormatItems(formatStringText, out formatStringItems) ??
                TryValidateFormatString(formatStringText, formatStringItems, argumentList, formatArgumentIndex,
                    semanticModel);
        }

        private static ValidationFailure ExtractFormatItems(string formatString,
            out List<FormatStringItem> formatStringItems)
        {
            formatStringItems = new List<FormatStringItem>();
            var curlyBraceCount = 0;
            StringBuilder currentFormatItemBuilder = null;
            var isEscapingOpenCurlyBrace = false;
            var isEscapingCloseCurlyBrace = false;
            for (int i = 0; i < formatString.Length; i++)
            {
                var currentChar = formatString[i];
                var previousChar = i > 0 ? formatString[i - 1] : '\0';

                if (currentChar == '{')
                {
                    if (previousChar == '{' && !isEscapingOpenCurlyBrace)
                    {
                        curlyBraceCount--;
                        isEscapingOpenCurlyBrace = true;
                        currentFormatItemBuilder = null;
                        continue;
                    }

                    curlyBraceCount++;
                    isEscapingOpenCurlyBrace = false;
                    if (currentFormatItemBuilder == null)
                    {
                        currentFormatItemBuilder = new StringBuilder();
                    }
                    continue;
                }

                if (previousChar == '{' && !char.IsDigit(currentChar) && currentFormatItemBuilder != null)
                {
                    return new ValidationFailure(ValidationFailure.FailureKind.InvalidCharacterAfterOpenCurlyBrace);
                }

                if (currentChar == '}')
                {
                    isEscapingCloseCurlyBrace = previousChar == '}' && !isEscapingCloseCurlyBrace;
                    curlyBraceCount = isEscapingCloseCurlyBrace
                        ? curlyBraceCount + 1
                        : curlyBraceCount - 1;

                    if (currentFormatItemBuilder != null)
                    {
                        FormatStringItem formatStringItem;
                        var failure = TryParseItem(currentFormatItemBuilder.ToString(), out formatStringItem);
                        if (failure != null)
                        {
                            return failure;
                        }

                        formatStringItems.Add(formatStringItem);
                        currentFormatItemBuilder = null;
                    }
                    continue;
                }

                currentFormatItemBuilder?.Append(currentChar);
            }

            if (curlyBraceCount != 0)
            {
                return new ValidationFailure(ValidationFailure.FailureKind.UnbalancedCurlyBraceCount);
            }

            return null;
        }

        private static ValidationFailure TryParseItem(string formatItem, out FormatStringItem formatStringItem)
        {
            formatStringItem = null;
            var indexOfComma = formatItem.IndexOf(',');
            var indexOfColon = formatItem.IndexOf(':');
            var split = formatItem.Split(',', ':');

            if (indexOfComma >= 0 && indexOfColon >= 0 && indexOfColon < indexOfComma ||
                split.Length > 3)
            {
                return new ValidationFailure(ValidationFailure.FailureKind.FormatItemMalformed);
            }

            int index;
            int? alignment = null;
            string formatString = null;

            if (!int.TryParse(split[0], out index))
            {
                return new ValidationFailure(ValidationFailure.FailureKind.FormatItemIndexIsNaN);
            }

            if (indexOfComma >= 0)
            {
                int localAlignment;
                if (!int.TryParse(split[1], out localAlignment))
                {
                    return new ValidationFailure(ValidationFailure.FailureKind.FormatItemAlignmentIsNaN);
                }
                alignment = localAlignment;
            }

            if (indexOfColon >= 0)
            {
                formatString = indexOfComma >= 0
                    ? split[2]
                    : split[1];
            }

            formatStringItem = new FormatStringItem(index, alignment, formatString);
            return null;
        }

        private static ValidationFailure TryValidateFormatString(string formatStringText,
            ICollection<FormatStringItem> formatStringItems, ArgumentListSyntax argumentList, int formatArgumentIndex,
            SemanticModel semanticModel)
        {
            var formatArguments = argumentList.Arguments
                .Skip(formatArgumentIndex + 1)
                .Select(arg => FormatStringArgument.Create(arg.Expression, semanticModel))
                .ToList();
            var maxFormatItemIndex = formatStringItems.Max(item => (int?)item.Index);

            var realArgumentsCount = formatArguments.Count;
            if (formatArguments.Count == 1 &&
                formatArguments[0].TypeSymbol.Is(TypeKind.Array))
            {
                realArgumentsCount = formatArguments[0].ArraySize;
                if (realArgumentsCount == -1)
                {
                    // can't statically check the override that supplies args in an array variable
                    return null;
                }
            }

            return IsFormatValidSafetyNet(formatStringText) ??
                IsSimpleString(formatStringItems.Count, realArgumentsCount) ??
                HasFormatItemIndexTooBig(maxFormatItemIndex, realArgumentsCount) ??
                HasMissingFormatItemIndex(formatStringItems, maxFormatItemIndex) ??
                HasUnusedArguments(formatArguments, maxFormatItemIndex);
        }


        private static ValidationFailure IsFormatValidSafetyNet(string formatString)
        {
            try
            {
                var _ = string.Format(formatString, new object[1000000]);
                return null;
            }
            catch (FormatException)
            {
                return new ValidationFailure(ValidationFailure.FailureKind.UnknownError);
            }
        }

        private static ValidationFailure HasFormatItemIndexTooBig(int? maxFormatItemIndex, int argumentsCount)
        {
            if (maxFormatItemIndex.HasValue &&
                maxFormatItemIndex.Value + 1 > argumentsCount)
            {
                return new ValidationFailure(ValidationFailure.FailureKind.FormatItemIndexTooHigh);
            }

            return null;
        }

        private static ValidationFailure IsSimpleString(int formatStringItemsCount, int argumentsCount)
        {
            if (formatStringItemsCount == 0 && argumentsCount == 0)
            {
                return new ValidationFailure(ValidationFailure.FailureKind.SimpleString);
            }

            return null;
        }

        private static ValidationFailure HasMissingFormatItemIndex(IEnumerable<FormatStringItem> formatStringItems,
            int? maxFormatItemIndex)
        {
            if (!maxFormatItemIndex.HasValue)
            {
                return null;
            }

            var missingFormatItemIndexes = Enumerable.Range(0, maxFormatItemIndex.Value + 1)
                .Except(formatStringItems.Select(item => item.Index))
                .ToList();

            if (missingFormatItemIndexes.Count > 0)
            {
                return new ValidationFailure(ValidationFailure.FailureKind.MissingFormatItemIndex,
                    DiagnosticReportHelper.CreateStringFromArgs(missingFormatItemIndexes));
            }

            return null;
        }

        private static ValidationFailure HasUnusedArguments(List<FormatStringArgument> formatArguments,
            int? maxFormatItemIndex)
        {
            var unusedArgumentNames = formatArguments.Skip((maxFormatItemIndex ?? -1) + 1)
                .Select(arg => arg.Name)
                .ToList();

            if (unusedArgumentNames.Count > 0)
            {
                return new ValidationFailure(ValidationFailure.FailureKind.UnusedFormatArguments,
                    DiagnosticReportHelper.CreateStringFromArgs(unusedArgumentNames));
            }

            return null;
        }

        private static string ProcessErrorMessage(ValidationFailure failure)
        {
            switch (failure.Kind)
            {
                case ValidationFailure.FailureKind.NullFormatString:
                    return "Invalid string format, the format string cannot be null.";
                case ValidationFailure.FailureKind.InvalidCharacterAfterOpenCurlyBrace:
                    return "Invalid string format, opening curly brace can only be followed by a digit or an opening curly brace.";
                case ValidationFailure.FailureKind.UnbalancedCurlyBraceCount:
                    return "Invalid string format, unbalanced curly brace count.";
                case ValidationFailure.FailureKind.FormatItemMalformed:
                    return "Invalid string format, all format items should comply with the following pattern '{index[,alignment][:formatString]}'.";
                case ValidationFailure.FailureKind.FormatItemIndexIsNaN:
                    return "Invalid string format, all format item indexes should be numbers.";
                case ValidationFailure.FailureKind.FormatItemAlignmentIsNaN:
                    return "Invalid string format, all format item alignments should be numbers.";
                case ValidationFailure.FailureKind.FormatItemIndexTooHigh:
                    return "Invalid string format, the highest string format item index should not be greater than the arguments count.";
                case ValidationFailure.FailureKind.SimpleString:
                    return "Remove this formatting call and simply use the input string.";
                case ValidationFailure.FailureKind.MissingFormatItemIndex:
                    return string.Concat("The format string might be wrong, the following item indexes are missing: ",
                        failure.AdditionalInformation, ".");
                case ValidationFailure.FailureKind.UnusedFormatArguments:
                    return string.Concat("The format string might be wrong, the following arguments are unused: ",
                        failure.AdditionalInformation, ".");
                case ValidationFailure.FailureKind.UnknownError:
                default:
                    return "Invalid string format, the format string is invalid and is likely to throw at runtime.";
            }
        }

        protected class ValidationFailure
        {
            public enum FailureKind
            {
                Success,
                FormatStringNotConstant,
                UnknownError,
                SimpleString,
                NullFormatString,
                InvalidCharacterAfterOpenCurlyBrace,
                UnbalancedCurlyBraceCount,
                FormatItemMalformed,
                FormatItemIndexIsNaN,
                FormatItemAlignmentIsNaN,
                FormatItemIndexTooHigh,
                MissingFormatItemIndex,
                UnusedFormatArguments
            }

            public ValidationFailure(FailureKind kind, string additionalInformation = "")
            {
                Kind = kind;
                AdditionalInformation = additionalInformation;
            }

            public FailureKind Kind { get; }
            public string AdditionalInformation { get; }
        }

        private sealed class FormatStringItem
        {
            public FormatStringItem(int index, int? alignment, string formatString)
            {
                Index = index;
                Alignment = alignment;
                FormatString = formatString;
            }

            public int Index { get; }
            public int? Alignment { get; }
            public string FormatString { get; }
        }

        private sealed class FormatStringArgument
        {
            public FormatStringArgument(string name, ITypeSymbol typeSymbol, int arraySize = -1)
            {
                Name = name;
                TypeSymbol = typeSymbol;
                ArraySize = arraySize;
            }

            public static FormatStringArgument Create(ExpressionSyntax expression, SemanticModel semanticModel)
            {
                var type = semanticModel.GetTypeInfo(expression).Type;
                var arraySize = -1;
                if (type.Is(TypeKind.Array))
                {
                    var implicitArray = expression as ImplicitArrayCreationExpressionSyntax;
                    if (implicitArray != null)
                    {
                        arraySize = implicitArray.Initializer.Expressions.Count;
                    }

                    var array = expression as ArrayCreationExpressionSyntax;
                    if (array != null)
                    {
                        arraySize = array.Initializer.Expressions.Count;
                    }
                }

                return new FormatStringArgument(expression.ToString(), type, arraySize);
            }

            public string Name { get; }
            public ITypeSymbol TypeSymbol { get; }
            public int ArraySize { get; }
        }
    }
}