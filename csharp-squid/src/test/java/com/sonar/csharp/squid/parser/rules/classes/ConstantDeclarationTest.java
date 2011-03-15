/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class ConstantDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.constantDeclaration);
    g.attributes.mock();
    g.type.mock();
    g.constantDeclarator.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("const type constantDeclarator;"));
    assertThat(p, parse("const type constantDeclarator, constantDeclarator;"));
    assertThat(p, parse("attributes const type constantDeclarator;"));
    assertThat(p, parse("attributes internal const type constantDeclarator;"));
    assertThat(p, parse("public new protected private const type constantDeclarator;"));
  }

}
