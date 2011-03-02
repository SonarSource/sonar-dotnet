/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class ArgumentValueTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.argumentValue);
  }

  @Test
  public void testOk() {
    g.expression.mock();
    g.variableReference.mock();
    assertThat(p, parse("expression"));
    assertThat(p, parse("ref variableReference"));
    assertThat(p, parse("out variableReference"));
  }

  @Test
  public void testKo() throws Exception {
    assertThat(p, notParse(""));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("(x,y)=>String.Compare(x, y, true)"));
    assertThat(p, parse("item => item.Id == prdId"));
  }
  
}
