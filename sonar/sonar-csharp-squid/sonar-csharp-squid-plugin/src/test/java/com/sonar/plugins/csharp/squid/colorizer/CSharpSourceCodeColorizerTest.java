/*
 * Sonar C# Plugin :: C# Squid :: Sonar C# Squid Plugin
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
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
