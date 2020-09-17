/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2020 SonarSource SA
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
 *
 * Note that this uses a different orchestrator instance than {@link com.sonar.it.csharp.Tests} or {@link com.sonar.it.vbnet.Tests}
 */
public class ScannerCliTest {
  private static final String RAZOR_PAGES_PROJECT = "WebApplication";

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
  public void init(){
    TestUtils.deleteLocalCache();
  }

  @Test
  public void scannerCliWithRazorPages() {
    // As done for java, "AppSec" mode is activated to properly test sanitizers,
    // so every unknown method is processes as a pass-through for all arguments
    SonarScanner scanner = getSonarScanner(RAZOR_PAGES_PROJECT, "projects/" + RAZOR_PAGES_PROJECT);
    BuildResult result = ORCHESTRATOR.executeBuild(scanner);

    assertThat(result.getLogsLines(l -> l.contains("WARN")))
      .containsExactlyInAnyOrder(
        "WARN: No protobuf reports found. The C# files will not have highlighting and metrics.",
        "WARN: No Roslyn issue reports were found. The C# files have not been analyzed.",
        "WARN: Your project contains C# files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET files, you must use the Scanner for MSBuild 4.x, read more on https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html . For any questions you may have, open a topic on https://community.sonarsource.com .",
        "WARN: No protobuf reports found. The VB.NET files will not have highlighting and metrics.",
        "WARN: No Roslyn issue reports were found. The VB.NET files have not been analyzed.",
        "WARN: Your project contains VB.NET files which cannot be analyzed with the scanner you are using. To analyze C# or VB.NET files, you must use the Scanner for MSBuild 4.x, read more on https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html . For any questions you may have, open a topic on https://community.sonarsource.com ."
      );
    // The HTML plugin works
    assertThat(TestUtils.getMeasureAsInt(ORCHESTRATOR, RAZOR_PAGES_PROJECT, "violations")).isEqualTo(1);
  }

  private SonarScanner getSonarScanner(String projectKey, String projectDir) {
    return SonarScanner.create(new File(projectDir))
      .setProjectKey(projectKey)
      .setSourceDirs(".");
  }
}
