/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ConfiguringLoggersBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4792";
        protected const string MessageFormat = "Make sure that this logger's configuration is safe.";

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }
        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            // ASP.NET Core
            InvocationTracker.Track(context,
                InvocationTracker.MatchSimpleNames(
                    new MethodSignature(KnownType.Microsoft_AspNetCore_Hosting_WebHostBuilderExtensions, "ConfigureLogging"),
                    new MethodSignature(KnownType.Microsoft_Extensions_DependencyInjection_LoggingServiceCollectionExtensions, "AddLogging"),

                    new MethodSignature(KnownType.Microsoft_Extensions_Logging_ConsoleLoggerExtensions, "AddConsole"),
                    new MethodSignature(KnownType.Microsoft_Extensions_Logging_DebugLoggerFactoryExtensions, "AddDebug"),
                    new MethodSignature(KnownType.Microsoft_Extensions_Logging_EventLoggerFactoryExtensions, "AddEventLog"),
                    new MethodSignature(KnownType.Microsoft_Extensions_Logging_EventLoggerFactoryExtensions, "AddEventSourceLogger"),
                    new MethodSignature(KnownType.Microsoft_Extensions_Logging_EventSourceLoggerFactoryExtensions, "AddEventSourceLogger"),
                    new MethodSignature(KnownType.Microsoft_Extensions_Logging_AzureAppServicesLoggerFactoryExtensions, "AddAzureWebAppDiagnostics")),
                InvocationTracker.IsExtensionMethod());

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.WhenImplements(KnownType.Microsoft_Extensions_Logging_ILoggerFactory));


            // log4net
            InvocationTracker.Track(context,
                InvocationTracker.MatchSimpleNames(
                    new MethodSignature(KnownType.log4net_Config_XmlConfigurator, "Configure"),
                    new MethodSignature(KnownType.log4net_Config_XmlConfigurator, "ConfigureAndWatch"),

                    new MethodSignature(KnownType.log4net_Config_DOMConfigurator, "Configure"),
                    new MethodSignature(KnownType.log4net_Config_DOMConfigurator, "ConfigureAndWatch"),

                    new MethodSignature(KnownType.log4net_Config_BasicConfigurator, "Configure")));

        }
    }
}
