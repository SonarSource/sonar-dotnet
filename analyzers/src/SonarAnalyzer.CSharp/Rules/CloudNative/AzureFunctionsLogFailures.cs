/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

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

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var catchClause = (CatchClauseSyntax)c.Node;
                if (c.AzureFunctionMethod() is { } entryPoint
                    && HasLoggerInScope(entryPoint))
                {
                    var walker = new LoggerCallWalker(c.Model, c.Cancel);
                    walker.SafeVisit(catchClause.Block);
                    // Exception handling in the filter clause preserves log scopes and is therefore recommended
                    // See https://blog.stephencleary.com/2020/06/a-new-pattern-for-exception-logging.html
                    walker.SafeVisit(catchClause.Filter?.FilterExpression);
                    if (!walker.HasValidLoggerCall)
                    {
                        c.ReportIssue(Rule, catchClause.CatchKeyword.GetLocation(), walker.InvalidLoggerInvocationLocations);
                    }
                }
            },
            SyntaxKind.CatchClause);

        private static bool HasLoggerInScope(IMethodSymbol entryPoint) =>
            entryPoint.Parameters.Any(x => x.Type.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger))
                // Instance method entry points might have access to an ILogger via injected fields/properties
                // https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
                || (entryPoint is { IsStatic: false, ContainingType: { } container }
                    && HasLoggerMember(container));

        internal static bool HasLoggerMember(ITypeSymbol typeSymbol)
        {
            var isOriginalType = true;
            foreach (var type in typeSymbol.GetSelfAndBaseTypes())
            {
                if (type.GetMembers().Any(x => x.Kind is SymbolKind.Field or SymbolKind.Property
                                               && (isOriginalType || x.GetEffectiveAccessibility() != Accessibility.Private)
                                               && x.GetSymbolType()?.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger) is true))
                {
                    return true;
                }
                isOriginalType = false;
            }
            return false;
        }

        private sealed class LoggerCallWalker : SafeCSharpSyntaxWalker
        {
            private readonly SemanticModel model;
            private readonly CancellationToken cancel;
            private List<SecondaryLocation> invalidInvocations;

            public bool HasValidLoggerCall { get; private set; }
            public IEnumerable<SecondaryLocation> InvalidLoggerInvocationLocations => invalidInvocations ?? [];

            public LoggerCallWalker(SemanticModel model, CancellationToken cancel)
            {
                this.model = model;
                this.cancel = cancel;
            }

            public override void Visit(SyntaxNode node)
            {
                if (!HasValidLoggerCall)
                {
                    cancel.ThrowIfCancellationRequested();
                    base.Visit(node);
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (model.GetSymbolInfo(node, cancel).Symbol is IMethodSymbol { ReceiverType: { } receiver } methodSymbol
                    && receiver.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger))
                {
                    if (IsValidLogCall(node, methodSymbol))
                    {
                        HasValidLoggerCall = true;
                    }
                    else
                    {
                        invalidInvocations ??= new();
                        invalidInvocations.Add(node.ToSecondaryLocation());
                    }
                }
                base.VisitInvocationExpression(node);
            }

            public override void VisitArgument(ArgumentSyntax node)
            {
                HasValidLoggerCall = HasValidLoggerCall || model.GetTypeInfo(node.Expression, cancel).Type?.DerivesOrImplements(KnownType.Microsoft_Extensions_Logging_ILogger) is true;
                base.VisitArgument(node);
            }

            private bool IsValidLogCall(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol) =>
                methodSymbol switch
                {
                    // Wellknown LoggerExtensions methods invocations
                    { IsExtensionMethod: true } when methodSymbol.ContainingType.Is(KnownType.Microsoft_Extensions_Logging_LoggerExtensions) =>
                        methodSymbol.Name switch
                        {
                            "LogInformation" or "LogWarning" or "LogError" or "LogCritical" => true,
                            "Log" => IsPassingValidLogLevel(invocation, methodSymbol),
                            "LogTrace" or "LogDebug" or "BeginScope" => false,
                            _ => true, // Some unknown extension method on LoggerExtensions was called. Avoid FPs and assume it logs something.
                        },
                    { IsExtensionMethod: true } => true, // Any other extension method is assumed to log something to avoid FP.
                    _ => // Instance invocations on an ILogger instance.
                        methodSymbol.Name switch
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
