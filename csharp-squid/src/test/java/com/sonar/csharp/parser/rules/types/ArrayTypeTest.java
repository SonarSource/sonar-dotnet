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

public class ArrayTypeTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.arrayType);
  }

  @Test
  public void testOk() {
    g.valueType.mock();
    g.classType.mock();
    g.interfaceType.mock();
    g.delegateType.mock();
    g.typeParameter.mock();
    g.rankSpecifier.mock();
    assertThat(p, parse("valueType rankSpecifier"));
    assertThat(p, parse("classType rankSpecifier"));
    assertThat(p, parse("interfaceType rankSpecifier"));
    assertThat(p, parse("delegateType rankSpecifier"));
    assertThat(p, parse("typeParameter rankSpecifier"));
  }

  @Test
  public void testKo() {
    g.valueType.mock();
    g.classType.mock();
    g.interfaceType.mock();
    g.delegateType.mock();
    g.typeParameter.mock();
    g.rankSpecifier.mock();
    g.referenceType.mock();
    assertThat(p, notParse("referenceType rankSpecifier"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("AClass[]"));
  }
  
}
