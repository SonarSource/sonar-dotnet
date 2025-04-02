/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins;

import java.util.Set;
import java.util.stream.Collectors;
import javax.annotation.Nullable;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition;
import org.sonarsource.analyzer.commons.BuiltInQualityProfileJsonLoader;

public abstract class AbstractSonarWayProfile implements BuiltInQualityProfilesDefinition {

  protected final PluginMetadata metadata;
  private final RoslynRules roslynRules;
  private final ProfileRegistrar[] profileRegistrars;

  protected AbstractSonarWayProfile(PluginMetadata metadata, RoslynRules roslynRules) {
    this(metadata, roslynRules, null);
  }

  protected AbstractSonarWayProfile(PluginMetadata metadata, RoslynRules roslynRules, @Nullable ProfileRegistrar[] profileRegistrars) {
    this.metadata = metadata;
    this.roslynRules = roslynRules;
    this.profileRegistrars = profileRegistrars;
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
    registerRulesFromRegistrars(sonarWay);
    sonarWay.done();
  }

  private void registerRulesFromRegistrars(NewBuiltInQualityProfile profile) {
    if(profileRegistrars != null) {
      for (var profileRegistrar : profileRegistrars) {
        profileRegistrar.register((languageKey, rules) -> {
          if(languageKey.equals(metadata.languageKey())) {
            for(RuleKey ruleKey : rules) {
              profile.activateRule(ruleKey.repository(), ruleKey.rule());
            }
          }
        });
      }
    }
  }
  protected void activateSecurityRules(NewBuiltInQualityProfile sonarWay) {
  }
}
