/*
 * Sonar C# Plugin :: Gendarme
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

package org.sonar.plugins.csharp.gendarme.profiles;

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
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RuleQuery;
import org.sonar.api.utils.ValidationMessages;
import org.sonar.plugins.csharp.gendarme.GendarmeConstants;
import org.sonar.test.TestUtils;

public class GendarmeProfileImporterTest {

  private ValidationMessages messages;
  private GendarmeProfileImporter importer;

  @Before
  public void before() {
    messages = ValidationMessages.create();
    importer = new GendarmeProfileImporter(newRuleFinder());
  }

  @Test
  public void testImportSimpleProfile() {
    Reader reader = new StringReader(TestUtils.getResourceContent("/ProfileImporter/SimpleRules.Gendarme.xml"));
    RulesProfile profile = importer.importProfile(reader, messages);

    for (ActiveRule activeRule : profile.getActiveRules()) {
      System.out.println(activeRule);
    }

    assertThat(profile.getActiveRules().size(), is(4));
    assertNotNull(profile.getActiveRuleByConfigKey(GendarmeConstants.REPOSITORY_KEY, "AvoidLongMethodsRule@Gendarme.Rules.Smells.dll"));
    assertNotNull(profile.getActiveRuleByConfigKey(GendarmeConstants.REPOSITORY_KEY, "AvoidLargeClassesRule@Gendarme.Rules.Smells.dll"));
    assertNotNull(profile.getActiveRuleByConfigKey(GendarmeConstants.REPOSITORY_KEY,
        "AvoidCodeDuplicatedInSameClassRule@Gendarme.Rules.Smells.dll"));
    ActiveRule rule = profile.getActiveRuleByConfigKey(GendarmeConstants.REPOSITORY_KEY,
        "AvoidComplexMethodsRule@Gendarme.Rules.Smells.dll");
    assertNotNull(rule);
    assertThat(rule.getParameter("SuccessThreshold"), is("25"));
    assertThat(messages.hasErrors(), is(false));
  }

  private RuleFinder newRuleFinder() {
    RuleFinder ruleFinder = mock(RuleFinder.class);
    when(ruleFinder.find((RuleQuery) anyObject())).thenAnswer(new Answer<Rule>() {

      public Rule answer(InvocationOnMock iom) throws Throwable {
        RuleQuery query = (RuleQuery) iom.getArguments()[0];
        Rule rule = null;
        if (StringUtils.equals(query.getConfigKey(), "AvoidLongMethodsRule@Gendarme.Rules.Smells.dll")
            || StringUtils.equals(query.getKey(), "AvoidLongMethodsRule")) {
          rule = Rule.create(query.getRepositoryKey(), "AvoidLongMethodsRule", "AvoidLongMethodsRule").setConfigKey(
              "AvoidLongMethodsRule@Gendarme.Rules.Smells.dll");

        } else if (StringUtils.equals(query.getConfigKey(), "AvoidLargeClassesRule@Gendarme.Rules.Smells.dll")
            || StringUtils.equals(query.getKey(), "AvoidLargeClassesRule")) {
          rule = Rule.create(query.getRepositoryKey(), "AvoidLargeClassesRule", "AvoidLargeClassesRule").setConfigKey(
              "AvoidLargeClassesRule@Gendarme.Rules.Smells.dll");

        } else if (StringUtils.equals(query.getConfigKey(), "AvoidCodeDuplicatedInSameClassRule@Gendarme.Rules.Smells.dll")
            || StringUtils.equals(query.getKey(), "AvoidCodeDuplicatedInSameClassRule")) {
          rule = Rule.create(query.getRepositoryKey(), "AvoidCodeDuplicatedInSameClassRule", "AvoidCodeDuplicatedInSameClassRule")
              .setConfigKey("AvoidCodeDuplicatedInSameClassRule@Gendarme.Rules.Smells.dll");

        } else if (StringUtils.equals(query.getConfigKey(), "AvoidComplexMethodsRule@Gendarme.Rules.Smells.dll")
            || StringUtils.equals(query.getKey(), "AvoidComplexMethodsRule")) {
          rule = Rule.create(query.getRepositoryKey(), "AvoidComplexMethodsRule", "AvoidComplexMethodsRule").setConfigKey(
              "AvoidComplexMethodsRule@Gendarme.Rules.Smells.dll");
          rule.createParameter("SuccessThreshold");
        }
        return rule;
      }
    });
    return ruleFinder;
  }

}
