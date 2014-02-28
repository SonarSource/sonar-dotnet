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
package com.sonar.csharp.squid.integration;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.impl.Parser;
import org.junit.Before;
import org.junit.Test;

import java.io.File;
import java.nio.charset.Charset;

/**
 * Class used to test only one file.
 */
public class CSharpPreIntegrationFileTest {

  private final String filePath = "/parser/cSharpSyntaxAllInOneFile.cs";
  private File cSharpFile;
  private final Parser<Grammar> parser = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));

  @Before
  public void init() throws Exception {
    cSharpFile = new File(this.getClass().getResource(filePath).toURI());
  }

  @Test
  public void parseCSharpSourceFile() throws Exception {
    parser.parse(cSharpFile);
  }

}
