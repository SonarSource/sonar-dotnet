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

public class SetAccessorDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.setAccessorDeclaration);
    g.attributes.mock();
    g.accessorModifier.mock();
    g.accessorBody.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("set accessorBody"));
    assertThat(p, parse("attributes accessorModifier set accessorBody"));
  }

}
