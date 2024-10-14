/*
 * SonarC#
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
package org.sonar.plugins.csharp;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.RegisterExtension;
import org.slf4j.event.Level;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.BuiltInQualityProfile;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.Context;
import org.sonar.api.testfixtures.log.LogTesterJUnit5;
import org.sonarsource.csharp.core.CSharpSonarWayProfile;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

import static org.assertj.core.api.Assertions.assertThat;

class CSharpSonarWayProfileTest {
  @RegisterExtension
  public LogTesterJUnit5 logTester = new LogTesterJUnit5().setLevel(Level.DEBUG);

  @Test
  void hotspots_in_sonar_way() {
    Context context = new Context();
    String repositoryKey = CSharpPlugin.METADATA.repositoryKey();

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(CSharpPlugin.METADATA, new RoslynRules(CSharpPlugin.METADATA));
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("cs", "Sonar way");

    assertThat(profile.rule(RuleKey.of(repositoryKey, "S1313"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2068"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2092"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2245"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S3330"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S4507"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S4790"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S5042"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2077"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S5766"))).isNotNull();
  }

  @Test
  void symbolic_execution_not_in_sonar_way() {
    Context context = new Context();
    String repositoryKey = CSharpPlugin.METADATA.repositoryKey();

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(CSharpPlugin.METADATA, new RoslynRules(CSharpPlugin.METADATA));
    profileDef.define(context);
    BuiltInQualityProfile profile = context.profile("cs", "Sonar way");

    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2222"))).isNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2259"))).isNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2583"))).isNull();
    assertThat(profile.rule(RuleKey.of(repositoryKey, "S2589"))).isNull();
  }
}
