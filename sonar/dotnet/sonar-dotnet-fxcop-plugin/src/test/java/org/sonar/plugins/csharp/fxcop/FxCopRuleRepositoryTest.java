/*
 * Sonar .NET Plugin :: FxCop
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
package org.sonar.plugins.csharp.fxcop;

import org.junit.Test;
import org.sonar.api.config.Settings;
import org.sonar.api.platform.ServerFileSystem;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.XMLRuleParser;

import java.util.List;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;

public class FxCopRuleRepositoryTest {

  private static final int FXCOP_STANDARD_RULES_COUNT = 240;

  @Test
  public void loadRepositoryFromXml() {
    ServerFileSystem fileSystem = mock(ServerFileSystem.class);
    Settings settings = Settings.createForComponent(FxCopRuleRepositoryProvider.class);
    FxCopRuleRepository repository = new FxCopRuleRepository("", "", fileSystem, new XMLRuleParser(), settings);
    List<Rule> rules = repository.createRules();
    assertThat(rules.size(), is(FXCOP_STANDARD_RULES_COUNT));
  }

  @Test
  public void loadRepositoryWithProperty() {
    ServerFileSystem fileSystem = mock(ServerFileSystem.class);
    Settings settings = Settings.createForComponent(FxCopRuleRepositoryProvider.class);
    settings.setProperty(FxCopRuleRepositoryProvider.SONAR_FXCOP_CUSTOM_RULES_PROP_KEY,
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
          + "<rules>"
          + "<rule key=\"NewRule\">"
          + "<name>New rule</name>"
          + "<configKey>Foo</configKey>"
          + "<category name=\"Maintainability\" />"
          + "<description>Blabla</description>"
          + "</rule>"
          + "</rules>"
        );

    FxCopRuleRepository repository = new FxCopRuleRepository("", "", fileSystem, new XMLRuleParser(), settings);
    List<Rule> rules = repository.createRules();
    assertThat(rules.size(), is(FXCOP_STANDARD_RULES_COUNT + 1));
  }
}
