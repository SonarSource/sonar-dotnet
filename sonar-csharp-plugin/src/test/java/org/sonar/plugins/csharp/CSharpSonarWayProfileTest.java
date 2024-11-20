/*
 * SonarC#
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
package org.sonar.plugins.csharp;

import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.BuiltInQualityProfile;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.Context;
import org.sonarsource.csharp.core.CSharpSonarWayProfile;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

import static org.assertj.core.api.Assertions.assertThat;

class CSharpSonarWayProfileTest {

  private static final RoslynRules ROSLYN_RULES = new RoslynRules(CSharpPlugin.METADATA);
  private static final String REPOSITORY_KEY = CSharpPlugin.METADATA.repositoryKey();
  private static BuiltInQualityProfile profile;

  @BeforeAll
  static void setup() {
    Context context = new Context();
    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(CSharpPlugin.METADATA, ROSLYN_RULES);
    profileDef.define(context);
    profile = context.profile("cs", "Sonar way");
  }

  @Test
  void expected_rules_in_sonar_way() {
    // SonarWay rules
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2198"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2223"))).isNotNull();

    // Non Sonarway rules
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2330"))).isNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2952"))).isNull();
  }

  @Test
  void hotspots_in_sonar_way() {
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S1313"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2068"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2092"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2245"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S3330"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S4507"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S4790"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S5042"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2077"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S5766"))).isNotNull();
  }

  @Test
  void symbolic_execution_not_in_sonar_way() {
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2222"))).isNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2259"))).isNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2583"))).isNull();
    assertThat(profile.rule(RuleKey.of(REPOSITORY_KEY, "S2589"))).isNull();
  }
}
