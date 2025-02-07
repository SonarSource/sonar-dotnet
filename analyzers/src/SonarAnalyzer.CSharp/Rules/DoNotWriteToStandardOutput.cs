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
public class DoNotWriteToStandardOutput : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S106";
    private const string MessageFormat = "Remove this logging statement.";

    private static readonly string[] BannedConsoleMembers = ["WriteLine", "Write"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected static DiagnosticDescriptor Rule =>
        DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                if (c.Compilation.Options.OutputKind != OutputKind.ConsoleApplication
                    && c.Model.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol method
                    && method.IsAny(KnownType.System_Console, BannedConsoleMembers)
                    && !c.Node.IsInDebugBlock()
                    && !invocation.IsInConditionalDebug(c.Model))
                {
                    c.ReportIssue(Rule, invocation.Expression);
                }
            },
            SyntaxKind.InvocationExpression);
}
