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
package org.sonar.plugins.dotnet.tests;

import java.util.Optional;

public interface FileService {

  /**
   * Returns true if the absolute path is indexed by the scanner and has the correct extension.
   */
  boolean isSupportedAbsolute(String absolutePath);

  /**
   * Returns the absolute path for a deterministic build path.
   *
   * Note that the absolute path returned by the Scanner may be different from the absolute path returned by the Operating System
   * @see org.sonar.api.batch.fs.InputFile#uri()
   *
   * @param deterministicBuildPath - the path in the code coverage report when builds are done with the `-deterministic` option
   */
  Optional<String> getAbsolutePath(String deterministicBuildPath);
}
