/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.interfaces;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class InterfaceIndexerDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.interfaceIndexerDeclaration);
    g.attributes.mock();
    g.type.mock();
    g.formalParameterList.mock();
    g.interfaceAccessors.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("type this [formalParameterList] {interfaceAccessors}"));
    assertThat(p, parse("attributes new type this [formalParameterList] {interfaceAccessors}"));
  }

}
