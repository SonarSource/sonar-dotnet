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

public class VisualStudioTestResults extends UnitTestResults {

  VisualStudioTestResults(String outcome, Long executionTime) {
    this.tests = 1;
    this.executionTime = executionTime;
    switch(outcome) {
      case "Passed",
           "Warning":
        // success
        break;
      case "Failed":
        // failure
        this.failures = 1;
        break;
      case "Error":
        // error
        this.errors = 1;
        break;
      case "PassedButRunAborted",
            "NotExecuted",
            "Inconclusive",
            "Completed",
            "Timeout",
            "Aborted",
            "Blocked",
            "NotRunnable":
        //skipped
        this.skipped = 1;
        break;
      default:
        throw new IllegalArgumentException("Outcome of unit test must match VSTest Format");
    }
  }
}
