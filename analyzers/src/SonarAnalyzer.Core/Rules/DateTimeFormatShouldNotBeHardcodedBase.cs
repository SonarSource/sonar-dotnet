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

namespace SonarAnalyzer.Core.Rules;

public abstract class DateTimeFormatShouldNotBeHardcodedBase<TSyntaxKind, TInvocation> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocation : SyntaxNode
{
    private const string DiagnosticId = "S6585";

    protected abstract Location HardCodedArgumentLocation(TInvocation invocation);
    protected abstract bool HasInvalidFirstArgument(TInvocation invocation, SemanticModel semanticModel);

    protected override string MessageFormat => "Do not hardcode the format specifier.";

    protected IEnumerable<KnownType> CheckedTypes { get; } = new List<KnownType>
        {
            KnownType.System_DateTime,
            KnownType.System_DateTimeOffset,
            KnownType.System_DateOnly,
            KnownType.System_TimeOnly,
            KnownType.System_TimeSpan,
        };

    protected DateTimeFormatShouldNotBeHardcodedBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, AnalyzeInvocation, Language.SyntaxKind.InvocationExpression);

    private void AnalyzeInvocation(SonarSyntaxNodeReportingContext analysisContext)
    {
        if ((TInvocation)analysisContext.Node is var invocation
            && Language.Syntax.InvocationIdentifier(invocation) is { } identifier
            && identifier.ValueText.Equals("ToString", Language.NameComparison)
            && HasInvalidFirstArgument(invocation, analysisContext.Model) // Standard date and time format strings are 1 char long and they are allowed
            && analysisContext.Model.GetSymbolInfo(identifier.Parent).Symbol is { } methodCallSymbol
            && CheckedTypes.Any(x => methodCallSymbol.ContainingType.ConstructedFrom.Is(x)))
        {
            analysisContext.ReportIssue(SupportedDiagnostics[0], HardCodedArgumentLocation(invocation));
        }
    }
}
