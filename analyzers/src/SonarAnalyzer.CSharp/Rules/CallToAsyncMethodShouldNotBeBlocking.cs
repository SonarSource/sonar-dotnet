/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    public sealed class CallToAsyncMethodShouldNotBeBlocking : SonarDiagnosticAnalyzer
    {
        private const string MessageFormat = "Replace this use of '{0}' with '{1}'.";
        private const string ResultName = "Result";
        private const string ContinueWithName = "ContinueWith";
        private const string SleepName = "Sleep";
        private const string AzureFunctionSuffix = @" Do not perform blocking operations in Azure Functions.";

        private static readonly DiagnosticDescriptor RuleS4462 = DescriptorFactory.Create("S4462", MessageFormat);
        private static readonly DiagnosticDescriptor RuleS6422 = DescriptorFactory.Create("S6422", MessageFormat + AzureFunctionSuffix);

        private static readonly Dictionary<string, ImmutableArray<KnownType>> InvalidMemberAccess = new()
        {
            ["GetResult"] = ImmutableArray.Create(KnownType.System_Runtime_CompilerServices_TaskAwaiter, KnownType.System_Runtime_CompilerServices_TaskAwaiter_TResult),
            [ResultName] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task_T),
            [SleepName] = ImmutableArray.Create(KnownType.System_Threading_Thread),
            ["Wait"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task),
            ["WaitAll"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task),
            ["WaitAny"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task),
        };

        private static readonly Dictionary<string, string[]> MemberNameToMessageArguments = new()
        {
            ["GetResult"] = new[] { "Task.GetAwaiter.GetResult", "await" },
            [ResultName] = new[] { "Task.Result", "await" },
            [SleepName] = new[] { "Thread.Sleep", "await Task.Delay" },
            ["Wait"] = new[] { "Task.Wait", "await" },
            ["WaitAll"] = new[] { "Task.WaitAll", "await Task.WhenAll" },
            ["WaitAny"] = new[] { "Task.WaitAny", "await Task.WhenAny" },
        };

        private static readonly Dictionary<string, KnownType> TaskThreadPoolCalls = new()
        {
            ["StartNew"] = KnownType.System_Threading_Tasks_TaskFactory,
            ["Run"] = KnownType.System_Threading_Tasks_Task,
        };

        private static readonly Dictionary<string, KnownType> WaitForMultipleTasksExecutionCalls = new()
        {
            ["WhenAll"] = KnownType.System_Threading_Tasks_Task,
            ["WaitAll"] = KnownType.System_Threading_Tasks_Task,
        };

        private static readonly Dictionary<string, KnownType> WaitForSingleExecutionCalls = new()
        {
            ["Wait"] = KnownType.System_Threading_Tasks_Task,
            ["RunSynchronously"] = KnownType.System_Threading_Tasks_Task,
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(RuleS4462, RuleS6422);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(ReportOnViolation, SyntaxKind.SimpleMemberAccessExpression);

        private static void ReportOnViolation(SonarSyntaxNodeReportingContext context)
        {
            var simpleMemberAccess = (MemberAccessExpressionSyntax)context.Node;
            var memberAccessNameName = simpleMemberAccess.GetName();

            if (memberAccessNameName == null
                || !InvalidMemberAccess.ContainsKey(memberAccessNameName)
                || IsResultInContinueWithCall(memberAccessNameName, simpleMemberAccess)
                || IsChainedAfterThreadPoolCall(context.Model, simpleMemberAccess)
                || simpleMemberAccess.IsInNameOfArgument(context.Model)
                || simpleMemberAccess.HasAncestor(SyntaxKind.GlobalStatement))
            {
                return;
            }

            var possibleMemberAccesses = InvalidMemberAccess[memberAccessNameName];
            var memberAccessSymbol = context.Model.GetSymbolInfo(simpleMemberAccess).Symbol;
            if (memberAccessSymbol?.ContainingType == null || !memberAccessSymbol.ContainingType.ConstructedFrom.IsAny(possibleMemberAccesses))
            {
                return;
            }
            if (simpleMemberAccess.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>() is { } enclosingMethod)
            {
                if (memberAccessNameName == SleepName && !enclosingMethod.Modifiers.Any(SyntaxKind.AsyncKeyword))
                {
                    return; // Thread.Sleep should not be used only in async methods
                }
                if (context.Model.GetDeclaredSymbol(enclosingMethod).IsMainMethod())
                {
                    return; // Main methods are not subject to deadlock issue so no need to report an issue
                }
                if (memberAccessNameName == ResultName && IsAwaited(context, simpleMemberAccess))
                {
                    return;  // No need to report an issue on a waited object
                }
            }
            context.ReportIssue(context.IsAzureFunction() ? RuleS6422 : RuleS4462, simpleMemberAccess, MemberNameToMessageArguments[memberAccessNameName]);
        }

        private static bool IsAwaited(SonarSyntaxNodeReportingContext context, MemberAccessExpressionSyntax simpleMemberAccess)
        {
            return context.Model.GetSymbolInfo(simpleMemberAccess.Expression).Symbol is { } accessedSymbol
                   && simpleMemberAccess.FirstAncestorOrSelf<StatementSyntax>() is { } currentStatement
                   && currentStatement.GetPreviousStatements().Any(ContainsAwaitedInvocation);

            bool ContainsAwaitedInvocation(StatementSyntax statement) =>
                statement.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(x => x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression)).Any(IsTaskAwaited);

            bool IsTaskAwaited(InvocationExpressionSyntax invocation) =>
                IsAwaitForMultipleTasksExecutionCall(context.Model, invocation, accessedSymbol)
                || IsAwaitForSingleTaskExecutionCall(context.Model, invocation, accessedSymbol);
        }

        private static bool IsAwaitForMultipleTasksExecutionCall(SemanticModel model, InvocationExpressionSyntax invocation, ISymbol accessedSymbol) =>
            IsNamedSymbolOfExpectedType(model, (MemberAccessExpressionSyntax)invocation.Expression, WaitForMultipleTasksExecutionCalls)
            && invocation.ArgumentList.Arguments.Any(x => Equals(accessedSymbol, model.GetSymbolInfo(x.Expression).Symbol));

        private static bool IsAwaitForSingleTaskExecutionCall(SemanticModel model, InvocationExpressionSyntax invocation, ISymbol accessedSymbol) =>
            IsNamedSymbolOfExpectedType(model, (MemberAccessExpressionSyntax)invocation.Expression, WaitForSingleExecutionCalls)
            && Equals(accessedSymbol, model.GetSymbolInfo(((MemberAccessExpressionSyntax)invocation.Expression).Expression).Symbol);

        private static bool IsResultInContinueWithCall(string memberAccessName, MemberAccessExpressionSyntax memberAccess) =>
            memberAccessName == ResultName
            && memberAccess.Expression is IdentifierNameSyntax identifierNameSyntax
            && identifierNameSyntax.GetName() is { } identifierName
            && memberAccess.FirstAncestorOrSelf<InvocationExpressionSyntax>(x => IsContinueWithCallWithArgumentName(x, identifierName)) is not null;

        private static bool IsContinueWithCallWithArgumentName(InvocationExpressionSyntax invocation, string argumentName) =>
            invocation.Expression.NameIs(ContinueWithName)
            && invocation.ArgumentList.Arguments.Any(argument => IsLambdaExpressionWithArgumentName(argument.Expression, argumentName));

        private static bool IsLambdaExpressionWithArgumentName(ExpressionSyntax expression, string argumentName) =>
            expression switch
            {
                SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Parameter.Identifier.ValueText == argumentName,
                ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda.ParameterList.Parameters.Any(parameter => parameter.Identifier.ValueText == argumentName),
                _ => false
            };

        private static bool IsChainedAfterThreadPoolCall(SemanticModel model, MemberAccessExpressionSyntax memberAccess) =>
            memberAccess.Expression.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Any(x => IsNamedSymbolOfExpectedType(model, x, TaskThreadPoolCalls));

        private static bool IsNamedSymbolOfExpectedType(SemanticModel model, MemberAccessExpressionSyntax memberAccess, Dictionary<string, KnownType> expectedTypes) =>
            expectedTypes.Keys.Any(memberAccess.NameIs)
            && model.GetSymbolInfo(memberAccess).Symbol?.ContainingType?.ConstructedFrom is { } memberAccessSymbol
            && memberAccessSymbol.Is(expectedTypes[memberAccess.Name.Identifier.ValueText]);
    }
}
