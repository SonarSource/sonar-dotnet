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
package com.sonar.csharp.squid.parser.rules.classes;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class FormalParameterListTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.FORMAL_PARAMETER_LIST));
  }

  @Test
  public void ok() {
    p.getGrammar().rule(CSharpGrammar.FIXED_PARAMETERS).override("fixedParameters");
    p.getGrammar().rule(CSharpGrammar.PARAMETER_ARRAY).override("parameterArray");

    assertThat(p)
        .matches("fixedParameters")
        .matches("parameterArray")
        .matches("fixedParameters, parameterArray");
  }

  @Test
  public void reallife() {
    assertThat(p)
        .matches("int i")
        .matches("this IEnumerable<TSource> source, Func<TSource, int, bool> predicate")
        .matches("RequestStatusDto? status, UserActionDto? action, OTCTypeDto? dealType");
  }

}
