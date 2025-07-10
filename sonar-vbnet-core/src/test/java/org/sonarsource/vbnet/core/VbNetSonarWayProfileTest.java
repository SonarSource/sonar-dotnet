/*
 * SonarSource :: VB.NET :: Core
 * Copyright (C) 2012-2025 SonarSource SA
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
package org.sonarsource.vbnet.core;

import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition;
import org.sonar.plugins.vbnetenterprise.api.ProfileRegistrar;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

import java.util.ArrayList;
import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

class VbNetSonarWayProfileTest {
  private static RoslynRules roslynRules;
  private static final PluginMetadata metadata = new TestVbNetMetadata() {
    @Override
    public String resourcesDirectory() {
      return "VbNetSonarWayProfileTest";
    }
  };

  @BeforeAll
  static void beforeAll() {
    roslynRules = mock(RoslynRules.class);
    when(roslynRules.rules()).thenReturn(new ArrayList<>());
  }

  @Test
  void does_not_throw() {
    assertDoesNotThrow(() -> new VbNetSonarWayProfile(TestVbNetMetadata.INSTANCE, mock(RoslynRules.class)));
  }

  @Test
  void sonar_way_can_be_define_with_no_profile_registrars_given() {
    assertDoesNotThrow(() -> {
      VbNetSonarWayProfile sonarWay = new VbNetSonarWayProfile(metadata, roslynRules);
      sonarWay.define(new BuiltInQualityProfilesDefinition.Context());
    });
  }

  @Test
  void profile_registrars_can_add_rules_to_sonar_way() {
    BuiltInQualityProfilesDefinition.Context context = new BuiltInQualityProfilesDefinition.Context();
    ProfileRegistrar[] profileRegistrars = new ProfileRegistrar[] {
      r -> r.registerDefaultQualityProfileRules(
        List.of(
          RuleKey.of(metadata.repositoryKey(), "additionalRule1"),
          RuleKey.of(metadata.repositoryKey(), "additionalRule2"))),
      r -> r.registerDefaultQualityProfileRules(
        List.of(RuleKey.of(metadata.repositoryKey(), "anotherRule1")))};
    VbNetSonarWayProfile sonarWay = new VbNetSonarWayProfile(metadata, roslynRules, profileRegistrars);
    sonarWay.define(context);

    BuiltInQualityProfilesDefinition.BuiltInQualityProfile builtIn = context.profile("vbnet", "Sonar way");
    assertThat(builtIn.language()).isEqualTo(metadata.languageKey());
    List.of("additionalRule1", "additionalRule2", "anotherRule1")
      .forEach(
        ruleKey -> assertThat(builtIn.rule(RuleKey.of(metadata.repositoryKey(), ruleKey))).isNotNull());
  }
}
