/*
 * Sonar C# Plugin :: Rules
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
package org.sonar.plugins.csharp.checks.impl;

import static org.hamcrest.Matchers.containsString;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;

import org.junit.Test;
import org.sonar.plugins.csharp.checks.CheckUtils;
import org.sonar.squid.api.CheckMessage;

public class MethodComplexityCheckTest {

  @Test
  public void testCheck() {
    MethodComplexityCheck check = new MethodComplexityCheck();
    check.setMaximumMethodComplexityThreshold(2);
    CheckMessage message = CheckUtils.extractViolation("/checks/methodComplexity.cs", check);

    assertThat(message.getLine(), is(12));
    assertThat(message.formatDefaultMessage(), containsString("Method has a complexity of 7 which is greater than 2 authorized."));
  }
}
