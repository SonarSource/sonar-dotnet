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

public class InterfaceBodyTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.interfaceBody);
    g.interfaceMethodDeclaration.mock();
    g.interfacePropertyDeclaration.mock();
    g.interfaceEventDeclaration.mock();
    g.interfaceIndexerDeclaration.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("{}"));
    assertThat(p, parse("{interfaceMethodDeclaration}"));
    assertThat(p, parse("{interfacePropertyDeclaration interfaceEventDeclaration interfaceIndexerDeclaration}"));
  }

}
