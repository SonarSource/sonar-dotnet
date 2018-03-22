/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly Dictionary<string, HashSet<KnownType>> InvalidMemberAccesses =
            new Dictionary<string, HashSet<KnownType>>
            {
                ["Wait"] = new HashSet<KnownType> { KnownType.System_Threading_Tasks_Task },
                ["WaitAny"] = new HashSet<KnownType> { KnownType.System_Threading_Tasks_Task },
                ["WaitAll"] = new HashSet<KnownType> { KnownType.System_Threading_Tasks_Task },
                ["Result"] = new HashSet<KnownType> { KnownType.System_Threading_Tasks_Task_T },
                ["Sleep"] = new HashSet<KnownType> { KnownType.System_Threading_Thread },
                ["GetResult"] = new HashSet<KnownType> { KnownType.System_Runtime_CompilerServices_TaskAwaiter,
                    KnownType.System_Runtime_CompilerServices_TaskAwaiter_TResult },
            };

        private static readonly Dictionary<string, KnownType> DeadlockPreventingConstructs =
            new Dictionary<string, KnownType>
            {
                ["StartNew"] = KnownType.System_Threading_Tasks_TaskFactory,
                ["Run"] = KnownType.System_Threading_Tasks_Task,
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (c.IsTest())
                    {
                        return;
                    }

                    var simpleMemberAccess = (MemberAccessExpressionSyntax)c.Node;
                    var memberAccessNameName = simpleMemberAccess.Name?.Identifier.ValueText;

                    if (memberAccessNameName == null ||
                        !InvalidMemberAccesses.ContainsKey(memberAccessNameName) ||
                        IsPartOfDeadlockPreventingConstruct(simpleMemberAccess, c.SemanticModel))
                    {
                        return;
                    }

                    var possibleMemberAccesses = InvalidMemberAccesses[memberAccessNameName];

                    var memberAccessSymbol = c.SemanticModel.GetSymbolInfo(simpleMemberAccess).Symbol;
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
                            !enclosingMethod.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
                        {
                            return; // Thread.Sleep should not be used only in async methods
                        }

                        var methodSymbol = c.SemanticModel.GetDeclaredSymbol(enclosingMethod);
                        if (methodSymbol != null &&
                            methodSymbol.IsMainMethod())
                        {
                            return; // Main methods are not subject to deadlock issue so no need to report an issue
                        }
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, simpleMemberAccess.GetLocation(),
                        messageArgs: GetMessageArguments(memberAccessNameName)));
                },
                SyntaxKind.SimpleMemberAccessExpression);
        }

        private static bool IsPartOfDeadlockPreventingConstruct(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
            memberAccess.Expression.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Any(subMemberAccess => IsDeadlockPreventingConstruct(subMemberAccess, semanticModel));

        private static bool IsDeadlockPreventingConstruct(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            if (memberAccess?.Name == null ||
                !DeadlockPreventingConstructs.ContainsKey(memberAccess.Name.Identifier.ValueText))
            {
                return false;
            }

            var memberAccessSymbol = semanticModel.GetSymbolInfo(memberAccess).Symbol?.ContainingType?.ConstructedFrom;
            return memberAccessSymbol.Is(DeadlockPreventingConstructs[memberAccess.Name.Identifier.ValueText]);
        }

        private static string[] GetMessageArguments(string memberAccessName)
        {
            switch (memberAccessName)
            {
                case "Result":
                    return new[] { "Task.Result", "await" };
                case "Wait":
                    return new[] { "Task.Wait", "await" };
                case "GetResult":
                    return new[] { "Task.GetAwaiter.GetResult", "await" };
                case "WaitAny":
                    return new[] { "Task.WaitAny", "await Task.WhenAny" };
                case "WaitAll":
                    return new[] { "Task.WaitAll", "await Task.WhenAll" };
                case "Sleep":
                    return new[] { "Thread.Sleep", "await Task.Delay" };
                default:
                    return new string[0];
            }
        }
    }
}
