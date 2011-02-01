/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class FixedParameterTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.fixedParameter);
  }

  @Test
  public void testOk() {
    g.attributes.mock();
    g.parameterModifier.mock();
    g.type.mock();
    g.expression.mock();
    assertThat(p, parse("type id"));
    assertThat(p, parse("parameterModifier type id"));
    assertThat(p, parse("attributes type id"));
    assertThat(p, parse("attributes parameterModifier type id"));
    assertThat(p, parse("attributes parameterModifier type id = expression"));
    assertThat(p, parse("type id = expression"));
  }

  @Test
  public void testKo() throws Exception {
    assertThat(p, notParse(""));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("int i"));
    assertThat(p, parse("this IEnumerable<TSource> source"));
    assertThat(p, parse("Func<TSource, int, bool> predicate"));
    assertThat(p, parse("RequestStatusDto? status"));
  }

}
