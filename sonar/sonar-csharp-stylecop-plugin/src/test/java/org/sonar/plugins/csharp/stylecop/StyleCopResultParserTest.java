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

package org.sonar.plugins.csharp.stylecop;

import static org.mockito.Matchers.any;
import static org.mockito.Matchers.anyBoolean;
import static org.mockito.Matchers.anyObject;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.io.File;
import java.nio.charset.Charset;

import org.apache.commons.lang.StringUtils;
import org.junit.Before;
import org.junit.Test;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RuleQuery;
import org.sonar.api.rules.Violation;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

public class StyleCopResultParserTest {

  private SensorContext context;
  private StyleCopResultParser parser;
  private File resultFile;
  private Rule uppercaseLetterRule;
  private Rule parenthesisRule;

  @Before
  public void init() {
    context = mock(SensorContext.class);
    when(context.isIndexed(any(org.sonar.api.resources.File.class), anyBoolean())).thenReturn(true);
    Project project = mock(Project.class);
    ProjectFileSystem fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSourceDirs()).thenReturn(Lists.newArrayList(new File("C:\\MyProject\\src")));
    when(project.getFileSystem()).thenReturn(fileSystem);

    parser = new StyleCopResultParser(project, context, newRuleFinder());
    parser.setEncoding(Charset.forName("UTF-8"));

    resultFile = TestUtils.getResource("/Results/stylecop-report.xml");
  }

  @Test
  public void testParseFile() throws Exception {
    parser.parse(resultFile);

    // Verify calls on context to save violations
    verify(context, times(4)).saveViolation(any(Violation.class));
  }

  private RuleFinder newRuleFinder() {
    uppercaseLetterRule = Rule.create("stylecop", "ElementMustBeginWithUpperCaseLetter", "ElementMustBeginWithUpperCaseLetter")
        .setConfigKey("Microsoft.StyleCop.CSharp.NamingRules#ElementMustBeginWithUpperCaseLetter");
    parenthesisRule = Rule.create("stylecop", "StatementMustNotUseUnnecessaryParenthesis", "StatementMustNotUseUnnecessaryParenthesis")
        .setConfigKey("Microsoft.StyleCop.CSharp.MaintainabilityRules#StatementMustNotUseUnnecessaryParenthesis");
    RuleFinder ruleFinder = mock(RuleFinder.class);
    when(ruleFinder.find((RuleQuery) anyObject())).thenAnswer(new Answer<Rule>() {

      public Rule answer(InvocationOnMock iom) throws Throwable {
        RuleQuery query = (RuleQuery) iom.getArguments()[0];
        Rule rule = null;
        if (StringUtils.equals(query.getConfigKey(),
            "Microsoft.StyleCop.CSharp.MaintainabilityRules#StatementMustNotUseUnnecessaryParenthesis")) {
          rule = parenthesisRule;
        } else if (StringUtils.equals(query.getConfigKey(), "Microsoft.StyleCop.CSharp.NamingRules#ElementMustBeginWithUpperCaseLetter")) {
          rule = uppercaseLetterRule;
        }
        return rule;
      }
    });
    return ruleFinder;
  }

}
