/*
 * SonarQube .NET Tests Library
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
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.util.WeakHashMap;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

public class CoverageCache {

  private static final Logger LOG = Loggers.get(CoverageCache.class);

  private final WeakHashMap<String, Coverage> cache = new WeakHashMap<>();

  public Coverage readCoverageFromCacheOrParse(CoverageParser parser, File reportFile) {
    String path = reportFile.getAbsolutePath();
    Coverage coverage = cache.get(path);
    if (coverage == null) {
      coverage = new Coverage();
      parser.accept(reportFile, coverage);
      cache.put(path, coverage);
      LOG.info("Adding this code coverage report to the cache for later reuse: " + path);
    } else {
      LOG.info("Successfully retrieved this code coverage report results from the cache: " + path);
    }
    return coverage;
  }

}
