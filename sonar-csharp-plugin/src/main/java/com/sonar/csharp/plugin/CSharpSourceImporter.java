/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.plugin;

import org.sonar.api.batch.AbstractSourceImporter;

public class CSharpSourceImporter extends AbstractSourceImporter {

  public CSharpSourceImporter(CSharp cSharp) {
    super(cSharp);
  }
}
