/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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

package org.sonarsource.dotnet.shared.plugins.testutils;

import java.io.IOException;
import java.nio.file.FileVisitResult;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.SimpleFileVisitor;
import java.nio.file.StandardCopyOption;
import java.nio.file.attribute.BasicFileAttributes;

public class FileUtils {

  // see also https://stackoverflow.com/a/10068306
  public static void copyDirectory(Path sourceDir, Path targetDir) throws IOException {
    // Ensure the source directory exists
    if (!Files.exists(sourceDir) || !Files.isDirectory(sourceDir)) {
      throw new IllegalArgumentException("Source must be an existing directory");
    }

    // Create the target directory if it does not exist
    if (!Files.exists(targetDir)) {
      Files.createDirectories(targetDir);
    }

    // Walk the file tree and copy each file/directory
    Files.walkFileTree(sourceDir, new SimpleFileVisitor<Path>() {
      @Override
      public FileVisitResult preVisitDirectory(Path dir, BasicFileAttributes attrs) throws IOException {
        Path targetPath = targetDir.resolve(sourceDir.relativize(dir));
        if (!Files.exists(targetPath)) {
          Files.createDirectory(targetPath);
        }
        return FileVisitResult.CONTINUE;
      }

      @Override
      public FileVisitResult visitFile(Path file, BasicFileAttributes attrs) throws IOException {
        Path targetPath = targetDir.resolve(sourceDir.relativize(file));
        Files.copy(file, targetPath, StandardCopyOption.REPLACE_EXISTING, StandardCopyOption.COPY_ATTRIBUTES);
        return FileVisitResult.CONTINUE;
      }
    });
  }
}