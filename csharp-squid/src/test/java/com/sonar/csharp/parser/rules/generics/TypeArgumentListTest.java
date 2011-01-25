/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.generics;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class TypeArgumentListTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.typeArgumentList);
  }

  @Test
  public void testOk() {
    g.type.mock();
    assertThat(p, parse("< type >"));
    assertThat(p, parse("< type, type >"));
  }
  
  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("<TSource>"));
    assertThat(p, parse("<TSource, AClass>"));
    assertThat(p, parse("<TSource, int, bool>"));
  }

}
