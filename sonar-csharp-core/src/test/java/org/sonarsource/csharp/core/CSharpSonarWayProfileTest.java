/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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

import java.util.ArrayList;
import java.util.List;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.RegisterExtension;
import org.slf4j.event.Level;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.BuiltInQualityProfile;
import org.sonar.api.server.profile.BuiltInQualityProfilesDefinition.Context;
import org.sonar.api.testfixtures.log.LogTesterJUnit5;
import org.sonar.plugins.csharpenterprise.api.ProfileRegistrar;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

import static org.assertj.core.api.Assertions.assertThat;
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

  @Test
  void profileRegistrars_registerRules() {
    Context context = new Context();
    ProfileRegistrar[] profileRegistrars = new ProfileRegistrar[]{
      registrarContext -> {
        registrarContext.registerDefaultQualityProfileRules(
          List.of(RuleKey.of(metadata.repositoryKey(), "additionalRule"))
        );
      }};
    CSharpSonarWayProfile sonarWay = new CSharpSonarWayProfile(metadata, roslynRules, profileRegistrars);
    sonarWay.define(context);

    BuiltInQualityProfile builtIn = context.profile("cs", "Sonar way");
    assertThat(builtIn.language()).isEqualTo(metadata.languageKey());
    assertThat(builtIn.rule(RuleKey.of(metadata.repositoryKey(), "additionalRule"))).isNotNull();
  }
}
