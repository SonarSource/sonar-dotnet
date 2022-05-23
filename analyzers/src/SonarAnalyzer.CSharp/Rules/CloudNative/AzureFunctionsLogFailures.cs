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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AzureFunctionsLogFailures : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6423";
        private const string MessageFormat = "Log exception via ILogger with LogLevel Information, Warning, Error, or Critical.";

        private static readonly int[] InvalidLogLevel = new[]
        {
            0, // Trace
            1, // Debug
            6, // None
        };

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                if (c.AzureFunctionMethod() is { } entryPoint
                    && c.Node is CatchClauseSyntax catchClause
                    && c.SemanticModel.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Extensions_Logging_ILogger.TypeName) is { TypeKind: not TypeKind.Error } iLogger
                    && LoggerIsInScopeInEntryPoint(c.SemanticModel, c.Node.SpanStart, entryPoint, c.CancellationToken))
                {
                    var walker = new LoggerCallWalker(c.SemanticModel, iLogger, c.CancellationToken);

                    walker.SafeVisit(catchClause.Block);
                    // Exception handling in the filter clause preserves log scopes and is therefore recommended
                    // See https://blog.stephencleary.com/2020/06/a-new-pattern-for-exception-logging.html
                    walker.SafeVisit(catchClause.Filter?.FilterExpression);
                    if (!walker.HasValidLoggerCall)
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, catchClause.CatchKeyword.GetLocation(), walker.InvalidLoggerInvocationLocations));
                    }
                }
            },
            SyntaxKind.CatchClause);

        private static bool LoggerIsInScopeInEntryPoint(SemanticModel semanticModel, int position, IMethodSymbol entryPoint, CancellationToken cancellationToken) =>
            entryPoint.Parameters.Any(x => x.Type.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger))
                // Instance method entrypoints might have access to an ILogger via injected fields/properties
                // https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
                || (entryPoint is { IsStatic: false, ContainingType: { } container }
                    && container.AllAccessibleMembers(semanticModel, position, cancellationToken)
                        .Any(x => x.GetSymbolType()?.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger) is true));

        private sealed class LoggerCallWalker : SafeCSharpSyntaxWalker
        {
            private readonly SemanticModel model;
            private readonly ITypeSymbol iLogger;
            private readonly CancellationToken cancellationToken;
            private readonly Lazy<IMethodSymbol> iLoggerLog;
            private List<Location> invalidInvocations;

            public LoggerCallWalker(SemanticModel model, ITypeSymbol iLoggerSymbol, CancellationToken cancellationToken)
            {
                this.model = model;
                iLogger = iLoggerSymbol;
                this.cancellationToken = cancellationToken;
                iLoggerLog = new Lazy<IMethodSymbol>(() => iLogger.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(x => x.Name == "Log"));
            }

            public bool HasValidLoggerCall { get; private set; }
            public IEnumerable<Location> InvalidLoggerInvocationLocations => invalidInvocations;

            public override void Visit(SyntaxNode node)
            {
                if (!HasValidLoggerCall)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    base.Visit(node);
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (model.GetSymbolInfo(node, cancellationToken).Symbol is IMethodSymbol { ReceiverType: { } receiver } methodSymbol
                    && receiver.DerivesOrImplements(iLogger))
                {
                    if (IsValidLogCall(node, methodSymbol))
                    {
                        HasValidLoggerCall = true;
                    }
                    else
                    {
                        invalidInvocations ??= new();
                        invalidInvocations.Add(node.GetLocation());
                    }
                }
                base.VisitInvocationExpression(node);
            }

            public override void VisitArgument(ArgumentSyntax node)
            {
                if (model.GetTypeInfo(node.Expression, cancellationToken).Type?.DerivesOrImplements(iLogger) is true)
                {
                    HasValidLoggerCall = true;
                }
                base.VisitArgument(node);
            }

            private bool IsValidLogCall(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol) =>
                methodSymbol switch
                {
                    // Check wellknown LoggerExtensions methods invocations
                    { IsExtensionMethod: true, Name: { } name } when methodSymbol.ContainingType.Is(KnownType.Microsoft_Extensions_Logging_LoggerExtensions) =>
                        name switch
                        {
                            "LogInformation" or "LogWarning" or "LogError" or "LogCritical" => true,
                            "Log" => IsPassingValidLogLevel(invocation, methodSymbol),
                            "LogTrace" or "LogDebug" or "BeginScope" => false,
                            _ => true, // Some unknown extension method on LoggerExtensions was called. Avoid FPs and assume it logs something.
                        },
                    // Check Log invocations on the instance
                    { ContainingType: { } container, Name: { } name } when container.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger) =>
                        name switch
                        {
                            "Log" => IsPassingValidLogLevel(invocation, methodSymbol),
                            "IsEnabled" or "BeginScope" => false,
                            _ => true, // Some unknown method on an ILogger was called. Avoid FPs and assume it logs something.
                        },
                    // The invocations receiver was an ILogger, but none of the known log methods was called.
                    // We assume some kind of logging is performed by the unknown invocation to avoid FPs.
                    _ => true,
                };

            private bool IsPassingValidLogLevel(InvocationExpressionSyntax invocation, IMethodSymbol symbol) =>
                symbol.Parameters.FirstOrDefault(x => x.Name == "logLevel") is { } logLevelParameter
                    && new CSharpMethodParameterLookup(invocation, symbol).TryGetNonParamsSyntax(logLevelParameter, out var argumentSyntax)
                    && argumentSyntax.FindConstantValue(model) is int logLevel
                        ? !InvalidLogLevel.Contains(logLevel)
                        : true; // Compliant: Some non-constant value is passed as loglevel or there is no logLevel parameter
        }
    }
}
