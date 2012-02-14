/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class PredefinedTypeTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.predefinedType);
  }

  @Test
  public void testOk() {
    assertThat(p, parse("bool"));
    assertThat(p, parse("byte"));
    assertThat(p, parse("char"));
    assertThat(p, parse("decimal"));
    assertThat(p, parse("double"));
    assertThat(p, parse("float"));
    assertThat(p, parse("int"));
    assertThat(p, parse("long"));
    assertThat(p, parse("object"));
    assertThat(p, parse("sbyte"));
    assertThat(p, parse("short"));
    assertThat(p, parse("string"));
    assertThat(p, parse("uint"));
    assertThat(p, parse("ulong"));
    assertThat(p, parse("ushort"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("String"));
  }

}
