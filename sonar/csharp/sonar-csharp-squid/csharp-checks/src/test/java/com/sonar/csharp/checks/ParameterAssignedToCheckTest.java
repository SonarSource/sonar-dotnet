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

public class ParameterAssignedToCheckTest {

  @Rule
  public CheckMessagesVerifierRule checkMessagesVerifier = new CheckMessagesVerifierRule();

  @Test
  public void test() {
    SourceFile file = CSharpAstScanner.scanSingleFile(new File("src/test/resources/checks/parameterAssignedTo.cs"), new ParameterAssignedToCheck());

    checkMessagesVerifier.verify(file.getCheckMessages())
        .next().atLine(5).withMessage("Remove this assignment to the method parameter 'a'.")
        .next().atLine(30).withMessage("Remove this assignment to the method parameter 'b'.")
        .next().atLine(31).withMessage("Remove this assignment to the method parameter 'e'.")
        .next().atLine(36)
        .next().atLine(55)
        .next().atLine(59)
        .next().atLine(65)
        .next().atLine(68)
        .next().atLine(69)
        .next().atLine(78)
        .next().atLine(89);
  }

}
