/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class ClassModifierTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.classModifier);
  }

  @Test
  public void testOk() {
    assertThat(p, parse("public"));
    assertThat(p, parse("sealed"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("PUBLIC"));
    assertThat(p, notParse("Public"));
    assertThat(p, notParse("class"));
  }

}
