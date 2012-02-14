/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.basic;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class CompilationUnitTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    g.externAliasDirective.mock();
    g.usingDirective.mock();
    g.globalAttributes.mock();
    g.namespaceMemberDeclaration.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("externAliasDirective"));
    assertThat(p, parse("externAliasDirective externAliasDirective"));
    assertThat(p, parse("usingDirective"));
    assertThat(p, parse("usingDirective usingDirective"));
    assertThat(p, parse("globalAttributes"));
    assertThat(p, parse("namespaceMemberDeclaration"));
    assertThat(p, parse("namespaceMemberDeclaration namespaceMemberDeclaration"));
    assertThat(p, parse("externAliasDirective usingDirective globalAttributes namespaceMemberDeclaration"));
    assertThat(p, parse("externAliasDirective externAliasDirective usingDirective globalAttributes namespaceMemberDeclaration"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse("namespaceMemberDeclaration externAliasDirective"));
  }

}
