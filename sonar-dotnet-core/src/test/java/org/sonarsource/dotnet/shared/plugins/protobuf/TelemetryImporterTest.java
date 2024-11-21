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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.file.Path;
import org.junit.Test;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;
import org.sonarsource.dotnet.shared.plugins.testutils.AutoDeletingTempFile;

import static org.mockito.Mockito.inOrder;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verifyNoMoreInteractions;

public class TelemetryImporterTest {
  private static void WriteTelemetryToFile(Path file, SonarAnalyzer.Telemetry... telemetry) throws IOException {
    try (var output = new FileOutputStream(file.toFile())) {
      for (var t : telemetry) {
        t.writeDelimitedTo(output);
      }
    }
  }

  @Test
  public void importTelemetryMessagesFromSingleFile() throws IOException {
    SensorContext context = mock(SensorContext.class);
    TelemetryImporter sut = new TelemetryImporter(context, "dotnet", "cs");
    try (var tmp = new AutoDeletingTempFile()) {
      WriteTelemetryToFile(tmp.getFile(),
        SonarAnalyzer.Telemetry.newBuilder()
          .setProjectFullPath("A.csproj")
          .addTargetFramework("tfm1")
          .addTargetFramework("tfm2")
          .setLanguageVersion("cs12")
          .build(),
        // Technically we only expect a single entry in "telemetry.pb", but we read it as if there could be multiple
        // Let's have UT where we have multiple entries, just to be sure we do not regress here.
        SonarAnalyzer.Telemetry.newBuilder()
          .setProjectFullPath("B.csproj")
          .addTargetFramework("tfm1")
          .addTargetFramework("tfm2")
          .addTargetFramework("tfm3")
          .setLanguageVersion("cs12")
          .build());
      sut.accept(tmp.getFile());
      sut.save();
      var inOrder = inOrder(context);
      inOrder.verify(context).addTelemetryProperty("dotnet.cs.language_version.cs12", "2");
      inOrder.verify(context).addTelemetryProperty("dotnet.cs.target_framework.tfm1", "2");
      inOrder.verify(context).addTelemetryProperty("dotnet.cs.target_framework.tfm2", "2");
      inOrder.verify(context).addTelemetryProperty("dotnet.cs.target_framework.tfm3", "1");
      verifyNoMoreInteractions(context);
    }
  }

  @Test
  public void importTelemetryMessagesFromMultipleFile() throws IOException {
    SensorContext context = mock(SensorContext.class);
    TelemetryImporter sut = new TelemetryImporter(context, "dotnet", "cs");
    try (
      var tmp1 = new AutoDeletingTempFile();
      var tmp2 = new AutoDeletingTempFile();) {
      WriteTelemetryToFile(tmp1.getFile(),
        SonarAnalyzer.Telemetry.newBuilder()
          .setProjectFullPath("A.csproj")
          .addTargetFramework("tfm1")
          .addTargetFramework("tfm2")
          .setLanguageVersion("cs12")
          .build());
      WriteTelemetryToFile(tmp2.getFile(),
        SonarAnalyzer.Telemetry.newBuilder()
          .setProjectFullPath("B.csproj")
          .addTargetFramework("tfm1")
          .addTargetFramework("tfm2")
          .addTargetFramework("tfm3")
          .setLanguageVersion("cs12")
          .build());
      sut.accept(tmp1.getFile());
      sut.accept(tmp2.getFile());
      sut.save();
      var inOrder = inOrder(context);
      inOrder.verify(context).addTelemetryProperty("dotnet.cs.language_version.cs12", "2");
      inOrder.verify(context).addTelemetryProperty("dotnet.cs.target_framework.tfm1", "2");
      inOrder.verify(context).addTelemetryProperty("dotnet.cs.target_framework.tfm2", "2");
      inOrder.verify(context).addTelemetryProperty("dotnet.cs.target_framework.tfm3", "1");
      verifyNoMoreInteractions(context);
    }
  }
}
