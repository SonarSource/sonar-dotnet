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

package org.sonar.plugins.csharp.gendarme.results;

import static org.mockito.Matchers.any;
import static org.mockito.Matchers.anyObject;
import static org.mockito.Matchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.io.File;
import java.nio.charset.Charset;
import java.util.Map;

import org.junit.Before;
import org.junit.Test;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RuleQuery;
import org.sonar.test.TestUtils;

import com.google.common.collect.Maps;

public class GendarmeResultParserTest {

  private GendarmeResultParser parser;
  private GendarmeViolationMaker violationMaker;
  private File resultFile;
  private Rule avoidRedundancyInMethodNameRule;

  @Before
  public void init() {
    violationMaker = mock(GendarmeViolationMaker.class);

    parser = new GendarmeResultParser(newRuleFinder(), violationMaker);
    parser.setEncoding(Charset.forName("UTF-8"));
    resultFile = TestUtils.getResource("/Results/gendarme-report.xml");
  }

  @Test
  public void testParseFile() throws Exception {
    parser.parse(resultFile);

    verify(violationMaker, times(20)).createViolation();
    verify(violationMaker, times(10)).registerRuleType(any(Rule.class), anyString());

    verify(violationMaker).setCurrentRule(avoidRedundancyInMethodNameRule);
    verify(violationMaker)
        .setCurrentDefaultViolationMessage(
            "This method's name includes the type name of the first parameter. This usually makes an API more verbose and less future-proof than necessary.");
    verify(violationMaker).setCurrentTargetName("Example.Core.IMoney Example.Core.IMoney::AddMoney(Example.Core.Money)");
    verify(violationMaker).setCurrentLocation("Example.Core.IMoney Example.Core.IMoney::AddMoney(Example.Core.Money)");
  }

  private RuleFinder newRuleFinder() {
    final Map<String, Rule> rulesMap = Maps.newHashMap();
    avoidRedundancyInMethodNameRule = Rule.create("gendarme", "AvoidRedundancyInMethodNameRule", "AvoidRedundancyInMethodNameRule");
    rulesMap.put("AvoidRedundancyInMethodNameRule", avoidRedundancyInMethodNameRule);
    rulesMap.put("AvoidSmallNamespaceRule", Rule.create("gendarme", "AvoidSmallNamespaceRule", "AvoidSmallNamespaceRule"));
    rulesMap.put("AvoidSwitchStatementsRule", Rule.create("gendarme", "AvoidSwitchStatementsRule", "AvoidSwitchStatementsRule"));
    rulesMap.put("AvoidUnusedParametersRule", Rule.create("gendarme", "AvoidUnusedParametersRule", "AvoidUnusedParametersRule"));
    rulesMap.put("ConsiderUsingStaticTypeRule", Rule.create("gendarme", "ConsiderUsingStaticTypeRule", "ConsiderUsingStaticTypeRule"));
    rulesMap.put("ImplementEqualsTypeRule", Rule.create("gendarme", "ImplementEqualsTypeRule", "ImplementEqualsTypeRule"));
    rulesMap.put("MarkAssemblyWithCLSCompliantRule",
        Rule.create("gendarme", "MarkAssemblyWithCLSCompliantRule", "MarkAssemblyWithCLSCompliantRule"));
    rulesMap.put("MarkEnumerationsAsSerializableRule",
        Rule.create("gendarme", "MarkEnumerationsAsSerializableRule", "MarkEnumerationsAsSerializableRule"));
    rulesMap.put("MethodCanBeMadeStaticRule", Rule.create("gendarme", "MethodCanBeMadeStaticRule", "MethodCanBeMadeStaticRule"));
    rulesMap.put("ReviewInconsistentIdentityRule",
        Rule.create("gendarme", "ReviewInconsistentIdentityRule", "ReviewInconsistentIdentityRule"));

    RuleFinder ruleFinder = mock(RuleFinder.class);
    when(ruleFinder.find((RuleQuery) anyObject())).thenAnswer(new Answer<Rule>() {

      public Rule answer(InvocationOnMock iom) throws Throwable {
        RuleQuery query = (RuleQuery) iom.getArguments()[0];
        return rulesMap.get(query.getKey());
      }
    });
    return ruleFinder;
  }

}
