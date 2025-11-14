/*
 * SonarC#
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
package org.sonar.plugins.csharp;

import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.utils.Version;
import org.sonarsource.dotnet.shared.plugins.DotNetRulesDefinition;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

import static org.assertj.core.api.Assertions.assertThat;

class CSharpRulesDefinitionTest {
  private static final RulesDefinition.Context CONTEXT = new RulesDefinition.Context();
  private static final SonarRuntime SONAR_RUNTIME = SonarRuntimeImpl.forSonarQube(Version.create(10, 10), SonarQubeSide.SCANNER,
    SonarEdition.COMMUNITY);
  private static final RoslynRules ROSLYN_RULES = new RoslynRules(CSharpPlugin.METADATA);

  private static RulesDefinition.Repository ruleRepo;

  @BeforeAll
  static void setupContext() {
    DotNetRulesDefinition definition = new DotNetRulesDefinition(CSharpPlugin.METADATA, SONAR_RUNTIME, ROSLYN_RULES);
    definition.define(CONTEXT);
    ruleRepo = CONTEXT.repository("csharpsquid");
  }

  @Test
  void rules_areDefined() {
    assertThat(CONTEXT.repositories()).hasSize(1);
    RulesDefinition.Rule s100 = ruleRepo.rule("S100");
    assertThat(s100).isNotNull();
    assertThat(s100.name()).isEqualTo("Methods and properties should be named in PascalCase");
  }

  @Test
  void symbolicExecutionRules_areNotDefined() {
    assertThat(CONTEXT.repositories()).hasSize(1);
    assertThat(ruleRepo.rule("S2259")).isNull();
  }

  @Test
  void allRules_haveMetadata() {
    for (RulesDefinition.Rule rule : ruleRepo.rules()) {
      assertThat(rule.name()).isNotEmpty();
      assertThat(rule.type()).isNotNull();
      assertThat(rule.status()).isNotNull();
      assertThat(rule.severity()).isNotEmpty();
    }
  }

  @Test
  void allRules_haveHtmlDescription() {
    for (RulesDefinition.Rule rule : ruleRepo.rules()) {
      assertThat(rule.htmlDescription()).isNotEmpty().hasSizeGreaterThan(100);
    }
  }
}
