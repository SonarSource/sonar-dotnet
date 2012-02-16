/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.checks;

import org.junit.Test;

import static com.sonar.csharp.checks.ResourceParser.*;
import static com.sonar.sslr.test.squid.CheckMatchers.*;
import static org.hamcrest.Matchers.*;

public class ParsingErrorCheckTest {

  @Test
  public void testCheck() {
    setCurrentSourceFile(scanFile("/checks/parsingError.cs", new ParsingErrorCheck()));

    assertOnlyOneViolation().atLine(8).withMessage(containsString("DOT expected but \"}\" [RCURLYBRACE] found"));
  }
}
