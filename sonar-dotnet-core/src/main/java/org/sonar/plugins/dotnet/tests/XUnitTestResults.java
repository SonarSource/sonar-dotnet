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

public class XUnitTestResults extends UnitTestResults {

  XUnitTestResults(String outcome, @Nullable Long executionTime) {
    this.tests = 1;
    this.executionTime = executionTime;
    switch(outcome) {
      case "Pass":
        break;
      case "Fail":
        this.failures = 1;
        break;
      case "Skip",
           "NotRun":
        this.skipped = 1;
        break;
      default:
        throw new IllegalArgumentException("Outcome of unit test must match XUnit Test Format");
    }
  }
}
