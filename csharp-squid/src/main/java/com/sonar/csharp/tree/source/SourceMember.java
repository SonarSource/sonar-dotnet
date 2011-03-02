/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree.source;

import org.sonar.squid.api.SourceCode;

/**
 * SourceCode class that represents a member in C# (methods, ... XXXXXXXXXXXXXXXX )
 */
public class SourceMember extends SourceCode {

  public SourceMember(String key) {
    super(key);
  }

  public SourceMember(SourceType parentType, String methodSignature, int startAtLine) {
    super(parentType.getKey() + "#" + methodSignature, methodSignature);
    setStartAtLine(startAtLine);
  }

}
