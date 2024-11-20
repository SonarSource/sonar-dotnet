/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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

import java.util.List;
import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.Context;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class AbstractSonarWayProfileTest {

  private static PluginMetadata metadata;
  private static RoslynRules roslynRules;

  @BeforeClass
  public static void beforeAll() {
    metadata = mock(PluginMetadata.class);
    when(metadata.repositoryKey()).thenReturn("REPO");
    when(metadata.languageKey()).thenReturn("LANG");
    when(metadata.resourcesDirectory()).thenReturn("/AbstractSonarWayProfile/");

    RoslynRules.Rule rule1 = new RoslynRules.Rule();
    rule1.id = "S-REAL-1";
    RoslynRules.Rule rule2 = new RoslynRules.Rule();
    rule2.id = "S-REAL-2";
    roslynRules = mock(RoslynRules.class);
    when(roslynRules.rules()).thenReturn(List.of(rule1, rule2));
  }

  @Test
  public void define_createsProfile() {
    AbstractSonarWayProfile sut = new AbstractSonarWayProfile(metadata, roslynRules) {
    };
    Context context = new Context();
    sut.define(context);

    BuiltInQualityProfilesDefinition.BuiltInQualityProfile profile = context.profile("LANG", "Sonar way");
    assertThat(profile).isNotNull();
    assertThat(profile.rules()).extracting(BuiltInQualityProfilesDefinition.BuiltInActiveRule::ruleKey).containsExactlyInAnyOrder("S-REAL-1", "S-REAL-2");
  }

  @Test
  public void define_activateSecurityRules() {
    AbstractSonarWayProfile sut = new AbstractSonarWayProfile(metadata, roslynRules) {
      @Override
      protected void activateSecurityRules(NewBuiltInQualityProfile sonarWay) {
        sonarWay.activateRule("SECURITY-REPO", "SECURITY-RULE");
      }
    };
    Context context = new Context();
    sut.define(context);

    BuiltInQualityProfilesDefinition.BuiltInQualityProfile profile = context.profile("LANG", "Sonar way");
    assertThat(profile).isNotNull();
    assertThat(profile.rule(RuleKey.of("SECURITY-REPO", "SECURITY-RULE"))).isNotNull();
  }
}
