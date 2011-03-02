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

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class LocalVariableDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.localVariableDeclaration);
  }

  @Test
  public void testOk() {
    g.type.mock();
    g.localVariableInitializer.mock();
    assertThat(p, parse("type id"));
    assertThat(p, parse("type id = localVariableInitializer"));
    assertThat(p, parse("type id1, id2 = localVariableInitializer, id3"));
  }

  @Test
  public void testRealLife() {
    assertThat(p, parse("int a = 1"));
    assertThat(p, parse("int a = 1, b, c = 4"));
    assertThat(p, parse("Message message = \"Hello World\""));
    assertThat(p, parse("int num = count"));
    assertThat(p, parse("int? num = count"));
    assertThat(p, parse("Color? red = Colors.red"));
  }

}
