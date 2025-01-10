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
    public sealed class ExceptionRethrow : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3445";
        private const string MessageFormat = "Consider using 'throw;' to preserve the stack trace.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;
                    if (catchClause.Declaration == null ||
                        catchClause.Declaration.Identifier.IsKind(SyntaxKind.None))
                    {
                        return;
                    }

                    var exceptionIdentifier = c.SemanticModel.GetDeclaredSymbol(catchClause.Declaration);
                    if (exceptionIdentifier == null)
                    {
                        return;
                    }

                    var throws = catchClause.DescendantNodes(n =>
                            n == catchClause ||
                            !n.IsKind(SyntaxKind.CatchClause))
                        .OfType<ThrowStatementSyntax>()
                        .Where(t => t.Expression != null);

                    foreach (var @throw in throws)
                    {
                        var thrown = c.SemanticModel.GetSymbolInfo(@throw.Expression).Symbol as ILocalSymbol;
                        if (Equals(thrown, exceptionIdentifier))
                        {
                            c.ReportIssue(rule, @throw);
                        }
                    }
                },
                SyntaxKind.CatchClause);
        }
    }
}
