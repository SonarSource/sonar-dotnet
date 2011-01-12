/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser;

import org.apache.commons.io.FileUtils;
import org.junit.Ignore;
import org.junit.Test;

/**
 * Test class for the C# parser
 */
public class CSharpParserTest {

  @Test
  public void testParsingSimpleSourceFile() {
    CSharpParser parser = new CSharpParser();
    parser.parse(FileUtils.toFile(getClass().getResource("/parser/simpleFile.cs")));
  }

  @Test
  @Ignore
  public void testParsingRealLifeSourceFile() {
    CSharpParser parser = new CSharpParser();
    parser.parse(FileUtils.toFile(getClass().getResource("/parser/NUnitFramework.cs")));
  }

  @Test
  @Ignore
  public void testLinqFile() {
    CSharpParser parser = new CSharpParser();
    parser.parse(FileUtils.toFile(getClass().getResource("/parser/LinqBridge-1.2.cs")));
  }

}
