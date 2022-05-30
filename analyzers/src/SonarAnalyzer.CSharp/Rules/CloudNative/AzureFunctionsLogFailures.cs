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

        private static readonly int[] InvalidLogLevel =
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
                var catchClause = (CatchClauseSyntax)c.Node;
                if (c.AzureFunctionMethod() is { } entryPoint
                    && HasLoggerInScope(entryPoint, c.CancellationToken))
                {
                    var walker = new LoggerCallWalker(c.SemanticModel, c.CancellationToken);
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

        private static bool HasLoggerInScope(IMethodSymbol entryPoint, CancellationToken cancellationToken) =>
            entryPoint.Parameters.Any(x => x.Type.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger))
                // Instance method entry points might have access to an ILogger via injected fields/properties
                // https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
                || (entryPoint is { IsStatic: false, ContainingType: { } container }
                    && HasLoggerMember(container, cancellationToken) is not null);

        internal static ISymbol HasLoggerMember(ITypeSymbol typeSymbol, CancellationToken cancellationToken)
        {
            var isOriginalType = true;
            foreach (var type in typeSymbol.GetSelfAndBaseTypes())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (type.GetMembers().FirstOrDefault(x => AccessibilityValid(x.GetEffectiveAccessibility(), isOriginalType)
                                                          && x.GetSymbolType()?.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger) is true) is { } result)
                {
                    return result;
                }
                isOriginalType = false;
            }
            return null;

            static bool AccessibilityValid(Accessibility accessibility, bool isOriginalType) =>
                isOriginalType || accessibility != Accessibility.Private;
        }

        private sealed class LoggerCallWalker : SafeCSharpSyntaxWalker
        {
            private readonly SemanticModel model;
            private readonly CancellationToken cancellationToken;
            private List<Location> invalidInvocations;

            public bool HasValidLoggerCall { get; private set; }
            public IEnumerable<Location> InvalidLoggerInvocationLocations => invalidInvocations;

            public LoggerCallWalker(SemanticModel model, CancellationToken cancellationToken)
            {
                this.model = model;
                this.cancellationToken = cancellationToken;
        }

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
                    && receiver.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger))
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
                if (model.GetTypeInfo(node.Expression, cancellationToken).Type?.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger) is true)
                {
                    HasValidLoggerCall = true;
                }
                base.VisitArgument(node);
            }

            private bool IsValidLogCall(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol) =>
                methodSymbol switch
                {
                    // Wellknown LoggerExtensions methods invocations
                    { IsExtensionMethod: true } when methodSymbol.ContainingType.Is(KnownType.Microsoft_Extensions_Logging_LoggerExtensions) => methodSymbol.Name switch
                        {
                            "LogInformation" or "LogWarning" or "LogError" or "LogCritical" => true,
                            "Log" => IsPassingValidLogLevel(invocation, methodSymbol),
                            "LogTrace" or "LogDebug" or "BeginScope" => false,
                            _ => true, // Some unknown extension method on LoggerExtensions was called. Avoid FPs and assume it logs something.
                        },
                    { IsExtensionMethod: true } => true, // Any other extension method is assumed to log something to avoid FP.
                    _ => methodSymbol.Name switch // Instance invocations on an ILogger instance.
                        {
                            "Log" => IsPassingValidLogLevel(invocation, methodSymbol),
                            "IsEnabled" or "BeginScope" => false,
                            _ => true, // Some unknown method on an ILogger was called. Avoid FPs and assume it logs something.
                        },
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
