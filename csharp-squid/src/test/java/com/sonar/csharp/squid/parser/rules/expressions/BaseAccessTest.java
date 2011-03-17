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

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class BaseAccessTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.baseAccess);
    g.expressionList.mock();
    g.argumentList.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("base.id"));
    assertThat(p, parse("base[ argumentList ]"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
    assertThat(p, notParse("base.id typeArgumentList"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("base.GetService"));
    assertThat(p, parse("base.GetService<T>"));
  }

}
