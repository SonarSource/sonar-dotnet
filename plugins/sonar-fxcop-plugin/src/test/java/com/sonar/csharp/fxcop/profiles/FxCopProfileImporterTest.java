/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.fxcop.profiles;

import static org.hamcrest.core.Is.is;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertThat;
import static org.mockito.Matchers.anyObject;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.Reader;
import java.io.StringReader;

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
import org.sonar.test.TestUtils;

public class FxCopProfileImporterTest {

  private ValidationMessages messages;
  private FxCopProfileImporter importer;

  @Before
  public void before() {
    messages = ValidationMessages.create();
    importer = new FxCopProfileImporter(newRuleFinder());
  }

  @Test
  public void importSimpleProfile() {
    Reader reader = new StringReader(TestUtils.getResourceContent("/ProfileImporter/SimpleRules.FxCop"));
    RulesProfile profile = importer.importProfile(reader, messages);

    assertThat(profile.getActiveRules().size(), is(3));
    assertNotNull(profile.getActiveRuleByConfigKey("fxcop", "AssembliesShouldHaveValidStrongNames@$(FxCopDir)\\Rules\\DesignRules.dll"));
    assertNotNull(profile.getActiveRuleByConfigKey("fxcop", "UsePropertiesWhereAppropriate@$(FxCopDir)\\Rules\\DesignRules.dll"));
    assertNotNull(profile.getActiveRuleByConfigKey("fxcop", "AvoidDuplicateAccelerators@$(FxCopDir)\\Rules\\GlobalizationRules.dll"));
    assertThat(messages.hasErrors(), is(false));
  }

  private RuleFinder newRuleFinder() {
    RuleFinder ruleFinder = mock(RuleFinder.class);
    when(ruleFinder.find((RuleQuery) anyObject())).thenAnswer(new Answer<Rule>() {

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
