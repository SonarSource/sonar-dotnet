/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class MemberAccessTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.memberAccess);
    g.primaryExpression.mock();
    g.typeArgumentList.mock();
    g.predefinedType.mock();
    g.qualifiedAliasMember.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("primaryExpression.id"));
    assertThat(p, parse("primaryExpression.id typeArgumentList"));
    assertThat(p, parse("predefinedType.id"));
    assertThat(p, parse("predefinedType.id typeArgumentList"));
    assertThat(p, parse("qualifiedAliasMember.id"));
    assertThat(p, parse("qualifiedAliasMember.id typeArgumentList"));
  }

}
