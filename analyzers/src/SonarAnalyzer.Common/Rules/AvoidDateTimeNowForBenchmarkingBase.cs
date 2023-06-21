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

public abstract class AvoidDateTimeNowForBenchmarkingBase<TMemberAccess, TBinaryExpression, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TMemberAccess : SyntaxNode
    where TBinaryExpression : SyntaxNode
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6561";
    private const string Now = "Now";
    protected override string MessageFormat => "Avoid using \"DateTime.Now\" for benchmarking or timing operations";

    protected abstract SyntaxNode GetLeftNode(TBinaryExpression binaryExpression);

    protected AvoidDateTimeNowForBenchmarkingBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                var binaryExpression = (TBinaryExpression)c.Node;

                if (GetLeftNode(binaryExpression) is TMemberAccess memberAccess
                    && Language.Syntax.IsMemberAccessOnKnownType(memberAccess, Now, KnownType.System_DateTime, c.SemanticModel))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            },
            Language.SyntaxKind.SubtractExpression);
}
