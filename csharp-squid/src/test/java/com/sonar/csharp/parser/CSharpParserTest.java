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
 * 
 * CLASSE ORIGINALE A MODIFIER
 *
 */
public class CSharpParserTest {
  
  @Test
  @Ignore
  public void testParsingRealLifeSourceFile(){
    CSharpParser parser = new CSharpParser();
    parser.parse(FileUtils.toFile(getClass().getResource("/csharpExample.cs")));
  }

}
