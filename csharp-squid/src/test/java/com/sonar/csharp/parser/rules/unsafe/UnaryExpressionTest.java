/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.unsafe;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class UnaryExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.unaryExpression);
    g.unsafe.pointerIndirectionExpression.mock();
    g.unsafe.addressOfExpression.mock();
    g.primaryNoArrayCreationExpression.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("pointerIndirectionExpression"));
    assertThat(p, parse("addressOfExpression"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

}
