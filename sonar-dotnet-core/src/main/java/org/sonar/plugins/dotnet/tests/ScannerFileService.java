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

import java.util.List;
import java.util.Optional;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.util.stream.StreamSupport;
import org.sonar.api.batch.fs.FilePredicates;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.scanner.ScannerSide;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugins.dotnet.tests.coverage.CoverageParser;

@ScannerSide
public class ScannerFileService implements FileService {
  private static final Logger LOG = LoggerFactory.getLogger(ScannerFileService.class);
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
        .toList();

      if (foundFiles.size() == 1) {
        String foundFile = foundFiles.get(0);
        LOG.trace("Found indexed file '{}' for '{}' (normalized to '{}').", foundFile, deterministicBuildPath, pathSuffix);
        return Optional.of(foundFile);
      } else if (foundFiles.isEmpty()) {
        LOG.debug("The file '{}' is not indexed or does not have the supported language. Will skip this coverage entry. "
            + CoverageParser.VERIFY_SONARPROJECTPROPERTIES_MESSAGE,
          deterministicBuildPath);
        return Optional.empty();
      } else {
        LOG.debug("Found {} indexed files for '{}' (normalized to '{}'). Will skip this coverage entry. "
            + CoverageParser.VERIFY_SONARPROJECTPROPERTIES_MESSAGE,
          foundFiles.size(), deterministicBuildPath, pathSuffix);
        return Optional.empty();
      }
    } else {
      LOG.debug("The file '{}' does not have a deterministic build path and is either not indexed or does not have a supported language. "
          + "Will skip this coverage entry. " + CoverageParser.VERIFY_SONARPROJECTPROPERTIES_MESSAGE,
        deterministicBuildPath);
      return Optional.empty();
    }
  }
}
