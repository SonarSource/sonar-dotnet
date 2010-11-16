/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.lexer;

import static org.hamcrest.Matchers.hasItem;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;

import java.util.Set;

import org.apache.commons.io.FileUtils;
import org.junit.Test;

public class CSharpNoSonarScannerTest {

  @Test
  public void testScan() {
    CSharpNoSonarScanner scanner = new CSharpNoSonarScanner();
    Set<Integer> noSonarTags = scanner.scan(FileUtils.toFile(getClass().getResource("/lexer/noSonar-cSharpExample.cs")));

    assertThat(noSonarTags, hasItem(27));
    assertThat(noSonarTags, hasItem(33));

    assertThat(noSonarTags.size(), is(2));
  }

}
