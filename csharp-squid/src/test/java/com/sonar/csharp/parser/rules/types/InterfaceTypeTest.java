/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.types;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class InterfaceTypeTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.interfaceType);
  }

  @Test
  public void testOk() {
    g.typeName.mock();
    assertThat(p, parse("typeName"));
  }

  @Test
  public void testKo() {
    g.typeName.mock();
    assertThat(p, notParse("object"));
    assertThat(p, notParse("string"));
    assertThat(p, notParse("this"));
    assertThat(p, notParse("typeName.this"));
  }
  
  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("ICollection"));
    assertThat(p, notParse("IList.this"));
  }

}
