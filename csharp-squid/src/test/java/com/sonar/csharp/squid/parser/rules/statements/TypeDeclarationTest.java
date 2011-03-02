/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.statements;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class TypeDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.typeDeclaration);
    g.classDeclaration.mock();
    g.structDeclaration.mock();
    g.interfaceDeclaration.mock();
    g.enumDeclaration.mock();
    g.delegateDeclaration.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("classDeclaration"));
    assertThat(p, parse("structDeclaration"));
    assertThat(p, parse("interfaceDeclaration"));
    assertThat(p, parse("enumDeclaration"));
    assertThat(p, parse("delegateDeclaration"));
  }

}
