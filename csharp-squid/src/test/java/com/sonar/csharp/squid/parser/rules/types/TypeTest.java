/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.types;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class TypeTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.type);
  }

  @Test
  public void testOk() {
    g.valueType.mock();
    g.referenceType.mock();
    g.typeParameter.mock();
    assertThat(p, parse("valueType"));
    assertThat(p, parse("referenceType"));
    assertThat(p, parse("typeParameter"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("bool"));
    assertThat(p, parse("AClass"));
    assertThat(p, parse("AClass[]"));
    assertThat(p, parse("int?"));
    assertThat(p, parse("IEnumerable<TSource>"));
    assertThat(p, parse("Func<TSource, int, bool>"));
    assertThat(p, parse("RequestStatusDto?"));
  }

}
