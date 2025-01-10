/*
 * SonarSource :: C# :: Core
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
package com.sonar.plugins.security.api;

import java.util.HashSet;
import java.util.Set;

public class CsRules {
  public static Set<String> ruleKeys = new HashSet<>();
  public static Exception exceptionToThrow;
  public static boolean returnRepository;

  public static Set<String> getRuleKeys() throws Exception {
    if (exceptionToThrow != null) {
      throw exceptionToThrow;
    }
    return ruleKeys;
  }

  public static String getRepositoryKey() throws Exception {
    if (returnRepository) {
      return "roslyn.TEST";
    } else {
      throw exceptionToThrow;
    }
  }
}
