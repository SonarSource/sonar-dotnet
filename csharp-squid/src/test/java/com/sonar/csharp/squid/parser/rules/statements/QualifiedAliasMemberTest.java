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

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class QualifiedAliasMemberTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.qualifiedAliasMember);
  }

  @Test
  public void testOk() {
    g.typeArgumentList.mock();
    assertThat(p, parse("id :: id"));
    assertThat(p, parse("id :: id typeArgumentList"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("Foo::NonExisting"));
  }

}
