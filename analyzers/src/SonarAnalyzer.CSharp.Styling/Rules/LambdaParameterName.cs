/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp.Styling;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LambdaParameterName : StylingAnalyzer
{
    public LambdaParameterName() : base("T0010", "Use 'x' for the lambda parameter name.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var parameter = ((SimpleLambdaExpressionSyntax)c.Node).Parameter;
                if (parameter.Identifier.ValueText is not ("x" or "_")
                    && c.Node.Parent.FirstAncestorOrSelf<LambdaExpressionSyntax>() is null
                    && !IsSonarContextAction(c))
                {
                    c.ReportIssue(Rule, parameter);
                }
            },
            SyntaxKind.SimpleLambdaExpression);

    private static bool IsSonarContextAction(SonarSyntaxNodeReportingContext context) =>
        context.SemanticModel.GetSymbolInfo(context.Node).Symbol is IMethodSymbol lambda
        && lambda.ReturnsVoid
        && IsSonarContextName(lambda.Parameters.Single().Type.Name);

    private static bool IsSonarContextName(string name) =>
        name.StartsWith("Sonar") && (name.EndsWith("AnalysisContext") || name.EndsWith("ReportingContext"));
}
