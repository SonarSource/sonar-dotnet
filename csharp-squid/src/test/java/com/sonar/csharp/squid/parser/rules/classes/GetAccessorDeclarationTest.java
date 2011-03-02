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

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class GetAccessorDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.getAccessorDeclaration);
  }

  @Test
  public void testOk() {
    g.attributes.mock();
    g.accessorModifier.mock();
    g.accessorBody.mock();
    assertThat(p, parse("get accessorBody"));
    assertThat(p, parse("attributes accessorModifier get accessorBody"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("get { return RightContext is CollectionOperator ? base.LeftPrecedence + 10 : base.LeftPrecedence; }"));
  }
  
}
