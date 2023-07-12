/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2023 SonarSource SA
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
package com.sonar.it.csharp;

import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.platform.suite.api.SelectPackages;
import org.junit.platform.suite.api.Suite;

@Suite
@SelectPackages("com.sonar.it.csharp") // This will run all classes from current package containing @Test methods.
public class Tests {

  // ToDo: This is a temporary workaround for jUnit5 migration that should be removed in https://github.com/SonarSource/sonar-dotnet/pull/7574
  @BeforeAll
  public static void startOrchestrator() {
    System.out.println("Start Orchestrator C#");
  }

  // ToDo: This is a temporary workaround for jUnit5 migration that should be removed in https://github.com/SonarSource/sonar-dotnet/pull/7574
  @AfterAll
  public static void stopOrchestrator() {
    System.out.println("Stop Orchestrator C#");
  }
}
