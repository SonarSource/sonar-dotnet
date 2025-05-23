/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition;
import org.sonarsource.analyzer.commons.BuiltInQualityProfileJsonLoader;

public abstract class AbstractSonarWayProfile implements BuiltInQualityProfilesDefinition {
  private final String languageKey;
  private final String pluginKey;
  private final String repositoryKey;

  protected AbstractSonarWayProfile(String languageKey, String pluginKey, String repositoryKey) {
    this.languageKey = languageKey;
    this.pluginKey = pluginKey;
    this.repositoryKey = repositoryKey;
  }

  @Override
  public void define(Context context) {
    NewBuiltInQualityProfile sonarWay = context.createBuiltInQualityProfile("Sonar way", languageKey);
    String sonarWayJsonPath = getSonarWayJsonPath();
    BuiltInQualityProfileJsonLoader.load(sonarWay, repositoryKey, sonarWayJsonPath);
    activateSecurityRules(sonarWay);
    sonarWay.done();
  }

  private String getSonarWayJsonPath() {
    return "org/sonar/plugins/" + pluginKey + "/Sonar_way_profile.json";
  }

  protected void activateSecurityRules(NewBuiltInQualityProfile sonarWay) {
  }
}
