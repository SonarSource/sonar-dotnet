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

using System.Text.RegularExpressions;
using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Rules;

public abstract class DebuggerDisplayUsesExistingMembersBase<TAttributeArgumentSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TAttributeArgumentSyntax : SyntaxNode
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S4545";

    private static readonly ArgumentDescriptor ConstructorDescriptor = ArgumentDescriptor.AttributeArgument(KnownType.System_Diagnostics_DebuggerDisplayAttribute, "value", 0);
    private static readonly ArgumentDescriptor NameDescriptor = ArgumentDescriptor.AttributeProperty(KnownType.System_Diagnostics_DebuggerDisplayAttribute, nameof(DebuggerDisplayAttribute.Name));
    private static readonly ArgumentDescriptor TypeDescriptor = ArgumentDescriptor.AttributeProperty(KnownType.System_Diagnostics_DebuggerDisplayAttribute, nameof(DebuggerDisplayAttribute.Type));

    // Source of the regex: https://stackoverflow.com/questions/546433/regular-expression-to-match-balanced-parentheses#comment120990638_35271017
    private static readonly Regex EvaluatedExpressionRegex = new(@"\{(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!))\}", RegexOptions.None, Constants.DefaultRegexTimeout);
    // Remove the "nq" (no quotes) modifier, others like "d" or "raw" are undocumented. We allow any word here.
    private static readonly Regex RemoveNqModifierRegex = new(@"\s*,\s*[a-zA-Z]*\s*$", RegexOptions.None, Constants.DefaultRegexTimeout);
    protected abstract SyntaxNode AttributeTarget(TAttributeArgumentSyntax attribute);
    protected abstract ImmutableArray<SyntaxNode> ResolvableIdentifiers(SyntaxNode expression);

    protected override string MessageFormat => "{0}";

    protected DebuggerDisplayUsesExistingMembersBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
            c =>
            {
                var attributeArgument = (TAttributeArgumentSyntax)c.Node;
                var trackingContext = new ArgumentContext(attributeArgument, c.Model);
                var argumentMatcher = Language.Tracker.Argument;
                if (argumentMatcher.Or(
                    argumentMatcher.MatchArgument(ConstructorDescriptor),
                    argumentMatcher.MatchArgument(NameDescriptor),
                    argumentMatcher.MatchArgument(TypeDescriptor))(trackingContext)
                    && Language.Syntax.NodeExpression(attributeArgument) is { } formatString
                    && Language.FindConstantValue(c.Model, formatString) is string formatStringText
                    && FirstInvalidExpression(c, formatStringText, attributeArgument) is { } firstInvalidMember)
                {
                    c.ReportIssue(Rule, formatString, firstInvalidMember);
                }
            },
            Language.SyntaxKind.AttributeArgument);

    private string FirstInvalidExpression(SonarSyntaxNodeReportingContext context, string formatString, TAttributeArgumentSyntax attributeSyntax)
    {
        return (AttributeTarget(attributeSyntax) is { } targetSyntax
            && context.Model.GetDeclaredSymbol(targetSyntax) is { } targetSymbol
            && TypeContainingReferencedMembers(targetSymbol) is { } typeSymbol)
                ? FirstInvalidMemberName(typeSymbol)
                : null;

        string FirstInvalidMemberName(ITypeSymbol typeSymbol)
        {
            var allMembers = typeSymbol
                .GetSelfAndBaseTypes()
                .SelectMany(x => x.GetMembers())
                .Select(x => x.Name)
                .ToHashSet(Language.NameComparer);

            foreach (Match match in EvaluatedExpressionRegex.SafeMatches(formatString))
            {
                if (match is { Success: true, Value: var evaluatedExpression }
                    && ParseExpression(evaluatedExpression) is { } parsedExpression
                    && CheckParsedExpression(allMembers, parsedExpression, evaluatedExpression) is { } message)
                {
                    return message;
                }
            }
            return null;
        }

        string CheckParsedExpression(HashSet<string> allMembers, SyntaxNode parsedExpression, string evaluatedExpression)
        {
            if (parsedExpression.ContainsDiagnostics
                && parsedExpression.GetDiagnostics() is { } diagnostics
                && diagnostics.FirstOrDefault(x => x.Severity == DiagnosticSeverity.Error) is { } firstError)
            {
                return $"""'{evaluatedExpression}' is not a valid expression. {firstError.Id}: {firstError.GetMessage()}.""";
            }
            if (ResolvableIdentifiers(parsedExpression).Select(Language.GetName).ToImmutableHashSet(Language.NameComparer) is { } resolvableNames
                && resolvableNames.Except(allMembers) is { Count: > 0 } unresolvableNames)
            {
                return $"""'{unresolvableNames.First()}' doesn't exist in this context.""";
            }
            return null;
        }

        SyntaxNode ParseExpression(string evaluatedExpression)
        {
            var removeBraces = evaluatedExpression.TrimStart('{').TrimEnd('}').Trim();
            var sanitizedExpression = RemoveNqModifierRegex.SafeReplace(removeBraces, string.Empty);
            return Language.Syntax.ParseExpression(sanitizedExpression);
        }

        static ITypeSymbol TypeContainingReferencedMembers(ISymbol symbol) =>
            symbol is ITypeSymbol typeSymbol ? typeSymbol : symbol.ContainingType;
    }
}
