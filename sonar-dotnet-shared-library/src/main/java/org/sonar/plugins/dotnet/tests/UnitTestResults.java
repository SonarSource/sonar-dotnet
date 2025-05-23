/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonar.plugins.dotnet.tests;

import javax.annotation.CheckForNull;
import javax.annotation.Nullable;

public class UnitTestResults {

  private int tests;
  private int skipped;
  private int failures;
  private int errors;
  private Long executionTime;

  public void add(int tests, int skipped, int failures, int errors, @Nullable Long executionTime) {
    this.tests += tests;
    this.skipped += skipped;
    this.failures += failures;
    this.errors += errors;

    if (executionTime != null) {
      if (this.executionTime == null) {
        this.executionTime = 0L;
      }
      this.executionTime += executionTime;
    }
  }

  public int tests() {
    return tests;
  }

  public int skipped() {
    return skipped;
  }

  public int failures() {
    return failures;
  }

  public int errors() {
    return errors;
  }

  @CheckForNull
  Long executionTime() {
    return executionTime;
  }

}
