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

import com.google.common.base.Preconditions;
import com.google.common.base.Throwables;
import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.stream.Stream;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import static java.util.stream.Collectors.toList;

public class DotCoverReportsAggregator implements CoverageParser {

  private static final Logger LOG = Loggers.get(DotCoverReportsAggregator.class);

  private final DotCoverReportParser parser;

  public DotCoverReportsAggregator(DotCoverReportParser parser) {
    this.parser = parser;
  }

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.info("Aggregating the HTML reports from " + file.getAbsolutePath());
    checkIsHtml(file);

    String folderName = extractFolderName(file);
    File folder = new File(file.getParentFile(), folderName + "/src");
    Preconditions.checkArgument(folder.exists(), "The following report dotCover report HTML sources folder cannot be found: " + folder.getAbsolutePath());

    List<File> reportFiles = listReportFiles(folder);
    for (File reportFile : reportFiles) {
      if (!isExcluded(reportFile)) {
        parser.accept(reportFile, coverage);
      }
    }
    Preconditions.checkArgument(!reportFiles.isEmpty(), "No dotCover report HTML source file found under: " + folder.getAbsolutePath());
  }

  private List<File> listReportFiles(File folder) {
    Stream<Path> pathStream;
    try {
      pathStream = Files.list(folder.toPath());
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }

    return pathStream
      .map(Path::toFile)
      .filter(f -> f.isFile() && f.getName().endsWith(".html"))
      .collect(toList());
  }

  private static void checkIsHtml(File file) {
    String contents;
    try {
      contents = new String(Files.readAllBytes(file.toPath()), StandardCharsets.UTF_8);
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }
    Preconditions.checkArgument(contents.startsWith("<!DOCTYPE html>"), "Only dotCover HTML reports which start with \"<!DOCTYPE html>\" are supported.");
  }

  private static String extractFolderName(File file) {
    String name = file.getName();
    int lastDot = name.lastIndexOf('.');
    Preconditions.checkArgument(lastDot != -1, "The following dotCover report name should have an extension: " + name);

    return name.substring(0, lastDot);
  }

  private static boolean isExcluded(File file) {
    return "nosource.html".equals(file.getName());
  }

}
