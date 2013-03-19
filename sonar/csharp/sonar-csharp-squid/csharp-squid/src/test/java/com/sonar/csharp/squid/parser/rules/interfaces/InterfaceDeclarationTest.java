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
package com.sonar.csharp.squid.parser.rules.interfaces;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class InterfaceDeclarationTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.INTERFACE_DECLARATION));
    p.getGrammar().rule(CSharpGrammar.ATTRIBUTES).override("attributes");
    p.getGrammar().rule(CSharpGrammar.VARIANT_TYPE_PARAMETER_LIST).override("variantTypeParameterList");
    p.getGrammar().rule(CSharpGrammar.INTERFACE_BASE).override("interfaceBase");
    p.getGrammar().rule(CSharpGrammar.TYPE_PARAMETER_CONSTRAINTS_CLAUSES).override("typeParameterConstraintsClauses");
    p.getGrammar().rule(CSharpGrammar.INTERFACE_BODY).override("interfaceBody");
  }

  @Test
  public void ok() {
    assertThat(p)
        .matches("interface id interfaceBody")
        .matches("attributes new partial interface id variantTypeParameterList interfaceBase typeParameterConstraintsClauses interfaceBody;")
        .matches("public protected internal private interface id interfaceBody");
  }

  @Test
  public void ko() {
    assertThat(p)
        .notMatches("");
  }

}
