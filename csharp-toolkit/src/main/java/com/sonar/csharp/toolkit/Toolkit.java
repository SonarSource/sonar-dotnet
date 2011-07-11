/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.toolkit;

import com.sonar.csharp.toolkit.squid.CSharpParserCLI;
import com.sonar.csharp.toolkit.squid.CSharpSsdkGui;

public final class Toolkit {

  private Toolkit() {
  }

  public static void main(String[] args) {
    if (args.length == 0) {
      CSharpSsdkGui.main(args);
    } else {
      CSharpParserCLI cli = new CSharpParserCLI(args);
      cli.parseAndDumpAst();
    }
  }
}
