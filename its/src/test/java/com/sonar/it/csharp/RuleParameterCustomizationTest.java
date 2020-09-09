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

package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import java.util.List;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getComponent;
import static com.sonar.it.csharp.Tests.getHotspots;
import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarqube.ws.Hotspots.SearchWsResponse.Hotspot;

public class RuleParameterCustomizationTest {
  private static final String LANGUAGE_KEY = "cs";
  private static final String PROFILE_NAME = "custom_parameters";
  private static final String CustomParametersProjectName = "DefaultRuleParametersCanBeCustomized";

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  @Before
  public void init() {
    TestUtils.reset(ORCHESTRATOR);

    provisionProject();
  }

  @Test
  public void doNotHardcodeCredentials_defaultParameters_canBeCustomized() throws Exception {
    Tests.analyzeProject(temp, CustomParametersProjectName, null);

    final String componentKey = "DefaultRuleParametersCanBeCustomized:DefaultRuleParametersCanBeCustomized/DefaultRuleParametersCanBeCustomized.cs";

    assertThat(getComponent(componentKey)).isNotNull();

    List<Hotspot> hotspots = getHotspots(CustomParametersProjectName);

    assertThat(hotspots.size()).isEqualTo(1);

    assertHotspot(hotspots.get(0), 7, "\"senha\" detected here, make sure this is not a hard-coded credential.");
  }

  private void assertHotspot(Hotspot hotspot, int line, String message){
    assertThat(hotspot.getLine()).isEqualTo(line);
    assertThat(hotspot.getMessage()).isEqualTo(message);
  }

  private void provisionProject() {
    ORCHESTRATOR.getServer().provisionProject(CustomParametersProjectName, CustomParametersProjectName);
    ORCHESTRATOR.getServer().associateProjectToQualityProfile(CustomParametersProjectName, LANGUAGE_KEY, PROFILE_NAME);
  }
}
