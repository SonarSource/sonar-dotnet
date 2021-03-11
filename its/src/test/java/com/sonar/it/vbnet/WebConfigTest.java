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
package com.sonar.it.vbnet;

import com.sonar.it.shared.TestUtils;
import com.sonar.it.shared.WebConfigBase;
import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;
import org.sonarqube.ws.Hotspots;

import java.util.List;

import static com.sonar.it.vbnet.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

public class WebConfigTest extends WebConfigBase {

  @Before
  public void init() {
    TestUtils.reset(ORCHESTRATOR);
  }

  @Test
  public void should_raise_hotspot_on_web_config() throws Exception {
    final String projectName = "WebConfig.Vb";

    Tests.analyzeProject(temp, projectName, null);
    List<Hotspots.SearchWsResponse.Hotspot> hotspots = Tests.getHotspots(projectName);
    // One from project directory, one from PathOutsideProjectRoot added with Directory.Build.props
    assertThat(hotspots.size()).isEqualTo(2);
    assertHotspot(hotspots.get(0), 6, "PathOutsideProjectRoot\\Web.config");
    assertHotspot(hotspots.get(1), 4, "WebConfig.VB\\Web.config");
  }
}
