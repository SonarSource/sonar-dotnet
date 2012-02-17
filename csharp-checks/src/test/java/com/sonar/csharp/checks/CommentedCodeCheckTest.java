/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.checks;

import org.junit.Test;

import static com.sonar.csharp.checks.ResourceParser.*;
import static com.sonar.sslr.test.squid.CheckMatchers.*;

public class CommentedCodeCheckTest {

  @Test
  public void testCheck() {
    setCurrentSourceFile(scanFile("/checks/commentedCode.cs", new CommentedCodeCheck()));

    assertNumberOfViolations(2);

    assertViolation().atLine(6).withMessage("Sections of code should not be \"commented out\".");
    assertViolation().atLine(11);
  }

}
