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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StringOffsetMethods : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4635";
        private const string MessageFormat = "Replace '{0}' with the overload that accepts a startIndex parameter.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private readonly string[] methodsToCheck = new string[]
        {
            "IndexOf",
            "IndexOfAny",
            "LastIndexOf",
            "LastIndexOfAny"
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(analysisContext =>
                {
                    var invocationExpression = (InvocationExpressionSyntax)analysisContext.Node;
                    var semanticModel = analysisContext.Model;

                    if (IsTargetMethodInvocation(invocationExpression, semanticModel) &&
                        HasSubstringMethodInvocationChild(invocationExpression, semanticModel))
                    {
                        var memberAccess = (MemberAccessExpressionSyntax)invocationExpression.Expression;

                        analysisContext.ReportIssue(rule, invocationExpression, memberAccess.Name.ToString());
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private bool IsTargetMethodInvocation(InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel) =>
            methodsToCheck.Any(methodName => invocationExpression.IsMethodInvocation(KnownType.System_String, methodName, semanticModel));

        private bool HasSubstringMethodInvocationChild(InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel) =>
            invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
            memberAccessExpression.Expression is InvocationExpressionSyntax childInvocationExpression &&
            childInvocationExpression.IsMethodInvocation(KnownType.System_String, "Substring", semanticModel);

    }
}

