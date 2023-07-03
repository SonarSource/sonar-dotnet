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

public abstract class DoNotUseDateTimeNowBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6563";

    protected override string MessageFormat => "Do not use 'DateTime.Now' for recording instants";

    protected DoNotUseDateTimeNowBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if (IsDateTimeNowOrToday(c.Node, c.SemanticModel)
                || IsDateTimeOffsetNowDateTime(c.Node, c.SemanticModel))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
        }, Language.SyntaxKind.SimpleMemberAccessExpression);

    private bool IsDateTimeNowOrToday(SyntaxNode node, SemanticModel semanticModel) =>
        MatchesAnyProperty(node, semanticModel, KnownType.System_DateTime, nameof(DateTime.Now), nameof(DateTime.Today));

    private bool IsDateTimeOffsetNowDateTime(SyntaxNode node, SemanticModel semanticModel) =>
        MatchesAnyProperty(node, semanticModel, KnownType.System_DateTimeOffset, nameof(DateTimeOffset.DateTime), nameof(DateTimeOffset.Date))
        && MatchesAnyProperty(Language.Syntax.NodeExpression(node), semanticModel, KnownType.System_DateTimeOffset, nameof(DateTimeOffset.Now));

    private bool MatchesAnyProperty(SyntaxNode node, SemanticModel semanticModel, KnownType containingType, params string[] propertyNames) =>
        Language.Syntax.NodeIdentifier(node) is { IsMissing: false } identifier
        && propertyNames.Any(x => identifier.ValueText.Equals(x, Language.NameComparison))
        && semanticModel.GetSymbolInfo(node) is { Symbol: IPropertySymbol propertySymbol }
        && containingType.Matches(propertySymbol.ContainingType);
}
