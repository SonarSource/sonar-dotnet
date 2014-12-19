/*
 * Sonar C# Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp;

import org.sonar.plugins.csharp.CSharpCommonRulesEngine;

import org.junit.Test;
import org.sonar.commonrules.api.CommonRulesEngine;
import org.sonar.commonrules.api.CommonRulesRepository;
import static org.fest.assertions.Assertions.assertThat;

public class CSharpCommonRulesEngineTest {

  @Test
  public void provide_extensions() {
    CommonRulesEngine engine = new CSharpCommonRulesEngine();
    assertThat(engine.provide()).isNotEmpty();
  }

  @Test
  public void declare_rules() {
    CommonRulesEngine engine = new CSharpCommonRulesEngine();
    CommonRulesRepository repository = engine.newRepository();
    assertThat(repository.rules()).hasSize(3);
    assertThat(repository.rule("InsufficientCommentDensity")).isNotNull();
    assertThat(repository.rule("DuplicatedBlocks")).isNotNull();
    assertThat(repository.rule("InsufficientLineCoverage")).isNotNull();
  }

}
