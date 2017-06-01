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

import com.google.common.collect.Multimap;
import java.io.File;
import java.nio.file.Path;
import java.util.HashMap;
import java.util.Map;
import org.apache.commons.lang.SystemUtils;
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
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.AbstractSensor;
import org.sonarsource.dotnet.shared.plugins.EncodingPerFile;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;
import org.sonarsource.dotnet.shared.sarif.SarifParserFactory;

import static java.util.stream.Collectors.toList;

public class CSharpSensor extends AbstractSensor implements Sensor {

  private static final Logger LOG = Loggers.get(CSharpSensor.class);

  private final Settings settings;
  private final CSharpConfiguration config;

  public CSharpSensor(Settings settings, FileLinesContextFactory fileLinesContextFactory,
    NoSonarFilter noSonarFilter, CSharpConfiguration config, EncodingPerFile encodingPerFile) {
    super(fileLinesContextFactory, noSonarFilter, config, encodingPerFile, CSharpSonarRulesDefinition.REPOSITORY_KEY);
    this.settings = settings;
    this.config = config;
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

  private static boolean shouldExecuteOnProject(FileSystem fs) {
    if (!SystemUtils.IS_OS_WINDOWS) {
      LOG.debug("OS is not Windows. Skip Sensor.");
      return false;
    }

    if (!filesToAnalyze(fs).iterator().hasNext()) {
      LOG.debug("No files to analyze. Skip Sensor.");
      return false;
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

    LOG.info("Importing analysis results from " + protobufReportsDirectory.toAbsolutePath().toString());
    importResults(context, protobufReportsDirectory, !hasRoslynReportPath);

    if (hasRoslynReportPath) {
      LOG.info("Importing Roslyn report");
      importRoslynReport(roslynReportPath, context);
    }
  }

  private static void importRoslynReport(String reportPath, final SensorContext context) {
    Multimap<String, RuleKey> activeRoslynRulesByPartialRepoKey = RoslynProfileExporter.activeRoslynRulesByPartialRepoKey(context.activeRules()
      .findAll()
      .stream()
      .map(ActiveRule::ruleKey)
      .collect(toList()));
    final Map<String, String> repositoryKeyByRoslynRuleKey = new HashMap<>();
    for (RuleKey activeRoslynRuleKey : activeRoslynRulesByPartialRepoKey.values()) {
      String previousRepositoryKey = repositoryKeyByRoslynRuleKey.put(activeRoslynRuleKey.rule(), activeRoslynRuleKey.repository());
      if (previousRepositoryKey != null) {
        throw new IllegalArgumentException("Rule keys must be unique, but \"" + activeRoslynRuleKey.rule() +
          "\" is defined in both the \"" + previousRepositoryKey + "\" and \"" + activeRoslynRuleKey.repository() +
          "\" rule repositories.");
      }
    }

    SarifParserCallback callback = new SarifParserCallbackImplementation(context, repositoryKeyByRoslynRuleKey);
    SarifParserFactory.create(new File(reportPath)).accept(callback);
  }

}
