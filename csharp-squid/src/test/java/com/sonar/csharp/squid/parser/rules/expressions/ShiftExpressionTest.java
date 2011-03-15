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

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class ShiftExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.shiftExpression);
  }

  @Test
  public void testOk() {
    g.additiveExpression.mock();
    assertThat(p, parse("additiveExpression"));
    assertThat(p, parse("additiveExpression << additiveExpression "));
    assertThat(p, parse("additiveExpression >> additiveExpression << additiveExpression >> additiveExpression"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("i << 5"));
    assertThat(p, parse("i >> 5"));
  }

}
