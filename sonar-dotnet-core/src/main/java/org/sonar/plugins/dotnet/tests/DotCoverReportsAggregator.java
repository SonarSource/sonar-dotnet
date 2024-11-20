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
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.stream.Stream;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;

public class DotCoverReportsAggregator implements CoverageParser {

  private static final Logger LOG = LoggerFactory.getLogger(DotCoverReportsAggregator.class);

  private final DotCoverReportParser parser;

  DotCoverReportsAggregator(DotCoverReportParser parser) {
    this.parser = parser;
  }

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.debug("The current user dir is '{}'.", lazy(() -> System.getProperty("user.dir")));
    LOG.info("Aggregating the HTML reports from '{}'.", file.getAbsolutePath());
    checkIsHtml(file);

    String folderName = extractFolderName(file);
    File folder = new File(file.getParentFile(), folderName + "/src");
    if (!folder.exists()) {
      throw new IllegalArgumentException("The following report dotCover report HTML sources folder cannot be found: " + folder.getAbsolutePath());
    }

    List<File> reportFiles = listReportFiles(folder);
    LOG.debug("dotCover aggregator: collected {} report files to parse.", reportFiles.size());

    for (File reportFile : reportFiles) {
      if (!isExcluded(reportFile)) {
        parser.accept(reportFile, coverage);
      }
    }
    if (reportFiles.isEmpty()) {
      throw new IllegalArgumentException("No dotCover report HTML source file found under: " + folder.getAbsolutePath());
    }
  }

  private static List<File> listReportFiles(File folder) {
    try (Stream<Path> pathStream = Files.list(folder.toPath())) {
      return pathStream
        .map(Path::toFile)
        .filter(f -> f.isFile() && f.getName().endsWith(".html"))
        .toList();
    } catch (IOException e) {
      throw new IllegalStateException(e);
    }
  }

  private static void checkIsHtml(File file) {
    String contents;
    try {
      contents = new String(Files.readAllBytes(file.toPath()), StandardCharsets.UTF_8);
    } catch (IOException e) {
      throw new IllegalStateException(e);
    }
    if (!contents.startsWith("<!DOCTYPE html>")) {
      throw new IllegalArgumentException("Only dotCover HTML reports which start with \"<!DOCTYPE html>\" are supported.");
    }
  }

  private static String extractFolderName(File file) {
    String name = file.getName();
    int lastDot = name.lastIndexOf('.');
    if (lastDot == -1) {
      throw new IllegalArgumentException("The following dotCover report name should have an extension: " + name);
    }

    return name.substring(0, lastDot);
  }

  private static boolean isExcluded(File file) {
    return "nosource.html".equals(file.getName());
  }

}
