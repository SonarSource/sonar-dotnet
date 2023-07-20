/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules;

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
            && HasInvalidFirstArgument(invocation, analysisContext.SemanticModel) // Standard date and time format strings are 1 char long and they are allowed
            && analysisContext.SemanticModel.GetSymbolInfo(identifier.Parent).Symbol is { } methodCallSymbol
            && CheckedTypes.Any(x => methodCallSymbol.ContainingType.ConstructedFrom.Is(x)))
        {
            analysisContext.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], HardCodedArgumentLocation(invocation)));
        }
    }
}
