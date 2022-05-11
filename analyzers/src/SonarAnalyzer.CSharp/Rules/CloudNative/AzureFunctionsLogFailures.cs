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
        private const string MessageFormat = "Log exception via ILogger with LogLevel Information, Warning, Error, or Critical";

        private static readonly int[] ValidLogLevel = new[]
        {
            2, // Information
            3, // Warning
            4, // Error
            5, // Critical
        };

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        private static readonly ILanguageFacade LanguageFacade = CSharpFacade.Instance;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;
                    if (c.AzureFunctionMethod() is { } entryPoint)
                    {
                        var iLogger = c.SemanticModel.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Extensions_Logging_ILogger.TypeName);
                        if (iLogger is not null
                            && entryPoint.Parameters.Any(p => p.Type.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger)))
                        {
                            var walker = new LoggerCallWalker(LanguageFacade, c.SemanticModel, iLogger, c.CancellationToken);

                            walker.SafeVisit(catchClause.Block);
                            // Exception handling in the filter clause preserves log scopes and is therefore recommended
                            // See https://blog.stephencleary.com/2020/06/a-new-pattern-for-exception-logging.html
                            walker.SafeVisit(catchClause.Filter?.FilterExpression);
                            if (!walker.HasValidLoggerCall)
                            {
                                c.ReportIssue(Diagnostic.Create(Rule, catchClause.CatchKeyword.GetLocation(), walker.InvalidLoggerInvocations.Select(x => x.GetLocation())));
                            }
                        }
                    }
                },
                SyntaxKind.CatchClause);

        private sealed class LoggerCallWalker : SafeCSharpSyntaxWalker
        {
            private readonly ImmutableHashSet<ExpressionSyntax>.Builder invalidIvocationsBuilder = ImmutableHashSet.CreateBuilder<ExpressionSyntax>();

            public LoggerCallWalker(ILanguageFacade languageFacade, SemanticModel model, ITypeSymbol iLoggerSymbol, CancellationToken cancellationToken)
            {
                Language = languageFacade;
                Model = model;
                ILogger = iLoggerSymbol;
                CancellationToken = cancellationToken;
                LoggerExtensions = new Lazy<INamedTypeSymbol>(() => Model.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Extensions_Logging_LoggerExtensions.TypeName));
                ILogger_Log = new Lazy<IMethodSymbol>(() => ILogger.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(x => x.Name == "Log"));
            }

            private ILanguageFacade Language { get; }
            private SemanticModel Model { get; }
            private ITypeSymbol ILogger { get; }
            private CancellationToken CancellationToken { get; }
            private Lazy<INamedTypeSymbol> LoggerExtensions { get; }
            private Lazy<IMethodSymbol> ILogger_Log { get; }
            public bool HasValidLoggerCall { get; private set; }
            public ImmutableArray<ExpressionSyntax> InvalidLoggerInvocations => invalidIvocationsBuilder.ToImmutableArray();

            public override void Visit(SyntaxNode node)
            {
                CancellationToken.ThrowIfCancellationRequested();
                if (HasValidLoggerCall)
                {
                    return;
                }
                base.Visit(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (Model.GetSymbolInfo(node, CancellationToken).Symbol is IMethodSymbol { ReceiverType: { } receiver } methodSymbol
                    && receiver.DerivesOrImplements(ILogger))
                {
                    if (IsValidLogCall(node, methodSymbol))
                    {
                        HasValidLoggerCall = true;
                    }
                    else
                    {
                        invalidIvocationsBuilder.Add(node);
                    }
                }
                base.VisitInvocationExpression(node);
            }

            private bool IsValidLogCall(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
            {
                // Check wellknown LoggerExtensions extension methods invocation
                if (methodSymbol.IsExtensionMethod
                    && LoggerExtensions.Value is { } loggerExtensions
                    && loggerExtensions.Equals(methodSymbol.ContainingType))
                {
                    return methodSymbol.Name switch
                    {
                        "LogInformation" or "LogWarning" or "LogError" or "LogCritical" => true,
                        "LogTrace" or "LogDebug" or "BeginScope" => false,
                        "Log" => IsPassingValidLogLevel(invocation, methodSymbol),
                        _ => true, // Some unknown extension method on LoggerExtensions was called. Avoid FPs and assume it logs something.
                    };
                }
                // Check direct ILogger.Log invocations
                if (ILogger_Log.Value is { } loggerLog
                    && (methodSymbol.OriginalDefinition.Equals(loggerLog)
                        || methodSymbol.ContainingType.FindImplementationForInterfaceMember(loggerLog)?.Equals(methodSymbol.OriginalDefinition) is true))
                {
                    return IsPassingValidLogLevel(invocation, methodSymbol);
                }
                // The invocations receiver was an ILogger, but none of the known log methods was called.
                // We assume some kind of logging is performed by the unknown invocation to avoid FPs.
                return true;
            }

            private bool IsPassingValidLogLevel(InvocationExpressionSyntax invocation, IMethodSymbol symbol) =>
                symbol.Parameters.FirstOrDefault(x => x.Name == "logLevel") is { } logLevelParameter
                    && Language.MethodParameterLookup(invocation, symbol).TryGetNonParamsSyntax(logLevelParameter, out var argumentSyntax)
                    && Model.GetConstantValue(argumentSyntax, CancellationToken) is { HasValue: true, Value: int logLevel }
                        ? ValidLogLevel.Contains(logLevel)
                        : true; // Compliant: Some non-constant value is passed as loglevel.

            public override void VisitArgument(ArgumentSyntax node)
            {
                if (Model.GetTypeInfo(node.Expression).Type?.DerivesOrImplements(ILogger) == true)
                {
                    HasValidLoggerCall = true;
                }
                base.VisitArgument(node);
            }
        }
    }
}
