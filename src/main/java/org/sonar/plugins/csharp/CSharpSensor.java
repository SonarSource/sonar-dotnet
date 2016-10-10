/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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

import com.google.common.base.Charsets;
import com.google.common.base.Throwables;
import com.google.common.collect.ImmutableMultimap;
import com.google.common.collect.Maps;
import java.io.File;
import java.io.IOException;
import java.nio.file.Path;
import java.nio.file.Files;
import java.nio.file.Paths;
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
import org.sonar.api.utils.command.Command;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.AbstractSensor;
import org.sonarsource.dotnet.shared.plugins.AnalysisInputXml;
import org.sonarsource.dotnet.shared.plugins.SonarAnalyzerScannerExtractor;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;
import org.sonarsource.dotnet.shared.sarif.SarifParserFactory;

import static java.util.stream.Collectors.toList;

public class CSharpSensor extends AbstractSensor implements Sensor {

  private static final Logger LOG = Loggers.get(CSharpSensor.class);

  static final String ROSLYN_REPORT_PATH_PROPERTY_KEY = "sonar.cs.roslyn.reportFilePath";
  static final String ANALYZER_PROJECT_OUT_PATH_PROPERTY_KEY = "sonar.cs.analyzer.projectOutPath";
  static final String ANALYSIS_OUTPUT_DIRECTORY_NAME = "output-cs";

  private final Settings settings;
  private final SonarAnalyzerScannerExtractor extractor;

  public CSharpSensor(Settings settings, SonarAnalyzerScannerExtractor extractor, FileLinesContextFactory fileLinesContextFactory,
    NoSonarFilter noSonarFilter) {
    super(fileLinesContextFactory, noSonarFilter, CSharpSonarRulesDefinition.REPOSITORY_KEY);
    this.settings = settings;
    this.extractor = extractor;
  }

  @Override
  public void describe(SensorDescriptor descriptor) {
    descriptor.name("C#").onlyOnLanguage(CSharpPlugin.LANGUAGE_KEY);
  }

  @Override
  public void execute(SensorContext context) {
    if (!shouldExecuteOnProject(context.fileSystem())) {
      LOG.debug("OS is not Windows. Skip Sensor.");
      return;
    }
    executeInternal(context);
  }

  boolean shouldExecuteOnProject(FileSystem fs) {
    return SystemUtils.IS_OS_WINDOWS && filesToAnalyze(fs).iterator().hasNext();
  }

  private static Iterable<File> filesToAnalyze(FileSystem fs) {
    return fs.files(fs.predicates().and(fs.predicates().hasType(Type.MAIN), fs.predicates().hasLanguage(CSharpPlugin.LANGUAGE_KEY)));
  }

  void executeInternal(SensorContext context) {
    String roslynReportPath = settings.getString(ROSLYN_REPORT_PATH_PROPERTY_KEY);
    boolean hasRoslynReportPath = roslynReportPath != null;

    String analyzerWorkDirPath = settings.getString(ANALYZER_PROJECT_OUT_PATH_PROPERTY_KEY);
    boolean requiresAnalyzerScannerExecution = requiresAnalyzerScannerExecution(analyzerWorkDirPath, ANALYSIS_OUTPUT_DIRECTORY_NAME);

    LOG.info("SonarAnalyzer.Scanner needs to be executed: " + requiresAnalyzerScannerExecution);

    if (requiresAnalyzerScannerExecution) {
      analyze(!hasRoslynReportPath, context);
    }

    Path workDirectory = requiresAnalyzerScannerExecution
      ? toolOutput(context.fileSystem())
      : analyzerOutputDir(analyzerWorkDirPath);

    LOG.info("Importing analysis results from " + workDirectory.toAbsolutePath().toString());
    importResults(context, workDirectory, !hasRoslynReportPath);

    if (hasRoslynReportPath) {
      LOG.info("Importing Roslyn report");
      importRoslynReport(roslynReportPath, context);
    }
  }

