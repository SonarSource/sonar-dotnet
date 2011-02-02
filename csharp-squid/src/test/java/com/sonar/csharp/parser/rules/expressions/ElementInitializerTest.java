/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class ElementInitializerTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.elementInitializer);
  }

  @Test
  public void testOk() {
    g.nonAssignmentExpression.mock();
    g.expressionList.mock();
    assertThat(p, parse("nonAssignmentExpression"));
    assertThat(p, parse("{expressionList}"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
    assertThat(p, notParse("{}"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("{1, \"\"}"));
  }

}
