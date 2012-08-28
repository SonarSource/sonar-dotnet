/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.integration;

import java.io.File;
import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

/**
 * Class used to test only one file.
 */
public class CSharpPreIntegrationFileTest {

  private final String filePath = "/parser/cSharpSyntaxAllInOneFile.cs";
  private File cSharpFile;
  private final Parser<CSharpGrammar> parser = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));

  @Before
  public void init() throws Exception {
    cSharpFile = new File(this.getClass().getResource(filePath).toURI());
  }

  @Test
  public void parseCSharpSourceFile() throws Exception {
    parser.parse(cSharpFile);
  }

}