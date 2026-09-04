/*
 * SonarSource :: .NET :: Core
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
import java.nio.file.LinkOption;
import java.nio.file.Paths;
import java.util.HashMap;
import java.util.Map;

/**
 * This class is designed to provide some caching around the transformation from a path to the real path on the system.
 * We are doing some caching because the toRealPath operation can be expensive, and we know that Roslyn paths will always use the same pattern so we expect a lot of read
 *
 * Symbolic links are deliberately not resolved (NET-1883): Roslyn reports the path MSBuild passed to the compiler and the scanner indexes the same path, both in their
 * un-resolved form. Resolving the links here would rewrite the path to the link target, which is not indexed, and the file would be dropped from the analysis.
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
      return Paths.get(path).toRealPath(LinkOption.NOFOLLOW_LINKS).toString();
    } catch (IOException e) {
      LOG.debug("Failed to retrieve the real full path for '{}'", path);
      return path;
    }
  }
}

