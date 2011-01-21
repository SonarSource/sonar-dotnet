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

public class FixedParameterTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.fixedParameter);
  }

  @Test
  public void testOk() {
    g.attributes.mock();
    g.type.mock();
    assertThat(p, parse("type id"));
    assertThat(p, parse("out type id"));
    assertThat(p, parse("attributes ref type id"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("int i"));
  }

}
