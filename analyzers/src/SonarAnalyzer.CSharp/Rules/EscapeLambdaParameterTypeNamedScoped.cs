/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EscapeLambdaParameterTypeNamedScoped : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S8381";
    private const string MessageFormat = "'scoped' should be escaped when used as a type name in lambda parameters";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
            {
                if (!compilationStart.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp14))
                {
                    compilationStart.RegisterNodeAction(c =>
                        {
                            foreach (var parameter in ((ParenthesizedLambdaExpressionSyntax)c.Node).ParameterList.Parameters)
                            {
                                CheckParameter(c, parameter);
                            }
                        },
                        SyntaxKind.ParenthesizedLambdaExpression);
                }
            });

    private static void CheckParameter(SonarSyntaxNodeReportingContext c, ParameterSyntax parameter)
    {
        if (parameter.Type?.Unwrap() is SimpleNameSyntax { Identifier: { } id } && IsScopedToken(id))
        {
            c.ReportIssue(Rule, id);
        }
        else if (parameter.Type is null && IsScopedToken(parameter.Identifier))
        {
            c.ReportIssue(Rule, parameter.Identifier);
        }
    }

    private static bool IsScopedToken(SyntaxToken token) =>
        token.ValueText == "scoped" && !token.Text.StartsWith("@", StringComparison.Ordinal);
}
