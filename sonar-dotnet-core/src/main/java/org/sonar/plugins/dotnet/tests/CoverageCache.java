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
package org.sonar.plugins.dotnet.tests;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.util.WeakHashMap;

class CoverageCache {

  private static final Logger LOG = LoggerFactory.getLogger(CoverageCache.class);

  private final WeakHashMap<String, Coverage> cache = new WeakHashMap<>();

  Coverage readCoverageFromCacheOrParse(CoverageParser parser, File reportFile) {
    String path = reportFile.getAbsolutePath();
    Coverage coverage = cache.get(path);
    if (coverage == null) {
      coverage = new Coverage();
      parser.accept(reportFile, coverage);
      cache.put(path, coverage);
      LOG.info("Adding this code coverage report to the cache for later reuse: {}", path);
    } else {
      LOG.info("Successfully retrieved this code coverage report results from the cache: {}", path);
    }
    return coverage;
  }

}
