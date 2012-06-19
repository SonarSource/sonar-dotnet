/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class EventDeclarationTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.eventDeclaration);
  }

  @Test
  public void testOk() {
    g.attributes.mock();
    g.type.mock();
    g.variableDeclarator.mock();
    g.memberName.mock();
    g.eventAccessorDeclarations.mock();
    assertThat(p, parse("event type variableDeclarator;"));
    assertThat(p, parse("event type variableDeclarator, variableDeclarator, variableDeclarator;"));
    assertThat(p, parse("attributes new event type variableDeclarator, variableDeclarator ;"));
    assertThat(p, parse("attributes new event type memberName {eventAccessorDeclarations}"));
    assertThat(p, parse("public protected internal private static virtual sealed override abstract extern event type variableDeclarator;"));
  }

  @Test
  public void testKo() {
    g.attributes.mock();
    g.type.mock();
    g.variableDeclarator.mock();
    g.memberName.mock();
    g.eventAccessorDeclarations.mock();
    assertThat(p, notParse("event type;"));
    assertThat(p, notParse("attributes eventModifier event type memberName {eventAccessorDeclarations};"));
    assertThat(p, notParse("attributes eventModifier event type memberName {}"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("public event EventHandler OnInitLoad, OnFilter, OnReset;"));
  }

}
