/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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

import java.util.List;
import java.util.Optional;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;
import org.sonar.api.batch.fs.FilePredicates;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

@ScannerSide
public class ScannerFileService implements FileService {
  private static final Logger LOG = Loggers.get(ScannerFileService.class);
  private static final Pattern DETERMINISTIC_SOURCE_PATH_PREFIX = Pattern.compile("^(/_\\d*/)");
  private FileSystem fileSystem;
  private String languageKey;

  public ScannerFileService(String languageKey, FileSystem fileSystem) {
    this.languageKey = languageKey;
    this.fileSystem = fileSystem;
  }

  public boolean isSupportedAbsolute(String absolutePath) {
    FilePredicates fp = fileSystem.predicates();
    return fileSystem.hasFiles(
      fp.and(
        fp.hasAbsolutePath(absolutePath),
        fp.hasLanguage(languageKey)));
  }

  public Optional<String> getAbsolutePath(String deterministicBuildPath) {
    Matcher matcher = DETERMINISTIC_SOURCE_PATH_PREFIX.matcher(deterministicBuildPath.replace('\\', '/'));
    if (matcher.find()) {
      String pathSuffix = matcher.replaceFirst("");
      FilePredicates fp = fileSystem.predicates();
      List<String> foundFiles = StreamSupport
        .stream(
          fileSystem.inputFiles(fp.and(fp.hasLanguage(languageKey), new PathSuffixPredicate(pathSuffix))).spliterator(),
          false)
        .map(x -> x.uri().getPath())
        .collect(Collectors.toList());

      if (foundFiles.size() == 1) {
        String foundFile = foundFiles.get(0);
        LOG.trace("Found indexed file '{}' for '{}' (normalized to '{}').", foundFile, deterministicBuildPath, pathSuffix);
        return Optional.of(foundFile);
      } else {
        LOG.debug("Found {} indexed files for '{}' (normalized to '{}'). Will skip this coverage entry. Verify sonar.sources in .sonarqube\\out\\sonar-project.properties.",
          foundFiles.size(), deterministicBuildPath, pathSuffix);
        return Optional.empty();
      }
    }
    LOG.debug("Did not find deterministic source path in '{}'. Will skip this coverage entry. Verify sonar.sources in .sonarqube\\out\\sonar-project.properties.", deterministicBuildPath);
    return Optional.empty();
  }
}
