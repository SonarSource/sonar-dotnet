/*
 * SonarVB
 * Copyright (C) 2012-2024 SonarSource SA
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
package org.sonar.plugins.vbnet;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.RegisterExtension;
import org.slf4j.event.Level;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.BuiltInQualityProfile;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.Context;
import org.sonar.api.testfixtures.log.LogTesterJUnit5;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;
import org.sonarsource.vbnet.core.VbNetSonarWayProfile;

import static org.assertj.core.api.Assertions.assertThat;

class VbNetSonarWayProfileTest {
  @RegisterExtension
  public LogTesterJUnit5 logTester = new LogTesterJUnit5().setLevel(Level.DEBUG);

  private static final RoslynRules ROSLYN_RULES = new RoslynRules(VbNetPlugin.METADATA);

  @Test
  void hotspots_in_sonar_way() {
    Context context = new Context();
    String repositoryKey = VbNetPlugin.METADATA.repositoryKey();

    VbNetSonarWayProfile profileDef = new VbNetSonarWayProfile(VbNetPlugin.METADATA, ROSLYN_RULES);
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("vbnet", "Sonar way");
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S4507"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S5042"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2077"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2068"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S1313"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S4790"))).isNotNull();
  }


  @Test
  void symbolic_execution_not_in_sonar_way() {
    Context context = new Context();
    String repositoryKey = VbNetPlugin.METADATA.repositoryKey();

    VbNetSonarWayProfile profileDef = new VbNetSonarWayProfile(VbNetPlugin.METADATA, ROSLYN_RULES);
    profileDef.define(context);
    BuiltInQualityProfile profile = context.profile("vbnet", "Sonar way");

    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2222"))).isNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2259"))).isNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2583"))).isNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2589"))).isNull();
  }
}
