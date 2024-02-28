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

using static Roslyn.Utilities.SonarAnalyzer.Shared.LoggingFrameworkMethods;

namespace SonarAnalyzer.Rules.MessageTemplates;

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
