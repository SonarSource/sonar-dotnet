/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser;

import static org.junit.Assert.fail;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.util.HashSet;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.apache.commons.io.FileUtils;
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
  public void testParsingRealLifeSourceFile() {
    CSharpParser parser = new CSharpParser();
    parser.parse(FileUtils.toFile(getClass().getResource("/parser/NUnitFramework.cs")));
  }

  @Test
  public void testLinqFile() {
    CSharpParser parser = new CSharpParser();
    parser.parse(FileUtils.toFile(getClass().getResource("/parser/LinqBridge-1.2.cs")));
  }

  /**
   * Test method used to check if the C# grammar definition (which is the "CSharpGrammarDecorator.java" file) does not have duplicate
   * definition for a single rule. <br>
   * TODO: see if this wouldn't be worth pushing it up into SSLR parser testing harness module
   */
  @Test
  public void assertGrammarDecoratorIsCorrect() {
    Pattern pattern = Pattern.compile("\\.[a-zA-Z]+\\.is");
    File grammarJavaFile = new File("src/main/java/com/sonar/csharp/parser/CSharpGrammarDecorator.java");
    HashSet<String> foundDefinedRules = new HashSet<String>();
    HashSet<String> duplicateRules = new HashSet<String>();
    try {
      BufferedReader reader = new BufferedReader(new FileReader(grammarJavaFile));
      String line = reader.readLine();
      String rule;
      Matcher matcher;
      while (line != null) {
        matcher = pattern.matcher(line);
        if (matcher.find()) {
          rule = line.substring(matcher.start() + 1, matcher.end() - 3);
          if (foundDefinedRules.contains(rule)) {
            duplicateRules.add(rule);
          } else {
            foundDefinedRules.add(rule);
          }
        }
        line = reader.readLine();
      }
    } catch (FileNotFoundException e) {
      fail("Cannot read the grammar definition file \"CSharpGrammarDecorator.java\".");
    } catch (IOException e) {
      fail("Cannot read the grammar definition file \"CSharpGrammarDecorator.java\".");
    }
    if ( !duplicateRules.isEmpty()) {
      StringBuilder sb = new StringBuilder(
          "The following rule(s) has(have) duplicate definitions in the \"CSharpGrammarDecorator.java\" file:");
      for (String rule : duplicateRules) {
        sb.append("\n\t - ");
        sb.append(rule);
      }
      fail(sb.toString());
    }
  }

}
