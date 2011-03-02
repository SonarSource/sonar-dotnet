/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.attributes;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class AttributeSectionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.attributeSection);
  }

  @Test
  public void testOk() {
    g.attributeTargetSpecifier.mock();
    g.attributeList.mock();
    assertThat(p, parse("[attributeList]"));
    assertThat(p, parse("[attributeTargetSpecifier attributeList]"));
    assertThat(p, parse("[attributeTargetSpecifier attributeList , ]"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("[Obsolete(\"Use Fix property\")]"));
  }

}
