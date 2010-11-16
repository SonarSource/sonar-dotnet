/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;
/**
 * 
 * CLASSE ORIGINALE A MODIFIER
 *
 */
public class BlockTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.block);
  }

  @Test
  @Ignore
  public void realLife() {
    assertThat(p, parse("public class MyClass { }"));
    assertThat(p, parse("MyFirstClass {MySecondClass { } }"));
    assertThat(p, parse("MyFirstClass {line;lin(); MySecondCl{}}"));
    assertThat(p, parse("MyFirstClass {MySecondClass { } MySecondCl{ line;} line;}"));
  }

}
