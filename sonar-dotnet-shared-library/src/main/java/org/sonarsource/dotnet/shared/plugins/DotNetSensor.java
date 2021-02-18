/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2021 SonarSource SA
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
  private final ProjectTypeCollector projectTypeCollector;

  public DotNetSensor(DotNetPluginMetadata pluginMetadata, ReportPathCollector reportPathCollector,ProjectTypeCollector projectTypeCollector,
                      ProtobufDataImporter protobufDataImporter, RoslynDataImporter roslynDataImporter) {
    this.pluginMetadata = pluginMetadata;
    this.reportPathCollector = reportPathCollector;
    this.projectTypeCollector = projectTypeCollector;
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
    projectTypeCollector.getSummary().ifPresent(LOG::info);
  }

  private boolean shouldExecuteOnProject(FileSystem fs) {
    if (SensorContextUtils.hasFilesOfType(fs, Type.MAIN, pluginMetadata.languageKey())) {
      return true;
    } else if (SensorContextUtils.hasFilesOfType(fs, Type.TEST, pluginMetadata.languageKey())) {
      LOG.warn("This sensor will be skipped, because the current solution contains only TEST files and no MAIN files. " +
          "Your SonarQube/SonarCloud project will not have results for {} files. " +
          "You can read more about the detection of test projects here: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects",
        pluginMetadata.languageKey());
    } else {
      // it's not a .NET project
      LOG.debug("No files to analyze. Skip Sensor.");
    }
    return false;
  }

  private void executeInternal(SensorContext context) {
    UnaryOperator<String> toRealPath = new RealPathProvider();

    boolean shouldSuggestScannerForMSBuild = false;

    List<Path> protobufPaths = reportPathCollector.protobufDirs();
    if (protobufPaths.isEmpty()) {
      LOG.warn("No protobuf reports found. The {} files will not have highlighting and metrics.", pluginMetadata.shortLanguageName());
      shouldSuggestScannerForMSBuild = true;
    } else {
      protobufDataImporter.importResults(context, protobufPaths, toRealPath);
    }

    List<RoslynReport> roslynReports = reportPathCollector.roslynReports();
    if (roslynReports.isEmpty()) {
      LOG.warn("No Roslyn issue reports were found. The {} files have not been analyzed.", pluginMetadata.shortLanguageName());
      shouldSuggestScannerForMSBuild = true;
    } else {
      Map<String, List<RuleKey>> activeRoslynRulesByPartialRepoKey = activeRoslynRulesByPartialRepoKey(pluginMetadata, context.activeRules()
        .findAll()
        .stream()
        .map(ActiveRule::ruleKey)
        .collect(toList()));
      roslynDataImporter.importRoslynReports(roslynReports, context, activeRoslynRulesByPartialRepoKey, toRealPath);
    }

    if (shouldSuggestScannerForMSBuild) {
      LOG.warn("Your project contains {} files which cannot be analyzed with the scanner you are using."
        + " To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html",
        pluginMetadata.shortLanguageName());
    }
  }
}
