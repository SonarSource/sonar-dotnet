/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.api.squid;

import org.sonar.squid.api.SourceCode;

public class CSharpNamespace extends SourceCode {

  public CSharpNamespace(String key, String name) {
    super(key, name);
  }

}
