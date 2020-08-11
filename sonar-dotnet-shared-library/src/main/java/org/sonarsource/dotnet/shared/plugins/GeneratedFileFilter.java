/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonarsource.dotnet.shared.plugins;

import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFileFilter;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

/**
 * This class allows to filter files to process based on whether or not they are auto-generated.
 * This filter refuses (filters) all generated files.
 *
 * Note: the InputFileFilter, starting the scanner-api version 7.6, is evaluated at solution (scanner "project") level,
 * thus all its dependencies must be instantiated at solution level.
 */
public class GeneratedFileFilter implements InputFileFilter {
  private static final Logger LOG = Loggers.get(GeneratedFileFilter.class);

  private final AbstractGlobalProtobufFileProcessor globalReportProcessor;
  private final boolean analyzeGeneratedCode;

  public GeneratedFileFilter(AbstractGlobalProtobufFileProcessor globalReportProcessor, AbstractSolutionConfiguration configuration) {
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
    boolean isGenerated = globalReportProcessor.getGeneratedFileUris().contains(inputFile.uri());
    if (isGenerated) {
      LOG.debug("Skipping auto generated file: {}", inputFile);
    }
    return !isGenerated;
  }

}
