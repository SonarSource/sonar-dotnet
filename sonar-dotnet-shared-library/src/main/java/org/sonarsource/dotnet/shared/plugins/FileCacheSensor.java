/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2022 SonarSource SA
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

import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.nio.file.Path;
import java.nio.file.Paths;

@ScannerSide
public class FileCacheSensor implements ProjectSensor {
  private static final Logger LOG = Loggers.get(FileCacheSensor.class);
  private final HashProvider hashProvider;

  public FileCacheSensor(HashProvider hashProvider) {
    this.hashProvider = hashProvider;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name("File Hash Caching Sensor");
  }

  @Override
  public void execute(SensorContext context) {
    var configuration = context.config();
    if (configuration.get(AbstractPropertyDefinitions.getPullRequestBase()).isPresent()) {
      LOG.debug("Incremental PR analysis: Cache is not uploaded for pull requests.");
      return;
    }

    if (!context.isCacheEnabled()) {
      LOG.info("Incremental PR analysis: Analysis cache is disabled.");
      return;
    }

    var basePath = configuration
      .get(AbstractPropertyDefinitions.getPullRequestCacheBasePath())
      .map(x -> Paths.get(x).toUri());
    if (basePath.isEmpty()) {
      LOG.warn("Incremental PR analysis: Could not determine common base path, cache will not be computed. Consider setting 'sonar.projectBaseDir' property.");
      return;
    }

    LOG.debug("Incremental PR analysis: Preparing to upload file hashes.");
    var fileSystem = context.fileSystem();
    fileSystem.inputFiles(fileSystem.predicates().all()).forEach(inputFile -> {
      // Normalize to unix style separators. The scanner should be able to read the files on both windows and unix.
      var uri = inputFile.uri();
      var key = basePath.get().relativize(uri).getPath().replace('\\','/');
      var next = context.nextCache();
      try {
        LOG.debug("Incremental PR analysis: Adding hash for '" + key + "' to the cache.");
        next.write(key, hashProvider.computeHash(Path.of(uri)));
      } catch (Exception exception) {
        LOG.warn("Incremental PR analysis: An error occurred while computing the hash for " + key, exception);
      }
    });
  }
}
