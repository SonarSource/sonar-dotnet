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

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class ConstructorDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.constructorDeclaration);
    g.attributes.mock();
    g.constructorDeclarator.mock();
    g.constructorBody.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("constructorDeclarator constructorBody"));
    assertThat(p, parse("attributes public constructorDeclarator constructorBody"));
    assertThat(p, parse("protected internal private extern constructorDeclarator constructorBody"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("static constructorDeclarator constructorBody"));
  }

}
