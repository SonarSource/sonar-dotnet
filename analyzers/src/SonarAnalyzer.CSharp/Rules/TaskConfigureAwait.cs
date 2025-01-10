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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TaskConfigureAwait : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3216";
        private const string MessageFormat = "Add '.ConfigureAwait(false)' to this call to allow execution to continue in any thread.";

        private static readonly DiagnosticDescriptor rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    if (c.Compilation.Options.OutputKind != OutputKind.DynamicallyLinkedLibrary
                        || !c.Compilation.IsNetFrameworkTarget())
                    {
                        // This rule only makes sense in libraries under .NET Framework
                        return;
                    }

                    if (((AwaitExpressionSyntax)c.Node).Expression is { } expression
                        && c.SemanticModel.GetTypeInfo(expression).Type is { } type
                        && type.DerivesFrom(KnownType.System_Threading_Tasks_Task))
                    {
                        c.ReportIssue(rule, expression);
                    }
                },
                SyntaxKind.AwaitExpression);
    }
}
