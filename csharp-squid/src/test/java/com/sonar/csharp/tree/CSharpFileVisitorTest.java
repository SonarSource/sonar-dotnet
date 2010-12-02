/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;

import java.io.File;

import org.apache.commons.io.FileUtils;
import org.junit.Ignore;
import org.junit.Test;
import org.sonar.squid.Squid;
import org.sonar.squid.api.SourceProject;

import com.sonar.csharp.CSharpAstScanner;
import com.sonar.csharp.CSharpConfiguration;
import com.sonar.csharp.api.metric.CSharpMetric;

public class CSharpFileVisitorTest {

  @Test
  @Ignore("Need to wait to have a real parser...")
  public void testScanFile() {
    Squid squid = new Squid(new CSharpConfiguration());
    squid.register(CSharpAstScanner.class).scanFile(readFile("/tree/NUnitFramework.cs"));
    SourceProject project = squid.decorateSourceCodeTreeWith(CSharpMetric.FILES);

    assertThat(project.getInt(CSharpMetric.FILES), is(1));
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
