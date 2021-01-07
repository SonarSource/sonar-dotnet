/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2021 SonarSource SA
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
package org.sonar.plugins.dotnet.tests;

import java.util.Optional;

public interface FileService {

  boolean isSupportedAbsolute(String absolutePath);

  /**
   * Note that the absolute path returned by the Scanner may be different from the absolute path returned by the Operating System
   * @see org.sonar.api.batch.fs.InputFile#uri()
   *
   * @param deterministicBuildPath - the path in the code coverage report when builds are done with the `-deterministic` option
   */
  Optional<String> getAbsolutePath(String deterministicBuildPath);
}
