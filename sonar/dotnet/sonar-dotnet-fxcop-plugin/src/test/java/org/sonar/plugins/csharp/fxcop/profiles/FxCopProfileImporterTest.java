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
package org.sonar.plugins.csharp.fxcop.profiles;

import org.apache.commons.lang.StringUtils;
import org.junit.Before;
import org.junit.Test;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RuleQuery;
import org.sonar.api.utils.ValidationMessages;
import org.sonar.plugins.csharp.fxcop.FxCopConstants;
import org.sonar.test.TestUtils;

import java.io.Reader;
import java.io.StringReader;

import static org.fest.assertions.Assertions.assertThat;
import static org.hamcrest.core.Is.is;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertThat;
import static org.mockito.Matchers.anyObject;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class FxCopProfileImporterTest {

  private ValidationMessages messages;
  private FxCopProfileImporter importer;

  @Before
  public void before() {
    messages = ValidationMessages.create();
    importer = new FxCopProfileImporter.CSharpRegularFxCopProfileImporter(newRuleFinder());
  }

  // FIXME: Crappy test (breaks in SQ 4.2-SNAPSHOT)
  @Test
  public void testImportSimpleProfile() {
    Reader reader = new StringReader(TestUtils.getResourceContent("/ProfileImporter/SimpleRules.FxCop.xml"));
    RulesProfile profile = importer.importProfile(reader, messages);

    assertThat(profile.getActiveRules().size(), is(3));
    assertNotNull(profile.getActiveRuleByConfigKey(FxCopConstants.REPOSITORY_KEY,
      "AssembliesShouldHaveValidStrongNames@$(FxCopDir)\\Rules\\DesignRules.dll"));
    assertNotNull(profile.getActiveRuleByConfigKey(FxCopConstants.REPOSITORY_KEY,
      "UsePropertiesWhereAppropriate@$(FxCopDir)\\Rules\\DesignRules.dll"));
    assertNotNull(profile.getActiveRuleByConfigKey(FxCopConstants.REPOSITORY_KEY,
      "AvoidDuplicateAccelerators@$(FxCopDir)\\Rules\\GlobalizationRules.dll"));
    assertThat(messages.hasErrors(), is(false));
    // check the name of the repo
    assertThat(profile.getActiveRules().get(0).getRepositoryKey()).isEqualTo("fxcop");
  }

  // FIXME: Crappy test (breaks in SQ 4.2-SNAPSHOT)
  @Test
  public void shouldCreateImporterForVbNet() {
    importer = new FxCopProfileImporter.VbNetRegularFxCopProfileImporter(newRuleFinder());

    Reader reader = new StringReader(TestUtils.getResourceContent("/ProfileImporter/SimpleRules.FxCop.xml"));
    RulesProfile profile = importer.importProfile(reader, messages);

    // check the name of the repo
    assertThat(profile.getActiveRules().get(0).getRepositoryKey()).isEqualTo("fxcop-vbnet");
  }

  private RuleFinder newRuleFinder() {
    RuleFinder ruleFinder = mock(RuleFinder.class);
    when(ruleFinder.find((RuleQuery) anyObject())).thenAnswer(new Answer<Rule>() {

      @Override
      public Rule answer(InvocationOnMock iom) throws Throwable {
        RuleQuery query = (RuleQuery) iom.getArguments()[0];
        Rule rule = null;
        if (StringUtils.equals(query.getConfigKey(), "AssembliesShouldHaveValidStrongNames@$(FxCopDir)\\Rules\\DesignRules.dll")
          || StringUtils.equals(query.getKey(), "AssembliesShouldHaveValidStrongNames")) {
          rule = Rule.create(query.getRepositoryKey(), "AssembliesShouldHaveValidStrongNames", "Assemblies should have valid strong names")
            .setConfigKey("AssembliesShouldHaveValidStrongNames@$(FxCopDir)\\Rules\\DesignRules.dll");

        } else if (StringUtils.equals(query.getConfigKey(), "UsePropertiesWhereAppropriate@$(FxCopDir)\\Rules\\DesignRules.dll")
          || StringUtils.equals(query.getKey(), "UsePropertiesWhereAppropriate")) {
          rule = Rule.create(query.getRepositoryKey(), "UsePropertiesWhereAppropriate", "Use properties where appropriate").setConfigKey(
            "UsePropertiesWhereAppropriate@$(FxCopDir)\\Rules\\DesignRules.dll");

        } else if (StringUtils.equals(query.getConfigKey(), "AvoidDuplicateAccelerators@$(FxCopDir)\\Rules\\GlobalizationRules.dll")
          || StringUtils.equals(query.getKey(), "AvoidDuplicateAccelerators")) {
          rule = Rule.create(query.getRepositoryKey(), "AvoidDuplicateAccelerators", "Avoid duplicate accelerators").setConfigKey(
            "AvoidDuplicateAccelerators@$(FxCopDir)\\Rules\\GlobalizationRules.dll");
        }
        return rule;
      }
    });
    return ruleFinder;
  }

}
