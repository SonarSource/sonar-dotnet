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
import org.junit.rules.ExpectedException;
import org.sonar.api.utils.SonarException;
import org.sonar.squidbridge.api.SourceFile;

import java.io.File;

public class MethodNameCheckTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public CheckMessagesVerifierRule checkMessagesVerifier = new CheckMessagesVerifierRule();

  @Test
  public void test() {
    SourceFile file = CSharpAstScanner.scanSingleFile(new File("src/test/resources/checks/methodName.cs"), new MethodNameCheck());

    checkMessagesVerifier.verify(file.getCheckMessages())
        .next().atLine(5).withMessage("Rename this method to match the regular expression: [A-Z][a-zA-Z0-9]++")
        .next().atLine(6)
        .next().atLine(13)
        .next().atLine(27);
  }

  @Test
  public void custom() {
    MethodNameCheck check = new MethodNameCheck();
    check.format = "foo";

    SourceFile file = CSharpAstScanner.scanSingleFile(new File("src/test/resources/checks/methodName.cs"), check);

    checkMessagesVerifier.verify(file.getCheckMessages())
        .next().atLine(7).withMessage("Rename this method to match the regular expression: foo")
        .next().atLine(8)
        .next().atLine(14);
  }

  @Test
  public void should_fail_with_bad_regular_expression() {
    thrown.expect(SonarException.class);
    thrown.expectMessage("[" + MethodNameCheck.class.getSimpleName() + "] Unable to compile the regular expression: *");

    MethodNameCheck check = new MethodNameCheck();
    check.format = "*";
    check.init();
  }

}
