/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.plugin.source;

import org.sonar.api.batch.AbstractSourceImporter;

import com.sonar.csharp.plugin.CSharp;

public class CSharpSourceImporter extends AbstractSourceImporter {

  public CSharpSourceImporter(CSharp cSharp) {
    super(cSharp);
  }
}
