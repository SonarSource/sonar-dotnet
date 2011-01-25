/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.integration;

import java.io.File;
import java.net.URISyntaxException;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashSet;
import java.util.Set;

import org.apache.commons.io.FileUtils;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.Parameterized;

import com.sonar.csharp.CSharpConfiguration;
import com.sonar.csharp.parser.CSharpParser;

/**
 * Class used to test parsing Log4Net and NUnit C#-based libraries.
 */
@RunWith(value = Parameterized.class)
public class CSharpPreIntegrationTest {

  private File cSharpFile = null;
  private CSharpParser parser = new CSharpParser(new CSharpConfiguration(Charset.forName("UTF-8")));
  private Set<String> filesToIgnore = new HashSet<String>();

  public CSharpPreIntegrationTest(File f) {
    this.cSharpFile = f;
    populateFilesToIgnoreSet();
  }

  private void populateFilesToIgnoreSet() {
    // XXX Files ignored because they have preprocessing instructions that we do not handle yet
    filesToIgnore.add("PropertiesDictionary.cs");
    filesToIgnore.add("ReadOnlyPropertiesDictionary.cs");
    filesToIgnore.add("TestLoaderWatcherTests.cs");
    filesToIgnore.add("AssemblyWatcher.cs");
    filesToIgnore.add("DomainAgent.cs");
    filesToIgnore.add("RemoteTestRunner.cs");
    filesToIgnore.add("TestThread.cs");
    filesToIgnore.add("PNUnitTestRunner.cs");
    filesToIgnore.add("PairwiseTests.cs");
    // XXX Files ignored for other reasons: should fix the parsing issue some day
    filesToIgnore.add("TypeHelper.cs");
    filesToIgnore.add("EqualTest.cs");
    filesToIgnore.add("NullableTypesTests.cs");
    filesToIgnore.add("ArbitraryConstraintMatching.cs");
  }

  @Parameterized.Parameters
  public static Collection<Object[]> data() throws URISyntaxException {
    Collection<Object[]> parameters = new ArrayList<Object[]>();
    addParametersForPath(parameters, "/integration/");
    return parameters;
  }

  @Test
  // @Ignore
  public void parseCSharpSource() throws Exception {
    if (filesToIgnore.contains(cSharpFile.getName())) {
      System.out.println("... Ignoring \"" + cSharpFile.getName() + "\" for the moment...");
      return;
    }
    try {
      parser.parse(cSharpFile);
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