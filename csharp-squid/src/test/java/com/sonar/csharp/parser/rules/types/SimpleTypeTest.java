/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.types;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class SimpleTypeTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.simpleType);
  }

  @Test
  public void testOk() {
    assertThat(p, parse("bool"));
    assertThat(p, parse("decimal"));
    assertThat(p, parse("sbyte"));
    assertThat(p, parse("short"));
    assertThat(p, parse("ushort"));
    assertThat(p, parse("int"));
    assertThat(p, parse("uint"));
    assertThat(p, parse("long"));
    assertThat(p, parse("ulong"));
    assertThat(p, parse("char"));
    assertThat(p, parse("float"));
    assertThat(p, parse("double"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("Float"));
    assertThat(p, notParse("string"));
    assertThat(p, notParse("object"));
  }

}
