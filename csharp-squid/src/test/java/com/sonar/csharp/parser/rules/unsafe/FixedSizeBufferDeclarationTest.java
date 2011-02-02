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

public class FixedSizeBufferDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.unsafe.fixedSizeBufferDeclaration);
    g.type.mock();
    g.attributes.mock();
    g.unsafe.fixedSizeBufferModifier.mock();
    g.unsafe.fixedSizeBufferDeclarator.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("fixed type fixedSizeBufferDeclarator;"));
    assertThat(p, parse("fixed type fixedSizeBufferDeclarator fixedSizeBufferDeclarator fixedSizeBufferDeclarator;"));
    assertThat(p, parse("attributes fixed type fixedSizeBufferDeclarator;"));
    assertThat(p, parse("attributes fixedSizeBufferModifier fixed type fixedSizeBufferDeclarator;"));
    assertThat(p, parse("fixedSizeBufferModifier fixedSizeBufferModifier fixedSizeBufferModifier fixed type fixedSizeBufferDeclarator;"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

}
