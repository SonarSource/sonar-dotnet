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

public class ConstructorInitializerTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.constructorInitializer);
  }

  @Test
  public void testOk() {
    g.argumentList.mock();
    assertThat(p, parse(": base ()"));
    assertThat(p, parse(": this ()"));
    assertThat(p, parse(": base ( argumentList )"));
    assertThat(p, parse(": this ( argumentList )"));
  }

  @Test
  public void testRealLife() {
    assertThat(p, parse(": base ( id1, id2 )"));
    assertThat(p, parse(": base ( \"toto\" )"));
    assertThat(p, parse(": base ( typeof( Test ) )"));
    assertThat(p, parse(": base ( )"));
  }

}
