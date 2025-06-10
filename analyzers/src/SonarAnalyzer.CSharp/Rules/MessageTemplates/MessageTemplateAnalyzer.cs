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

using SonarAnalyzer.CSharp.Core.RegularExpressions;
using SonarAnalyzer.CSharp.Rules.MessageTemplates;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MessageTemplateAnalyzer : SonarDiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<IMessageTemplateCheck> Checks = ImmutableHashSet.Create<IMessageTemplateCheck>(
        new LoggingTemplatePlaceHoldersShouldBeInOrder(),
        new NamedPlaceholdersShouldBeUnique(),
        new UsePascalCaseForNamedPlaceHolders());

    private static readonly HashSet<SyntaxKind> StringExpressionSyntaxKinds =
    [
        SyntaxKind.StringLiteralExpression,
        SyntaxKind.AddExpression,
        SyntaxKind.InterpolatedStringExpression,
        SyntaxKind.InterpolatedStringText
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Checks.Select(x => x.Rule).ToImmutableArray();

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
                                && MessageTemplateExtractor.TemplateArgument(invocation, c.Model) is { } argument
                                && HasValidExpression(argument)
                                && MessageTemplatesParser.Parse(argument.Expression) is { Success: true } result)
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

    // Allow:
    // "regular string"
    // "concatenated " + "string"
    // condition ? "ternary" : "scenarios"
    // $"interpolated {string}"
    // Do not allow:
    // "complex" + $"interpolated {scenarios}"
    // condition ? "complex : $"interpolated {scenarios}"
    private static bool HasValidExpression(ArgumentSyntax argument) =>
        argument.Expression.IsKind(SyntaxKind.InterpolatedStringExpression)
        || argument.Expression.DescendantNodes().All(x => x.IsAnyKind(StringExpressionSyntaxKinds));
}
