/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.api.squid;

import org.sonar.squid.api.SourceClass;

public class CSharpClass extends SourceClass {

  public CSharpClass(String key, String className) {
    super(key, className);
  }

}
