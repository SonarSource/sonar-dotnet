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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Immutable;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class ClassName : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S101";
        private const string MessageFormat = "Rename {0} '{1}' to match camel case naming rules, {2}.";
        internal const string MessageFormatNonUnderscore = "consider using '{0}'";
        internal const string MessageFormatUnderscore = "trim underscores from the name";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var typeDeclaration = (BaseTypeDeclarationSyntax)c.Node;
                    var identifier = typeDeclaration.Identifier;

                    var symbol = c.SemanticModel.GetDeclaredSymbol(typeDeclaration);
                    if (IsTypeComRelated(symbol))
                    {
                        return;
                    }

                    if (identifier.ValueText.StartsWith("_", StringComparison.Ordinal) ||
                        identifier.ValueText.EndsWith("_", StringComparison.Ordinal))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, identifier.GetLocation(), TypeKindNameMapping[typeDeclaration.Kind()],
                            identifier.ValueText, MessageFormatUnderscore));
                        return;
                    }

                    string suggestion;
                    if (TryGetChangedName(identifier.ValueText, typeDeclaration, c.SemanticModel.Compilation.IsTest(), out suggestion))
                    {
                        var messageEnding = string.Format(MessageFormatNonUnderscore, suggestion);
                        c.ReportDiagnostic(Diagnostic.Create(rule, identifier.GetLocation(),
                            TypeKindNameMapping[typeDeclaration.Kind()], identifier.ValueText, messageEnding));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.StructDeclaration);
        }

        internal static bool IsTypeComRelated(INamedTypeSymbol symbol)
        {
            return symbol == null ||
                symbol.GetAttributes().Any(a =>
                    a.AttributeClass.Is(KnownType.System_Runtime_InteropServices_ComImportAttribute) ||
                    a.AttributeClass.Is(KnownType.System_Runtime_InteropServices_InterfaceTypeAttribute));
        }

        private static readonly Dictionary<SyntaxKind, string> TypeKindNameMapping = new Dictionary<SyntaxKind, string>
        {
            {SyntaxKind.StructDeclaration, "struct" },
            {SyntaxKind.ClassDeclaration, "class" },
            {SyntaxKind.InterfaceDeclaration, "interface" }
        };

        private static bool TryGetChangedName(string identifierName, BaseTypeDeclarationSyntax typeDeclaration, bool isUnderscoreAccepted,
            out string suggestion)
        {
            var suggestionPrefix = typeDeclaration is InterfaceDeclarationSyntax ? "I" : string.Empty;
            var namesToCheck = identifierName.Split(new[] { "_" }, StringSplitOptions.None);

            if (LeadingIShouldBeIgnored(namesToCheck[0], typeDeclaration))
            {
                namesToCheck[0] = namesToCheck[0].Substring(1);
            }

            var suggestedNames = namesToCheck.Select(s => CamelCaseConverter.Convert(s));

            if (isUnderscoreAccepted)
            {
                suggestion = string.Join("_", suggestedNames);
            }
            else
            {
                var concatenated = string.Join(string.Empty, suggestedNames);

                // do a second path, to suggest a real camel case string. A_B_C -> ABC -> Abc
                suggestion = CamelCaseConverter.Convert(concatenated);
            }

            suggestion = suggestionPrefix + suggestion;
            return identifierName != suggestion;
        }

        private static bool LeadingIShouldBeIgnored(string input, BaseTypeDeclarationSyntax typeDeclaration)
        {
            if (typeDeclaration is InterfaceDeclarationSyntax)
            {
                if (StartsWithUpperCaseI(input))
                {
                    return true;
                }
            }
            else
            {
                if (IsPossibleInterfaceName(input) &&
                    !typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool StartsWithUpperCaseI(string input)
        {
            return input.Length >= 1 &&
                input[0] == 'I';
        }

        private static bool IsPossibleInterfaceName(string input)
        {
            return StartsWithUpperCaseI(input) &&
                input.Length >= 3 &&
                char.IsUpper(input[1]) &&
                char.IsLower(input[2]);
        }

        internal class CamelCaseConverter
        {
            public static string Convert(string identifierName)
            {
                var name = identifierName ?? string.Empty;

                // handle special case of two upper case characters:
                if (name.Length == 2 &&
                    char.IsUpper(name[0]) &&
                    char.IsUpper(name[1]))
                {
                    return name[0].ToString() + char.ToLowerInvariant(name[1]);
                }

                var suggestion = name;
                var currentState = CamelCaseState.Start;
                var currentIndex = 0;
                while (currentIndex < suggestion.Length)
                {
                    switch (currentState)
                    {
                        case CamelCaseState.Start:
                        case CamelCaseState.Number:
                            if (char.IsLower(suggestion[currentIndex]))
                            {
                                suggestion = CreateSuggestionFirstLowerCase(suggestion, currentIndex);
                                currentState = CamelCaseState.SingleUpper;
                                currentIndex++;
                                continue;
                            }

                            if (char.IsNumber(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.Number;
                                currentIndex++;
                                continue;
                            }

                            if (char.IsUpper(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.SingleUpper;
                                currentIndex++;
                                continue;
                            }

                            break;
                        case CamelCaseState.SingleUpper:
                            if (char.IsLower(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.Lower;
                                currentIndex++;
                                continue;
                            }

                            if (char.IsNumber(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.Number;
                                currentIndex++;
                                continue;
                            }

                            if (char.IsUpper(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.DoubleUpper;
                                currentIndex++;
                                continue;
                            }

                            break;
                        case CamelCaseState.DoubleUpper:
                            if (char.IsLower(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.Lower;
                                currentIndex++;
                                continue;
                            }

                            if (char.IsNumber(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.Number;
                                currentIndex++;
                                continue;
                            }

                            if (char.IsUpper(suggestion[currentIndex]))
                            {
                                var suggestionResult = CreateSuggestionTooManyUpperCase(suggestion, currentIndex);
                                currentIndex = suggestionResult.LastProcessedIndex;
                                suggestion = suggestionResult.Suggestion;

                                currentState = CamelCaseState.Lower;
                                currentIndex++;
                                continue;
                            }

                            break;
                        case CamelCaseState.Lower:
                            if (char.IsLower(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.Lower;
                                currentIndex++;
                                continue;
                            }

                            if (char.IsNumber(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.Number;
                                currentIndex++;
                                continue;
                            }

                            if (char.IsUpper(suggestion[currentIndex]))
                            {
                                currentState = CamelCaseState.SingleUpper;
                                currentIndex++;
                                continue;
                            }

                            break;
                        default:
                            throw new NotSupportedException("State machine is in invalid state");
                    }
                }

                return suggestion;
            }

            private class TooManyUpperCaseSuggestionResult
            {
                public string Suggestion { get; set; }
                public int LastProcessedIndex { get; set; }
            }

            private static TooManyUpperCaseSuggestionResult CreateSuggestionTooManyUpperCase(string input, int currentIndex)
            {
                var suggestion = input;

                // current index
                if (suggestion.Length == currentIndex + 1 ||
                    !char.IsLower(suggestion[currentIndex + 1]))
                {
                    suggestion = ChangeToLowerBetween(suggestion, currentIndex, currentIndex);
                }

                // preceding characters
                var toLowerTill = GetStartingIndexForLowerCasing(suggestion, currentIndex - 1);
                if (toLowerTill <= 0)
                {
                    toLowerTill = 1;
                }

                suggestion = ChangeToLowerBetween(suggestion, toLowerTill, currentIndex - 1);

                // succeeding characters
                toLowerTill = GetEndingIndexForLowerCasing(suggestion, currentIndex + 1);
                if (toLowerTill == suggestion.Length)
                {
                    toLowerTill = suggestion.Length - 1;
                }

                suggestion = ChangeToLowerBetween(suggestion, currentIndex + 1, toLowerTill);

                return new TooManyUpperCaseSuggestionResult
                {
                    Suggestion = suggestion,
                    LastProcessedIndex = toLowerTill
                };
            }

            private static int GetEndingIndexForLowerCasing(string suggestion, int startIndex)
            {
                var currentIndex = startIndex;
                while (currentIndex < suggestion.Length)
                {
                    if (char.IsNumber(suggestion[currentIndex]))
                    {
                        return currentIndex - 1;
                    }
                    if (char.IsLower(suggestion[currentIndex]))
                    {
                        return currentIndex - 2;
                    }
                    currentIndex++;
                }

                return suggestion.Length;
            }

            private static int GetStartingIndexForLowerCasing(string suggestion, int startIndex)
            {
                var currentIndex = startIndex;
                while (currentIndex >= 0)
                {
                    if (char.IsLower(suggestion[currentIndex]) ||
                        char.IsNumber(suggestion[currentIndex]))
                    {
                        return currentIndex + 2;
                    }
                    currentIndex--;
                }

                return 0;
            }

            private static string CreateSuggestionFirstLowerCase(string input, int currentIndex)
            {
                var beginning = input.Substring(0, currentIndex);
                var continuation = currentIndex < input.Length - 1 ? input.Substring(currentIndex + 1) : string.Empty;
                return beginning + char.ToUpperInvariant(input[currentIndex]) + continuation;
            }

            private static string ChangeToLowerBetween(string input, int lower, int upper)
            {
                if (lower > upper)
                {
                    return input;
                }

                var pre = lower > 0 ? input.Substring(0, lower) : string.Empty;
                var post = upper < input.Length ? input.Substring(upper + 1) : string.Empty;
                var middle = input.Substring(lower, upper - lower + 1).ToLowerInvariant();

                return pre + middle + post;
            }

            private enum CamelCaseState
            {
                Start,
                SingleUpper,
                DoubleUpper,
                Lower,
                Number
            }
        }
    }
}
