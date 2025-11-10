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

using SonarAnalyzer.CSharp.Walkers;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterValidationInAsyncShouldBeWrapped : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4457";
    private const string MessageFormat = "Split this method into two, one handling parameters check and the other handling the asynchronous code.";
    private const string SecondaryLocationMessage = "This ArgumentException will be raised only after observing the task.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var method = (MethodDeclarationSyntax)c.Node;
                if (!method.Modifiers.Any(SyntaxKind.AsyncKeyword)
                    || method.HasReturnTypeVoid()
                    || (method.Identifier.ValueText == "Main" && c.Model.GetDeclaredSymbol(method).IsMainMethod()))
                {
                    return;
                }

                var walker = new ParameterValidationInAsyncWalker(c.Model);
                walker.SafeVisit(method);
                if (walker.ArgumentExceptionLocations.Any())
                {
                    c.ReportIssue(Rule, method.Identifier, walker.ArgumentExceptionLocations);
                }
            },
            SyntaxKind.MethodDeclaration);

    private sealed class ParameterValidationInAsyncWalker : ParameterValidationInMethodWalker
    {
        protected override string SecondaryMessage => SecondaryLocationMessage;

        public ParameterValidationInAsyncWalker(SemanticModel model)
            : base(model)
        {
        }

        public override void VisitAwaitExpression(AwaitExpressionSyntax node) =>
            keepWalking = false;
    }
}
