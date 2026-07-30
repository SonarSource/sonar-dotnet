/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.TestFramework.Common;

public static class TestEnvironment
{
    public static bool IsCiContext =>
        BuildReason() is not null                                            // Azure DevOps FixMe: NET-4160 Delete when Azure Pipeline is deleted
        || Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";   // GitHub Actions

    public static bool IsPullRequestBuild =>
        BuildReason() == "PullRequest"                                       // Azure DevOps FixMe: NET-4160 Delete when Azure Pipeline is deleted
        || Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME") == "pull_request";   // GitHub Actions

    // Azure DevOps FixMe: NET-4160 Delete when Azure Pipeline is deleted
    public static string BuildReason() =>
        Environment.GetEnvironmentVariable("BUILD_REASON");
}
