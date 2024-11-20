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

// This class has been taken from SonarSource/sonar-scanner-msbuild
public class VstsUtils {

  static final String ENV_BUILD_DIRECTORY = "AGENT_BUILDDIRECTORY";
  static final String ENV_SOURCES_DIRECTORY = "BUILD_SOURCESDIRECTORY";

  static Boolean isRunningUnderVsts(){
    return System.getenv(ENV_BUILD_DIRECTORY) != null;
  }

  static String getSourcesDirectory(){
    return GetVstsEnvironmentVariable(ENV_SOURCES_DIRECTORY);
  }

  private static String GetVstsEnvironmentVariable(String name){
    String value = System.getenv(name);
    if (name == null){
      throw new IllegalStateException("Unable to find VSTS environment variable: " + name);
    }
    return value;
  }
}
