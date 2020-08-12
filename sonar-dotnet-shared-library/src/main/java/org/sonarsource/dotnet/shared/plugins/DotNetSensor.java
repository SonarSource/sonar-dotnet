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
package org.sonarsource.dotnet.shared.plugins;

import java.nio.file.Path;
import java.util.List;
import java.util.Map;
import java.util.function.UnaryOperator;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.rule.ActiveRule;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import static java.util.stream.Collectors.toList;
import static org.sonarsource.dotnet.shared.plugins.RoslynProfileExporter.activeRoslynRulesByPartialRepoKey;

/**
 * This class is the main sensor for C# and VB.NET which is going to process all Roslyn reports and protobuf contents and then push them to SonarQube.
 *
 * This sensor is a SQ project sensor. It will execute once at the end.
 */
public class DotNetSensor implements ProjectSensor {

  private static final Logger LOG = Loggers.get(DotNetSensor.class);

  private final ProtobufDataImporter protobufDataImporter;
  private final RoslynDataImporter roslynDataImporter;
  private final DotNetPluginMetadata pluginMetadata;
  private final ReportPathCollector reportPathCollector;

  public DotNetSensor(DotNetPluginMetadata pluginMetadata, ReportPathCollector reportPathCollector,
    ProtobufDataImporter protobufDataImporter, RoslynDataImporter roslynDataImporter) {
    this.pluginMetadata = pluginMetadata;
    this.reportPathCollector = reportPathCollector;
    this.protobufDataImporter = protobufDataImporter;
    this.roslynDataImporter = roslynDataImporter;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name(pluginMetadata.shortLanguageName())
      .onlyOnLanguage(pluginMetadata.languageKey());
  }

  @Override
  public void execute(SensorContext context) {
    if (shouldExecuteOnProject(context.fileSystem())) {
      executeInternal(context);
    }
  }

  private boolean shouldExecuteOnProject(FileSystem fs) {
    if (!filesToAnalyze(fs).iterator().hasNext()) {
      LOG.debug("No files to analyze. Skip Sensor.");
      return false;
    }

    return true;
  }

  private Iterable<InputFile> filesToAnalyze(FileSystem fs) {
    return fs.inputFiles(fs.predicates().and(fs.predicates().hasType(Type.MAIN), fs.predicates().hasLanguage(pluginMetadata.languageKey())));
  }

  private void executeInternal(SensorContext context) {
    UnaryOperator<String> toRealPath = new RealPathProvider();

    List<Path> protobufPaths = reportPathCollector.protobufDirs();
    if (!protobufPaths.isEmpty()) {
      protobufDataImporter.importResults(context, protobufPaths, toRealPath);
    }

    List<RoslynReport> roslynDirs = reportPathCollector.roslynDirs();
    if (!roslynDirs.isEmpty()) {
      Map<String, List<RuleKey>> activeRoslynRulesByPartialRepoKey = activeRoslynRulesByPartialRepoKey(pluginMetadata, context.activeRules()
        .findAll()
        .stream()
        .map(ActiveRule::ruleKey)
        .collect(toList()));
      roslynDataImporter.importRoslynReports(roslynDirs, context, activeRoslynRulesByPartialRepoKey, toRealPath);
    }
  }
}
