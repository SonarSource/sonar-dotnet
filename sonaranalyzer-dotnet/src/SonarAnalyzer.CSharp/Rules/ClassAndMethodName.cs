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
using System.Text;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId_TypeName)]
    [Rule(DiagnosticId_MethodName)]
    public class ClassAndMethodName : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId_MethodName = "S101";
        internal const string DiagnosticId_TypeName = "S100";

        private const string MessageFormat = "Rename {0} '{1}' to match camel case naming rules, {2}.";
        internal const string MessageFormatNonUnderscore = "consider using '{0}'";
        internal const string MessageFormatUnderscore = "trim underscores from the name";

        private static readonly DiagnosticDescriptor rule_MethodName =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_TypeName, MessageFormat, RspecStrings.ResourceManager);

        private static readonly DiagnosticDescriptor rule_TypeName =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_MethodName, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(rule_MethodName, rule_TypeName);

        private static readonly Dictionary<SyntaxKind, string> TypeKindNameMapping = new Dictionary<SyntaxKind, string>
        {
            {SyntaxKind.StructDeclaration, "struct" },
            {SyntaxKind.ClassDeclaration, "class" },
            {SyntaxKind.InterfaceDeclaration, "interface" },
            {SyntaxKind.MethodDeclaration, "method" },
            {SyntaxKind.PropertyDeclaration, "property" }
        };

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckTypeName((BaseTypeDeclarationSyntax)c.Node, c),
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.StructDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (MethodDeclarationSyntax)c.Node;
                    CheckMemberName(declaration, declaration.Identifier, c);
                },
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (PropertyDeclarationSyntax)c.Node;
                    CheckMemberName(declaration, declaration.Identifier, c);
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static void CheckTypeName(BaseTypeDeclarationSyntax type,
            SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (BaseTypeDeclarationSyntax)context.Node;
            var identifier = typeDeclaration.Identifier;
            var symbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);

            if (IsTypeComRelated(symbol))
            {
                return;
            }

            if (identifier.ValueText.StartsWith("_", StringComparison.Ordinal) ||
                identifier.ValueText.EndsWith("_", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule_TypeName, identifier.GetLocation(), TypeKindNameMapping[typeDeclaration.Kind()],
                    identifier.ValueText, MessageFormatUnderscore));
                return;
            }

            string suggestion;
            bool isNameValid = IsTypeNameValid(identifier.ValueText,
                typeDeclaration is InterfaceDeclarationSyntax,
                typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                context.SemanticModel.Compilation.IsTest(),
                out suggestion);

            if (!isNameValid)
            {
                var messageEnding = string.Format(MessageFormatNonUnderscore, suggestion);
                context.ReportDiagnostic(Diagnostic.Create(rule_TypeName, identifier.GetLocation(),
                    TypeKindNameMapping[typeDeclaration.Kind()], identifier.ValueText, messageEnding));
            }
        }

        private static void CheckMemberName(MemberDeclarationSyntax member, SyntaxToken identifier,
            SyntaxNodeAnalysisContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(member);
            if (symbol == null)
            {
                return;
            }

            if (IsTypeComRelated(symbol.ContainingType) ||
                symbol.GetInterfaceMember() != null ||
                symbol.GetOverriddenMember() != null ||
                symbol.IsExtern)
            {
                return;
            }

            if (identifier.ValueText.StartsWith("_", StringComparison.Ordinal) ||
                identifier.ValueText.EndsWith("_", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule_MethodName, identifier.GetLocation(), TypeKindNameMapping[member.Kind()],
                    identifier.ValueText, MessageFormatUnderscore));
                return;
            }

            if (identifier.ValueText.Contains("_"))
            {
                return;
            }

            string suggestion;
            if (!IsMemberNameValid(identifier.ValueText, out suggestion))
            {
                var messageEnding = string.Format(MessageFormatNonUnderscore, suggestion);
                context.ReportDiagnostic(Diagnostic.Create(rule_MethodName, identifier.GetLocation(), TypeKindNameMapping[member.Kind()],
                    identifier.ValueText, messageEnding));
            }
        }

        private static bool IsMemberNameValid(string identifierName, out string suggestion)
        {
            if (identifierName.Length == 1)
            {
                suggestion = identifierName.ToUpperInvariant();
                return suggestion == identifierName;
            }

            var suggestionBuilder = new StringBuilder(identifierName.Length);

            foreach (var part in SplitToCamelCase(identifierName))
            {
                suggestionBuilder.Append(SuggestLessUppercase(part, 1));
            }

            suggestionBuilder[0] = char.ToUpperInvariant(suggestionBuilder[0]);
            suggestion = SuggestCapitalLetterAfterNonLetter(suggestionBuilder);

            return suggestion == identifierName;
        }

        private static bool IsTypeNameValid(string identifierName, bool requireInitialI, bool allowInitialI,
            bool areUnderscoresAllowed, out string suggestion)
        {
            if (identifierName.Length == 1)
            {
                suggestion = identifierName.ToUpperInvariant();
                return suggestion == identifierName;
            }

            var idealNameVariant = new StringBuilder(identifierName.Length);
            var acceptableNameVariant = new StringBuilder(identifierName.Length);

            var parts = SplitToCamelCase(identifierName).ToList();
            for (int i = 0; i < parts.Count; i++)
            {
                string part = parts[i];
                if (part.Length == 1 && part[0] == '_' && !areUnderscoresAllowed)
                {
                    continue;
                }

                var ideal = i == 0 ? HandleFirstTypePart(part, requireInitialI, allowInitialI, 1)
                                    : SuggestLessUppercase(part, 1);

                var acceptable = i == 0 ? HandleFirstTypePart(part, requireInitialI, allowInitialI, 2)
                                         : SuggestLessUppercase(part, 2);

                idealNameVariant.Append(ideal);
                acceptableNameVariant.Append(acceptable);
            }

            suggestion = SuggestCapitalLetterAfterNonLetter(idealNameVariant);
            var acceptableSuggestion = SuggestCapitalLetterAfterNonLetter(acceptableNameVariant);

            return acceptableSuggestion == identifierName ||
                   suggestion == identifierName;
        }

        private static string HandleFirstTypePart(string input,
            bool requireInitialI, bool allowInitialI, int maxUppercase)
        {
            bool startsWithI = input[0] == 'I';

            if (requireInitialI)
            {
                string prefix = startsWithI ? string.Empty : "I";
                return prefix + SuggestLessUppercase(ToInitCap(input), maxUppercase);
            }

            string suggestionToProcess;
            if (input.Length == 1 &&
                !allowInitialI &&
                startsWithI &&
                IsCharUpper(input, 0) &&
                !IsCharUpper(input, 1))
            {
                suggestionToProcess = ToInitCap(input.Substring(1));
            }
            else
            {
                suggestionToProcess = ToInitCap(input);
            }

            return SuggestLessUppercase(suggestionToProcess, maxUppercase);
        }

        private static string SuggestCapitalLetterAfterNonLetter(StringBuilder suggestion)
        {
            for (int i = 1; i < suggestion.Length; i++)
            {
                if (!char.IsLetter(suggestion[i - 1]) &&
                    char.IsLower(suggestion[i]))
                {
                    suggestion[i] = char.ToUpperInvariant(suggestion[i]);
                }
            }
            return suggestion.ToString();
        }

        private static string SuggestLessUppercase(string input, int maxUppercaseCount)
        {
            var upper = input.Take(maxUppercaseCount);
            var lower = input.Skip(maxUppercaseCount).Select(char.ToLowerInvariant);

            return new string(upper.Concat(lower).ToArray());
        }

        private static IEnumerable<string> SplitToCamelCase(string name)
        {
            var currentWord = new StringBuilder();
            foreach (var c in name)
            {
                if (char.IsUpper(c))
                {
                    if (currentWord.Length > 0 && !char.IsUpper(currentWord[currentWord.Length - 1]))
                    {
                        yield return currentWord.ToString();
                        currentWord.Clear();
                    }

                    currentWord.Append(c);
                }
                else if (char.IsLower(c))
                {
                    if (currentWord.Length > 1 && char.IsUpper(currentWord[currentWord.Length - 1]))
                    {
                        var lastChar = currentWord[currentWord.Length - 1];
                        currentWord.Length--;
                        yield return currentWord.ToString();
                        currentWord.Clear();
                        currentWord.Append(lastChar);
                    }

                    currentWord.Append(c);
                }
                else
                {
                    if (currentWord.Length > 0)
                    {
                        yield return currentWord.ToString();
                        currentWord.Clear();
                    }

                    yield return c.ToString();
                    continue;
                }
            }

            if (currentWord.Length > 0)
            {
                yield return currentWord.ToString();
            }
        }

        private static bool IsTypeComRelated(INamedTypeSymbol symbol)
        {
            return symbol == null ||
                symbol.GetAttributes().Any(a =>
                    a.AttributeClass.Is(KnownType.System_Runtime_InteropServices_ComImportAttribute) ||
                    a.AttributeClass.Is(KnownType.System_Runtime_InteropServices_InterfaceTypeAttribute));
        }

        private static string ToInitCap(string input)
        {
            return input.Length > 0
                ? char.ToUpperInvariant(input[0]) + input.Substring(1)
                : input;
        }

        private static bool IsCharUpper(string input, int idx)
        {
            return idx < 0 || idx >= input.Length
                ? false
                : char.IsUpper(input[idx]);
        }
    }
}
