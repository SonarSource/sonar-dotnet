/*
 * SonarVB
 * Copyright (C) 2012-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonar.plugins.vbnet;

import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.BuiltInQualityProfile;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.Context;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;
import org.sonarsource.vbnet.core.VbNetSonarWayProfile;

import static org.assertj.core.api.Assertions.assertThat;

class VbNetSonarWayProfileTest {

  private static final RoslynRules ROSLYN_RULES = new RoslynRules(VbNetPlugin.METADATA);
  private static final String REPOSITORY_KEY = VbNetPlugin.METADATA.repositoryKey();
  private static BuiltInQualityProfile profile;

  @BeforeAll
  static void setup() {
    Context context = new Context();
    VbNetSonarWayProfile profileDef = new VbNetSonarWayProfile(VbNetPlugin.METADATA, ROSLYN_RULES);
    profileDef.define(context);
    profile = context.profile("vbnet", "Sonar way");
  }

  @Test
  void expected_rules_in_sonar_way() {
    // SonarWay rules
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2347"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2349"))).isNotNull();

    // Non Sonarway rules
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2348"))).isNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2354"))).isNull();
  }

  @Test
  void hotspots_in_sonar_way() {
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S4507"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S5042"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2077"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2068"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S1313"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S4790"))).isNotNull();
  }


  @Test
  void symbolic_execution_not_in_sonar_way() {
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2222"))).isNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2259"))).isNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2583"))).isNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2589"))).isNull();
  }
}
