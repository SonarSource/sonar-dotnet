/*
 * SonarC#
 * Copyright (C) 2014-2017 SonarSource SA
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
package org.sonar.plugins.csharp;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.config.internal.MapSettings;

import static org.assertj.core.api.Assertions.assertThat;

public class CSharpConfigurationTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private Path workDir;
  private MapSettings settings = new MapSettings();

  private CSharpConfiguration config;

  @Before
  public void setUp() {
    workDir = temp.getRoot().toPath();
  }

  private void createProtobufOut() throws IOException {
    Path path = workDir.resolve("report");
    Path outputCs = path.resolve("output-cs");
    Files.createDirectories(outputCs);
    Files.createFile(outputCs.resolve("dummy.pb"));
    settings.setProperty(CSharpConfiguration.ANALYZER_PROJECT_OUT_PATH_PROPERTY_KEY, path.toString());
  }

  private void createRoslynOut() throws IOException {
    Path path = temp.newFile("roslyn-report.json").toPath();
    settings.setProperty(CSharpConfiguration.ROSLYN_REPORT_PATH_PROPERTY_KEY, path.toString());
  }

  @Test
  public void onlyRoslynReportPresent() throws IOException {
    createRoslynOut();
    config = new CSharpConfiguration(settings.asConfig());
    assertThat(config.protobufReportPath()).isNotPresent();
    assertThat(config.roslynReportPath().get()).isEqualTo(workDir.resolve("roslyn-report.json"));
  }

  @Test
  public void onlyProtobufReportsPresent() throws IOException {
    createProtobufOut();
    config = new CSharpConfiguration(settings.asConfig());
    assertThat(config.protobufReportPath()).isPresent();
    assertThat(config.roslynReportPath()).isNotPresent();
    assertThat(config.protobufReportPath().get()).isEqualTo(workDir.resolve("report").resolve("output-cs"));
  }
}
