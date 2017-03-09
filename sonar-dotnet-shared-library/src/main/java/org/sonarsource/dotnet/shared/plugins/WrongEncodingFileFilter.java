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

import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFileFilter;

public class WrongEncodingFileFilter implements InputFileFilter {

  private final AbstractConfiguration config;
  private final EncodingPerFile encodingPerFile;

  private boolean init;

  public WrongEncodingFileFilter(EncodingPerFile encodingPerFile, AbstractConfiguration config) {
    this.encodingPerFile = encodingPerFile;
    this.config = config;
  }

  /**
   * synchronized because InputFileFilter are executed in parallel
   */
  private synchronized boolean isApplicable() {
    return config.isReportsComingFromMSBuild();
  }

  @Override
  public boolean accept(InputFile inputFile) {
    if (!isApplicable()) {
      // Encoding reports not present (MSBuild 12 or old scanner) filtering will be done later
      return true;
    }
    initOnce();
    return encodingPerFile.encodingMatch(inputFile);
  }

  /**
   * synchronized because InputFileFilter are executed in parallel
   */
  private synchronized void initOnce() {
    if (!init) {
      encodingPerFile.init(config.protobufReportPathFromScanner());
      init = true;
    }
  }

}
