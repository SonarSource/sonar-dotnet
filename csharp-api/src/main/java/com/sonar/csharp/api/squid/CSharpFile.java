/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.api.squid;

import org.sonar.squid.api.SourceFile;

public class CSharpFile extends SourceFile {

  public CSharpFile(String key, String fileName) {
    super(key, fileName);
  }

}
