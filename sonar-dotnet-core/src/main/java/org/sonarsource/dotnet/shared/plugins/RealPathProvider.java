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

import java.util.function.UnaryOperator;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.scanner.ScannerSide;
import org.sonarsource.api.sonarlint.SonarLintSide;

import java.io.IOException;
import java.nio.file.Paths;
import java.util.HashMap;
import java.util.Map;

/**
 * This class is designed to provide some caching around the transformation from a path to the real path on the system.
 * We are doing some caching because the toRealPath operation can be expensive, and we know that Roslyn paths will always use the same pattern so we expect a lot of read
 */
@ScannerSide
@SonarLintSide
public class RealPathProvider implements UnaryOperator<String> {
  private static final Logger LOG = LoggerFactory.getLogger(RealPathProvider.class);
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

