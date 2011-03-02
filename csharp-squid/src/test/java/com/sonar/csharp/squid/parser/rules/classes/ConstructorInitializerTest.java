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

}
