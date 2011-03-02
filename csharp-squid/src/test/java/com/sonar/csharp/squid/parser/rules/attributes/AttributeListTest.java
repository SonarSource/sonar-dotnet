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

public class AttributeListTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.attributeList);
  }

  @Test
  public void testOk() {
    g.attribute.mock();
    assertThat(p, parse("attribute"));
    assertThat(p, parse("attribute, attribute"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("Obsolete(\"Use Fix property\")"));
  }

}
