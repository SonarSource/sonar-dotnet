/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.attributes;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class AttributeTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.attribute);
  }

  @Test
  public void testOk() {
    g.attributeName.mock();
    g.attributeArguments.mock();
    assertThat(p, parse("attributeName"));
    assertThat(p, parse("attributeName attributeArguments"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("Obsolete(\"Use Fix property\")"));
    assertThat(p, parse("Foo::NonExisting(var, 5)"));
  }

}
