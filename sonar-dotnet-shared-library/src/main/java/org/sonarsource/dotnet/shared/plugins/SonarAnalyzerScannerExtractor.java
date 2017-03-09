/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
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

import java.io.File;
import java.io.IOException;
import java.nio.file.Path;
import org.sonar.api.batch.BatchSide;
import org.sonar.api.batch.InstantiationStrategy;
import org.sonar.api.batch.bootstrap.ProjectReactor;
import org.sonar.api.utils.ZipUtils;

@BatchSide
@InstantiationStrategy(InstantiationStrategy.PER_BATCH)
public class SonarAnalyzerScannerExtractor {

  private static final String SCANNER = "SonarAnalyzer.Scanner";
  private static final String SCANNER_ZIP = SCANNER + ".zip";
  private static final String SCANNER_EXE = SCANNER + ".exe";

  private final ProjectReactor reactor;
  private File file = null;

  public SonarAnalyzerScannerExtractor(ProjectReactor reactor) {
    this.reactor = reactor;
  }

  /**
   * Unzip the SonarAnalyzer scanner in a directory that is unique for the provided language. Calling several times
   * this method for the same language will only extract the scanner once.
   * It assumes that the plugin contains the SonarAnalyzer.Scanner.zip as a resource.
   */
  public File executableFile(String language) {
    if (file == null) {
      file = unzipSonarAnalyzerScanner(language);
    }

    return file;
  }

  private File unzipSonarAnalyzerScanner(String language) {
    Path workingDir = reactor.getRoot().getWorkDir().toPath();
    Path toolWorkingDir = workingDir.resolve(SCANNER).resolve(language);

    try {
      ZipUtils.unzip(getClass().getResourceAsStream("/" + SCANNER_ZIP), toolWorkingDir.toFile());
      return toolWorkingDir.resolve(SCANNER_EXE).toFile();
    } catch (IOException e) {
      throw new IllegalStateException("Unable to extract SonarAnalyzer Scanner", e);
    }
  }

}
