/*
 * SonarVB
 * Copyright (C) 2012-2025 SonarSource Sàrl
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
package org.sonar.plugins.vbnet;

import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinition.Rule;
import org.sonar.api.utils.Version;
import org.sonarsource.dotnet.shared.plugins.DotNetRulesDefinition;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

import static org.assertj.core.api.Assertions.assertThat;

class VbNetRulesDefinitionTest {
  private static final RulesDefinition.Context CONTEXT = new RulesDefinition.Context();
  private static final SonarRuntime SONAR_RUNTIME = SonarRuntimeImpl.forSonarQube(Version.create(10, 10), SonarQubeSide.SCANNER,
    SonarEdition.COMMUNITY);
  private static final RoslynRules ROSLYN_RULES = new RoslynRules(VbNetPlugin.METADATA);

  private static RulesDefinition.Repository ruleRepo;

  @BeforeAll
  static void setupContext() {
    DotNetRulesDefinition definition = new DotNetRulesDefinition(VbNetPlugin.METADATA, SONAR_RUNTIME, ROSLYN_RULES);
    definition.define(CONTEXT);
    ruleRepo = CONTEXT.repository("vbnet");
  }

  @Test
  void rules_areDefined() {
    assertThat(CONTEXT.repositories()).hasSize(1);
    Rule s1197 = ruleRepo.rule("S1197");
    assertThat(s1197).isNotNull();
    assertThat(s1197.name()).isEqualTo("Array designators \"()\" should be on the type, not the variable");
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
