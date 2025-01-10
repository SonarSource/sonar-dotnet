/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins.filters;

import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFileFilter;
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;

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
