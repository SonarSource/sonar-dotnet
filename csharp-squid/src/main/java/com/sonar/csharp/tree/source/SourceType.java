/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree.source;

import org.sonar.squid.api.SourceCode;

/**
 * SourceCode class that represents a type in C# (classes, interfaces, delegates, enumerations and structures)
 */
public class SourceType extends SourceCode {

  public SourceType(String key) {
    super(key);
  }

  public SourceType(String key, String typeName) {
    super(key, typeName);
  }

}
