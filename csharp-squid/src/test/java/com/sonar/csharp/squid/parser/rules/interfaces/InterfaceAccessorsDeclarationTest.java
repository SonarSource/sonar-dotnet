/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.interfaces;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class InterfaceAccessorsDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.interfaceAccessors);
    g.attributes.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("get;"));
    assertThat(p, parse("set;"));
    assertThat(p, parse("attributes get;"));
    assertThat(p, parse("attributes set;"));
    assertThat(p, parse("get; set;"));
    assertThat(p, parse("set; get;"));
    assertThat(p, parse("attributes get; attributes set;"));
    assertThat(p, parse("attributes set; attributes get;"));
  }

}
