/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.enums;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class EnumDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.enumDeclaration);
    g.attributes.mock();
    g.enumBase.mock();
    g.enumBody.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("enum id enumBody"));
    assertThat(p, parse("attributes new enum id enumBase enumBody;"));
    assertThat(p, parse("public protected internal private enum id enumBody"));
  }

}
