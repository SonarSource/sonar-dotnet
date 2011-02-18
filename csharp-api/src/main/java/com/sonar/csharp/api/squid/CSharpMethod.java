/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.api.squid;

import org.sonar.squid.api.SourceMethod;

public class CSharpMethod extends SourceMethod {

  public CSharpMethod(CSharpClass parentClass, String methodSignature, int startAtLine) {
    super(parentClass, methodSignature, startAtLine);
  }

}
