/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.toolkit;

import com.sonar.csharp.toolkit.squid.CSharpParserCLI;

public class Toolkit {

  public static void main(String[] args) {
    CSharpParserCLI cli = new CSharpParserCLI(args);
    cli.parseAndDumpAst();
  }
}
