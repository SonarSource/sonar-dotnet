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

public abstract class ArrayPassedAsParamsBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S3878";
    protected override string MessageFormat => "Arrays should not be created for {0} parameters.";

    private readonly DiagnosticDescriptor rule;
    protected abstract string ParameterKeyword { get; }
    protected abstract bool ShouldReport(SonarSyntaxNodeReportingContext context, SyntaxNode expression);
    protected abstract Location GetLocation(SyntaxNode expression);

    protected ArrayPassedAsParamsBase() : base(DiagnosticId) =>
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

    protected sealed override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckExpression, Language.SyntaxKind.ObjectCreationExpressions);
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckExpression, Language.SyntaxKind.InvocationExpression);
    }

    private void CheckExpression(SonarSyntaxNodeReportingContext context)
    {
        if (context.Node is { } expression && ShouldReport(context, expression))
        {
            Report(context, GetLocation(expression));
        }
    }

    private void Report(SonarSyntaxNodeReportingContext context, Location location) =>
        context.ReportIssue(Diagnostic.Create(rule, location, ParameterKeyword));
}
