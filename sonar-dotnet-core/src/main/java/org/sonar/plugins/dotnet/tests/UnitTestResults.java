/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonar.plugins.dotnet.tests;

import javax.annotation.CheckForNull;
import javax.annotation.Nullable;

public class UnitTestResults {

  protected int tests;
  protected int skipped;
  protected int failures;
  protected int errors;
  protected Long executionTime;

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

  public void add(UnitTestResults unitTestResults) {
    this.tests += unitTestResults.tests();
    this.skipped += unitTestResults.skipped();
    this.failures += unitTestResults.failures();
    this.errors += unitTestResults.errors();
    if (unitTestResults.executionTime() != null) {
      if (this.executionTime == null) {
        this.executionTime = 0L;
      }
      this.executionTime += unitTestResults.executionTime();
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
