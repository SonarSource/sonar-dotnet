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

import java.util.Optional;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

@ScannerSide
public class ScannerFileService implements FileService {
  private static final Logger LOG = Loggers.get(ScannerFileService.class);
  private static final Pattern DETERMINISTIC_SOURCE_PATH_PREFIX = Pattern.compile("^(([a-zA-Z]:)?[\\\\][_][\\\\])|([\\/][_][\\/])");
  private FileSystem fileSystem;
  private String languageKey;

  public ScannerFileService(String languageKey, FileSystem fileSystem) {
    this.languageKey = languageKey;
    this.fileSystem = fileSystem;
  }

  public boolean isSupportedAbsolute(String absolutePath) {
    return fileSystem.hasFiles(
      fileSystem.predicates().and(
        fileSystem.predicates().hasAbsolutePath(absolutePath),
        fileSystem.predicates().hasLanguage(languageKey)));
  }

  public Optional<InputFile> getFilesByRelativePath(String filePath) {
    String normalizedRelativePath = getNormalizedRelativePath(filePath);
    Iterable<InputFile> files = fileSystem.inputFiles(fileSystem.predicates().all());
    int count = 0;
    InputFile foundFile = null;
    for (InputFile file : files) {
      String path = file.uri().getPath();
      if (path.endsWith(normalizedRelativePath)) {
        count++;
        foundFile = file;
      }
    }
    if (count == 0) {
      LOG.trace("Did not find any indexed file for '{}'", normalizedRelativePath);
      return Optional.empty();
    } else if (count > 1) {
      LOG.debug("Found {} indexed files for relative path '{}'. Will skip this coverage entry.", count, normalizedRelativePath);
      return Optional.empty();
    } else {
      LOG.trace("Found indexed file '{}' for coverage entry '{}'", foundFile.uri().getPath(), normalizedRelativePath);
      return Optional.of(foundFile);
    }
  }

  private String getNormalizedRelativePath(String filePath) {
    String relativePath = replaceDeterministicSourcePath(filePath);
    return relativePath.replace('\\', '/');
  }

  private String replaceDeterministicSourcePath(String filePath) {

    Matcher matcher = DETERMINISTIC_SOURCE_PATH_PREFIX.matcher(filePath);
    String subPath = matcher.replaceFirst("");
    if (!filePath.equals(subPath)) {
      LOG.trace("It seems Deterministic Source Paths are used, replacing '{}' with '{}'", filePath, subPath);
    }
    return subPath;
  }

}