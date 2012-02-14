/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.classes;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class ClassDeclarationTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.classDeclaration);
    g.attributes.mock();
    g.typeParameterList.mock();
    g.classBase.mock();
    g.typeParameterConstraintsClauses.mock();
    g.classBody.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("class MyClass classBody"));
    assertThat(p, parse("public class MyClass classBody;"));
    assertThat(p, parse("attributes partial class MyClass typeParameterList classBase typeParameterConstraintsClauses classBody"));
  }

}
