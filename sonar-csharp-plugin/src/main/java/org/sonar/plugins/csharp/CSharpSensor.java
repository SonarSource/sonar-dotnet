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

import static java.util.stream.Collectors.toList;

import java.io.File;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.rule.ActiveRule;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.utils.MessageException;
import org.sonar.api.utils.System2;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.AbstractSensor;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;
import org.sonarsource.dotnet.shared.sarif.SarifParserFactory;

public class CSharpSensor extends AbstractSensor implements Sensor {

  private static final Logger LOG = Loggers.get(CSharpSensor.class);

  private final Settings settings;
  private final CSharpConfiguration config;
  private final System2 system;

  public CSharpSensor(Settings settings, FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter, CSharpConfiguration config, System2 system) {
    super(fileLinesContextFactory, noSonarFilter, config, CSharpSonarRulesDefinition.REPOSITORY_KEY);
    this.settings = settings;
    this.config = config;
    this.system = system;
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

  private static Iterable<File> filesToAnalyze(FileSystem fs) {
    return fs.files(fs.predicates().and(fs.predicates().hasType(Type.MAIN), fs.predicates().hasLanguage(CSharpPlugin.LANGUAGE_KEY)));
  }

  void executeInternal(SensorContext context) {
    String roslynReportPath = settings.getString(config.getRoslynJsonReportPathProperty());
    boolean hasRoslynReportPath = roslynReportPath != null;

    Path protobufReportsDirectory = config.protobufReportPathFromScanner();

    LOG.info("Importing analysis results from " + protobufReportsDirectory.toAbsolutePath());
    importResults(context, protobufReportsDirectory, !hasRoslynReportPath);

    if (hasRoslynReportPath) {
      LOG.info("Importing Roslyn report");
      importRoslynReport(roslynReportPath, context);
    }
  }

  private static void importRoslynReport(String reportPath, final SensorContext context) {
    Map<String, List<RuleKey>> activeRoslynRulesByPartialRepoKey = RoslynProfileExporter.activeRoslynRulesByPartialRepoKey(context.activeRules()
      .findAll()
      .stream()
      .map(ActiveRule::ruleKey)
      .collect(toList()));
    final Map<String, String> repositoryKeyByRoslynRuleKey = new HashMap<>();
    for (List<RuleKey> rules : activeRoslynRulesByPartialRepoKey.values()) {
      for (RuleKey activeRoslynRuleKey : rules) {
        String previousRepositoryKey = repositoryKeyByRoslynRuleKey.put(activeRoslynRuleKey.rule(), activeRoslynRuleKey.repository());
        if (previousRepositoryKey != null) {
          throw new IllegalArgumentException("Rule keys must be unique, but \"" + activeRoslynRuleKey.rule() +
            "\" is defined in both the \"" + previousRepositoryKey + "\" and \"" + activeRoslynRuleKey.repository() +
            "\" rule repositories.");
        }
      }
    }

    SarifParserCallback callback = new SarifParserCallbackImplementation(context, repositoryKeyByRoslynRuleKey);
    SarifParserFactory.create(Paths.get(reportPath)).accept(callback);
  }

}
