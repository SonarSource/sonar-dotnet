/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.api.source;

import org.sonar.squid.api.SourceCode;

/**
 * SourceCode class that represents a type in C# (classes, interfaces, delegates, enumerations and structures)
 */
public class SourceType extends SourceCode {

  /**
   * Creates a new {@link SourceType} object.
   * 
   * @param key
   *          the key of the type
   */
  public SourceType(String key) {
    super(key);
  }

  /**
   * Creates a new {@link SourceType} object.
   * 
   * @param key
   *          the key
   * @param typeName
   *          the name of the type
   */
  public SourceType(String key, String typeName) {
    super(key, typeName);
  }

}