  void analyze(boolean includeRules, SensorContext context) {
    if (includeRules) {
      LOG.warn("***********************************************************************************");
      LOG.warn("*                 Use MSBuild 14 to get the best analysis results                 *");
      LOG.warn("* The use of MSBuild 12 or the sonar-scanner to analyze C# projects is DEPRECATED *");
      LOG.warn("***********************************************************************************");

      ImmutableMultimap<String, RuleKey> activeRoslynRulesByPartialRepoKey = RoslynProfileExporter.activeRoslynRulesByPartialRepoKey(context.activeRules()
        .findAll()
        .stream()
        .map(ActiveRule::ruleKey)
        .collect(toList()));

      if (activeRoslynRulesByPartialRepoKey.keySet().size() > 1) {
        throw new IllegalArgumentException(
          "Custom and 3rd party Roslyn analyzers are only by MSBuild 14. Either use MSBuild 14, or disable the custom/3rd party Roslyn analyzers in your quality profile.");
      }
    }

    String analysisSettings = AnalysisInputXml.generate(true, settings.getBoolean("sonar.cs.ignoreHeaderComments"), includeRules, context,
      CSharpSonarRulesDefinition.REPOSITORY_KEY, CSharpPlugin.LANGUAGE_KEY);

    Path analysisInput = toolInput(context.fileSystem());
    Path analysisOutput = toolOutput(context.fileSystem());

    try {
      Files.write(analysisInput, analysisSettings.getBytes(Charsets.UTF_8));
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }

    File executableFile = extractor.executableFile(CSharpPlugin.LANGUAGE_KEY);

    Command command = Command.create(executableFile.getAbsolutePath())
      .addArgument(analysisInput.toAbsolutePath().toString())
      .addArgument(analysisOutput.toAbsolutePath().toString())
      .addArgument(CSharpPlugin.LANGUAGE_KEY);

    int exitCode = CommandExecutor.create().execute(command, new LogInfoStreamConsumer(), new LogErrorStreamConsumer(), Integer.MAX_VALUE);
    if (exitCode != 0) {
      throw new IllegalStateException("The .NET analyzer failed with exit code: " + exitCode + " - Verify that the .NET Framework version 4.5.2 at least is installed.");
    }
  }

  private static void importRoslynReport(String reportPath, final SensorContext context) {
    ImmutableMultimap<String, RuleKey> activeRoslynRulesByPartialRepoKey = RoslynProfileExporter.activeRoslynRulesByPartialRepoKey(context.activeRules()
      .findAll()
      .stream()
      .map(ActiveRule::ruleKey)
      .collect(toList()));
    final Map<String, String> repositoryKeyByRoslynRuleKey = Maps.newHashMap();
    for (RuleKey activeRoslynRuleKey : activeRoslynRulesByPartialRepoKey.values()) {
      String previousRepositoryKey = repositoryKeyByRoslynRuleKey.put(activeRoslynRuleKey.rule(), activeRoslynRuleKey.repository());
      if (previousRepositoryKey != null) {
        throw new IllegalArgumentException("Rule keys must be unique, but \"" + activeRoslynRuleKey.rule() +
          "\" is defined in both the \"" + previousRepositoryKey + "\" and \"" + activeRoslynRuleKey.repository() +
          "\" rule repositories.");
      }
    }

    SarifParserCallback callback = new SarifParserCallbackImplementation(context, repositoryKeyByRoslynRuleKey);
    SarifParserFactory.create(new File(reportPath)).parse(callback);
  }

  private static Path toolOutput(FileSystem fs) {
    return fs.workDir().toPath().resolve(ANALYSIS_OUTPUT_DIRECTORY_NAME);
  }

  private static Path analyzerOutputDir(String analyzerWorkDirPath) {
    return Paths.get(analyzerWorkDirPath, ANALYSIS_OUTPUT_DIRECTORY_NAME);
  }

}
