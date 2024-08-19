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

import com.sonar.it.shared.WebConfigBase;
import java.io.IOException;
import java.util.List;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.sonarqube.ws.Hotspots.SearchWsResponse.Hotspot;
import org.sonarqube.ws.Issues;

import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
class WebConfigTest extends WebConfigBase {

  static final String PROJECT = "WebConfig.CSharp";

  @Test
  void should_raise_hotspot_on_web_config() throws IOException {
    Tests.analyzeProject(temp, PROJECT);
    List<Hotspot> hotspots = Tests.getHotspots(PROJECT);
    // One from project directory, one from PathOutsideProjectRoot added with Directory.Build.props
    assertThat(hotspots).hasSize(6);
    assertRequestValidationHotspot(hotspots.get(0), 6, "PathOutsideProjectRoot/Web.config");
    assertRequestValidationHotspot(hotspots.get(1), 4, "WebConfig.CSharp/Web.config");
    assertContentLengthHotspot(hotspots.get(2), 5, "PathOutsideProjectRoot/Web.config");
    assertContentLengthHotspot(hotspots.get(3), 14, "PathOutsideProjectRoot/Web.config");
    assertContentLengthHotspot(hotspots.get(4), 6, "WebConfig.CSharp/Web.config");
    assertContentLengthHotspot(hotspots.get(5), 14, "WebConfig.CSharp/Web.config");

    List<Issues.Issue> issues = Tests.getIssues(PROJECT);
    assertThat(issues).hasSize(7);
    assertIssue(issues.get(0), 9, "PathOutsideProjectRoot/Web.config");
    assertIssue(issues.get(1), 3, "PathOutsideProjectRoot/appsettings.json");
    assertIssue(issues.get(5), 9, "WebConfig.CSharp/Web.config");
    assertIssue(issues.get(6), 3, "WebConfig.CSharp/appsettings.json");
  }
}
