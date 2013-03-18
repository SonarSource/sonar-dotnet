/*
 * Sonar C# Plugin :: C# Squid :: Squid
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
package com.sonar.csharp.squid.parser;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.impl.Parser;
import org.apache.commons.io.FileUtils;
import org.junit.Test;

import java.nio.charset.Charset;

/**
 * Test class for the C# parser
 */
public class CSharpParserTest {

  private final Parser<Grammar> parser = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));

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
