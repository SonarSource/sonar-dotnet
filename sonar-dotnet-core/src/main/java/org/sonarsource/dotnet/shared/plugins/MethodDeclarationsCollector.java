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

import java.util.ArrayList;
import java.util.Collection;
import org.sonar.api.scanner.ScannerSide;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

/**
 * A simple project wide method declarations collector (created before all module sensors),
 * that allows MethodDeclarationsSensors to add method declarations from different protobuf files.
 */
@ScannerSide
public class MethodDeclarationsCollector {
  private final ArrayList<SonarAnalyzer.MethodDeclarationsInfo> methodDeclarations = new ArrayList<>();

  public void addDeclaration(SonarAnalyzer.MethodDeclarationsInfo methodDeclaration) {
    this.methodDeclarations.add(methodDeclaration);
  }

  public Collection<SonarAnalyzer.MethodDeclarationsInfo> getMethodDeclarations() {
    return methodDeclarations;
  }
}
