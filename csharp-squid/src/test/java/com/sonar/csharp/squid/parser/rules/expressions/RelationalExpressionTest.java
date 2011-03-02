/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class RelationalExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.relationalExpression);
  }

  @Test
  public void testOk() {
    g.shiftExpression.mock();
    assertThat(p, parse("shiftExpression"));
    assertThat(p, parse("shiftExpression < shiftExpression "));
    assertThat(p, parse("shiftExpression <= shiftExpression > shiftExpression >= shiftExpression"));
    assertThat(p, parse("shiftExpression <= shiftExpression > shiftExpression >= shiftExpression is type"));
    assertThat(p, parse("shiftExpression <= shiftExpression > shiftExpression >= shiftExpression as type"));
    assertThat(p, parse("shiftExpression <= shiftExpression as type > shiftExpression >= shiftExpression"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("arg is double"));
  }
  
}
