/*
 * Sonar C# Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp;

import com.google.common.base.Throwables;
import com.google.common.collect.ImmutableList;
import com.google.common.io.ByteStreams;
import com.google.common.io.Files;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.resources.Project;
import org.sonar.api.scan.filesystem.FileQuery;
import org.sonar.api.scan.filesystem.ModuleFileSystem;
import org.sonar.api.utils.command.Command;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.api.utils.command.StreamConsumer;
import org.sonar.plugins.csharp.api.CSharpConstants;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.util.List;

public class CSharpSensor implements Sensor {

  private static final String N_SONARQUBE_ANALYZER = "NSonarQubeAnalyzer";
  private static final String N_SONARQUBE_ANALYZER_ZIP = N_SONARQUBE_ANALYZER + ".zip";
  private static final String N_SONARQUBE_ANALYZER_EXE = N_SONARQUBE_ANALYZER + ".exe";

  private final ModuleFileSystem fileSystem;

  public CSharpSensor(ModuleFileSystem fileSystem) {
    this.fileSystem = fileSystem;
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    return !filesToAnalyze().isEmpty();
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    unzipNSonarQubeAnalyzer();

    for (File file : filesToAnalyze()) {
      double lines = lines(file);

      org.sonar.api.resources.File sonarFile = org.sonar.api.resources.File.fromIOFile(file, project);
      context.saveMeasure(sonarFile, CoreMetrics.LINES, lines);
    }
  }

  private double lines(File file) {
    // FIXME duplicated
    File workingDir = new File(fileSystem.workingDir(), N_SONARQUBE_ANALYZER);
    File executableFile = new File(workingDir, N_SONARQUBE_ANALYZER_EXE);

    Command command = Command.create(executableFile.getAbsolutePath())
      .addArgument(file.getAbsolutePath());

    BufferedStreamConsumer bufferedStreamConsumer = new BufferedStreamConsumer();
    CommandExecutor.create().execute(command, bufferedStreamConsumer, new SinkStreamConsumer(), Integer.MAX_VALUE);

    String firstLine = bufferedStreamConsumer.lines().get(0);

    return Double.parseDouble(firstLine);
  }

  private static class BufferedStreamConsumer implements StreamConsumer {

    ImmutableList.Builder<String> builder = ImmutableList.builder();

    @Override
    public void consumeLine(String line) {
      builder.add(line);
    }

    public List<String> lines() {
      return builder.build();
    }

  }

  private static class SinkStreamConsumer implements StreamConsumer {

    @Override
    public void consumeLine(String line) {
    }

  }

  private List<File> filesToAnalyze() {
    return fileSystem.files(FileQuery.onSource().onLanguage(CSharpConstants.LANGUAGE_KEY));
  }

  private void unzipNSonarQubeAnalyzer() {
    File workingDir = new File(fileSystem.workingDir(), N_SONARQUBE_ANALYZER);
    File zipFile = new File(workingDir, N_SONARQUBE_ANALYZER_ZIP);

    try {
      Files.createParentDirs(zipFile);

      InputStream is = getClass().getResourceAsStream("/" + N_SONARQUBE_ANALYZER_ZIP);
      try {
        Files.write(ByteStreams.toByteArray(is), zipFile);
      } finally {
        is.close();
      }

      new Zip(zipFile).unzip(workingDir);
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }
  }

}
