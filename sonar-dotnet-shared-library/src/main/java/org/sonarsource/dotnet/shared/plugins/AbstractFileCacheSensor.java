/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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
import org.sonar.api.resources.AbstractLanguage;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.stream.Collectors;

@ScannerSide
public abstract class AbstractFileCacheSensor implements ProjectSensor {
  private static final Logger LOG = LoggerFactory.getLogger(AbstractFileCacheSensor.class);
  private final AbstractLanguage language;
  private final HashProvider hashProvider;

  protected AbstractFileCacheSensor(AbstractLanguage language, HashProvider hashProvider) {
    this.language = language;
    this.hashProvider = hashProvider;
  }

  // Some file extensions are owned by other analyzers (like .cshtml) so we need this workaround to support caching for those files.
  protected String[] additionalSupportedExtensions() {
    return new String[0];
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name(language.getName() + " File Caching Sensor");
    descriptor.onlyOnLanguage(language.getKey());
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
    var predicateFactory = fileSystem.predicates();
    var filePredicates = Arrays.stream(additionalSupportedExtensions()).map(predicateFactory::hasExtension).collect(Collectors.toList());
    filePredicates.add(predicateFactory.hasLanguage(language.getKey()));

    fileSystem.inputFiles(predicateFactory.or(filePredicates)).forEach(inputFile -> {
      // Normalize to unix style separators. The scanner should be able to read the files on both windows and unix.
      var uri = inputFile.uri();
      var key = basePath.get().relativize(uri).getPath().replace('\\','/');
      var next = context.nextCache();
      try {
        LOG.debug("Incremental PR analysis: Adding hash for '{}' to the cache.", key);
        next.write(key, hashProvider.computeHash(Path.of(uri)));
      } catch (Exception exception) {
        LOG.warn("Incremental PR analysis: An error occurred while computing the hash for " + key, exception);
      }
    });
  }
}
