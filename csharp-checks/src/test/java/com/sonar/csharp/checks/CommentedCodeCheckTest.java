/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.checks;

import com.sonar.sslr.squid.checks.CheckMessagesVerifierRule;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.squid.api.SourceFile;

import static com.sonar.csharp.checks.ResourceParser.scanFile;

public class CommentedCodeCheckTest {

  @Rule
  public CheckMessagesVerifierRule checkMessagesVerifier = new CheckMessagesVerifierRule();

  @Test
  public void detected() {
    SourceFile file = scanFile("/checks/commentedCode.cs", new CommentedCodeCheck());

    checkMessagesVerifier.verify(file.getCheckMessages())
        .next().atLine(6).withMessage("Sections of code should not be \"commented out\".")
        .next().atLine(11);
  }

}
