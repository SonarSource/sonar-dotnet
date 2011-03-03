/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.rules;

import org.sonar.api.resources.AbstractLanguage;

public class CSharp extends AbstractLanguage {

  public CSharp() {
    super("cs", "C#");
  }

  public String[] getFileSuffixes() {
    return new String[] { ".cs" };
  }
}
