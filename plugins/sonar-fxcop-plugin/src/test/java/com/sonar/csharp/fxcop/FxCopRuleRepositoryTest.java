/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.fxcop;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;

import java.util.List;

import org.junit.Test;
import org.sonar.api.platform.ServerFileSystem;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.XMLRuleParser;

public class FxCopRuleRepositoryTest {

  @Test
  public void loadRepositoryFromXml() {
    ServerFileSystem fileSystem = mock(ServerFileSystem.class);
    FxCopRuleRepository repository = new FxCopRuleRepository(fileSystem, new XMLRuleParser());
    List<Rule> rules = repository.createRules();
    assertThat(rules.size(), is(240));
  }
}
