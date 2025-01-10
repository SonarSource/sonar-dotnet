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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.file.Path;
import org.junit.Test;
import org.sonarsource.dotnet.shared.plugins.MethodDeclarationsCollector;
import org.sonarsource.dotnet.shared.plugins.testutils.AutoDeletingTempFile;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.protobuf.SonarAnalyzer.MethodDeclarationsInfo;
import static org.sonarsource.dotnet.protobuf.SonarAnalyzer.MethodDeclarationInfo;

public class MethodDeclarationsImporterTest {
  @Test
  public void importMethodDeclarationsFromSingleFile() throws IOException {
    var collector = new MethodDeclarationsCollector();
    var sut = new MethodDeclarationsImporter(collector);
    try (var tmp = new AutoDeletingTempFile()) {
      writeMethodDeclarationsToFile(tmp.getFile(),
        MethodDeclarationsInfo.newBuilder()
          .setFilePath("C:\\MyFilePath\\Project0")
          .setAssemblyName("project0")
          .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
            .setTypeName("Type1")
            .setMethodName("Method1")
            .build())
          .build(),
        MethodDeclarationsInfo.newBuilder()
          .setFilePath("C:\\MyFilePath\\Project1")
          .setAssemblyName("project1")
          .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
            .setTypeName("Type2")
            .setMethodName("Method2")
            .build())
          .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
            .setTypeName("Type3")
            .setMethodName("Method3")
            .build())
          .build());
      sut.accept(tmp.getFile());
      sut.save();
      assertThat(collector.getMethodDeclarations()).satisfiesExactly(
        m -> {
          assertThat(m.getFilePath()).isEqualTo("C:\\MyFilePath\\Project0");
          assertThat(m.getAssemblyName()).isEqualTo("project0");
          assertThat(m.getMethodDeclarationsCount()).isEqualTo(1);
          assertThat(m.getMethodDeclarations(0).getTypeName()).isEqualTo("Type1");
          assertThat(m.getMethodDeclarations(0).getMethodName()).isEqualTo("Method1");
        },
        m -> {
          assertThat(m.getFilePath()).isEqualTo("C:\\MyFilePath\\Project1");
          assertThat(m.getAssemblyName()).isEqualTo("project1");
          assertThat(m.getMethodDeclarationsCount()).isEqualTo(2);
          assertThat(m.getMethodDeclarations(0).getTypeName()).isEqualTo("Type2");
          assertThat(m.getMethodDeclarations(0).getMethodName()).isEqualTo("Method2");
          assertThat(m.getMethodDeclarations(1).getTypeName()).isEqualTo("Type3");
          assertThat(m.getMethodDeclarations(1).getMethodName()).isEqualTo("Method3");
        });
    }
  }

  @Test
  public void importMethodDeclarationsFromMultipleFile() throws IOException {
    var collector = new MethodDeclarationsCollector();
    var sut = new MethodDeclarationsImporter(collector);
    try (
      var tmp1 = new AutoDeletingTempFile();
      var tmp2 = new AutoDeletingTempFile()) {
      writeMethodDeclarationsToFile(tmp1.getFile(),
        MethodDeclarationsInfo.newBuilder()
          .setFilePath("C:\\MyFilePath\\Project0")
          .setAssemblyName("project0")
          .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
            .setTypeName("Type1")
            .setMethodName("Method1")
            .build())
          .build());
      writeMethodDeclarationsToFile(tmp2.getFile(),
        MethodDeclarationsInfo.newBuilder()
          .setFilePath("C:\\MyFilePath\\Project1")
          .setAssemblyName("project1")
          .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
            .setTypeName("Type2")
            .setMethodName("Method2")
            .build())
          .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
            .setTypeName("Type3")
            .setMethodName("Method3")
            .build())
          .build());
      sut.accept(tmp1.getFile());
      sut.accept(tmp2.getFile());
      sut.save();
      assertThat(collector.getMethodDeclarations()).satisfiesExactly(
        m -> {
          assertThat(m.getFilePath()).isEqualTo("C:\\MyFilePath\\Project0");
          assertThat(m.getAssemblyName()).isEqualTo("project0");
          assertThat(m.getMethodDeclarationsCount()).isEqualTo(1);
          assertThat(m.getMethodDeclarations(0).getTypeName()).isEqualTo("Type1");
          assertThat(m.getMethodDeclarations(0).getMethodName()).isEqualTo("Method1");
        },
        m -> {
          assertThat(m.getFilePath()).isEqualTo("C:\\MyFilePath\\Project1");
          assertThat(m.getAssemblyName()).isEqualTo("project1");
          assertThat(m.getMethodDeclarationsCount()).isEqualTo(2);
          assertThat(m.getMethodDeclarations(0).getTypeName()).isEqualTo("Type2");
          assertThat(m.getMethodDeclarations(0).getMethodName()).isEqualTo("Method2");
          assertThat(m.getMethodDeclarations(1).getTypeName()).isEqualTo("Type3");
          assertThat(m.getMethodDeclarations(1).getMethodName()).isEqualTo("Method3");
        });
    }
  }

  private static void writeMethodDeclarationsToFile(Path file, MethodDeclarationsInfo... methodDeclarations) throws IOException {
    try (var output = new FileOutputStream(file.toFile())) {
      for (var t : methodDeclarations) {
        t.writeDelimitedTo(output);
      }
    }
  }
}
