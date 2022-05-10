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
        private const string MessageFormat = "Log caught exceptions via ILogger";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;
                    if (IsContainingMethodAzureFunction(c.SemanticModel, catchClause))
                    {
                        var iLogger = c.SemanticModel.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Extensions_Logging_ILogger.TypeName);
                        if (!CallsToILogger(c.SemanticModel, iLogger, catchClause, c.CancellationToken))
                        {
                            c.ReportIssue(Diagnostic.Create(Rule, catchClause.CatchKeyword.GetLocation()));
                        }
                    }
                },
                SyntaxKind.CatchClause);

        private static bool CallsToILogger(SemanticModel semanticModel, ITypeSymbol iLogger, CatchClauseSyntax catchClause, CancellationToken cancellationToken)
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

            return walker.HasValidLoggerCall;
        }

        private static bool IsContainingMethodAzureFunction(SemanticModel model, SyntaxNode node)
        {
            var method = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method.AttributeLists.Any()) // FunctionName has [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)] so there must be an attribute
            {
                var methodSymbol = model.GetDeclaredSymbol(method);
                var functionNameAttribute = model.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Azure_WebJobs_FunctionNameAttribute.TypeName);
                if (methodSymbol.GetAttributes().Any(a => a.AttributeClass.Equals(functionNameAttribute)))
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class LoggerCallWalker : SafeCSharpSyntaxWalker
        {
            private static readonly int[] ValidLogLevel = new[]
            {
                3, // Warning
                4, // Error
                5, // Critical
            };

            private readonly ImmutableHashSet<ExpressionSyntax>.Builder builder = ImmutableHashSet.CreateBuilder<ExpressionSyntax>();

            public LoggerCallWalker(SemanticModel model, ITypeSymbol iLoggerSymbol, CancellationToken cancellationToken)
            {
                Model = model;
                ILoggerSymbol = iLoggerSymbol;
                CancellationToken = cancellationToken;
                LoggerExtensions = new Lazy<INamedTypeSymbol>(() => Model.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Extensions_Logging_LoggerExtensions.TypeName));
                ILogger = new Lazy<INamedTypeSymbol>(() => Model.Compilation.GetTypeByMetadataName(KnownType.Microsoft_Extensions_Logging_ILogger.TypeName));
                ILogger_Log = new Lazy<IMethodSymbol>(() => ILogger.Value?.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(x => x.Name == "Log"));
            }

            private SemanticModel Model { get; }
            private ITypeSymbol ILoggerSymbol { get; }
            private CancellationToken CancellationToken { get; }
            private Lazy<INamedTypeSymbol> LoggerExtensions { get; }
            private Lazy<INamedTypeSymbol> ILogger { get; }
            private Lazy<IMethodSymbol> ILogger_Log { get; }
            public bool HasValidLoggerCall { get; private set; }
            public ImmutableArray<ExpressionSyntax> InvalidLoggerInvocations => builder.ToImmutableArray();

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
                        builder.Add(node);
                    }
                }
                base.VisitInvocationExpression(node);
            }

            private bool IsValidLogCall(InvocationExpressionSyntax invocation)
            {
                if (Model.GetSymbolInfo(invocation, CancellationToken).Symbol is IMethodSymbol symbol)
                {
                    if (LoggerExtensions.Value is { } loggerExtensions
                        && loggerExtensions?.Equals(symbol?.ContainingType) == true
                        && (symbol.Name is "LogWarning" or "LogError" or "LogCritical"
                           || (symbol.Name is "Log" && IsPassingAnValidLogLevel(invocation, symbol))))
                    {
                        return true;
                    }
                    else
                    {
                        if (symbol.ContainingType.Equals(ILogger.Value)
                            && symbol.OriginalDefinition.Equals(ILogger_Log.Value))
                        {
                            return IsPassingAnValidLogLevel(invocation, symbol);
                        }
                    }
                }

                return false;
            }

            private bool IsPassingAnValidLogLevel(InvocationExpressionSyntax invocation, IMethodSymbol symbol)
            {
                var lookup = new CSharpMethodParameterLookup(invocation.ArgumentList, symbol);
                var logLevelParameter = symbol.Parameters.FirstOrDefault(x => x.Name == "logLevel");
                if (lookup.TryGetNonParamsSyntax(logLevelParameter, out var argumentSyntax))
                {
                    var value = Model.GetConstantValue(argumentSyntax);
                    if (value.HasValue && value.Value is int logLevel && ValidLogLevel.Contains(logLevel))
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool IsExpressionAnILogger(ExpressionSyntax expression)
            {
                var methodSymbol = Model.GetSymbolInfo(expression, CancellationToken).Symbol as IMethodSymbol;
                return ILoggerSymbol.Equals(methodSymbol?.ReceiverType);
            }

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
