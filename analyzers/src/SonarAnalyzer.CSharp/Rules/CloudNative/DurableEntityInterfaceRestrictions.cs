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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DurableEntityInterfaceRestrictions : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6424";
        private const string MessageFormat = "{0}";
        private const string SignalEntityAsyncName = "SignalEntityAsync";
        private const string CreateEntityProxyName = "CreateEntityProxy";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var name = (GenericNameSyntax)c.Node;
                    if (name.Identifier.ValueText is SignalEntityAsyncName or CreateEntityProxyName
                        && name.TypeArgumentList.Arguments.Count == 1   // FIXME: Coverage
                        && c.SemanticModel.GetSymbolInfo(name).Symbol is IMethodSymbol method
                        && (method.Is(KnownType.Microsoft_Azure_WebJobs_Extensions_DurableTask_IDurableEntityClient, SignalEntityAsyncName)
                            || method.Is(KnownType.Microsoft_Azure_WebJobs_Extensions_DurableTask_IDurableOrchestrationContext, CreateEntityProxyName))
                        && InterfaceErrorMessage(method.TypeArguments.Single() as INamedTypeSymbol) is { } message) // FIXME: Null? Undefined?
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, name.GetLocation(), message));
                    }
                },
                SyntaxKind.GenericName);

        private static string InterfaceErrorMessage(INamedTypeSymbol entityInterface)
        {
            if (entityInterface is null) // FIXME: Coverage
            {
                return null;
            }
            else if (entityInterface.IsGenericType)
            {
                return "FIXME generic interface";
            }
            else
            {
                var members = entityInterface.GetMembers();
                return members.Any()
                    ? members.Select(MemberErrorMessage).WhereNotNull().FirstOrDefault()
                    : "FIXME Empty";
            }
        }

        private static string MemberErrorMessage(ISymbol member)
        {
            if (member is not IMethodSymbol method)
            {
                return "FIXME not a method: " + member.Name;
            }
            else if (method.IsGenericMethod)
            {
                return "FIXME is generic member: " + member.Name;
            }
            else if (method.Parameters.Length > 1)
            {
                return "FIXME: too many parameters: " + member.Name;
            }
            else if (method.ReturnsVoid
                || method.ReturnType.Is(KnownType.System_Threading_Tasks_Task)
                || method.ReturnType.Is(KnownType.System_Threading_Tasks_Task_T))
            {
                return null;
            }
            else
            {
                return "FIXME: return type";
            }
        }
    }
}
