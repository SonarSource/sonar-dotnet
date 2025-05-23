﻿/*
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

namespace SonarAnalyzer.Rules
{
    [Obsolete("This rule has been deprecated since 9.16")]
    public abstract class ConfiguringLoggersBase<TSyntaxKind> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4792";
        protected const string MessageFormat = "Make sure that this logger's configuration is safe.";

        protected ConfiguringLoggersBase(IAnalyzerConfiguration configuration) : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var inv = Language.Tracker.Invocation;
            var pa = Language.Tracker.PropertyAccess;
            var oc = Language.Tracker.ObjectCreation;
            // ASP.NET Core
            inv.Track(
                input,
                inv.MatchMethod(
                    new MemberDescriptor(KnownType.Microsoft_AspNetCore_Hosting_WebHostBuilderExtensions, "ConfigureLogging"),
                    new MemberDescriptor(KnownType.Microsoft_Extensions_DependencyInjection_LoggingServiceCollectionExtensions, "AddLogging"),
                    new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_ConsoleLoggerExtensions, "AddConsole"),
                    new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_DebugLoggerFactoryExtensions, "AddDebug"),
                    new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_EventLoggerFactoryExtensions, "AddEventLog"),
                    new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_EventLoggerFactoryExtensions, "AddEventSourceLogger"),
                    new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_EventSourceLoggerFactoryExtensions, "AddEventSourceLogger"),
                    new MemberDescriptor(KnownType.Microsoft_Extensions_Logging_AzureAppServicesLoggerFactoryExtensions, "AddAzureWebAppDiagnostics")),
                inv.MethodIsExtension());

            oc.Track(input, oc.WhenImplements(KnownType.Microsoft_Extensions_Logging_ILoggerFactory));

            // log4net
            inv.Track(
                input,
                inv.MatchMethod(
                    new MemberDescriptor(KnownType.log4net_Config_XmlConfigurator, "Configure"),
                    new MemberDescriptor(KnownType.log4net_Config_XmlConfigurator, "ConfigureAndWatch"),
                    new MemberDescriptor(KnownType.log4net_Config_DOMConfigurator, "Configure"),
                    new MemberDescriptor(KnownType.log4net_Config_DOMConfigurator, "ConfigureAndWatch"),
                    new MemberDescriptor(KnownType.log4net_Config_BasicConfigurator, "Configure")));

            // NLog
            pa.Track(
                input,
                pa.MatchSetter(),
                pa.MatchProperty(new MemberDescriptor(KnownType.NLog_LogManager, "Configuration")));

            // Serilog
            oc.Track(
                input,
                oc.WhenDerivesFrom(KnownType.Serilog_LoggerConfiguration));
        }
    }
}
