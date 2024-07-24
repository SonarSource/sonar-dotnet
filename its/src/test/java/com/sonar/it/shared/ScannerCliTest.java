/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
package com.sonar.it.shared;

import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.SonarScanner;
import java.io.File;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;

import static com.sonar.it.shared.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

/**
 * Regression tests for scanning projects with the scanner-cli.
 */
@ExtendWith(Tests.class)
class ScannerCliTest {
  private static final String RAZOR_PAGES_PROJECT = "WebApplication";
  private static final String HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS = "ScannerCli";

  // Note: setting the `sonar.projectBaseDir` only enables Incremental PR Analysis when used with the Scanner for .NET.
  private static final String INCREMENTAL_PR_ANALYSIS_WARNING = "WARN  Incremental PR analysis: Could not determine common base path, cache will not be computed. Consider setting 'sonar.projectBaseDir' property.";

  @Test
  void givenRazorPagesMainCode_whenScannerForCliIsUsed_logsCSharpWarning() {
    // by default, the `sonar.sources` are in the scan base directory
    SonarScanner scanner = getSonarScanner(RAZOR_PAGES_PROJECT, "projects/" + RAZOR_PAGES_PROJECT);
    BuildResult result = ORCHESTRATOR.executeBuild(scanner);

    assertThat(result.getLogsLines(l -> l.contains("WARN")))
      .hasSize(4)
      .doesNotHaveDuplicates()
      .anyMatch(x -> x.endsWith("WARN  Your project contains C# files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html"))
      .anyMatch(y -> y.endsWith("WARN  Your project contains VB.NET files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html"))
      .anyMatch(w -> w.endsWith(INCREMENTAL_PR_ANALYSIS_WARNING))
      .anyMatch(z -> z.endsWith(INCREMENTAL_PR_ANALYSIS_WARNING));

    // The HTML plugin works
    assertThat(TestUtils.getMeasureAsInt(ORCHESTRATOR, RAZOR_PAGES_PROJECT, "violations")).isEqualTo(2);
    TestUtils.verifyNoGuiWarnings(ORCHESTRATOR, result);
  }

  @Test
  void givenMainHtmlCodeAndTestCSharpCode_whenScannerForCliIsUsed_logsCSharpWarning() {
    SonarScanner scanner = getSonarScanner(HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS, "projects/" + HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS)
      .setSourceDirs("main")
      .setTestDirs("test");
    BuildResult result = ORCHESTRATOR.executeBuild(scanner);

    assertThat(result.getLogsLines(l -> l.contains("WARN")))
      .hasSize(2)
      .doesNotHaveDuplicates()
      .anyMatch(x -> x.endsWith("WARN  Your project contains C# files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html"))
      .anyMatch(y -> y.endsWith(INCREMENTAL_PR_ANALYSIS_WARNING));

    // The HTML plugin works
    assertThat(TestUtils.getMeasureAsInt(ORCHESTRATOR, HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS, "violations")).isEqualTo(2);
    TestUtils.verifyNoGuiWarnings(ORCHESTRATOR, result);
  }

  @Test
  void givenTestHtmlAndCSharpCode_whenScannerForCliIsUsed_logsCSharpWarning() {
    SonarScanner scanner = getSonarScanner(HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS, "projects/" + HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS)
      .setSourceDirs("")
      .setTestDirs("main,test");
    BuildResult result = ORCHESTRATOR.executeBuild(scanner);

    assertThat(result.getLogsLines(l -> l.contains("WARN")))
      .hasSize(2)
      .doesNotHaveDuplicates()
      .anyMatch(y -> y.endsWith("WARN  Your project contains C# files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html"))
      .anyMatch(z -> z.endsWith(INCREMENTAL_PR_ANALYSIS_WARNING));

    TestUtils.verifyNoGuiWarnings(ORCHESTRATOR, result);
  }

  @Test
  void givenTestHtmlCode_whenScannerForCliIsUsed_doesNotLogCsharpWarning() {
    SonarScanner scanner = getSonarScanner(HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS, "projects/" + HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS)
      .setSourceDirs("")
      .setTestDirs("main,test")
      .setProperty("sonar.cs.file.suffixes", ".no_extension");
    BuildResult result = ORCHESTRATOR.executeBuild(scanner);

    assertThat(result.getLogsLines(l -> l.contains("WARN"))).isEmpty();
    TestUtils.verifyNoGuiWarnings(ORCHESTRATOR, result);
  }

  private SonarScanner getSonarScanner(String projectKey, String projectDir) {
    File projectDirPath = new File(projectDir);
    return SonarScanner.create(projectDirPath)
      .setProjectKey(projectKey)
      // This is set just to underline that the message regarding Incremental PR Analysis is confusing when the Scanner for .NET is not used.
      // The Scanner for .NET under the hood sets the `sonar.pullrequest.cache.basepath` property (which is needed by the plugin) based on `sonar.projectBaseDir` property.
      .setProperty("sonar.projectBaseDir", projectDirPath.getAbsolutePath())
      // Without this property the following warning message appear:
      //   WARN: sonar.plugins.downloadOnlyRequired is false, so ALL available plugins will be downloaded
      .setProperty("sonar.plugins.downloadOnlyRequired", "true")
      .setSourceDirs(".");
  }
}
