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

using SonarAnalyzer.CSharp.Walkers;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterValidationInYieldShouldBeWrapped : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4456";
    private const string MessageFormat = "Split this method into two, one handling parameters check and the other handling the iterator.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var methodDeclaration = (MethodDeclarationSyntax)c.Node;

                var walker = new ParameterValidationInYieldWalker(c.Model);
                walker.SafeVisit(methodDeclaration);

                if (walker.HasYieldStatement &&
                    walker.ArgumentExceptionLocations.Any())
                {
                    c.ReportIssue(Rule, methodDeclaration.Identifier, walker.ArgumentExceptionLocations);
                }
            },
            SyntaxKind.MethodDeclaration);

    private sealed class ParameterValidationInYieldWalker : ParameterValidationInMethodWalker
    {
        public bool HasYieldStatement { get; private set; }

        public ParameterValidationInYieldWalker(SemanticModel model)
            : base(model)
        {
        }

        public override void VisitYieldStatement(YieldStatementSyntax node)
        {
            HasYieldStatement = true;
            base.VisitYieldStatement(node);
        }
    }
}
