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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseStringCreate : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6618";
    private const string MessageFormat = """Use "string.Create" instead of "FormattableString".""";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private ImmutableArray<string> methodNames = ImmutableArray.Create(
        "CurrentCulture",
        "Invariant");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(c =>
        {
            // The string.Create method with IFormatProvider parameter is only available from .NET 6.0
            if (!c.Compilation.IsMemberAvailable<IMethodSymbol>(
                KnownType.System_String,
                "Create",
                method => method.Parameters.Any(x => KnownType.System_IFormatProvider.Matches(x.Type))))
            {
                return;
            }

            c.RegisterNodeAction(c =>
            {
                var node = (InvocationExpressionSyntax)c.Node;

                if (methodNames.Any(x => NameIsEqual(node, x))
                    && node.TryGetOperands(out var left, out _)
                    && NameIsEqual(left, nameof(FormattableString))
                    && node.HasExactlyNArguments(1)
                    && node.ArgumentList.Arguments[0].Expression is InterpolatedStringExpressionSyntax
                    && c.SemanticModel.GetTypeInfo(left).Type.Is(KnownType.System_FormattableString))
                {
                    c.ReportIssue(Rule, node.GetIdentifier()?.GetLocation());
                }
            },
            SyntaxKind.InvocationExpression);
        });

    private static bool NameIsEqual(SyntaxNode node, string name) =>
        node.GetName().Equals(name, StringComparison.Ordinal);
}
