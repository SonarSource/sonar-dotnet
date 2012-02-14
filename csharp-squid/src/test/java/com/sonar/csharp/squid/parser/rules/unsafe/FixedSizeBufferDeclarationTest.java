/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.unsafe;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class FixedSizeBufferDeclarationTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.fixedSizeBufferDeclaration);
    g.type.mock();
    g.attributes.mock();
    g.fixedSizeBufferModifier.mock();
    g.fixedSizeBufferDeclarator.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("fixed type fixedSizeBufferDeclarator;"));
    assertThat(p, parse("fixed type fixedSizeBufferDeclarator fixedSizeBufferDeclarator fixedSizeBufferDeclarator;"));
    assertThat(p, parse("attributes fixed type fixedSizeBufferDeclarator;"));
    assertThat(p, parse("attributes fixedSizeBufferModifier fixed type fixedSizeBufferDeclarator;"));
    assertThat(p, parse("fixedSizeBufferModifier fixedSizeBufferModifier fixedSizeBufferModifier fixed type fixedSizeBufferDeclarator;"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

}
