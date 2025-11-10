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

namespace SonarAnalyzer.Core.Rules;

public abstract class UseDateTimeOffsetInsteadOfDateTimeBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6566";

    protected override string MessageFormat => "Prefer using \"DateTimeOffset\" instead of \"DateTime\"";

    private static readonly ImmutableArray<string> TargetMemberAccess = ImmutableArray.Create(
            nameof(DateTime.MaxValue),
            nameof(DateTime.MinValue),
            nameof(DateTime.Now),
            nameof(DateTime.Today),
            nameof(DateTime.UtcNow),
            "UnixEpoch");

    protected abstract string[] ValidNames { get; }

    protected UseDateTimeOffsetInsteadOfDateTimeBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if ((c.Node.RawKind == (int)SyntaxKindEx.ImplicitObjectCreationExpression || IsNamedDateTime(GetTypeName(c.Node)))
                    && IsDateTimeType(c.Node, c.Model))
                {
                    c.ReportIssue(Rule, c.Node);
                }
            },
            Language.SyntaxKind.ObjectCreationExpressions);

        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (Language.Syntax.NodeExpression(c.Node) is var expression
                    && IsNamedDateTime(Language.GetName(expression))
                    && TargetMemberAccess.Contains(Language.GetName(c.Node))
                    && IsDateTimeType(expression, c.Model))
                {
                    c.ReportIssue(Rule, Language.Syntax.NodeExpression(c.Node));
                }
            },
            Language.SyntaxKind.SimpleMemberAccessExpression);
    }

    private static bool IsDateTimeType(SyntaxNode node, SemanticModel model) =>
        model.GetTypeInfo(node).Type.Is(KnownType.System_DateTime);

    private bool IsNamedDateTime(string name) =>
        Array.Exists(ValidNames, x => x.Equals(name, Language.NameComparison));

    private string GetTypeName(SyntaxNode node) =>
        Language.Syntax.ObjectCreationTypeIdentifier(node) is { IsMissing: false } identifier
            ? identifier.ValueText
            : string.Empty;
}
