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
import org.junit.rules.ExpectedException;
import org.sonar.api.utils.SonarException;
import org.sonar.squid.api.SourceFile;

import java.io.File;

public class ClassNameCheckTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public CheckMessagesVerifierRule checkMessagesVerifier = new CheckMessagesVerifierRule();

  @Test
  public void test() {
    SourceFile file = CSharpAstScanner.scanSingleFile(new File("src/test/resources/checks/className.cs"), new ClassNameCheck());

    checkMessagesVerifier.verify(file.getCheckMessages())
        .next().atLine(3).withMessage("Rename this class to match the regular expression: [A-HJ-Z][a-zA-Z]++|I[a-z][a-zA-Z]*+")
        .next().atLine(11)
        .next().atLine(15);
  }

  @Test
  public void custom() {
    ClassNameCheck check = new ClassNameCheck();
    check.format = "IFoo";

    SourceFile file = CSharpAstScanner.scanSingleFile(new File("src/test/resources/checks/className.cs"), check);

    checkMessagesVerifier.verify(file.getCheckMessages())
        .next().atLine(3).withMessage("Rename this class to match the regular expression: IFoo")
        .next().atLine(7)
        .next().atLine(11)
        .next().atLine(19)
        .next().atLine(24);
  }

  @Test
  public void should_fail_with_bad_regular_expression() {
    thrown.expect(SonarException.class);
    thrown.expectMessage("[" + ClassNameCheck.class.getSimpleName() + "] Unable to compile the regular expression: *");

    ClassNameCheck check = new ClassNameCheck();
    check.format = "*";
    check.init();
  }

}
