/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CallToAsyncMethodShouldNotBeBlocking : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4462";
        private const string MessageFormat = "Replace this use of '{0}' with '{1}'.";
        private const string ResultName = "Result";
        private const string ContinueWithName = "ContinueWith";
        private const string SleepName = "Sleep";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly Dictionary<string, ImmutableArray<KnownType>> InvalidMemberAccess =
            new Dictionary<string, ImmutableArray<KnownType>>
            {
                ["GetResult"] = ImmutableArray.Create(KnownType.System_Runtime_CompilerServices_TaskAwaiter,
                    KnownType.System_Runtime_CompilerServices_TaskAwaiter_TResult),
                [ResultName] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task_T),
                [SleepName] = ImmutableArray.Create(KnownType.System_Threading_Thread),
                ["Wait"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task),
                ["WaitAll"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task),
                ["WaitAny"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task)
            };

        private static readonly Dictionary<string, string[]> MemberNameToMessageArguments =
            new Dictionary<string, string[]>
            {
                ["GetResult"] = new[] { "Task.GetAwaiter.GetResult", "await" },
                [ResultName] = new[] { "Task.Result", "await" },
                [SleepName] = new[] { "Thread.Sleep", "await Task.Delay" },
                ["Wait"] = new[] { "Task.Wait", "await" },
                ["WaitAll"] = new[] { "Task.WaitAll", "await Task.WhenAll" },
                ["WaitAny"] = new[] { "Task.WaitAny", "await Task.WhenAny" }
            };

        private static readonly Dictionary<string, KnownType> TaskThreadPoolCalls =
            new Dictionary<string, KnownType>
            {
                ["StartNew"] = KnownType.System_Threading_Tasks_TaskFactory,
                ["Run"] = KnownType.System_Threading_Tasks_Task,
            };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(ReportOnViolation, SyntaxKind.SimpleMemberAccessExpression);

        private static void ReportOnViolation(SyntaxNodeAnalysisContext context)
        {
            var simpleMemberAccess = (MemberAccessExpressionSyntax)context.Node;
            var memberAccessNameName = simpleMemberAccess.GetName();

            if (memberAccessNameName == null ||
                !InvalidMemberAccess.ContainsKey(memberAccessNameName) ||
                IsResultInContinueWithCall(memberAccessNameName, simpleMemberAccess) ||
                IsChainedAfterThreadPoolCall(simpleMemberAccess, context.SemanticModel) ||
                simpleMemberAccess.IsInNameOfArgument(context.SemanticModel))
            {
                return;
            }

            var possibleMemberAccesses = InvalidMemberAccess[memberAccessNameName];

            var memberAccessSymbol = context.SemanticModel.GetSymbolInfo(simpleMemberAccess).Symbol;
            if (memberAccessSymbol?.ContainingType == null ||
                !memberAccessSymbol.ContainingType.ConstructedFrom.IsAny(possibleMemberAccesses))
            {
                return;
            }

            var enclosingMethod = simpleMemberAccess.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
            if (enclosingMethod != null)
            {
                if (memberAccessNameName == SleepName &&
                    !enclosingMethod.Modifiers.Any(SyntaxKind.AsyncKeyword))
                {
                    return; // Thread.Sleep should not be used only in async methods
                }

                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(enclosingMethod);
                if (methodSymbol != null &&
                    methodSymbol.IsMainMethod())
                {
                    return; // Main methods are not subject to deadlock issue so no need to report an issue
                }
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, simpleMemberAccess.GetLocation(),
                messageArgs: MemberNameToMessageArguments[memberAccessNameName]));
        }

        private static bool IsResultInContinueWithCall(string memberAccessName, MemberAccessExpressionSyntax memberAccess) =>
            memberAccessName == ResultName &&
            memberAccess.Expression is IdentifierNameSyntax identifierNameSyntax &&
            identifierNameSyntax.GetName() is {} identifierName &&
            memberAccess.FirstAncestorOrSelf<InvocationExpressionSyntax>(invocation => IsContinueWithCallWithArgumentName(invocation, identifierName)) != null;

        private static bool IsContinueWithCallWithArgumentName(InvocationExpressionSyntax invocation, string argumentName) =>
            invocation.Expression.NameIs(ContinueWithName) &&
            invocation.ArgumentList.Arguments.Any(argument => IsLambdaExpressionWithArgumentName(argument.Expression, argumentName));

        private static bool IsLambdaExpressionWithArgumentName(ExpressionSyntax expression, string argumentName) =>
            expression switch
            {
                SimpleLambdaExpressionSyntax lambda => lambda.Parameter.Identifier.ValueText == argumentName,
                ParenthesizedLambdaExpressionSyntax parenthesizedLambda =>
                    parenthesizedLambda.ParameterList.Parameters.Any(parameter => parameter.Identifier.ValueText == argumentName),
                _ => false
            };

        private static bool IsChainedAfterThreadPoolCall(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
            memberAccess.Expression.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Any(subMemberAccess => IsThreadPoolCall(subMemberAccess, semanticModel));

        private static bool IsThreadPoolCall(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            if (memberAccess?.Name == null ||
                !TaskThreadPoolCalls.ContainsKey(memberAccess.Name.Identifier.ValueText))
            {
                return false;
            }

            var memberAccessSymbol = semanticModel.GetSymbolInfo(memberAccess).Symbol?.ContainingType?.ConstructedFrom;
            return memberAccessSymbol.Is(TaskThreadPoolCalls[memberAccess.Name.Identifier.ValueText]);
        }
    }
}
