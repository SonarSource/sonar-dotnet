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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class StringFormatArgumentNumberMismatch : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2275";
        private const string MessageFormat = "Invalid string format, {0}";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        protected override DiagnosticDescriptor Rule => rule;

        private readonly ISet<MethodSignature> HandledFormatMethods = new HashSet<MethodSignature>
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

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForInvalidFormatString,
                SyntaxKind.InvocationExpression);
        }

        private void CheckForInvalidFormatString(SyntaxNodeAnalysisContext c)
        {
            var invocation = (InvocationExpressionSyntax)c.Node;
            var methodSymbol = c.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            if (methodSymbol == null ||
                !IsHandledFormatMethod(methodSymbol))
            {
                return;
            }

            var formatArgumentIndex = methodSymbol.Parameters[0].IsType(KnownType.System_IFormatProvider) ? 1 : 0;
            var formatExpression = invocation.ArgumentList.Arguments[formatArgumentIndex];

            var constValue = c.SemanticModel.GetConstantValue(formatExpression.Expression);
            if (!constValue.HasValue)
            {
                return; // can't check non-constant format strings
            }

            string formatString = (string)constValue.Value;

            try
            {
                StringFormat.Parse(formatString)
                            .Validate(invocation.ArgumentList, formatArgumentIndex, c.SemanticModel);
            }
            catch (FormatException ex)
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), ex.Message));
                return;
            }

            // Safety-net to ensure we haven't missed any invalid case.
            if (!IsFormatValid(formatString))
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(),
                    "the format is malformed and will likely throw a 'FormatException'."));
            }
        }

        private bool IsHandledFormatMethod(IMethodSymbol methodSymbol)
        {
            return HandledFormatMethods.Where(hfm => methodSymbol.ContainingType.Is(hfm.ContainingType))
                .Any(method => method.Name == methodSymbol.Name);
        }

        internal static bool IsFormatValid(string format, int argsCount = 1000000)
        {
            try
            {
                string.Format(format, new object[argsCount]);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public class StringFormat
        {
            public class StringFormatItem
            {
                internal StringFormatItem(int index, int? alignment, string formatString)
                {
                    Index = index;
                    Alignment = alignment;
                    FormatString = formatString;
                }

                public int Index { get; }
                public int? Alignment { get; }
                public string FormatString { get; }
            }

            private class StringFormatArgument
            {
                public StringFormatArgument(ExpressionSyntax expression, SemanticModel semanticModel)
                {
                    Expression = expression;
                    Name = expression.ToString();
                    Type = semanticModel.GetTypeInfo(expression).Type;

                    ArraySize = -1;
                    if (Type.Is(TypeKind.Array))
                    {
                        var implicitArray = expression as ImplicitArrayCreationExpressionSyntax;
                        if (implicitArray != null)
                        {
                            ArraySize = implicitArray.Initializer.Expressions.Count;
                        }

                        var array = expression as ArrayCreationExpressionSyntax;
                        if (array != null)
                        {
                            ArraySize = array.Initializer.Expressions.Count;
                        }
                    }
                }

                public string Name { get; }
                public ITypeSymbol Type { get; }
                public ExpressionSyntax Expression { get; }
                public int ArraySize { get; }
            }

            private StringFormat(string format, IEnumerable<StringFormatItem> stringFormatItems)
            {
                Format = format;
                Items = stringFormatItems.ToList();
            }

            public string Format { get; }
            public IEnumerable<StringFormatItem> Items { get; }

            public static StringFormat Parse(string format)
            {
                if (format == null)
                {
                    throw new FormatException("the format cannot be null.");
                }

                var tokens = ExtractFormatItemTokens(format);
                var stringFormatItems = ConvertToStringFormatItems(tokens);

                return new StringFormat(format, stringFormatItems);
            }

            private static IEnumerable<string> ExtractFormatItemTokens(string format)
            {
                var curlyBraceCount = 0;
                var previousChar = '\0';
                StringBuilder currentFormatItemBuilder = null;
                for (int i = 0; i < format.Length; i++)
                {
                    var currentChar = format[i];

                    if (currentChar == '{')
                    {
                        curlyBraceCount++;
                        if (currentFormatItemBuilder == null)
                        {
                            currentFormatItemBuilder = new StringBuilder();
                        }
                    }
                    else if (previousChar == '{' && !char.IsDigit(currentChar))
                    {
                        throw new FormatException("opening curly brace can only be followed by a digit or an opening curly brace.");
                    }
                    else if (currentChar == '}')
                    {
                        curlyBraceCount--;
                        if (currentFormatItemBuilder != null)
                        {
                            yield return currentFormatItemBuilder.ToString();
                            currentFormatItemBuilder = null;
                        }
                    }
                    else
                    {
                        currentFormatItemBuilder?.Append(currentChar);
                    }

                    previousChar = currentChar;
                }

                if (curlyBraceCount != 0)
                {
                    throw new FormatException("unbalanced curly brace count.");
                }
            }

            private static IEnumerable<StringFormatItem> ConvertToStringFormatItems(IEnumerable<string> stringFormatItemTokens)
            {
                foreach (var formatItem in stringFormatItemTokens)
                {
                    var indexOfComma = formatItem.IndexOf(',');
                    var indexOfColon = formatItem.IndexOf(':');
                    var split = formatItem.Split(',', ':');

                    if (indexOfComma >= 0 && indexOfColon >= 0 && indexOfColon < indexOfComma ||
                        split.Length > 3)
                    {
                        throw new FormatException("format items should comply with the following pattern '{index[,alignment][:formatString]}'.");
                    }

                    int index;
                    int? alignment = null;
                    string formatString = null;

                    if (!int.TryParse(split[0], out index))
                    {
                        throw new FormatException("format item index should be a number.");
                    }

                    if (indexOfComma >= 0)
                    {
                        int localAlignment;
                        if (!int.TryParse(split[1], out localAlignment))
                        {
                            throw new FormatException("format item alignment should be a number.");
                        }
                        alignment = localAlignment;
                    }

                    if (indexOfColon >= 0)
                    {
                        formatString = indexOfComma >= 0
                            ? split[2]
                            : split[1];
                    }

                    yield return new StringFormatItem(index, alignment, formatString);
                }
            }

            public void Validate(ArgumentListSyntax argumentList, int formatArgumentIndex, SemanticModel semanticModel)
            {
                var maxFormatItemIndex = Items.Max(item => (int?)item.Index);

                CheckForMissingFormatItemIndex(maxFormatItemIndex);

                var formatArguments = argumentList.Arguments
                    .Skip(formatArgumentIndex + 1)
                    .Select(arg => new StringFormatArgument(arg.Expression, semanticModel))
                    .ToList();

                var realArgumentsCount = formatArguments.Count;
                if (formatArguments.Count == 1 &&
                    formatArguments[0].Type.Is(TypeKind.Array))
                {
                    realArgumentsCount = formatArguments[0].ArraySize;
                    if (realArgumentsCount == -1)
                    {
                        return; // can't statically check the override that supplies args in an array variable
                    }
                }
                if (maxFormatItemIndex.HasValue &&
                    maxFormatItemIndex.Value + 1 > realArgumentsCount)
                {
                    throw new FormatException("the highest string format item index should not be greater than the arguments count.");
                }

                CheckForUnusedArguments(formatArguments, maxFormatItemIndex);
            }

            private void CheckForMissingFormatItemIndex(int? maxFormatItemIndex)
            {
                if (!maxFormatItemIndex.HasValue)
                {
                    return;
                }

                var missingFormatItemIndexes = Enumerable.Range(0, maxFormatItemIndex.Value + 1)
                    .Except(Items.Select(item => item.Index))
                    .ToList();
                if (missingFormatItemIndexes.Count > 0)
                {
                    throw new FormatException(string.Format("the following format item indexes are missing: {0}.",
                        DiagnosticReportHelper.CreateStringFromArgs(missingFormatItemIndexes)));
                }
            }

            private void CheckForUnusedArguments(IEnumerable<StringFormatArgument> formatArguments,
                int? maxFormatItemIndex)
            {
                var unusedArgumentNames = formatArguments.Skip((maxFormatItemIndex ?? -1) + 1)
                    .Select(arg => arg.Name)
                    .ToList();
                if (unusedArgumentNames.Count > 0)
                {
                    throw new FormatException(string.Format("the following arguments are unused: {0}.",
                        DiagnosticReportHelper.CreateStringFromArgs(unusedArgumentNames)));
                }
            }
        }
    }
}
