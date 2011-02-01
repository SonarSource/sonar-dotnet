/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.types;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class StructTypeTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.structType);
  }

  @Test
  public void testOk() {
    g.nullableType.mock();
    g.typeName.mock();
    g.simpleType.mock();
    assertThat(p, parse("nullableType"));
    assertThat(p, parse("typeName"));
    assertThat(p, parse("simpleType"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("RequestStatusDto?"));
  }

}
