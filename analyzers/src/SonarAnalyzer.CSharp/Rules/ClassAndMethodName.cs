/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Text;

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ClassAndMethodName : SonarDiagnosticAnalyzer
    {
        private const string MethodNameDiagnosticId = "S100";
        private const string TypeNameDiagnosticId = "S101";

        private const string MessageFormat = "Rename {0} '{1}' to match pascal case naming rules, {2}.";
        private const string MessageFormatNonUnderscore = "consider using '{0}'";
        private const string MessageFormatUnderscore = "trim underscores from the name";

        private static readonly DiagnosticDescriptor MethodNameRule = DescriptorFactory.Create(MethodNameDiagnosticId, MessageFormat);
        private static readonly DiagnosticDescriptor TypeNameRule = DescriptorFactory.Create(TypeNameDiagnosticId, MessageFormat);

        private static readonly ImmutableArray<KnownType> ComRelatedTypes =
            ImmutableArray.Create(
                KnownType.System_Runtime_InteropServices_ComImportAttribute,
                KnownType.System_Runtime_InteropServices_InterfaceTypeAttribute);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(MethodNameRule, TypeNameRule);

        internal static IEnumerable<string> SplitToParts(string name)
        {
            var currentWord = new StringBuilder(name.Length);
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
                }
            }

            if (currentWord.Length > 0)
            {
                yield return currentWord.ToString();
            }
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
                {
                    if (c.IsRedundantPositionalRecordContext())
                    {
                        return;
                    }
                    CheckTypeName(c);
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration);

            context.RegisterNodeAction(c =>
                {
                    var identifier = GetDeclarationIdentifier(c.Node);
                    CheckMemberName(c, identifier);
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKindEx.LocalFunctionStatement);
        }

        private static void CheckTypeName(SonarSyntaxNodeReportingContext context)
        {
            var typeDeclaration = (BaseTypeDeclarationSyntax)context.Node;
            var identifier = typeDeclaration.Identifier;
            var symbol = context.Model.GetDeclaredSymbol(typeDeclaration);

            if (symbol.GetAttributes(ComRelatedTypes).Any())
            {
                return;
            }

            if (identifier.ValueText.StartsWith("_", StringComparison.Ordinal)
                || identifier.ValueText.EndsWith("_", StringComparison.Ordinal))
            {
                context.ReportIssue(TypeNameRule, identifier, typeDeclaration.GetDeclarationTypeName(), identifier.ValueText, MessageFormatUnderscore);
                return;
            }

            if (typeDeclaration is ClassDeclarationSyntax && IsTestClassName(typeDeclaration.Identifier.ValueText))
            {
                return;
            }

            if (symbol.DeclaringSyntaxReferences.Length > 1
                && symbol.DeclaringSyntaxReferences.Any(syntax => syntax.SyntaxTree.IsConsideredGenerated(CSharpGeneratedCodeRecognizer.Instance, context.IsRazorAnalysisEnabled())))
            {
                return;
            }

            var isNameValid = IsTypeNameValid(identifier.ValueText,
                                              requireInitialI: typeDeclaration is InterfaceDeclarationSyntax,
                                              allowInitialI: typeDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword),
                                              areUnderscoresAllowed: context.IsTestProject(),
                                              suggestion: out var suggestion);

            if (!isNameValid)
            {
                var messageEnding = string.Format(MessageFormatNonUnderscore, suggestion);
                context.ReportIssue(TypeNameRule, identifier, typeDeclaration.GetDeclarationTypeName(), identifier.ValueText, messageEnding);
            }
        }

        private static void CheckMemberName(SonarSyntaxNodeReportingContext context, SyntaxToken identifier)
        {
            var symbol = context.Model.GetDeclaredSymbol(context.Node);
            if (symbol == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(identifier.ValueText)
                || symbol.ContainingType.GetAttributes(ComRelatedTypes).Any()
                || symbol.GetInterfaceMember() != null
                || symbol.GetOverriddenMember() != null
                || symbol.IsExtern)
            {
                return;
            }

            if (identifier.ValueText.StartsWith("_", StringComparison.Ordinal)
                || identifier.ValueText.EndsWith("_", StringComparison.Ordinal))
            {
                context.ReportIssue(MethodNameRule, identifier, context.Node.GetDeclarationTypeName(), identifier.ValueText, MessageFormatUnderscore);
                return;
            }

            if (identifier.ValueText.Contains("_"))
            {
                return;
            }

            if (!IsMemberNameValid(identifier.ValueText, out var suggestion))
            {
                var messageEnding = string.Format(MessageFormatNonUnderscore, suggestion);
                context.ReportIssue(MethodNameRule, identifier, context.Node.GetDeclarationTypeName(), identifier.ValueText, messageEnding);
            }
        }

        private static bool IsMemberNameValid(string identifierName, out string suggestion)
        {
            if (identifierName.Length == 1)
            {
                suggestion = identifierName.ToUpperInvariant();
                return suggestion == identifierName;
            }

            var idealNameVariant = new StringBuilder(identifierName.Length);
            var acceptableNameVariant = new StringBuilder(identifierName.Length);

            foreach (var part in SplitToParts(identifierName))
            {
                idealNameVariant.Append(SuggestFixedCaseName(part, 1));
                acceptableNameVariant.Append(SuggestFixedCaseName(part, 2));
            }

            idealNameVariant[0] = char.ToUpperInvariant(idealNameVariant[0]);
            suggestion = SuggestCapitalLetterAfterNonLetter(idealNameVariant);

            acceptableNameVariant[0] = char.ToUpperInvariant(acceptableNameVariant[0]);
            var acceptableSuggestion = SuggestCapitalLetterAfterNonLetter(acceptableNameVariant);

            return acceptableSuggestion == identifierName
                   || suggestion == identifierName;
        }

        private static bool IsTypeNameValid(string identifierName, bool requireInitialI, bool allowInitialI, bool areUnderscoresAllowed, out string suggestion)
        {
            if (identifierName.Length == 1)
            {
                suggestion = identifierName.ToUpperInvariant();
                return suggestion == identifierName;
            }

            var idealNameVariant = new StringBuilder(identifierName.Length);
            var acceptableNameVariant = new StringBuilder(identifierName.Length);

            var parts = SplitToParts(identifierName).ToList();
            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (part.Length == 1 && part[0] == '_' && !areUnderscoresAllowed)
                {
                    continue;
                }

                var ideal = i == 0
                    ? HandleFirstPartOfTypeName(part, requireInitialI, allowInitialI, 1)
                    : SuggestFixedCaseName(part, 1);

                var acceptable = i == 0
                    ? HandleFirstPartOfTypeName(part, requireInitialI, allowInitialI, 2)
                    : SuggestFixedCaseName(part, 2);

                idealNameVariant.Append(ideal);
                acceptableNameVariant.Append(acceptable);
            }

            suggestion = SuggestCapitalLetterAfterNonLetter(idealNameVariant);
            var acceptableSuggestion = SuggestCapitalLetterAfterNonLetter(acceptableNameVariant);

            return acceptableSuggestion == identifierName
                   || suggestion == identifierName;
        }

        private static string HandleFirstPartOfTypeName(string input, bool requireInitialI, bool allowInitialI, int maxUppercase)
        {
            var startsWithI = input[0] == 'I';

            if (requireInitialI)
            {
                var prefix = startsWithI ? string.Empty : "I";
                return prefix + SuggestFixedCaseName(FirstCharToUpper(input), maxUppercase + 1);
            }

            var suggestionToProcess = ShouldExcludeFirstLetter()
                ? FirstCharToUpper(input.Substring(1))
                : FirstCharToUpper(input);

            return SuggestFixedCaseName(suggestionToProcess, maxUppercase);

            bool ShouldExcludeFirstLetter() =>
                input.Length == 1
                && !allowInitialI
                && startsWithI
                && IsCharUpper(input, 0);
        }

        private static string SuggestCapitalLetterAfterNonLetter(StringBuilder suggestion)
        {
            for (var i = 1; i < suggestion.Length; i++)
            {
                if (!char.IsLetter(suggestion[i - 1])
                    && char.IsLower(suggestion[i]))
                {
                    suggestion[i] = char.ToUpperInvariant(suggestion[i]);
                }
            }

            return suggestion.ToString();
        }

        private static string SuggestFixedCaseName(string input, int maxUppercaseCount)
        {
            var upper = input.Take(maxUppercaseCount);
            var lower = input.Skip(maxUppercaseCount).Select(char.ToLowerInvariant);

            return new string(upper.Concat(lower).ToArray());
        }

        private static string FirstCharToUpper(string input) =>
            input.Length > 0
                ? char.ToUpperInvariant(input[0]) + input.Substring(1)
                : input;

        private static bool IsCharUpper(string input, int idx) =>
            idx >= 0
            && idx < input.Length
            && char.IsUpper(input[idx]);

        private static SyntaxToken GetDeclarationIdentifier(SyntaxNode declaration) =>
            declaration.Kind() switch
            {
                SyntaxKind.MethodDeclaration => ((MethodDeclarationSyntax)declaration).Identifier,
                SyntaxKind.PropertyDeclaration => ((PropertyDeclarationSyntax)declaration).Identifier,
                SyntaxKindEx.LocalFunctionStatement => ((LocalFunctionStatementSyntaxWrapper)declaration).Identifier,
                _ => throw new InvalidOperationException("Method can only be called on known registered syntax kinds")
            };

        private static bool IsTestClassName(string className) =>
            className != "ITest"
            && className != "ITests"
            && (className.EndsWith("Test") || className.EndsWith("Tests"));
    }
}
