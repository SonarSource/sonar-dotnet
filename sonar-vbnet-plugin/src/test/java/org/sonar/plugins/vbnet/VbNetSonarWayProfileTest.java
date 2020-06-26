/*
 * SonarVB
 * Copyright (C) 2012-2020 SonarSource SA
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

import org.junit.Rule;
import org.junit.Test;
import org.sonar.api.SonarRuntime;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.BuiltInQualityProfile;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.Context;
import org.sonar.api.utils.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;

public class VbNetSonarWayProfileTest {
  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void hotspots_in_sonar_way() {
    Context context = new Context();

    VbNetSonarWayProfile profileDef = new VbNetSonarWayProfile();
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("vbnet", "Sonar way");
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S4792"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S4834"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S4507"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S5042"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S2077"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S2068"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S1313"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S4784"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S4790"))).isNotNull();
  }
}
