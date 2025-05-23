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
package com.sonar.it.vbnet;

import com.sonar.it.shared.WebConfigBase;
import java.io.IOException;
import java.util.List;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.sonarqube.ws.Hotspots;

import static org.assertj.core.api.Assertions.assertThat;

@ExtendWith(Tests.class)
public class WebConfigTest extends WebConfigBase {

  private static final String PROJECT = "WebConfig.VB";

  @Test
  public void should_raise_hotspot_on_web_config() throws IOException {
    Tests.analyzeProject(temp, PROJECT);
    List<Hotspots.SearchWsResponse.Hotspot> hotspots = Tests.getHotspots(PROJECT);
    // One from project directory, one from PathOutsideProjectRoot added with Directory.Build.props
    assertThat(hotspots).hasSize(2);
    assertRequestValidationHotspot(hotspots.get(0), 6, "PathOutsideProjectRoot/Web.config");
    assertRequestValidationHotspot(hotspots.get(1), 4, "WebConfig.VB/Web.config");
  }
}
