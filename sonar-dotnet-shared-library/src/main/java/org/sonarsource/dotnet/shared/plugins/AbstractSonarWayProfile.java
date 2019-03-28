/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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

import org.sonar.api.SonarRuntime;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition;
import org.sonar.api.utils.Version;
import org.sonarsource.analyzer.commons.BuiltInQualityProfileJsonLoader;

import javax.annotation.Nullable;

public abstract class AbstractSonarWayProfile implements BuiltInQualityProfilesDefinition {
  private final boolean supportsSecurityHotspots;
  private static final Version SQ_7_3 = Version.create(7, 3);
  private final String languageKey;
  private final String pluginKey;
  private final String repositoryKey;

  protected AbstractSonarWayProfile(@Nullable SonarRuntime sonarRuntime, String languageKey, String pluginKey, String repositoryKey) {
    this.supportsSecurityHotspots = sonarRuntime != null && sonarRuntime.getApiVersion().isGreaterThanOrEqual(SQ_7_3);
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
    return supportsSecurityHotspots
      ? ("org/sonar/plugins/" + pluginKey + "/Sonar_way_profile.json")
      : ("org/sonar/plugins/" + pluginKey + "/Sonar_way_profile_no_hotspot.json");
  }

  protected void activateSecurityRules(NewBuiltInQualityProfile sonarWay) {
  }
}
