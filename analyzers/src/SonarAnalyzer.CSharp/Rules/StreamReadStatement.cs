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

using System.IO;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StreamReadStatement : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2674";
    private const string MessageFormat = "Check the return value of the '{0}' call to see how many bytes were read.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ISet<string> ReadMethodNames = new HashSet<string>(StringComparer.Ordinal)
    {
        nameof(Stream.Read),
        nameof(Stream.ReadAsync),
        "ReadAtLeast", // Net7: https://learn.microsoft.com/dotnet/api/system.io.stream.readatleast#applies-to
        "ReadAtLeastAsync",
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var statement = (ExpressionStatementSyntax)c.Node;
                var expression = statement.Expression switch
                {
                    AwaitExpressionSyntax awaitExpression => awaitExpression.AwaitedExpressionWithoutConfigureAwait(),
                    var x => x,
                };
                expression = expression.RemoveConditionalAccess();
                if (expression is InvocationExpressionSyntax invocation
                    && invocation.GetMethodCallIdentifier() is { } methodIdentifier
                    && ReadMethodNames.Contains(methodIdentifier.Text)
                    && c.Model.GetSymbolInfo(expression).Symbol is IMethodSymbol method
                    && (method.ContainingType.Is(KnownType.System_IO_Stream)
                        || (method.IsOverride && method.ContainingType.DerivesOrImplements(KnownType.System_IO_Stream))))
                {
                    c.ReportIssue(Rule, expression, method.Name);
                }
            },
            SyntaxKind.ExpressionStatement);
}
