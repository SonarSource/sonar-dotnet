/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.attributes;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class AttributeTargetTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.attributeTarget);
  }

  @Test
  public void testOk() {
    assertThat(p, parse("field"));
    assertThat(p, parse("event"));
    assertThat(p, parse("method"));
    assertThat(p, parse("param"));
    assertThat(p, parse("property"));
    assertThat(p, parse("return"));
    assertThat(p, parse("type"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
    assertThat(p, notParse("10"));
    assertThat(p, notParse("myIdentifier"));
    assertThat(p, notParse("public"));
    assertThat(p, notParse("void"));
    assertThat(p, notParse("unchecked"));
  }

}
