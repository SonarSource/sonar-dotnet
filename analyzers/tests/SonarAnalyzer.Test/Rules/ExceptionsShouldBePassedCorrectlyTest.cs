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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ExceptionsShouldBePassedCorrectlyTest
{
    private const string EventIdParameter = "new EventId(1),";
    private const string LogLevelParameter = "LogLevel.Warning,";
    private const string NoParameter = "";
    private readonly VerifierBuilder builder = new VerifierBuilder<ExceptionsShouldBePassedCorrectly>();

    [TestMethod]
    public void ExceptionsShouldBePassedCorrectly_CS() =>
        builder.AddPaths("ExceptionsShouldBePassedCorrectly.cs").Verify();

    [DataTestMethod]
    [DataRow("Log", LogLevelParameter)]
    [DataRow("Log", LogLevelParameter, EventIdParameter)]
    [DataRow("LogCritical")]
    [DataRow("LogCritical", NoParameter, EventIdParameter)]
    [DataRow("LogDebug")]
    [DataRow("LogDebug", NoParameter, EventIdParameter)]
    [DataRow("LogError")]
    [DataRow("LogError", NoParameter, EventIdParameter)]
    [DataRow("LogInformation")]
    [DataRow("LogInformation", NoParameter, EventIdParameter)]
    [DataRow("LogTrace")]
    [DataRow("LogTrace", NoParameter, EventIdParameter)]
    [DataRow("LogWarning")]
    [DataRow("LogWarning", NoParameter, EventIdParameter)]
    public void ExceptionsShouldBeLogged_MicrosoftExtensionsLogging_NonCompliant_CS(string methodName, string logLevel = "", string eventId = "") =>
        builder.AddSnippet($$"""
            using System;
            using Microsoft.Extensions.Logging;
            using Microsoft.Extensions.Logging.Abstractions;

            public class Program
            {
                public void Method(ILogger logger, string message)
                {
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}({{logLevel}}{{eventId}} "An exception occured {Exception}.", e);          // Noncompliant
                        logger.{{methodName}}({{logLevel}}{{eventId}} "Expected exception.", e);                        // Noncompliant
                        logger.{{methodName}}({{logLevel}}{{eventId}} "Expected exception.", e, e);                     // Noncompliant
                                                                                                                        // Noncompliant@-1
                        LoggerExtensions.{{methodName}}(logger, {{logLevel}}{{eventId}} "Expected exception.", e);      // Noncompliant
                        LoggerExtensions.{{methodName}}(logger, {{logLevel}}{{eventId}} "Expected exception.", e, e);   // Noncompliant
                                                                                                                        // Noncompliant@-1
                    }
                    try { }
                    catch (Exception e)
                    {
                        logger.{{methodName}}({{logLevel}}{{eventId}} e, "Expected exception.");
                        logger.{{methodName}}({{logLevel}}{{eventId}} e, "Expected exception.", e);
                        LoggerExtensions.{{methodName}}(logger, {{logLevel}}{{eventId}} e, "Expected exception.");
                        LoggerExtensions.{{methodName}}(logger, {{logLevel}}{{eventId}} e, "Expected exception.", e);
                    }
                }
            }
            """)
           .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
           .Verify();
}
