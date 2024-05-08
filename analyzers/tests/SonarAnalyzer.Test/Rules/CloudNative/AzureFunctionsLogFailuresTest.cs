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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class AzureFunctionsLogFailuresTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<AzureFunctionsLogFailures>().WithBasePath("CloudNative").AddReferences(NuGetMetadataReference.MicrosoftNetSdkFunctions());

        [TestMethod]
        public void AzureFunctionsLogFailures_CS() =>
            builder.AddPaths("AzureFunctionsLogFailures.cs").Verify();

        [DataTestMethod]
        // Calls to LoggerExtensions.LogSomething extension methods
        [DataRow(true, "log.LogError(ex, string.Empty);")]
        [DataRow(true, "log.LogCritical(ex, string.Empty);")]
        [DataRow(true, "log.LogWarning(ex, string.Empty);")]
        [DataRow(true, "log.LogInformation(ex, string.Empty);")]
        [DataRow(false, "log.LogDebug(ex, string.Empty);")]
        [DataRow(false, "log.LogTrace(ex, string.Empty);")]
        // LoggerExtensions.Log with LogLevel parameter
        [DataRow(true, "log.Log(LogLevel.Information, ex, string.Empty);")]
        [DataRow(true, "log.Log(LogLevel.Warning, ex, string.Empty);")]
        [DataRow(true, "log.Log(LogLevel.Error, ex, string.Empty);")]
        [DataRow(true, "log.Log(LogLevel.Critical, ex, string.Empty);")]
        [DataRow(false, "log.Log(LogLevel.Trace, ex, string.Empty);")]
        [DataRow(false, "log.Log(LogLevel.Debug, ex, string.Empty);")]
        [DataRow(false, "log.Log(LogLevel.None, ex, string.Empty);")]
        // Calls with complications in it
        [DataRow(true, "log.Log(LogLevel.Critical, string.Empty);")] // It is not required to pass the exception to the log call
        [DataRow(true, "log.Log(exception: ex, message: string.Empty, logLevel: LogLevel.Error);")] // Out of order named args
        [DataRow(true, "log.Log(message: string.Empty, logLevel: LogLevel.Error);")]
        [DataRow(true, @"log.Log((LogLevel)Enum.Parse(typeof(LogLevel), ""Trace""), string.Empty);")] // call is compliant, if LogLevel is not known at compile time
        // Calls to ILogger.Log
        [DataRow(true, "log.Log(LogLevel.Error, new EventId(), (object)null, ex, (s, e) => string.Empty);")]
        [DataRow(true, "log.Log(eventId: new EventId(), state: (object)null, exception: ex, formatter: (s, e) => string.Empty, logLevel: LogLevel.Error);")]
        [DataRow(false, "log.Log(eventId: new EventId(), state: (object)null, exception: ex, formatter: (s, e) => string.Empty, logLevel: LogLevel.Trace);")]
        // Receiver is complicated expression
        [DataRow(true, "((ILogger)log).Log(LogLevel.Critical, string.Empty);")]
        [DataRow(false, "((ILogger)log).Log(LogLevel.Trace, string.Empty);")]
        [DataRow(true, "new Func<ILogger>(()=>log)().Log(LogLevel.Critical, string.Empty);")]
        [DataRow(true, "new Func<ILogger>(()=>log)().Log(LogLevel.Error, new EventId(), (object)null, ex, (s, e) => string.Empty);")]
        // Non logging methods
        [DataRow(false, "log.BeginScope(string.Empty, string.Empty);")]
        [DataRow(false, "log.BeginScope<object>(null);")]
        [DataRow(false, "log.IsEnabled(LogLevel.Warning);")]
        public void AzureFunctionsLogFailures_VerifyLoggerCalls(bool isCompliant, string loggerInvocation)
        {
            var x = builder.AddSnippet($$"""
                using Microsoft.Azure.WebJobs;
                using Microsoft.Extensions.Logging;
                using System;

                    public static class Function1
                    {
                        [FunctionName("Function1")]
                        public static void Run(ILogger log)
                        {
                            try { }
                            catch(Exception ex) // {{(isCompliant ? "Compliant" : "Noncompliant")}}
                            {
                                {{loggerInvocation}} // {{(isCompliant ? string.Empty : "Secondary")}}
                            }
                        }
                    }
                """);
            if (isCompliant)
            {
                x.VerifyNoIssues();
            }
            else
            {
                x.Verify();
            }
        }
    }
}
