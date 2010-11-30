/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class AccessorModifierTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.accessorModifier);
  }

  @Test
  public void testOk() {
    assertThat(p, parse("protected"));
    assertThat(p, parse("internal"));
    assertThat(p, parse("private"));
    assertThat(p, parse("protected internal"));
    assertThat(p, parse("internal protected"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("private internal"));
  }

}
