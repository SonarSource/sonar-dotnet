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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AzureFunctionsLogFailures : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S6423";
        private const string MessageFormat = "Log caught exceptions via ILogger with LogLevel Warning, Error, or Critical";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;
                    if (IsContainingMethodAzureFunction(c.SemanticModel, catchClause, c.CancellationToken))
                    {
                        var iLogger = c.SemanticModel.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Extensions_Logging_ILogger.TypeName);
                        if (iLogger is not null && InvalidCallsToILogger(c.SemanticModel, iLogger, catchClause, c.CancellationToken) is ImmutableArray<ExpressionSyntax> secondaryLocations)
                        {
                            c.ReportIssue(Diagnostic.Create(Rule, catchClause.CatchKeyword.GetLocation(), additionalLocations: secondaryLocations.Select(x => x.GetLocation())));
                        }
                    }
                },
                SyntaxKind.CatchClause);

        private static ImmutableArray<ExpressionSyntax>? InvalidCallsToILogger(SemanticModel semanticModel, ITypeSymbol iLogger, CatchClauseSyntax catchClause, CancellationToken cancellationToken)
        {
            var walker = new LoggerCallWalker(semanticModel, iLogger, cancellationToken);
            if (catchClause.Block is { } block)
            {
                walker.Visit(block);
            }

            if (!walker.HasValidLoggerCall && catchClause.Filter?.FilterExpression is { } filterExpression)
            {
                walker.Visit(filterExpression);
            }

            return walker.HasValidLoggerCall
                ? null
                : walker.InvalidLoggerInvocations;
        }

        private static bool IsContainingMethodAzureFunction(SemanticModel model, SyntaxNode node, CancellationToken cancellationToken)
        {
            return node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault() is { } method
                && method.AttributeLists.Any() // FunctionName has [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)] so there must be an attribute
                && model.GetDeclaredSymbol(method, cancellationToken) is { } methodSymbol
                && model.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Azure_WebJobs_FunctionNameAttribute.TypeName) is { } functionNameAttribute
                && methodSymbol.GetAttributes().Any(a => a.AttributeClass.Equals(functionNameAttribute));
        }

        private sealed class LoggerCallWalker : SafeCSharpSyntaxWalker
        {
            private static readonly int[] ValidLogLevel = new[]
            {
                3, // Warning
                4, // Error
                5, // Critical
            };

            private readonly ImmutableHashSet<ExpressionSyntax>.Builder invalidIvocationsBuilder = ImmutableHashSet.CreateBuilder<ExpressionSyntax>();

            public LoggerCallWalker(SemanticModel model, ITypeSymbol iLoggerSymbol, CancellationToken cancellationToken)
            {
                Model = model;
                ILogger = iLoggerSymbol;
                CancellationToken = cancellationToken;
                LoggerExtensions = new Lazy<INamedTypeSymbol>(() => Model.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Extensions_Logging_LoggerExtensions.TypeName));
                ILogger_Log = new Lazy<IMethodSymbol>(() => ILogger.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(x => x.Name == "Log"));
            }

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
                if (IsExpressionAnILogger(node))
                {
                    if (IsValidLogCall(node))
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

            private bool IsValidLogCall(InvocationExpressionSyntax invocation)
            {
                if (Model.GetSymbolInfo(invocation, CancellationToken).Symbol is IMethodSymbol symbol)
                {
                    if (symbol.IsExtensionMethod
                        && LoggerExtensions.Value is { } loggerExtensions
                        && loggerExtensions.Equals(symbol.ContainingType)
                        && (symbol.Name is "LogWarning" or "LogError" or "LogCritical"
                           || (symbol.Name is "Log" && IsPassingValidLogLevel(invocation, symbol))))
                    {
                        return true;
                    }
                    else
                    {
                        if (ILogger_Log.Value is { } loggerLog
                            && (symbol.OriginalDefinition.Equals(loggerLog)
                                || symbol.ContainingType.FindImplementationForInterfaceMember(loggerLog)?.Equals(symbol.OriginalDefinition) == true))
                        {
                            return IsPassingValidLogLevel(invocation, symbol);
                        }
                    }
                }

                return false;
            }

            private bool IsPassingValidLogLevel(InvocationExpressionSyntax invocation, IMethodSymbol symbol) =>
                symbol.Parameters.FirstOrDefault(x => x.Name == "logLevel") is { } logLevelParameter
                    && CSharpFacade.Instance.MethodParameterLookup(invocation, symbol).TryGetNonParamsSyntax(logLevelParameter, out var argumentSyntax)
                    && Model.GetConstantValue(argumentSyntax) is { HasValue: true, Value: int logLevel }
                        ? ValidLogLevel.Contains(logLevel)
                        : true; // Compliant: Some non-constant value is passed as loglevel.

            private bool IsExpressionAnILogger(ExpressionSyntax expression) =>
                Model.GetSymbolInfo(expression, CancellationToken).Symbol switch
                {
                    IMethodSymbol { ReceiverType: { } receiver } => receiver.DerivesOrImplements(ILogger),
                    IParameterSymbol { Type: { } type } => type.DerivesOrImplements(ILogger),
                    _ => false,
                };

            public override void VisitArgument(ArgumentSyntax node)
            {
                if (IsExpressionAnILogger(node.Expression))
                {
                    HasValidLoggerCall = true;
                }
                base.VisitArgument(node);
            }
        }
    }
}
