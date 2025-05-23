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
package com.sonar.it.csharp;

import com.sonar.orchestrator.http.HttpMethod;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStreamReader;
import java.nio.file.Path;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
class QualityProfileExportTest {

  @TempDir
  private static Path temp;

  @Test
  void sonarLintRuleSet_can_be_downloaded_from_SonarQube_UI() throws Exception {
    // Regression test for SONAR-6969: This URL is called to manually download a ruleset for SL4VS from SQ UI
    File file = ORCHESTRATOR.getServer()
      .newHttpCall("/api/qualityprofiles/export")
      .setAdminCredentials()
      .setMethod(HttpMethod.GET)
      .setParam("exporterKey", "sonarlint-vs-cs")
      .setParam("language", "cs")
      .setParam("qualityProfile", "Sonar way")
      .setParam("organization", "default-organization")
      .downloadToDirectory(temp.toFile());

    assertThat(file).exists();
    BufferedReader reader = new BufferedReader(new InputStreamReader(new FileInputStream(file)));
    assertThat(reader.lines()).contains(
      "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
      "<RuleSet Name=\"Rules for SonarLint\" Description=\"This rule set was automatically generated from SonarQube.\" ToolsVersion=\"14.0\">",
      "  <Rules AnalyzerId=\"SonarAnalyzer.CSharp\" RuleNamespace=\"SonarAnalyzer.CSharp\">");
    reader.close();
  }
}
