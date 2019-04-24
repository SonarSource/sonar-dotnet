/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(BugDiagnosticId)]
    [Rule(CodeSmellDiagnosticId)]
    public sealed class StringFormatValidator : SonarDiagnosticAnalyzer
    {
        private const string BugDiagnosticId = "S2275";
        private const string CodeSmellDiagnosticId = "S3457";
        private const string MessageFormat = "{0}";

        // This is the value as defined in .Net Framework
        private const int MaxValueForArgumentIndexAndAlignment = 1000000;

        private static readonly DiagnosticDescriptor bugRule =
          DiagnosticDescriptorBuilder.GetDescriptor(BugDiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static readonly DiagnosticDescriptor codeSmellRule =
          DiagnosticDescriptorBuilder.GetDescriptor(CodeSmellDiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(bugRule, codeSmellRule);

        private static readonly ISet<MemberDescriptor> HandledFormatMethods = new HashSet<MemberDescriptor>
        {
            new MemberDescriptor(KnownType.System_String, "Format"),
            new MemberDescriptor(KnownType.System_Console, "Write"),
            new MemberDescriptor(KnownType.System_Console, "WriteLine"),
            new MemberDescriptor(KnownType.System_Text_StringBuilder, "AppendFormat"),
            new MemberDescriptor(KnownType.System_IO_TextWriter, "Write"),
            new MemberDescriptor(KnownType.System_IO_TextWriter, "WriteLine"),
            new MemberDescriptor(KnownType.System_Diagnostics_Debug, "WriteLine"),
            new MemberDescriptor(KnownType.System_Diagnostics_Trace, "TraceError"),
            new MemberDescriptor(KnownType.System_Diagnostics_Trace, "TraceInformation"),
            new MemberDescriptor(KnownType.System_Diagnostics_Trace, "TraceWarning"),
            new MemberDescriptor(KnownType.System_Diagnostics_TraceSource, "TraceInformation")
        };

        private static readonly ISet<ValidationFailure> bugRelatedFailures = new HashSet<ValidationFailure>
        {
            ValidationFailure.UnknownError,
            ValidationFailure.NullFormatString,
            ValidationFailure.InvalidCharacterAfterOpenCurlyBrace,
            ValidationFailure.UnbalancedCurlyBraceCount,
            ValidationFailure.FormatItemMalformed,
            ValidationFailure.FormatItemIndexBiggerThanArgsCount,
            ValidationFailure.FormatItemIndexBiggerThanMaxValue,
            ValidationFailure.FormatItemAlignmentBiggerThanMaxValue
        };

        private static readonly ISet<ValidationFailure> codeSmellRelatedFailures = new HashSet<ValidationFailure>
        {
            ValidationFailure.SimpleString,
            ValidationFailure.MissingFormatItemIndex,
            ValidationFailure.UnusedFormatArguments
        };

        private static readonly Regex StringFormatItemRegex = // pattern is: index[,alignment][:formatString]
            new Regex(@"^(?<Index>\d+)(\s*,\s*(?<Alignment>-?\d+)\s*)?(:(?<Format>.+))?$", RegexOptions.Compiled);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForFormatStringIssues, SyntaxKind.InvocationExpression);
        }

        private static void CheckForFormatStringIssues(SyntaxNodeAnalysisContext analysisContext)
        {
            var invocation = (InvocationExpressionSyntax)analysisContext.Node;

            if (!(analysisContext.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol) ||
                !methodSymbol.Parameters.Any() ||
                methodSymbol.Parameters.All(x => x.Name != "format"))
            {
                return;
            }

            var currentMethodSignature = HandledFormatMethods
                .Where(hfm => methodSymbol.ContainingType.Is(hfm.ContainingType))
                .FirstOrDefault(method => method.Name == methodSymbol.Name);
            if (currentMethodSignature == null)
            {
                return;
            }

            var formatArgumentIndex = methodSymbol.Parameters[0].IsType(KnownType.System_IFormatProvider)
                ? 1 : 0;
            var formatStringExpression = invocation.ArgumentList.Arguments[formatArgumentIndex];

            var constValue = analysisContext.SemanticModel.GetConstantValue(formatStringExpression.Expression);
            if (!constValue.HasValue)
            {
                // can't check non-constant format strings
                return;
            }

            var failure = TryParseAndValidate(constValue.Value as string, invocation.ArgumentList,
                formatArgumentIndex, analysisContext.SemanticModel);
            if (failure == null ||
                CanIgnoreFailure(failure, currentMethodSignature.Name, invocation.ArgumentList.Arguments.Count))
            {
                return;
            }

            if (bugRelatedFailures.Contains(failure))
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(bugRule, invocation.Expression.GetLocation(),
                    failure.ToString()));
            }

            if (codeSmellRelatedFailures.Contains(failure))
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(codeSmellRule, invocation.Expression.GetLocation(),
                    failure.ToString()));
            }
        }

        private static bool CanIgnoreFailure(ValidationFailure failure, string methodName, int argumentsCount)
        {
            if (methodName.EndsWith("Format") ||
                failure == ValidationFailure.UnusedFormatArguments ||
                failure == ValidationFailure.FormatItemIndexBiggerThanArgsCount)
            {
                return false;
            }

            // All methods in HandledFormatMethods that do not end on Format have an overload
            // with only one argument and the rule should not raise an issue
            return argumentsCount == 1;
        }

        private static ValidationFailure TryParseAndValidate(string formatStringText, ArgumentListSyntax argumentList,
            int formatArgumentIndex, SemanticModel semanticModel)
        {
            if (formatStringText == null)
            {
                return ValidationFailure.NullFormatString;
            }

            return ExtractFormatItems(formatStringText, out var formatStringItems) ??
                TryValidateFormatString(formatStringItems, argumentList, formatArgumentIndex, semanticModel);
        }

        private static ValidationFailure ExtractFormatItems(string formatString,
            out List<FormatStringItem> formatStringItems)
        {
            formatStringItems = new List<FormatStringItem>();
            var curlyBraceCount = 0;
            StringBuilder currentFormatItemBuilder = null;
            var isEscapingOpenCurlyBrace = false;
            var isEscapingCloseCurlyBrace = false;
            for (var i = 0; i < formatString.Length; i++)
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
                    return ValidationFailure.InvalidCharacterAfterOpenCurlyBrace;
                }

                if (currentChar == '}')
                {
                    isEscapingCloseCurlyBrace = previousChar == '}' && !isEscapingCloseCurlyBrace;
                    curlyBraceCount = isEscapingCloseCurlyBrace
                        ? curlyBraceCount + 1
                        : curlyBraceCount - 1;

                    if (currentFormatItemBuilder != null)
                    {
                        var failure = TryParseItem(currentFormatItemBuilder.ToString(), out var formatStringItem);
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
                return ValidationFailure.UnbalancedCurlyBraceCount;
            }

            return null;
        }

        private static ValidationFailure TryParseItem(string formatItem, out FormatStringItem formatStringItem)
        {
            formatStringItem = null;

            var matchResult = StringFormatItemRegex.Match(formatItem);
            if (!matchResult.Success)
            {
                return ValidationFailure.FormatItemMalformed;
            }

            var index = int.Parse(matchResult.Groups["Index"].Value);
            var alignment = matchResult.Groups["Alignment"].Success
                ? (int?)int.Parse(matchResult.Groups["Alignment"].Value)
                : null;
            var formatString = matchResult.Groups["Format"].Success
                ? matchResult.Groups["Format"].Value
                : null;
            formatStringItem = new FormatStringItem(index, alignment, formatString);

            return null;
        }

        private static ValidationFailure TryValidateFormatString(ICollection<FormatStringItem> formatStringItems,
            ArgumentListSyntax argumentList, int formatArgumentIndex, SemanticModel semanticModel)
        {
            if (formatStringItems.Any(x => x.Index > MaxValueForArgumentIndexAndAlignment))
            {
                return ValidationFailure.FormatItemIndexBiggerThanMaxValue;
            }
            if (formatStringItems.Any(x => x.Alignment > MaxValueForArgumentIndexAndAlignment))
            {
                return ValidationFailure.FormatItemAlignmentBiggerThanMaxValue;
            }

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

            return IsSimpleString(formatStringItems.Count, realArgumentsCount) ??
                HasFormatItemIndexTooBig(maxFormatItemIndex, realArgumentsCount) ??
                HasMissingFormatItemIndex(formatStringItems, maxFormatItemIndex) ??
                HasUnusedArguments(formatArguments, maxFormatItemIndex);
        }

        private static ValidationFailure HasFormatItemIndexTooBig(int? maxFormatItemIndex, int argumentsCount)
        {
            if (maxFormatItemIndex.HasValue &&
                maxFormatItemIndex.Value + 1 > argumentsCount)
            {
                return ValidationFailure.FormatItemIndexBiggerThanArgsCount;
            }

            return null;
        }

        private static ValidationFailure IsSimpleString(int formatStringItemsCount, int argumentsCount)
        {
            if (formatStringItemsCount == 0 && argumentsCount == 0)
            {
                return ValidationFailure.SimpleString;
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
                .Select(i => i.ToString())
                .ToList();

            if (missingFormatItemIndexes.Count > 0)
            {
                var failure = ValidationFailure.MissingFormatItemIndex;
                failure.AdditionalData = missingFormatItemIndexes;
                return failure;
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
                var failure = ValidationFailure.UnusedFormatArguments;
                failure.AdditionalData = unusedArgumentNames;
                return failure;
            }

            return null;
        }

        public class ValidationFailure
        {
            public static readonly ValidationFailure NullFormatString =
                new ValidationFailure("Invalid string format, the format string cannot be null.");
            public static readonly ValidationFailure InvalidCharacterAfterOpenCurlyBrace =
                new ValidationFailure("Invalid string format, opening curly brace can only be followed by a digit " +
                    "or an opening curly brace.");
            public static readonly ValidationFailure UnbalancedCurlyBraceCount =
                new ValidationFailure("Invalid string format, unbalanced curly brace count.");
            public static readonly ValidationFailure FormatItemMalformed =
                new ValidationFailure("Invalid string format, all format items should comply with the following " +
                    "pattern '{index[,alignment][:formatString]}'.");
            public static readonly ValidationFailure FormatItemIndexBiggerThanArgsCount =
                new ValidationFailure("Invalid string format, the highest string format item index should not be " +
                    "greater than the arguments count.");
            public static readonly ValidationFailure FormatItemIndexBiggerThanMaxValue =
                new ValidationFailure("Invalid string format, the string format item index should not be " +
                    $"greater than {MaxValueForArgumentIndexAndAlignment}.");
            public static readonly ValidationFailure FormatItemAlignmentBiggerThanMaxValue =
                new ValidationFailure("Invalid string format, the string format item alignment should not be " +
                    $"greater than {MaxValueForArgumentIndexAndAlignment}.");
            public static readonly ValidationFailure SimpleString =
                new ValidationFailure("Remove this formatting call and simply use the input string.");
            public static readonly ValidationFailure UnknownError =
                new ValidationFailure("Invalid string format, the format string is invalid and is likely to throw at " +
                    "runtime.");
            public static readonly ValidationFailure MissingFormatItemIndex =
                new ValidationFailure("The format string might be wrong, the following item indexes are missing: ");
            public static readonly ValidationFailure UnusedFormatArguments =
                new ValidationFailure("The format string might be wrong, the following arguments are unused: ");

            private readonly string message;

            private ValidationFailure(string message)
            {
                this.message = message;
            }

            public IEnumerable<string> AdditionalData { get; set; }

            public override string ToString()
            {
                return AdditionalData == null
                    ? this.message
                    : string.Concat(this.message, AdditionalData.ToSentence(quoteWords: true), ".");
            }
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
                if (type != null && type.Is(TypeKind.Array))
                {
                    if (expression is ImplicitArrayCreationExpressionSyntax implicitArray)
                    {
                        arraySize = implicitArray.Initializer.Expressions.Count;
                    }

                    if (expression is ArrayCreationExpressionSyntax array &&
                        array.Initializer != null)
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
