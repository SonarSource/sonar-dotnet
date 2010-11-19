/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.statements;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class LocalVariableDeclarationTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.localVariableDeclaration);
    g.type.mock();
    g.localVariableInitializer.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("type id"));
    assertThat(p, parse("type id = localVariableInitializer"));
    assertThat(p, parse("type id1, id2 = localVariableInitializer, id3"));
  }
  
}
