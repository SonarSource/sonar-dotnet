/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.generics;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class TypeParameterConstraintsTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.typeParameterConstraints);
    g.primaryConstraint.mock();
    g.secondaryConstraints.mock();
    g.constructorConstraint.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("primaryConstraint"));
    assertThat(p, parse("secondaryConstraints"));
    assertThat(p, parse("constructorConstraint"));
    assertThat(p, parse("primaryConstraint, secondaryConstraints"));
    assertThat(p, parse("primaryConstraint, constructorConstraint"));
    assertThat(p, parse("secondaryConstraints, constructorConstraint"));
    assertThat(p, parse("primaryConstraint, secondaryConstraints, constructorConstraint"));
  }

}
