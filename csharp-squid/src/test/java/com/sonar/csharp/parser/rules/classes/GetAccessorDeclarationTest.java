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

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class GetAccessorDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.getAccessorDeclaration);
    g.attributes.mock();
    g.accessorModifier.mock();
    g.accessorBody.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("get accessorBody"));
    assertThat(p, parse("attributes accessorModifier get accessorBody"));
  }

}
