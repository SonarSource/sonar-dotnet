/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.types;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class ArrayTypeTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.arrayType);
  }

  @Test
  public void testOk() {
    g.type.mock();
    g.rankSpecifier.mock();
    assertThat(p, parse("type rankSpecifier"));
    assertThat(p, parse("type rankSpecifier rankSpecifier"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("AClass[]"));
  }

}
