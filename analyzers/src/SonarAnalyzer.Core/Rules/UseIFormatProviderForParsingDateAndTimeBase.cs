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

namespace SonarAnalyzer.Rules;

public abstract class UseIFormatProviderForParsingDateAndTimeBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6580";

    private static readonly string[] ParseMethodNames = new[]
    {
        nameof(DateTime.Parse),
        nameof(DateTime.ParseExact),
        nameof(DateTime.TryParse),
        nameof(DateTime.TryParseExact)
    };
    private static readonly KnownType[] TemporalTypes = new[]
    {
        KnownType.System_DateOnly,
        KnownType.System_DateTime,
        KnownType.System_DateTimeOffset,
        KnownType.System_TimeOnly,
        KnownType.System_TimeSpan
    };

    protected override string MessageFormat => "Use a format provider when parsing date and time.";

    protected UseIFormatProviderForParsingDateAndTimeBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if (Language.Syntax.NodeIdentifier(c.Node) is { IsMissing: false } identifier
                && Array.Exists(ParseMethodNames, x => identifier.ValueText.Equals(x, Language.NameComparison))
                && c.SemanticModel.GetSymbolInfo(c.Node) is { Symbol: IMethodSymbol methodSymbol }
                && TemporalTypes.Any(x => x.Matches(methodSymbol.ReceiverType))
                && NotUsingFormatProvider(methodSymbol, c.Node))
            {
                c.ReportIssue(Rule, c.Node);
            }
        }, Language.SyntaxKind.InvocationExpression);

    private bool NotUsingFormatProvider(IMethodSymbol methodSymbol, SyntaxNode node)
    {
        var parameterLookup = Language.MethodParameterLookup(node, methodSymbol);
        return (!parameterLookup.TryGetSyntax("provider", out var parameter)
                && !parameterLookup.TryGetSyntax("formatProvider", out parameter))
               || Language.Syntax.IsNullLiteral(parameter[0]);
    }
}
