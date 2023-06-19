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

public abstract class UseDateOnlyTimeOnlyBase<TSyntaxKind, TLiteralExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TLiteralExpression : SyntaxNode
{
    private const string DiagnosticId = "S6576";
    private const string DateOnly = nameof(DateOnly);
    private const string TimeOnly = nameof(TimeOnly);
    private const string Date = "date";
    private const string Time = "time";

    protected override string MessageFormat => "Use \"{0}\" instead of just setting the {1} for a \"DateTime\" struct"; // DateOnly TimeOnly / date time

    protected abstract bool IsEqualToOne(TLiteralExpression expression);

    protected UseDateOnlyTimeOnlyBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (IsCandidateCtor(c.Node, c.SemanticModel, out var type, out var dateOrTime))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation(), type, dateOrTime));
                }
            },
            Language.SyntaxKind.ObjectCreationExpressions);

    private bool IsCandidateCtor(SyntaxNode ctorNode, SemanticModel model, out string type, out string dateOrTime)
    {
        var argumentCount = Language.Syntax.ArgumentExpressions(ctorNode).Count();

        if (argumentCount is 3 && Language.Syntax.ArgumentExpressions(ctorNode).All(x => x is TLiteralExpression))
        {
            type = DateOnly;
            dateOrTime = Date;
            return true;
        }
        else if (argumentCount is 6 or 7 or 8)
        {
            var methodSymbol = (IMethodSymbol)model.GetSymbolInfo(ctorNode).Symbol;

            if (methodSymbol.Parameters.All(x => x.Type.Is(KnownType.System_Int32)) && IsYearMonthDayEqualsOne(ctorNode, methodSymbol))
            {
                type = TimeOnly;
                dateOrTime = Time;
                return true;
            }
        }

        type = string.Empty;
        dateOrTime = string.Empty;
        return false;
    }

    private bool IsYearMonthDayEqualsOne(SyntaxNode node, IMethodSymbol methodSymbol) =>
        Language.MethodParameterLookup(node, methodSymbol) is var lookup
        && IsParameterEqualOne("year", lookup)
        && IsParameterEqualOne("month", lookup)
        && IsParameterEqualOne("day", lookup);

    private bool IsParameterEqualOne(string parameterName, IMethodParameterLookup lookup) =>
        lookup.TryGetSyntax(parameterName, out var expressions)
        && expressions[0] is TLiteralExpression literal
        && IsEqualToOne(literal);
}
