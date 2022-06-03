/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CallToAsyncMethodShouldNotBeBlocking : SonarDiagnosticAnalyzer
    {
        private const string MessageFormat = "Replace this use of '{0}' with '{1}'.";
        private const string ResultName = "Result";
        private const string ContinueWithName = "ContinueWith";
        private const string SleepName = "Sleep";
        private const string AzureFunctionSuffix = @" Calls to ""async"" methods should not be blocking in Azure Functions.";

        private static readonly DiagnosticDescriptor RuleS4462 = DiagnosticDescriptorBuilder.GetDescriptor("S4462", MessageFormat, RspecStrings.ResourceManager);
        private static readonly DiagnosticDescriptor RuleS6422 = DiagnosticDescriptorBuilder.GetDescriptor("S6422", MessageFormat + AzureFunctionSuffix, RspecStrings.ResourceManager);

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
            context.RegisterSyntaxNodeActionInNonGenerated(ReportOnViolation, SyntaxKind.SimpleMemberAccessExpression);

        private static void ReportOnViolation(SyntaxNodeAnalysisContext context)
        {
            var simpleMemberAccess = (MemberAccessExpressionSyntax)context.Node;
            var memberAccessNameName = simpleMemberAccess.GetName();

            if (memberAccessNameName == null
                || !InvalidMemberAccess.ContainsKey(memberAccessNameName)
                || IsResultInContinueWithCall(memberAccessNameName, simpleMemberAccess)
                || IsChainedAfterThreadPoolCall(simpleMemberAccess, context.SemanticModel)
                || simpleMemberAccess.IsInNameOfArgument(context.SemanticModel)
                || simpleMemberAccess.Ancestors().Any(x => x is GlobalStatementSyntax))
            {
                return;
            }

            var possibleMemberAccesses = InvalidMemberAccess[memberAccessNameName];
            var memberAccessSymbol = context.SemanticModel.GetSymbolInfo(simpleMemberAccess).Symbol;
            if (memberAccessSymbol?.ContainingType is not null
                && memberAccessSymbol.ContainingType.ConstructedFrom.IsAny(possibleMemberAccesses)
                && simpleMemberAccess.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>() is { } enclosingMethod)
            {
                if (memberAccessNameName == SleepName && !enclosingMethod.Modifiers.Any(SyntaxKind.AsyncKeyword))
                {
                    return; // Thread.Sleep should not be used only in async methods
                }
                if (context.SemanticModel.GetDeclaredSymbol(enclosingMethod).IsMainMethod())
                {
                    return; // Main methods are not subject to deadlock issue so no need to report an issue
                }
                if (memberAccessNameName == ResultName && IsAwaited(context, simpleMemberAccess))
                {
                    return;  // No need to report an issue on a waited object
                }
            }
            context.ReportIssue(Diagnostic.Create(context.IsAzureFunction() ? RuleS6422 : RuleS4462, simpleMemberAccess.GetLocation(), MemberNameToMessageArguments[memberAccessNameName]));
        }

        private static bool IsAwaited(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax simpleMemberAccess)
        {
            return context.SemanticModel.GetSymbolInfo(simpleMemberAccess.Expression).Symbol is { } accessedSymbol
                   && simpleMemberAccess.FirstAncestorOrSelf<StatementSyntax>() is { } currentStatement
                   && currentStatement.GetPreviousStatements().Any(ContainsAwaitedInvocation);

            bool ContainsAwaitedInvocation(StatementSyntax statement) =>
                statement.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(x => x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression)).Any(IsTaskAwaited);

            bool IsTaskAwaited(InvocationExpressionSyntax invocation) =>
                IsAwaitForMultipleTasksExecutionCall(invocation, context.SemanticModel, accessedSymbol)
                || IsAwaitForSingleTaskExecutionCall(invocation, context.SemanticModel, accessedSymbol);
        }

        private static bool IsAwaitForMultipleTasksExecutionCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, ISymbol accessedSymbol) =>
            IsNamedSymbolOfExpectedType((MemberAccessExpressionSyntax)invocation.Expression, semanticModel, WaitForMultipleTasksExecutionCalls)
            && invocation.ArgumentList.Arguments.Any(x => Equals(accessedSymbol, semanticModel.GetSymbolInfo(x.Expression).Symbol));

        private static bool IsAwaitForSingleTaskExecutionCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, ISymbol accessedSymbol) =>
            IsNamedSymbolOfExpectedType((MemberAccessExpressionSyntax)invocation.Expression, semanticModel, WaitForSingleExecutionCalls)
            && Equals(accessedSymbol, semanticModel.GetSymbolInfo(((MemberAccessExpressionSyntax)invocation.Expression).Expression).Symbol);

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

        private static bool IsChainedAfterThreadPoolCall(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
            memberAccess.Expression.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Any(x => IsNamedSymbolOfExpectedType(x, semanticModel, TaskThreadPoolCalls));

        private static bool IsNamedSymbolOfExpectedType(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, Dictionary<string, KnownType> expectedTypes) =>
            expectedTypes.Keys.Any(memberAccess.NameIs)
            && semanticModel.GetSymbolInfo(memberAccess).Symbol?.ContainingType?.ConstructedFrom is { } memberAccessSymbol
            && memberAccessSymbol.Is(expectedTypes[memberAccess.Name.Identifier.ValueText]);
    }
}
