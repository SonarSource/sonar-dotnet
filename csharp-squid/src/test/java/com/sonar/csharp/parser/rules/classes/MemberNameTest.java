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

public class MemberNameTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.memberName);
  }

  @Test
  public void testOk() {
    g.namespaceOrTypeName.mock();
    assertThat(p, parse("namespaceOrTypeName"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("ICollection.CopyTo"));
    // For the next one, which seems quite weird because not written in the ECMA specification, see
    // http://svn.apache.org/viewvc/logging/log4net/trunk/src/Plugin/PluginCollection.cs?view=markup, line 603
    assertThat(p, parse("IList.this[int i]"));
  }

}
