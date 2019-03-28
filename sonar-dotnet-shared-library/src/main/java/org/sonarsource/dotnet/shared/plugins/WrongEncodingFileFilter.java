/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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

/**
 * This class allows to filter files to process based on whether or not the encoding detected by Roslyn and SonarQube match.
 * This filter refuses (filters) all files with a different encoding.
 */
public class WrongEncodingFileFilter implements InputFileFilter {

  private final EncodingPerFile encodingPerFile;

  public WrongEncodingFileFilter(EncodingPerFile encodingPerFile) {
    this.encodingPerFile = encodingPerFile;
  }

  @Override
  public boolean accept(InputFile inputFile) {
    return encodingPerFile.encodingMatch(inputFile);
  }

}
