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

public class ForLoopCounterChangedCheckTest {

  @Rule
  public CheckMessagesVerifierRule checkMessagesVerifier = new CheckMessagesVerifierRule();

  @Test
  public void test() {
    SourceFile file = CSharpAstScanner.scanSingleFile(new File("src/test/resources/checks/forLoopCounterChanged.cs"), new ForLoopCounterChangedCheck());

    checkMessagesVerifier.verify(file.getCheckMessages())
      .next().atLine(6).withMessage("Refactor the code to avoid updating the loop counter \"a\" within the loop body.")
      .next().atLine(11)
      .next().atLine(15)
      .next().atLine(16)
      .next().atLine(21)
      .next().atLine(24)
      .next().atLine(25)
      .next().atLine(27)
      .next().atLine(34)
      .next().atLine(35)
      .next().atLine(45)
      .next().atLine(46)
      .next().atLine(47)
      .next().atLine(48)
      .next().atLine(53)
      .next().atLine(62)
      .next().atLine(64)
      .next().atLine(69)
      .next().atLine(73)
      .noMore();
  }

}
