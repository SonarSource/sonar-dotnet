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

public class NameSpaceOrTypeNameTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.namespaceOrTypeName);
  }

  @Test
  public void testOk() {
    g.typeArgumentList.mock();
    g.qualifiedAliasMember.mock();
    assertThat(p, parse("MyClass"));
    assertThat(p, parse("MyClass typeArgumentList"));
    assertThat(p, parse("qualifiedAliasMember"));
    assertThat(p, parse("qualifiedAliasMember.MyClass"));
    assertThat(p, parse("qualifiedAliasMember.MyClass typeArgumentList"));
    assertThat(p, parse("A.B.C.MyClass"));
  }

  @Test
  public void testRealLife() {
    assertThat(p, parse("NameSpaceOrTypeNameTest"));
    assertThat(p, parse("NameSpaceOrTypeNameTest<Class>"));
    assertThat(p, parse("Foo::NonExisting"));
    assertThat(p, parse("Foo::NonExisting.Class"));
    assertThat(p, parse("Foo::NonExisting.Class<AnotherClass>"));
    assertThat(p, parse("com.sonar.csharp.squid.squid.parser.rules.basic.NameSpaceOrTypeNameTest"));
    assertThat(p, parse("com.sonar.csharp.squid.squid.parser.rules.basic.NameSpaceOrTypeNameTest<Class>"));
    assertThat(p, parse("Func<TSource, int, bool>"));
  }

}
