/*
 * Sonar C# Plugin :: C# Squid :: Checks
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
package com.sonar.csharp.checks;

import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.sslr.squid.checks.CheckMessagesVerifierRule;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.squid.api.SourceFile;

import java.io.File;

import static org.hamcrest.Matchers.containsString;

public class ParsingErrorCheckTest {

  @Rule
  public CheckMessagesVerifierRule checkMessagesVerifier = new CheckMessagesVerifierRule();

  @Test
  public void detected() {
    SourceFile file = CSharpAstScanner.scanSingleFile(new File("src/test/resources/checks/parsingError.cs"), new ParsingErrorCheck());

    checkMessagesVerifier.verify(file.getCheckMessages())
        .next().atLine(8).withMessageThat(containsString("DOT expected but \"}\" [RCURLYBRACE] found"));
  }

}
