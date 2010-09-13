/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.rules;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class LineTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.line);
  }

  @Test
  public void realLife() {
    assertThat(p, parse("myDialog.Filter = \"something\";"));
    assertThat(p, notParse("line"));
    assertThat(p, notParse("line;line"));
    assertThat(p, notParse("line{;"));
    assertThat(p, notParse("public class MyClass {}"));
  }

}
