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

public class IndexerDeclaratorTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.indexerDeclarator);
  }

  @Test
  public void testOk() {
    g.type.mock();
    g.interfaceType.mock();
    g.formalParameterList.mock();
    assertThat(p, parse("type this [formalParameterList]"));
    assertThat(p, parse("type interfaceType.this [formalParameterList]"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("object this[int i]"));
    assertThat(p, parse("object IList.this[int i]"));
  }  

}
