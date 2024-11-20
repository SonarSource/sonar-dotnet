/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFileFilter;

/**
 * This class allows to filter files to process based on whether or not they are auto-generated.
 * This filter refuses (filters) all generated files.
 *
 * Note: the InputFileFilter, starting the scanner-api version 7.6, is evaluated at solution (scanner "project") level,
 * thus all its dependencies must be instantiated at solution level.
 */
public class GeneratedFileFilter implements InputFileFilter {
  private static final Logger LOG = LoggerFactory.getLogger(GeneratedFileFilter.class);

  private final GlobalProtobufFileProcessor globalReportProcessor;
  private final boolean analyzeGeneratedCode;

  public GeneratedFileFilter(GlobalProtobufFileProcessor globalReportProcessor, AbstractLanguageConfiguration configuration) {
    this.globalReportProcessor = globalReportProcessor;
    this.analyzeGeneratedCode = configuration.analyzeGeneratedCode();
    if (analyzeGeneratedCode) {
      LOG.debug("Will analyze generated code");
    } else {
      LOG.debug("Will ignore generated code");
    }
  }

  @Override
  public boolean accept(InputFile inputFile) {
    if (analyzeGeneratedCode) {
      return true;
    }
    boolean isGenerated = globalReportProcessor.isGenerated(inputFile);
    if (isGenerated) {
      LOG.debug("Skipping auto generated file: {}", inputFile);
    }
    return !isGenerated;
  }
}
