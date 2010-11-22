/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class FieldDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.fieldDeclaration);
    g.attributes.mock();
    g.type.mock();
    g.variableDeclarator.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("type variableDeclarator;"));
    assertThat(p, parse("type variableDeclarator, variableDeclarator;"));
    assertThat(p, parse("attributes type variableDeclarator;"));
    assertThat(p, parse("attributes new type variableDeclarator;"));
    assertThat(p, parse("public protected internal private static readonly volatile type variableDeclarator;"));
  }

}
