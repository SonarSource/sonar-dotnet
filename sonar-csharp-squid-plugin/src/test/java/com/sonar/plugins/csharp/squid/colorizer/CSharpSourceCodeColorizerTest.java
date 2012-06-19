/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid.colorizer;

import static org.hamcrest.Matchers.containsString;
import static org.junit.Assert.assertThat;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.Reader;

import org.apache.commons.io.FileUtils;
import org.junit.Test;
import org.sonar.colorizer.CodeColorizer;

public class CSharpSourceCodeColorizerTest {

  private CSharpSourceCodeColorizer cobolColorizerFormat = new CSharpSourceCodeColorizer();

  @Test
  public void cSharpToHtml() throws IOException {
    Reader cSharpFile = readFile("/cpd/NUnitFramework.cs");

    String html = new CodeColorizer(cobolColorizerFormat.getTokenizers()).toHtml(cSharpFile);

    assertHtml(html);
    save(html, "sample.html");
    assertContains(html, "<span class=\"cd\">/// Static methods that implement aspects of the NUnit framework that cut </span>");
    assertContains(html, "<span class=\"k\">public</span>");
    assertContains(html, "<span class=\"s\">\"NUnit.Framework.IgnoreAttribute\"</span>");
    assertContains(html, "<span class=\"j\">#endregion</span>");
    assertContains(html, "<span class=\"c\">0</span>");
  }

  private FileReader readFile(String path) throws FileNotFoundException {
    return new FileReader(FileUtils.toFile(getClass().getResource(path)));
  }

  private void assertHtml(String html) {
    assertContains(html, "<style", "<table class=\"code\"", "</html>");
  }

  private void assertContains(String html, String... strings) {
    for (String string : strings) {
      assertThat(html, containsString(string));
    }
  }

  private void save(String html, String filename) throws IOException {
    File output = new File("target/colorizer/" + filename);
    FileUtils.writeStringToFile(output, html);
  }
}
