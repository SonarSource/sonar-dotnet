/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;
import com.sonar.sslr.impl.loggers.SslrLogger;

public class MemberAccessTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.memberAccess);
  }

  @Test
  public void testOk() {
    g.primaryExpression.mock();
    g.typeArgumentList.mock();
    g.predefinedType.mock();
    g.qualifiedAliasMember.mock();
    assertThat(p, parse("primaryExpression.id"));
    assertThat(p, parse("primaryExpression.id typeArgumentList"));
    assertThat(p, parse("predefinedType.id"));
    assertThat(p, parse("predefinedType.id typeArgumentList"));
    assertThat(p, parse("qualifiedAliasMember.id"));
    assertThat(p, parse("qualifiedAliasMember.id typeArgumentList"));
  }
  
  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("CurrentDomain.Assemblies"));
    assertThat(p, parse("CurrentDomain.GetAssemblies().Name"));
  }

}
