/*
 * SonarVB
 * Copyright (C) 2012-2019 SonarSource SA
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
  private static final SonarRuntime SQ_67 = SonarVersion.SQ_67_RUNTIME;
  private static final SonarRuntime SQ_73 = SonarVersion.SQ_73_RUNTIME;

  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void hotspots_not_in_sonar_way_before_SQ_73() {
    Context context = new Context();

    VbNetSonarWayProfile profileDef = new VbNetSonarWayProfile(SQ_67);
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("vbnet", "Sonar way");
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S4823"))).isNull();
  }

  @Test
  public void hotspots_in_sonar_way_after_SQ_73() {
    Context context = new Context();

    VbNetSonarWayProfile profileDef = new VbNetSonarWayProfile(SQ_73);
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("vbnet", "Sonar way");
    assertThat(profile.rule(RuleKey.of(VbNetPlugin.REPOSITORY_KEY, "S4823"))).isNotNull();
  }
}
