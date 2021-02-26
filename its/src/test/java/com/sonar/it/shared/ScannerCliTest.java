/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2021 SonarSource SA
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

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.SonarScanner;
import com.sonar.orchestrator.container.Edition;
import com.sonar.orchestrator.locator.MavenLocation;
import java.io.File;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;

/**
 * Regression tests for scanning projects with the scanner-cli.
 * <p>
 * Note that this uses a different orchestrator instance than {@link com.sonar.it.csharp.Tests} or {@link com.sonar.it.vbnet.Tests}
 */
public class ScannerCliTest {
  private static final String RAZOR_PAGES_PROJECT = "WebApplication";
  private static final String HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS = "ScannerCli";

  @ClassRule
  public static final Orchestrator ORCHESTRATOR = Orchestrator.builderEnv()
    .setSonarVersion(TestUtils.replaceLtsVersion(System.getProperty("sonar.runtimeVersion", "LATEST_RELEASE")))
    .addPlugin(TestUtils.getPluginLocation("sonar-csharp-plugin"))
    .addPlugin(TestUtils.getPluginLocation("sonar-vbnet-plugin"))
    // Fixed version for the HTML plugin as we don't want to have failures in case of changes there.
    .addPlugin(MavenLocation.of("org.sonarsource.html", "sonar-html-plugin", "3.2.0.2082"))
    .setEdition(Edition.DEVELOPER)
    .activateLicense()
    .build();

  @Before
  public void init() {
    TestUtils.deleteLocalCache();
    TestUtils.reset(ORCHESTRATOR);
  }

  @Test
  public void givenRazorPagesMainCode_whenScannerForCliIsUsed_logsCSharpWarning() {
    // by default, the `sonar.sources` are in the scan base directory
    SonarScanner scanner = getSonarScanner(RAZOR_PAGES_PROJECT, "projects/" + RAZOR_PAGES_PROJECT);
    BuildResult result = ORCHESTRATOR.executeBuild(scanner);

    assertThat(result.getLogsLines(l -> l.contains("WARN")))
      .containsExactlyInAnyOrder(
        "WARN: Your project contains C# files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html",
        "WARN: Your project contains VB.NET files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html"
      );
    // The HTML plugin works
    assertThat(TestUtils.getMeasureAsInt(ORCHESTRATOR, RAZOR_PAGES_PROJECT, "violations")).isEqualTo(2);
    TestUtils.verifyNoGuiWarnings(ORCHESTRATOR, result);
  }

  @Test
  public void givenMainHtmlCodeAndTestCSharpCode_whenScannerForCliIsUsed_logsCSharpWarning() {
    SonarScanner scanner = getSonarScanner(HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS, "projects/" + HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS)
      .setSourceDirs("main")
      .setTestDirs("test");
    BuildResult result = ORCHESTRATOR.executeBuild(scanner);

    assertThat(result.getLogsLines(l -> l.contains("WARN")))
      .containsExactlyInAnyOrder(
        "WARN: Your project contains C# files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html"
      );
    // The HTML plugin works
    assertThat(TestUtils.getMeasureAsInt(ORCHESTRATOR, HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS, "violations")).isEqualTo(2);
    TestUtils.verifyNoGuiWarnings(ORCHESTRATOR, result);
  }

  @Test
  public void givenTestHtmlAndCSharpCode_whenScannerForCliIsUsed_logsCSharpWarning() {
    SonarScanner scanner = getSonarScanner(HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS, "projects/" + HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS)
      .setSourceDirs("")
      .setTestDirs("main,test");
    BuildResult result = ORCHESTRATOR.executeBuild(scanner);

    assertThat(result.getLogsLines(l -> l.contains("WARN")))
      .containsExactlyInAnyOrder(
        "WARN: Your project contains C# files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html"
      );
    TestUtils.verifyNoGuiWarnings(ORCHESTRATOR, result);
  }

  @Test
  public void givenTestHtmlCode_whenScannerForCliIsUsed_doesNotLogCsharpWarning() {
    SonarScanner scanner = getSonarScanner(HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS, "projects/" + HTML_IN_MAIN_AND_CSHARP_IN_TEST_SUBFOLDERS)
      .setSourceDirs("")
      .setTestDirs("main,test")
      .setProperty("sonar.cs.file.suffixes=", ".no_extension");
    BuildResult result = ORCHESTRATOR.executeBuild(scanner);

    assertThat(result.getLogsLines(l -> l.contains("WARN"))).isEmpty();
    TestUtils.verifyNoGuiWarnings(ORCHESTRATOR, result);
  }

  private SonarScanner getSonarScanner(String projectKey, String projectDir) {
    return SonarScanner.create(new File(projectDir))
      .setProjectKey(projectKey)
      .setSourceDirs(".");
  }
}
