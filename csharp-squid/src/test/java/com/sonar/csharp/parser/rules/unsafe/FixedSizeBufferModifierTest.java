/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.unsafe;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class FixedSizeBufferModifierTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.unsafe.fixedSizeBufferModifier);
  }

  @Test
  public void testOk() {
    assertThat(p, parse("new"));
    assertThat(p, parse("public"));
    assertThat(p, parse("protected"));
    assertThat(p, parse("internal"));
    assertThat(p, parse("private"));
    assertThat(p, parse("unsafe"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

}
