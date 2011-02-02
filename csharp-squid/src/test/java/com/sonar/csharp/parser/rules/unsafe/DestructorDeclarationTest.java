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

public class DestructorDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.unsafe.destructorDeclaration);
    g.attributes.mock();
    g.destructorBody.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("~ id() destructorBody"));
    assertThat(p, parse("attributes ~ id() destructorBody"));
    assertThat(p, parse("extern unsafe ~ id() destructorBody"));
    assertThat(p, parse("attributes unsafe ~ id() destructorBody"));
    assertThat(p, parse("attributes extern ~ id() destructorBody"));
    assertThat(p, parse("attributes extern unsafe ~ id() destructorBody"));
    assertThat(p, parse("attributes unsafe extern ~ id() destructorBody"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

}
