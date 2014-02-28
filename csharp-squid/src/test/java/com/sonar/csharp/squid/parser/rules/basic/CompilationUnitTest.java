/*
 * Sonar C# Plugin :: C# Squid :: Squid
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package com.sonar.csharp.squid.parser.rules.basic;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class CompilationUnitTest extends RuleTest {

  @Before
  public void init() {
    p.getGrammar().rule(CSharpGrammar.EXTERN_ALIAS_DIRECTIVE).override("externAliasDirective");
    p.getGrammar().rule(CSharpGrammar.USING_DIRECTIVE).override("usingDirective");
    p.getGrammar().rule(CSharpGrammar.GLOBAL_ATTRIBUTES).override("globalAttributes");
    p.getGrammar().rule(CSharpGrammar.NAMESPACE_MEMBER_DECLARATION).override("namespaceMemberDeclaration");
  }

  @Test
  public void ok() {
    assertThat(p)
        .matches("externAliasDirective")
        .matches("externAliasDirective externAliasDirective")
        .matches("usingDirective")
        .matches("usingDirective usingDirective")
        .matches("globalAttributes")
        .matches("namespaceMemberDeclaration")
        .matches("namespaceMemberDeclaration namespaceMemberDeclaration")
        .matches("externAliasDirective usingDirective globalAttributes namespaceMemberDeclaration")
        .matches("externAliasDirective externAliasDirective usingDirective globalAttributes namespaceMemberDeclaration");
  }

  @Test
  public void ko() {
    assertThat(p)
        .notMatches("namespaceMemberDeclaration externAliasDirective");
  }

}
