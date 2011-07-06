/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.squid.api.source;

/**
 * SourceCode that represents a C# class
 */
public class SourceClass extends SourceType {

  /**
   * Creates a new {@link SourceClass} object.
   * 
   * @param key
   *          the key of the class
   */
  public SourceClass(String key) {
    super(key);
  }

  /**
   * Creates a new {@link SourceClass} object.
   * 
   * @param key
   *          the key
   * @param className
   *          the name of the class
   */
  public SourceClass(String key, String className) {
    super(key, className);
  }

}
