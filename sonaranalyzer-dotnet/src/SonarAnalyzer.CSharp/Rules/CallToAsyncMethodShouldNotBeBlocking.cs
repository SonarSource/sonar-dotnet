/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
        internal const string DiagnosticId = "S4462";
        private const string MessageFormat = "Replace this use of '{0}' with '{1}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly Dictionary<string, ImmutableArray<KnownType>> InvalidMemberAccess =
            new Dictionary<string, ImmutableArray<KnownType>>
            {
                ["Wait"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task),
                ["WaitAny"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task),
                ["WaitAll"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task),
                ["Result"] = ImmutableArray.Create(KnownType.System_Threading_Tasks_Task_T),
                ["Sleep"] = ImmutableArray.Create(KnownType.System_Threading_Thread),
                ["GetResult"] = ImmutableArray.Create(KnownType.System_Runtime_CompilerServices_TaskAwaiter,
                    KnownType.System_Runtime_CompilerServices_TaskAwaiter_TResult),
            };

        private static readonly Dictionary<string, KnownType> TaskThreadPoolCalls =
            new Dictionary<string, KnownType>
            {
                ["StartNew"] = KnownType.System_Threading_Tasks_TaskFactory,
                ["Run"] = KnownType.System_Threading_Tasks_Task,
            };

        private static readonly Dictionary<string, string[]> MemberNameToMessageArguments =
            new Dictionary<string, string[]>
            {
                ["Result"] = new[] { "Task.Result", "await" },
                ["Wait"] = new[] { "Task.Wait", "await" },
                ["GetResult"] = new[] { "Task.GetAwaiter.GetResult", "await" },
                ["WaitAny"] = new[] { "Task.WaitAny", "await Task.WhenAny" },
                ["WaitAll"] = new[] { "Task.WaitAll", "await Task.WhenAll" },
                ["Sleep"] = new[] { "Thread.Sleep", "await Task.Delay" },
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                ReportOnViolation,
                SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void ReportOnViolation(SyntaxNodeAnalysisContext context)
        {
            var simpleMemberAccess = (MemberAccessExpressionSyntax)context.Node;
            var memberAccessNameName = simpleMemberAccess.Name?.Identifier.ValueText;

            if (memberAccessNameName == null ||
                !InvalidMemberAccess.ContainsKey(memberAccessNameName) ||
                IsChainedAfterThreadPoolCall(simpleMemberAccess, context.SemanticModel))
            {
                return;
            }

            var possibleMemberAccesses = InvalidMemberAccess[memberAccessNameName];

            var memberAccessSymbol = context.SemanticModel.GetSymbolInfo(simpleMemberAccess).Symbol;
            if (memberAccessSymbol == null ||
                memberAccessSymbol.ContainingType == null ||
                !memberAccessSymbol.ContainingType.ConstructedFrom.IsAny(possibleMemberAccesses))
            {
                return;
            }

            var enclosingMethod = simpleMemberAccess.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
            if (enclosingMethod != null)
            {
                if (memberAccessNameName == "Sleep" &&
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
