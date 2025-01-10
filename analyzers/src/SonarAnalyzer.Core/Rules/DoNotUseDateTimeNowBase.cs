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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Rules;

public abstract class DoNotUseDateTimeNowBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6563";

    private static readonly MemberDescriptor DateTimeNow = new(KnownType.System_DateTime, nameof(DateTime.Now));
    private static readonly MemberDescriptor DateTimeToday = new(KnownType.System_DateTime, nameof(DateTime.Today));
    private static readonly MemberDescriptor DateTimeOffsetNow = new(KnownType.System_DateTimeOffset, nameof(DateTimeOffset.Now));
    private static readonly MemberDescriptor DateTimeOffsetDate = new(KnownType.System_DateTimeOffset, nameof(DateTimeOffset.Date));
    private static readonly MemberDescriptor DateTimeOffsetDateTime = new(KnownType.System_DateTimeOffset, nameof(DateTimeOffset.DateTime));

    protected override string MessageFormat => "Use UTC when recording DateTime instants";

    protected abstract bool IsInsideNameOf(SyntaxNode node);

    protected DoNotUseDateTimeNowBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if ((IsDateTimeNowOrToday(c.Node, c.SemanticModel) || IsDateTimeOffsetNowDateTime(c.Node, c.SemanticModel))
                && !IsInsideNameOf(c.Node))
                {
                    c.ReportIssue(Rule, c.Node);
                }
        }, Language.SyntaxKind.SimpleMemberAccessExpression);

    private bool IsDateTimeNowOrToday(SyntaxNode node, SemanticModel semanticModel) =>
        MatchesAnyProperty(node, semanticModel, DateTimeNow, DateTimeToday);

    private bool IsDateTimeOffsetNowDateTime(SyntaxNode node, SemanticModel semanticModel) =>
        MatchesAnyProperty(node, semanticModel, DateTimeOffsetDateTime, DateTimeOffsetDate)
        && MatchesAnyProperty(Language.Syntax.NodeExpression(node), semanticModel, DateTimeOffsetNow);

    private bool MatchesAnyProperty(SyntaxNode node, SemanticModel semanticModel, params MemberDescriptor[] members) =>
        Language.Syntax.NodeIdentifier(node) is { IsMissing: false } identifier
        && Array.Exists(members, x => MemberDescriptor.MatchesAny(identifier.ValueText, new Lazy<ISymbol>(() => semanticModel.GetSymbolInfo(node).Symbol), false, Language.NameComparison, members));
}
