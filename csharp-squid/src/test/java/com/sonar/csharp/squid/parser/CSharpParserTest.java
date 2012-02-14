/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser;

import java.nio.charset.Charset;

import org.apache.commons.io.FileUtils;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.sslr.impl.Parser;

/**
 * Test class for the C# parser
 */
public class CSharpParserTest {

  private final Parser<CSharpGrammar> parser = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));

  @Test
  public void testParsingSimpleSourceFile() {
    parser.parse(FileUtils.toFile(getClass().getResource("/parser/simpleFile.cs")));
  }

  @Test
  public void testParsingRealLifeSourceFile() {
    parser.parse(FileUtils.toFile(getClass().getResource("/parser/NUnitFramework.cs")));
  }

  @Test
  public void testLinqFile() {
    parser.parse(FileUtils.toFile(getClass().getResource("/parser/LinqBridge-1.2.cs")));
  }

  @Test
  public void testAllInOneFile() {
    parser.parse(FileUtils.toFile(getClass().getResource("/parser/cSharpSyntaxAllInOneFile.cs")));
  }

}
