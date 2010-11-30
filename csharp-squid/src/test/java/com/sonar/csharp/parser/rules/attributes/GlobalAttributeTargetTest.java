/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.attributes;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class GlobalAttributeTargetTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.globalAttributeTarget);
  }

  @Test
  public void testOk() {
    assertThat(p, parse("myIdentifier"));
    assertThat(p, parse("public"));
    assertThat(p, parse("void"));
    assertThat(p, parse("unchecked"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("10"));
  }

}
