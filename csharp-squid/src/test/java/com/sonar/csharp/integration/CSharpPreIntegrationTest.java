/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.integration;

import java.io.File;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.Collection;

import org.apache.commons.io.FileUtils;
import org.junit.Ignore;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.Parameterized;

import com.sonar.csharp.parser.CSharpParser;

@RunWith(value = Parameterized.class)
public class CSharpPreIntegrationTest {

  private File cSharpFile = null;
  private CSharpParser parser = new CSharpParser();

  public CSharpPreIntegrationTest(File f) {
    this.cSharpFile = f;
  }

  @Parameterized.Parameters
  public static Collection<Object[]> data() throws URISyntaxException {
    Collection<Object[]> parameters = new ArrayList<Object[]>();
    addParametersForPath(parameters, "/integration/");
    return parameters;
  }

  @Test
  @Ignore("Trop beau pour être vrai...")
  public void parseCSharpSource() throws Exception {
    try {
      parser.parse(cSharpFile);
      System.out.println(cSharpFile);
    } catch (Exception e) {
      throw e;
    }
  }

  protected static void addParametersForPath(Collection<Object[]> parameters, String path) throws URISyntaxException {
    Collection<File> files;
    files = listFiles(path, true);
    for (File file : files) {
      parameters.add(new Object[] { file });
    }
  }

  @SuppressWarnings("unchecked")
  private static Collection<File> listFiles(String path, boolean recursive) throws URISyntaxException {
    return FileUtils.listFiles(new File((new Object()).getClass().getResource(path).toURI()), new String[] { "cs" }, recursive);
  }

}