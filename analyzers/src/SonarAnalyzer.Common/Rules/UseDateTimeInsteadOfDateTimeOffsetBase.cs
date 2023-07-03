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

public abstract class UseDateTimeInsteadOfDateTimeOffsetBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6566";

    protected override string MessageFormat => "Prefer using \"DateTimeOffset\" struct instead of \"DateTime\"";

    private static readonly ImmutableArray<string> TargetMemberAccess = ImmutableArray.Create(
            "MaxValue",
            "MinValue",
            "UnixEpoch",
            "Now",
            "Today",
            "UtcNow");

    protected abstract bool IsNamedDateTime(SyntaxNode node);

    protected UseDateTimeInsteadOfDateTimeOffsetBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(start =>
        {
            if (!IsDateTimeOffsetSupported(start.Compilation))
            {
                return;
            }

            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (IsDateTimeType(c.Node, c.SemanticModel))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                    }
                },
                Language.SyntaxKind.ObjectCreationExpressions);

            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (Language.Syntax.NodeExpression(c.Node) is var expression
                        && IsNamedDateTime(expression)
                        && TargetMemberAccess.Contains(Language.GetName(c.Node))
                        && IsDateTimeType(expression, c.SemanticModel))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                    }
                },
                Language.SyntaxKind.SimpleMemberAccessExpression);
        });

    private static bool IsDateTimeType(SyntaxNode node, SemanticModel model) =>
        model.GetTypeInfo(node).Type.Is(KnownType.System_DateTime);

    private static bool IsDateTimeOffsetSupported(Compilation compilation) =>
        compilation.GetTypeByMetadataName(KnownType.System_DateTimeOffset) is not null;
}
