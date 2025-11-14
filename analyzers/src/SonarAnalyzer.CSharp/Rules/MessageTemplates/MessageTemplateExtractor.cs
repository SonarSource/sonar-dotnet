/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using static Roslyn.Utilities.SonarAnalyzer.Shared.LoggingFrameworkMethods;

namespace SonarAnalyzer.CSharp.Rules.MessageTemplates;

internal static class MessageTemplateExtractor
{
    public static ArgumentSyntax TemplateArgument(InvocationExpressionSyntax invocation, SemanticModel model) =>
       TemplateArgument(invocation, model, KnownType.Microsoft_Extensions_Logging_LoggerExtensions, MicrosoftExtensionsLogging, "message")
       ?? TemplateArgument(invocation, model, KnownType.Serilog_Log, Serilog, "messageTemplate")
       ?? TemplateArgument(invocation, model, KnownType.Serilog_ILogger, Serilog, "messageTemplate", checkDerivedTypes: true)
       ?? TemplateArgument(invocation, model, KnownType.NLog_ILoggerExtensions, NLogLoggingMethods, "message")
       ?? TemplateArgument(invocation, model, KnownType.NLog_ILogger, NLogLoggingMethods, "message", checkDerivedTypes: true)
       ?? TemplateArgument(invocation, model, KnownType.NLog_ILoggerBase, NLogILoggerBase, "message", checkDerivedTypes: true);

    private static ArgumentSyntax TemplateArgument(InvocationExpressionSyntax invocation,
                                                   SemanticModel model,
                                                   KnownType type,
                                                   ICollection<string> methods,
                                                   string template,
                                                   bool checkDerivedTypes = false) =>
        methods.Contains(invocation.GetIdentifier().ToString())
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
        && method.HasContainingType(type, checkDerivedTypes)
        && CSharpFacade.Instance.MethodParameterLookup(invocation, method) is { } lookup
        && lookup.TryGetSyntax(template, out var argumentsFound) // Fetch Argument.Expression with IParameterSymbol.Name == templateName
        && argumentsFound.Length == 1
            ? (ArgumentSyntax)argumentsFound[0].Parent
            : null;
}
