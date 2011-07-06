/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.squid.api.source;

import org.sonar.squid.api.SourceCode;

/**
 * SourceCode class that represents a member in C# (methods, properties, ... )
 */
public class SourceMember extends SourceCode {

  /**
   * Creates a new {@link SourceMember} object.
   * 
   * @param key
   *          the key of the member
   */
  public SourceMember(String key) {
    super(key);
  }

  /**
   * Creates a new {@link SourceMember} object.
   * 
   * @param parent
   *          the parent of this member
   * @param memberSignature
   *          the signature of the member
   * @param startAtLine
   *          the line where this member begins
   */
  public SourceMember(SourceType parent, String memberSignature, int startAtLine) {
    super(parent.getKey() + "#" + memberSignature, memberSignature);
    setStartAtLine(startAtLine);
  }

}
