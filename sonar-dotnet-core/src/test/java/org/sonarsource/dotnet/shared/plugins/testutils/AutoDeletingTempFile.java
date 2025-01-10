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
import java.nio.file.Files;
import java.nio.file.Path;

// Source: https://stackoverflow.com/a/34050507
public class AutoDeletingTempFile implements AutoCloseable {

  private final Path file;

  public AutoDeletingTempFile() throws IOException {
    file = Files.createTempFile(null, null);
  }

  public Path getFile() {
    return file;
  }

  @Override
  public void close() throws IOException {
    Files.deleteIfExists(file);
  }
}
