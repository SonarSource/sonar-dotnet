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
import static org.hamcrest.Matchers.containsString;

public class ParsingErrorCheckTest {

  @Rule
  public CheckMessagesVerifierRule checkMessagesVerifier = new CheckMessagesVerifierRule();

  @Test
  public void detected() {
    SourceFile file = scanFile("/checks/parsingError.cs", new ParsingErrorCheck());

    checkMessagesVerifier.verify(file.getCheckMessages())
        .next().atLine(8).withMessageThat(containsString("DOT expected but \"}\" [RCURLYBRACE] found"));
  }

}
