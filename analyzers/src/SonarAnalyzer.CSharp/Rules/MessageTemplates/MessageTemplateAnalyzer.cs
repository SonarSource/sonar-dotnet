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

using SonarAnalyzer.Rules.MessageTemplates;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MessageTemplateAnalyzer : SonarDiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<SyntaxKind> ValidTemplateKinds = ImmutableHashSet.Create(
        SyntaxKind.StringLiteralExpression,
        SyntaxKind.AddExpression,
        SyntaxKind.InterpolatedStringExpression,
        SyntaxKind.InterpolatedStringText);

    private static readonly ImmutableHashSet<IMessageTemplateCheck> Checks = ImmutableHashSet.Create<IMessageTemplateCheck>(
               new LoggingTemplatePlaceHoldersShouldBeInOrder(),
               new NamedPlaceholdersShouldBeUnique(),
               new UsePascalCaseForNamedPlaceHolders());

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        Checks.Select(x => x.Rule).ToImmutableArray();

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            if (cc.Compilation.ReferencesAny(KnownAssembly.MicrosoftExtensionsLoggingAbstractions, KnownAssembly.Serilog, KnownAssembly.NLog))
            {
                cc.RegisterNodeAction(c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var enabledChecks = Checks.Where(x => x.Rule.IsEnabled(c)).ToArray();
                    if (enabledChecks.Length > 0
                        && MessageTemplateExtractor.TemplateArgument(invocation, c.SemanticModel) is { } argument
                        && HasValidExpression(argument)
                        && MessageTemplatesParser.Parse(argument.Expression.ToString()) is { Success: true } result)
                    {
                        foreach (var check in enabledChecks)
                        {
                            check.Execute(c, invocation, argument, result.Placeholders);
                        }
                    }
                },
                SyntaxKind.InvocationExpression);
            }
        });

    private static bool HasValidExpression(ArgumentSyntax argument) =>
        argument.Expression.DescendantNodes().All(x => x.IsAnyKind(ValidTemplateKinds));
}
