/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.tree.source;

/**
 * SourceCode that represents a C# class
 */
public class SourceClass extends SourceType {

  public SourceClass(String key) {
    super(key);
  }

  public SourceClass(String key, String typeName) {
    super(key, typeName);
  }

}
