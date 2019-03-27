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

import java.util.function.UnaryOperator;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.io.IOException;
import java.nio.file.Paths;
import java.util.HashMap;
import java.util.Map;

/**
 * This class is designed to provide some caching around the transformation from a path to the real path on the system.
 *
 * We are doing some caching because the toRealPath operation can be expensive and we know that Roslyn paths will always use the same pattern so we expect a lot of read
 */
public class RealPathProvider implements UnaryOperator<String> {
  private static final Logger LOG = Loggers.get(RealPathProvider.class);
  private final Map<String, String> cachedPaths = new HashMap<>();

  @Override
  public String apply(String path) {
    return cachedPaths.computeIfAbsent(path, this::getRealPath);
  }

  public String getRealPath(String path) {
    try {
      return Paths.get(path).toRealPath().toString();
    } catch (IOException e) {
      LOG.debug("Failed to retrieve the real full path for '{}'", path);
      return path;
    }
  }
}

