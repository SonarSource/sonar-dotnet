/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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

import javax.annotation.Nullable;

public class NUnitTestResults extends UnitTestResults {

  NUnitTestResults(String outcome, @Nullable String label, @Nullable Long executionTime) {
    this.tests = 1;
    this.executionTime = executionTime;
    switch(outcome) {
      case "Passed",
           "Success":
        break;
      case "Failed",
           "Failure":
        if (label != null && label.equals("Error")) {
          this.errors = 1;
        } else {
          this.failures = 1;
        }
        break;
      case "Error":
        this.errors = 1;
        break;
      case "Inconclusive",
           "Ignored",
           "NotRunnable",
           "Skipped":
        this.skipped = 1;
        break;
      default:
        throw new IllegalArgumentException("Outcome of unit test must match NUnit Test Format");
    }
  }
}
