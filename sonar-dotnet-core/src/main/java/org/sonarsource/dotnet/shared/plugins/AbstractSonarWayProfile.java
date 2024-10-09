/*
 * SonarSource :: .NET :: Core
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

import java.util.Set;
import java.util.stream.Collectors;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition;
import org.sonarsource.analyzer.commons.BuiltInQualityProfileJsonLoader;

public abstract class AbstractSonarWayProfile implements BuiltInQualityProfilesDefinition {

  protected final PluginMetadata metadata;
  private final RoslynRules roslynRules;

  protected AbstractSonarWayProfile(PluginMetadata metadata, RoslynRules roslynRules) {
    this.metadata = metadata;
    this.roslynRules = roslynRules;
  }

  @Override
  public void define(Context context) {
    NewBuiltInQualityProfile sonarWay = context.createBuiltInQualityProfile("Sonar way", metadata.languageKey());
    String sonarWayJsonPath = metadata.resourcesDirectory() + "/Sonar_way_profile.json";
    Set<String> roslynRuleIDs = roslynRules.rules().stream().map(RoslynRules.Rule::getId).collect(Collectors.toSet());
    for (String ruleID : BuiltInQualityProfileJsonLoader.loadActiveKeysFromJsonProfile(sonarWayJsonPath)) {
      if (roslynRuleIDs.contains(ruleID)) {
        sonarWay.activateRule(metadata.repositoryKey(), ruleID);
      }
    }
    activateSecurityRules(sonarWay);
    sonarWay.done();
  }

  protected void activateSecurityRules(NewBuiltInQualityProfile sonarWay) {
  }
}
