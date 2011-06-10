/*
 * Sonar C# Plugin :: StyleCop
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

package org.sonar.plugins.csharp.stylecop.profiles;

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
import org.sonar.plugins.csharp.stylecop.StyleCopConstants;
import org.sonar.test.TestUtils;

public class StyleCopProfileImporterTest {

  private ValidationMessages messages;
  private StyleCopProfileImporter importer;

  @Before
  public void before() {
    messages = ValidationMessages.create();
    importer = new StyleCopProfileImporter(newRuleFinder());
  }

  @Test
  public void testImportSimpleProfile() {
    Reader reader = new StringReader(TestUtils.getResourceContent("/ProfileImporter/SimpleRules.StyleCop.xml"));
    RulesProfile profile = importer.importProfile(reader, messages);

    assertThat(profile.getActiveRules().size(), is(2));
    assertNotNull(profile.getActiveRuleByConfigKey(StyleCopConstants.REPOSITORY_KEY,
        "Microsoft.StyleCop.CSharp.NamingRules#ElementMustBeginWithUpperCaseLetter"));
    assertNotNull(profile.getActiveRuleByConfigKey(StyleCopConstants.REPOSITORY_KEY,
        "Microsoft.StyleCop.CSharp.SpacingRules#KeywordsMustBeSpacedCorrectly"));
    assertThat(messages.hasErrors(), is(false));
  }

  private RuleFinder newRuleFinder() {
    RuleFinder ruleFinder = mock(RuleFinder.class);
    when(ruleFinder.find((RuleQuery) anyObject())).thenAnswer(new Answer<Rule>() {

      public Rule answer(InvocationOnMock iom) throws Throwable {
        RuleQuery query = (RuleQuery) iom.getArguments()[0];
        Rule rule = null;
        if (StringUtils.equals(query.getConfigKey(), "Microsoft.StyleCop.CSharp.NamingRules#ElementMustBeginWithUpperCaseLetter")
            || StringUtils.equals(query.getKey(), "ElementMustBeginWithUpperCaseLetter")) {
          rule = Rule.create(query.getRepositoryKey(), "ElementMustBeginWithUpperCaseLetter", "Element must begin with upper case letter")
              .setConfigKey("Microsoft.StyleCop.CSharp.NamingRules#ElementMustBeginWithUpperCaseLetter");

        } else if (StringUtils.equals(query.getConfigKey(), "Microsoft.StyleCop.CSharp.SpacingRules#KeywordsMustBeSpacedCorrectly")
            || StringUtils.equals(query.getKey(), "KeywordsMustBeSpacedCorrectly")) {
          rule = Rule.create(query.getRepositoryKey(), "KeywordsMustBeSpacedCorrectly", "Keywords must be spaced correctly").setConfigKey(
              "Microsoft.StyleCop.CSharp.SpacingRules#KeywordsMustBeSpacedCorrectly");

        }
        return rule;
      }
    });
    return ruleFinder;
  }

}
