/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseRegexSafeIsMatch : StylingAnalyzer
{
    public UseRegexSafeIsMatch() : base("T0004", "Use '{0}{1}' instead.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(c =>
        {
            var regexExtensions = c.Compilation.GetSymbolsWithName("RegexExtensions", SymbolFilter.Type).OfType<INamedTypeSymbol>().ToArray();
            Verify(c, "IsMatch", "SafeIsMatch", regexExtensions);
            Verify(c, "Match", "SafeMatch", regexExtensions);
            Verify(c, "Matches", "SafeMatches", regexExtensions);
        });

    private void Verify(
        SonarCompilationStartAnalysisContext context,
        string methodName,
        string replacementName,
        IEnumerable<INamedTypeSymbol> regexExtensions)
    {
        if (regexExtensions.Any(x => x.GetMembers(replacementName).Any()))
        {
            context.RegisterNodeAction(c =>
            {
                var memberAccess = (MemberAccessExpressionSyntax)c.Node;
                if (memberAccess.NameIs(methodName)
                    && c.Model.GetSymbolInfo(memberAccess).Symbol is IMethodSymbol method
                    && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex))
                {
                    c.ReportIssue(
                        Rule,
                        method.IsStatic ? memberAccess.Expression : memberAccess.Name,
                        method.IsStatic ? "SafeRegex." : "Safe",
                        method.Name);
                }
            },
            SyntaxKind.SimpleMemberAccessExpression);
        }
    }
}
