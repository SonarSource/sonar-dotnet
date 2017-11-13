/*
 * SonarC#
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
package org.sonar.plugins.csharp;

import java.nio.file.Path;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.rule.ActiveRule;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.utils.MessageException;
import org.sonar.api.utils.System2;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.RoslynDataImporter;

import static java.util.stream.Collectors.toList;

public class CSharpSensor implements Sensor {

  private static final Logger LOG = Loggers.get(CSharpSensor.class);

  private final CSharpConfiguration config;
  private final System2 system;
  private final ProtobufDataImporter protobufDataImporter;
  private final RoslynDataImporter roslynDataImporter;

  public CSharpSensor(CSharpConfiguration config, System2 system, ProtobufDataImporter protobufDataImporter, RoslynDataImporter roslynDataImporter) {
    this.config = config;
    this.system = system;
    this.protobufDataImporter = protobufDataImporter;
    this.roslynDataImporter = roslynDataImporter;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name("C#").onlyOnLanguage(CSharpPlugin.LANGUAGE_KEY);
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

    if (!system.isOsWindows()) {
      throw MessageException.of("C# analysis is not supported on " + System.getProperty("os.name") +
        ". Please, refer to the SonarC# documentation page for more information.");
    }

    return true;
  }

  private static Iterable<InputFile> filesToAnalyze(FileSystem fs) {
    return fs.inputFiles(fs.predicates().and(fs.predicates().hasType(Type.MAIN), fs.predicates().hasLanguage(CSharpPlugin.LANGUAGE_KEY)));
  }

  private void executeInternal(SensorContext context) {
    Optional<Path> roslynReportPath = config.roslynReportPath();
    Optional<Path> protobufReportsDirectory = config.protobufReportPath();

    if (protobufReportsDirectory.isPresent()) {
      LOG.info("Importing analysis results from " + protobufReportsDirectory.get().toAbsolutePath());
      protobufDataImporter.importResults(context, protobufReportsDirectory.get(), CSharpSonarRulesDefinition.REPOSITORY_KEY, !roslynReportPath.isPresent());
    }

    if (roslynReportPath.isPresent()) {
      LOG.info("Importing Roslyn report");
      Map<String, List<RuleKey>> activeRoslynRulesByPartialRepoKey = RoslynProfileExporter.activeRoslynRulesByPartialRepoKey(context.activeRules()
        .findAll()
        .stream()
        .map(ActiveRule::ruleKey)
        .collect(toList()));
      roslynDataImporter.importRoslynReport(roslynReportPath.get(), context, activeRoslynRulesByPartialRepoKey);
    }
  }
}
