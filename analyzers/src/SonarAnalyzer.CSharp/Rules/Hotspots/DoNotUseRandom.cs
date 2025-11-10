/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseRandom : HotspotDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2245";
        private const string MessageFormat = "Make sure that using this pseudorandom number generator is safe here.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public DoNotUseRandom() : base(AnalyzerConfiguration.Hotspot) { }

        public DoNotUseRandom(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                ccc =>
                {
                    if (!IsEnabled(ccc.Options))
                    {
                        return;
                    }

                    ccc.RegisterNodeAction(
                        c =>
                        {
                            var objectCreationSyntax = (ObjectCreationExpressionSyntax)c.Node;
                            var argumentsCount = objectCreationSyntax.ArgumentList?.Arguments.Count;

                            if (argumentsCount <= 1 // Random has two ctors - with zero and one parameter
                                && c.Model.GetSymbolInfo(objectCreationSyntax).Symbol is IMethodSymbol methodSymbol
                                && methodSymbol.ContainingType.Is(KnownType.System_Random))
                            {
                                c.ReportIssue(Rule, objectCreationSyntax);
                            }
                        },
                        SyntaxKind.ObjectCreationExpression);
                });
    }
}
