/*
 * SonarSource :: C# :: Core
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
package org.sonarsource.csharp.core;

import com.sonar.plugins.security.api.CsRules;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Set;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.RegisterExtension;
import org.slf4j.event.Level;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.BuiltInQualityProfile;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.Context;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.NewBuiltInQualityProfile;
import org.sonar.api.testfixtures.log.LogTesterJUnit5;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

class CSharpSonarWayProfileTest {
  @RegisterExtension
  public LogTesterJUnit5 logTester = new LogTesterJUnit5().setLevel(Level.DEBUG);

  private static RoslynRules roslynRules;
  private static PluginMetadata metadata = new TestCSharpMetadata() {
    @Override
    public String resourcesDirectory() {
      return "CSharpSonarWayProfileTest";
    }
  };

  @BeforeAll
  public static void beforeAll() {
    roslynRules = mock(RoslynRules.class);
    when(roslynRules.rules()).thenReturn(new ArrayList<>());
  }

  @BeforeEach
  public void reset() {
    CsRules.returnRepository = false;
    CsRules.ruleKeys = Collections.emptySet();
    CsRules.exceptionToThrow = null;
  }

  @Test
  void sonar_security_with_already_activated_rule() {
    NewBuiltInQualityProfile builtIn = mock(NewBuiltInQualityProfile.class);
    when(builtIn.activateRule(metadata.repositoryKey(), "TEST")).thenThrow(IllegalArgumentException.class);
    Context context = mock(Context.class);
    when(context.createBuiltInQualityProfile(anyString(), anyString())).thenReturn(builtIn);
    CsRules.ruleKeys = Set.of("TEST");

    CSharpSonarWayProfile sonarWay = new CSharpSonarWayProfile(metadata, roslynRules);
    sonarWay.define(context);

    assertThat(logTester.logs(Level.WARN)).hasSize(1);
  }

  @Test
  void sonar_security_with_unknown_rule_repository() {
    // we could still fail if we are using a SQ >= 7.4 and old version of SonarSecurity (returning some keys)
    // case in which IllegalStateException will be thrown
    NewBuiltInQualityProfile builtIn = mock(NewBuiltInQualityProfile.class);
    when(builtIn.activateRule("roslyn.TEST", "TEST")).thenThrow(IllegalStateException.class);
    Context context = mock(Context.class);
    when(context.createBuiltInQualityProfile(anyString(), anyString())).thenReturn(builtIn);
    CsRules.ruleKeys = Set.of("TEST");
    CsRules.returnRepository = true;

    CSharpSonarWayProfile sonarWay = new CSharpSonarWayProfile(metadata, roslynRules);
    sonarWay.define(context);

    assertThat(logTester.logs(Level.WARN)).hasSize(1);
  }

  @Test
  void sonar_security_with_custom_frontend_plugin() {
    Context context = new Context();
    CsRules.ruleKeys = Set.of("S3649");
    CsRules.returnRepository = true;

    CSharpSonarWayProfile sonarWay = new CSharpSonarWayProfile(metadata, roslynRules);
    sonarWay.define(context);

    BuiltInQualityProfile builtIn = context.profile("cs", "Sonar way");
    assertThat(builtIn.language()).isEqualTo(metadata.languageKey());
    assertThat(builtIn.rule(RuleKey.of("roslyn.TEST", "S3649"))).isNotNull();
  }

  @Test
  void sonar_security_with_duplicated_quality_profile_name() {
    Context context = new Context();
    NewBuiltInQualityProfile sonarWay = context.createBuiltInQualityProfile("Sonar way", metadata.languageKey());
    sonarWay.activateRule(metadata.repositoryKey(), "S1");
    sonarWay.done();
    CsRules.ruleKeys = Set.of("S2");

    CSharpSonarWayProfile builtIn = new CSharpSonarWayProfile(metadata, roslynRules);

    assertThrows(java.lang.IllegalArgumentException.class, () -> builtIn.define(context));
  }

  @Test
  void sonar_security_missing() {
    Context context = new Context();

    CSharpSonarWayProfile sonarWay = new CSharpSonarWayProfile(metadata, roslynRules);
    sonarWay.define(context);

    BuiltInQualityProfile builtIn = context.profile("cs", "Sonar way");
    assertThat(builtIn.language()).isEqualTo(metadata.languageKey());
    assertThat(builtIn.rule(RuleKey.of(metadata.repositoryKey(), "S3649"))).isNull();
  }

  @Test
  void sonar_security_7_3_present() {
    Context context = new Context();
    CsRules.ruleKeys = Set.of("S3649");

    CSharpSonarWayProfile sonarWay = new CSharpSonarWayProfile(metadata, roslynRules);
    sonarWay.define(context);

    BuiltInQualityProfile builtIn = context.profile("cs", "Sonar way");
    assertThat(builtIn.language()).isEqualTo(metadata.languageKey());
    assertThat(builtIn.rule(RuleKey.of(metadata.repositoryKey(), "S3649"))).isNotNull();
  }

  @Test
  void sonar_security_Exception() {
    Context context = new Context();
    CsRules.exceptionToThrow = new Exception();
    CsRules.returnRepository = true;

    CSharpSonarWayProfile sonarWay = new CSharpSonarWayProfile(metadata, roslynRules);
    sonarWay.define(context);

    assertThat(logTester.logs(Level.DEBUG)).hasSize(1);
  }
}
