/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.integration;

import java.io.File;
import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;

import com.sonar.csharp.CSharpConfiguration;
import com.sonar.csharp.parser.CSharpParser;

public class CSharpPreIntegrationFileTest {

  private String filePath = "/integration/NUnit/tests/test-utilities/TestAssert.cs";
  private File cSharpFile;
  private CSharpParser parser = new CSharpParser(new CSharpConfiguration(Charset.forName("UTF-8")));

  @Before
  public void init() throws Exception {
    cSharpFile = new File(this.getClass().getResource(filePath).toURI());
  }

  @Test
  @Ignore
  public void parseCSharpSourceFile() throws Exception {
    parser.parse(cSharpFile);
  }

}