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

    private static readonly ImmutableArray<string> MessageParameterNames = ImmutableArray.Create("format", "message", "messageTemplate");

    private static readonly ImmutableArray<MemberDescriptor> LogMethods = ImmutableArray.Create(
        new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "Log"),
        new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogCritical"),
        new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogDebug"),
        new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogError"),
        new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogInformation"),
        new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogTrace"),
        new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, "LogWarning"),
        new MemberDescriptor(KnownType.Serilog_ILogger, "Debug"),
        new MemberDescriptor(KnownType.Serilog_ILogger, "Error"),
        new MemberDescriptor(KnownType.Serilog_ILogger, "Fatal"),
        new MemberDescriptor(KnownType.Serilog_ILogger, "Information"),
        new MemberDescriptor(KnownType.Serilog_ILogger, "Verbose"),
        new MemberDescriptor(KnownType.Serilog_ILogger, "Warning"),
        new MemberDescriptor(KnownType.Serilog_Log, "Debug"),
        new MemberDescriptor(KnownType.Serilog_Log, "Error"),
        new MemberDescriptor(KnownType.Serilog_Log, "Fatal"),
        new MemberDescriptor(KnownType.Serilog_Log, "Information"),
        new MemberDescriptor(KnownType.Serilog_Log, "Verbose"),
        new MemberDescriptor(KnownType.Serilog_Log, "Warning"),
        new MemberDescriptor(KnownType.log4net_ILog, "Debug"),
        new MemberDescriptor(KnownType.log4net_ILog, "DebugFormat"),
        new MemberDescriptor(KnownType.log4net_ILog, "Error"),
        new MemberDescriptor(KnownType.log4net_ILog, "ErrorFormat"),
        new MemberDescriptor(KnownType.log4net_ILog, "Fatal"),
        new MemberDescriptor(KnownType.log4net_ILog, "FatalFormat"),
        new MemberDescriptor(KnownType.log4net_ILog, "Info"),
        new MemberDescriptor(KnownType.log4net_ILog, "InfoFormat"),
        new MemberDescriptor(KnownType.log4net_ILog, "Trace"),
        new MemberDescriptor(KnownType.log4net_ILog, "TraceFormat"),
        new MemberDescriptor(KnownType.log4net_ILog, "Warn"),
        new MemberDescriptor(KnownType.log4net_ILog, "WarnFormat"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "Debug"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "DebugFormat"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "Error"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "ErrorFormat"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "Fatal"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "FatalFormat"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "Info"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "InfoFormat"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "Trace"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "TraceFormat"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "Warn"),
        new MemberDescriptor(KnownType.Castle_Core_Logging_Ilogger, "WarnFormat"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            var invocation = (InvocationExpressionSyntax)c.Node;
            var candidates = LogMethods.Where(x => x.Name == invocation.GetName()).ToArray();
            if (candidates.Any()
                && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
                && Array.Exists(candidates, x => x.ContainingType.Matches(method.ContainingType))
                && method.Parameters.FirstOrDefault(x => Array.Exists(candidates, c => MessageParameterNames.Contains(x.Name))) is { } messageParameter
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
}
