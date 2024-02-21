/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseConstantLoggingTemplate : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2629";
    private const string MessageFormat = "{0}";
    private const string OnUsingStringInterpolation = "Don't use string interpolation in logging message templates.";
    private const string OnUsingStringFormat = "Don't use String.Format in logging message templates.";
    private const string OnUsingStringConcatenation = "Don't use string concatenation in logging message templates.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableDictionary<SyntaxKind, string> Messages = new Dictionary<SyntaxKind, string>
    {
        {SyntaxKind.AddExpression, OnUsingStringConcatenation},
        {SyntaxKind.InterpolatedStringExpression, OnUsingStringInterpolation},
        {SyntaxKind.InvocationExpression, OnUsingStringFormat},
    }.ToImmutableDictionary();

    private static readonly ImmutableArray<LogMethod> LogMethods = ImmutableArray.Create(
        new LogMethod(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "Log", "message"),
        new LogMethod(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogCritical", "message"),
        new LogMethod(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogDebug", "message"),
        new LogMethod(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogError", "message"),
        new LogMethod(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogInformation", "message"),
        new LogMethod(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogTrace", "message"),
        new LogMethod(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogWarning", "message"),
        new LogMethod(KnownType.Serilog_ILogger, "Debug", "messageTemplate"),
        new LogMethod(KnownType.Serilog_ILogger, "Error", "messageTemplate"),
        new LogMethod(KnownType.Serilog_ILogger, "Fatal", "messageTemplate"),
        new LogMethod(KnownType.Serilog_ILogger, "Information", "messageTemplate"),
        new LogMethod(KnownType.Serilog_ILogger, "Verbose", "messageTemplate"),
        new LogMethod(KnownType.Serilog_ILogger, "Warning", "messageTemplate"),
        new LogMethod(KnownType.Serilog_Log, "Debug", "messageTemplate"),
        new LogMethod(KnownType.Serilog_Log, "Error", "messageTemplate"),
        new LogMethod(KnownType.Serilog_Log, "Fatal", "messageTemplate"),
        new LogMethod(KnownType.Serilog_Log, "Information", "messageTemplate"),
        new LogMethod(KnownType.Serilog_Log, "Verbose", "messageTemplate"),
        new LogMethod(KnownType.Serilog_Log, "Warning", "messageTemplate"),
        new LogMethod(KnownType.log4net_ILog, "Debug", "message"),
        new LogMethod(KnownType.log4net_ILog, "DebugFormat", "format"),
        new LogMethod(KnownType.log4net_ILog, "Error", "message"),
        new LogMethod(KnownType.log4net_ILog, "ErrorFormat", "format"),
        new LogMethod(KnownType.log4net_ILog, "Fatal", "message"),
        new LogMethod(KnownType.log4net_ILog, "FatalFormat", "format"),
        new LogMethod(KnownType.log4net_ILog, "Info", "message"),
        new LogMethod(KnownType.log4net_ILog, "InfoFormat", "format"),
        new LogMethod(KnownType.log4net_ILog, "Trace", "message"),
        new LogMethod(KnownType.log4net_ILog, "TraceFormat", "format"),
        new LogMethod(KnownType.log4net_ILog, "Warn", "message"),
        new LogMethod(KnownType.log4net_ILog, "WarnFormat", "format"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "Debug", "message"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "DebugFormat", "format"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "Error", "message"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "ErrorFormat", "format"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "Fatal", "message"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "FatalFormat", "format"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "Info", "message"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "InfoFormat", "format"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "Trace", "message"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "TraceFormat", "format"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "Warn", "message"),
        new LogMethod(KnownType.Castle_Core_Logging_Ilogger, "WarnFormat", "format"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            var invocation = (InvocationExpressionSyntax)c.Node;
            var candidates = LogMethods.Where(x => x.MethodName == invocation.GetName()).ToArray();
            if (candidates.Any()
                && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
                && Array.Exists(candidates, x =>  x.ContainingType.Matches(method.ContainingType))
                && method.Parameters.FirstOrDefault(x => Array.Exists(candidates, c => c.MessageParameterName == x.Name)) is { } messageParameter
                && ArgumentValue(invocation, method, messageParameter) is { } argumentValue
                && InvalidSyntaxNode(argumentValue, c.SemanticModel) is { } invalidNode)
            {
                c.ReportIssue(Diagnostic.Create(Rule, invalidNode.GetLocation(), Messages[invalidNode.Kind()]));
            }
        },
        SyntaxKind.InvocationExpression);

    private static CSharpSyntaxNode ArgumentValue(InvocationExpressionSyntax invocation, IMethodSymbol method, IParameterSymbol parameter)
    {
        if (invocation.ArgumentList.Arguments.FirstOrDefault(x => x.NameColon?.GetName() == parameter.Name) is { } argument)
        {
            return argument.Expression;
        }
        else
        {
            var paramIndex = method.Parameters.IndexOf(parameter);
            return invocation.ArgumentList.Arguments[paramIndex].Expression;
        }
    }

    private static SyntaxNode InvalidSyntaxNode(SyntaxNode messageArgument, SemanticModel model) =>
        messageArgument.DescendantNodesAndSelf().FirstOrDefault(x =>
            x.Kind() is SyntaxKind.InterpolatedStringExpression or SyntaxKind.AddExpression
            || IsStringFormatInvocation(x, model));

    private static bool IsStringFormatInvocation(SyntaxNode node, SemanticModel model) =>
        node is InvocationExpressionSyntax invocation
        && node.GetName() == "Format"
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
        && KnownType.System_String.Matches(method.ContainingType);

    private sealed record LogMethod(KnownType ContainingType, string MethodName, string MessageParameterName);
}
