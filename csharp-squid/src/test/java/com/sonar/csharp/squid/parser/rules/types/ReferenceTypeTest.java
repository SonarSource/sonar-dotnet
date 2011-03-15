/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.types;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class ReferenceTypeTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.referenceType);
  }

  @Test
  public void testOk() {
    g.classType.mock();
    g.interfaceType.mock();
    g.arrayType.mock();
    g.delegateType.mock();
    assertThat(p, parse("classType"));
    assertThat(p, parse("interfaceType"));
    assertThat(p, parse("arrayType"));
    assertThat(p, parse("delegateType"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("AClass"));
    assertThat(p, parse("AClass[]"));
  }

}
