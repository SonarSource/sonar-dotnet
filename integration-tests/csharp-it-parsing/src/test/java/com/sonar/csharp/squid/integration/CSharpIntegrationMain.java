/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.squid.integration;

import java.io.File;
import java.net.URISyntaxException;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.junit.AfterClass;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.Parameterized;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

/**
 * Class used to test parsing Log4Net and NUnit C#-based libraries.
 */
@RunWith(value = Parameterized.class)
public class CSharpIntegrationMain {

  private static final Logger logger = LoggerFactory.getLogger(CSharpIntegrationMain.class);

  private File cSharpFile = null;
  private final Parser<CSharpGrammar> parser = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private static List<Throwable> exceptions = new ArrayList<Throwable>();

  public CSharpIntegrationMain(File f) {
    this.cSharpFile = f;
  }

  @AfterClass
  public static void traceExceptionAtEnd() {
    for (Throwable e : exceptions) {
      logger.error(e.getMessage());
    }
  }

  @Test
  public void parseCSharpSource() throws Throwable {
    logger.info("parsing File '" + cSharpFile + "'");
    try {
      parser.parse(cSharpFile);
    } catch (Throwable e) {
      logger.error("Unable to parse File '" + cSharpFile + "'", e);
      exceptions.add(e);
      throw e;
    }
  }

  @Parameterized.Parameters
  public static Collection<Object[]> data() throws Exception {
    Collection<Object[]> parameters = new ArrayList<Object[]>();
    String pathToSources = System.getProperty("sonar.csharp.integration.main");
    if (StringUtils.isEmpty(pathToSources)) {
      throw new IllegalStateException("The property 'sonar.csharp.integration.main' is empty and should be set.");
    }
    addParametersForPath(parameters, pathToSources);
    return parameters;
  }

  private static void addParametersForPath(Collection<Object[]> parameters, String path) throws URISyntaxException {
    Collection<File> files;
    files = listFiles(path, true);
    for (File file : files) {
      parameters.add(new Object[] { file });
    }
  }

  @SuppressWarnings("unchecked")
  private static Collection<File> listFiles(String path, boolean recursive) throws URISyntaxException {
    return FileUtils.listFiles(new File(path), new String[] { "cs" }, recursive);
  }

}