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
import org.sonar.squidbridge.checks.CheckMessagesVerifierRule;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.squidbridge.api.SourceFile;

import java.io.File;

public class TooManyFunctionParametersCheckTest {

  @Rule
  public CheckMessagesVerifierRule checkMessagesVerifier = new CheckMessagesVerifierRule();

  private TooManyFunctionParametersCheck check = new TooManyFunctionParametersCheck();

  @Test
  public void defaultValue() {
    SourceFile file = CSharpAstScanner.scanSingleFile(new File("src/test/resources/checks/tooManyFunctionParameters.cs"), check);
    checkMessagesVerifier.verify(file.getCheckMessages())
      .next().atLine(5).withMessage("Method \"F1\" has 8 parameters, which is greater than the " + check.DEFAULT + " authorized.")
      .noMore();
  }

  @Test
  public void custom() {
    check.max = 2;

    SourceFile file = CSharpAstScanner.scanSingleFile(new File("src/test/resources/checks/tooManyFunctionParameters.cs"), check);
    checkMessagesVerifier.verify(file.getCheckMessages())
      .next().atLine(3).withMessage("Delegate \"Foo\" has 3 parameters, which is greater than the " + check.max + " authorized.")
      .next().atLine(5)
      .next().atLine(7).withMessage("Function has 3 parameters, which is greater than the " + check.max + " authorized.")
      .next().atLine(8).withMessage("Lambda has 3 parameters, which is greater than the " + check.max + " authorized.")
      .next().atLine(9).withMessage("Lambda has 3 parameters, which is greater than the " + check.max + " authorized.")
      .next().atLine(12).withMessage("Method \"F2\" has 4 parameters, which is greater than the " + check.max + " authorized.")
      .next().atLine(27)
      .noMore();
  }
}
