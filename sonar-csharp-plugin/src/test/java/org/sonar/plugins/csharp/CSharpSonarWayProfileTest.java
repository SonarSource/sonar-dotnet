/*
 * SonarC#
 * Copyright (C) 2014-2019 SonarSource SA
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

import com.google.common.collect.Sets;
import com.sonar.plugins.security.api.CsRules;
import java.lang.reflect.InvocationTargetException;
import java.util.Collections;
import java.util.HashSet;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.mockito.Mockito;
import org.sonar.api.SonarRuntime;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.BuiltInQualityProfile;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.Context;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.NewBuiltInQualityProfile;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.anyString;

public class CSharpSonarWayProfileTest {
  private static final SonarRuntime SQ_67 = SonarVersion.SQ_67_RUNTIME;
  private static final SonarRuntime SQ_73 = SonarVersion.SQ_73_RUNTIME;

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void reset() {
    CsRules.returnRepository = false;
    CsRules.ruleKeys = Collections.emptySet();
    CsRules.exceptionToThrow = null;
  }

  @Test
  public void sonar_security_with_already_activated_rule() {
    NewBuiltInQualityProfile profile = Mockito.mock(NewBuiltInQualityProfile.class);
    Mockito.when(profile.activateRule(CSharpPlugin.REPOSITORY_KEY, "TEST")).thenThrow(IllegalArgumentException.class);
    Context context = Mockito.mock(Context.class);
    Mockito.when(context.createBuiltInQualityProfile(anyString(), anyString())).thenReturn(profile);
    CsRules.ruleKeys = Sets.newHashSet("TEST");

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    assertThat(logTester.logs(LoggerLevel.WARN)).hasSize(1);
  }

  @Test
  public void sonar_security_with_unknown_rule_repository() {
    // we could still fail if we are using a SQ >= 7.4 and old version of SonarSecurity (returning some keys)
    // case in which IllegalStateException will be thrown
    NewBuiltInQualityProfile profile = Mockito.mock(NewBuiltInQualityProfile.class);
    Mockito.when(profile.activateRule("roslyn.TEST", "TEST")).thenThrow(IllegalStateException.class);
    Context context = Mockito.mock(Context.class);
    Mockito.when(context.createBuiltInQualityProfile(anyString(), anyString())).thenReturn(profile);
    CsRules.ruleKeys = Sets.newHashSet("TEST");
    CsRules.returnRepository = true;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    assertThat(logTester.logs(LoggerLevel.WARN)).hasSize(1);
  }

  @Test
  public void sonar_security_with_custom_frontend_plugin() {
    Context context = new Context();
    CsRules.ruleKeys = Sets.newHashSet("S3649");
    CsRules.returnRepository = true;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("cs", "Sonar way");
    assertThat(profile.language()).isEqualTo(CSharpPlugin.LANGUAGE_KEY);
    assertThat(profile.rule(RuleKey.of("roslyn.TEST", "S3649"))).isNotNull();
  }

  @Test(expected=java.lang.IllegalArgumentException.class)
  public void sonar_security_with_duplicated_quality_profile_name() {
    Context context = new Context();
    NewBuiltInQualityProfile sonarWay = context.createBuiltInQualityProfile("Sonar way", CSharpPlugin.LANGUAGE_KEY);
    sonarWay.activateRule(CSharpPlugin.REPOSITORY_KEY, "S1");
    sonarWay.done();
    CsRules.ruleKeys = Sets.newHashSet("S2");

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);
  }

  @Test
  public void sonar_security_missing() {
    Context context = new Context();
    CsRules.ruleKeys = new HashSet<>();
    CsRules.returnRepository = false;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("cs", "Sonar way");
    assertThat(profile.language()).isEqualTo(CSharpPlugin.LANGUAGE_KEY);
    assertThat(profile.rule(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "S3649"))).isNull();
  }

  @Test
  public void sonar_security_7_3_present() {
    Context context = new Context();
    CsRules.ruleKeys = Sets.newHashSet("S3649");
    CsRules.returnRepository = false;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("cs", "Sonar way");
    assertThat(profile.language()).isEqualTo(CSharpPlugin.LANGUAGE_KEY);
    assertThat(profile.rule(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "S3649"))).isNotNull();
  }

  @Test
  public void sonar_security_ClassNotFoundException() {
    Context context = new Context();
    CsRules.exceptionToThrow = new ClassNotFoundException();
    CsRules.returnRepository = true;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).hasSize(1);
  }

  @Test
  public void sonar_security_NoSuchMethodException() {
    Context context = new Context();
    CsRules.exceptionToThrow = new NoSuchMethodException();
    CsRules.returnRepository = true;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).hasSize(1);
  }

  @Test
  public void sonar_security_IllegalAccessException() {
    Context context = new Context();
    CsRules.exceptionToThrow = new IllegalAccessException();
    CsRules.returnRepository = true;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).hasSize(1);
  }

  @Test
  public void sonar_security_InvocationTargetException() {
    Context context = new Context();
    CsRules.exceptionToThrow = new InvocationTargetException(new Exception());
    CsRules.returnRepository = true;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).hasSize(1);
  }

  @Test
  public void hotspots_not_in_sonar_way_before_SQ_73() {
    Context context = new Context();
    CsRules.ruleKeys = new HashSet<>();
    CsRules.returnRepository = false;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_67);
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("cs", "Sonar way");
    assertThat(profile.rule(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "S2245"))).isNull();
    assertThat(profile.rule(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "S2255"))).isNull();
  }

  @Test
  public void hotspots_in_sonar_way_after_SQ_74() {
    Context context = new Context();
    CsRules.ruleKeys = new HashSet<>();
    CsRules.returnRepository = false;

    CSharpSonarWayProfile profileDef = new CSharpSonarWayProfile(SQ_73);
    profileDef.define(context);

    BuiltInQualityProfile profile = context.profile("cs", "Sonar way");
    assertThat(profile.rule(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "S2245"))).isNotNull();
    assertThat(profile.rule(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "S2255"))).isNotNull();
  }
}
