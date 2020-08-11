/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2020 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package com.sonar.it.vbnet;

import com.sonar.it.shared.EnsureAllTestsRunBase;
import org.junit.Test;

public class EnsureAllTestsRunTest extends EnsureAllTestsRunBase {

  private static final String PACKAGE_NAME = "com.sonar.it.vbnet";

  private static final String TEST_SUITE_CLASS = PACKAGE_NAME + ".Tests";

  /**
   * In order for the tests to run when executing "mvn verify", all test classes need to be mentioned in the
   * "@SuiteClasses" annotation on {@link com.sonar.it.csharp.Tests}
   */
  @Test
  public void testClassesAreListedInTestsSuite() {
    assertAllTestClassesAreReferencedInTestSuite(PACKAGE_NAME, TEST_SUITE_CLASS);
  }

}
