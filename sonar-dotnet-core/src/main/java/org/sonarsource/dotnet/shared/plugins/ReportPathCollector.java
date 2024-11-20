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
package org.sonarsource.dotnet.shared.plugins;

import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import org.sonar.api.scanner.ScannerSide;

@ScannerSide
public class ReportPathCollector {
  private final List<Path> protobufDirs = new ArrayList<>();
  private final List<RoslynReport> roslynReports = new ArrayList<>();

  public void addProtobufDirs(List<Path> paths) {
    protobufDirs.addAll(paths);
  }

  public List<Path> protobufDirs() {
    return Collections.unmodifiableList(new ArrayList<>(protobufDirs));
  }

  public void addRoslynReport(List<RoslynReport> reports) {
    roslynReports.addAll(reports);
  }

  public List<RoslynReport> roslynReports() {
    return Collections.unmodifiableList(new ArrayList<>(roslynReports));
  }
}
