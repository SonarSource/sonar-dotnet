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
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableMultimap;
import com.google.common.collect.Maps;
import com.google.common.io.Files;
import java.io.File;
import java.io.IOException;
import java.util.Collection;
import java.util.Map;
import java.util.Map.Entry;
import org.apache.commons.lang.SystemUtils;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.rule.ActiveRule;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.batch.sensor.issue.NewIssue;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.utils.command.Command;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.api.utils.command.StreamConsumer;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.SonarAnalyzerScannerExtractor;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporter;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;
import org.sonarsource.dotnet.shared.sarif.SarifParserFactory;

import static java.util.stream.Collectors.toList;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.CPDTOKENS_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.HIGHLIGHT_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.ISSUES_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.METRICS_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.SYMBOLREFS_OUTPUT_PROTOBUF_NAME;

public class CSharpSensor implements Sensor {

  private static final Logger LOG = Loggers.get(CSharpSensor.class);

  static final String ROSLYN_REPORT_PATH_PROPERTY_KEY = "sonar.cs.roslyn.reportFilePath";

  static final String ANALYSIS_OUTPUT_DIRECTORY_NAME = "output-cs";

  private final Settings settings;
  private final SonarAnalyzerScannerExtractor extractor;
  private final FileLinesContextFactory fileLinesContextFactory;
  private final NoSonarFilter noSonarFilter;

  public CSharpSensor(Settings settings, SonarAnalyzerScannerExtractor extractor, FileLinesContextFactory fileLinesContextFactory,
    NoSonarFilter noSonarFilter) {
    this.settings = settings;
    this.extractor = extractor;
    this.fileLinesContextFactory = fileLinesContextFactory;
    this.noSonarFilter = noSonarFilter;
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

  void executeInternal(SensorContext context) {
    String roslynReportPath = settings.getString(ROSLYN_REPORT_PATH_PROPERTY_KEY);
    boolean hasRoslynReportPath = roslynReportPath != null;

    analyze(!hasRoslynReportPath, context);
    importResults(context);

    if (hasRoslynReportPath) {
      importRoslynReport(roslynReportPath, context);
    }
  }

  private void analyze(boolean includeRules, SensorContext context) {
    if (includeRules) {
      LOG.warn("***********************************************************************************");
      LOG.warn("*                 Use MSBuild 14 to get the best analysis results                 *");
      LOG.warn("* The use of MSBuild 12 or the sonar-scanner to analyze C# projects is DEPRECATED *");
      LOG.warn("***********************************************************************************");

      ImmutableMultimap<String, RuleKey> activeRoslynRulesByPartialRepoKey =
        RoslynProfileExporter.activeRoslynRulesByPartialRepoKey(context.activeRules()
          .findAll()
          .stream()
          .map(ActiveRule::ruleKey)
          .collect(toList()));

      if (activeRoslynRulesByPartialRepoKey.keySet().size() > 1) {
        throw new IllegalArgumentException(
          "Custom and 3rd party Roslyn analyzers are only by MSBuild 14. Either use MSBuild 14, or disable the custom/3rd party Roslyn analyzers in your quality profile.");
      }
    }

    String analysisSettings = analysisSettings(true, settings.getBoolean("sonar.cs.ignoreHeaderComments"), includeRules, context);

    File analysisInput = toolInput(context.fileSystem());
    File analysisOutput = toolOutput(context.fileSystem());

    try {
      Files.write(analysisSettings, analysisInput, Charsets.UTF_8);
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }

    File executableFile = extractor.executableFile(CSharpPlugin.LANGUAGE_KEY);

    Command command = Command.create(executableFile.getAbsolutePath())
      .addArgument(analysisInput.getAbsolutePath())
      .addArgument(analysisOutput.getAbsolutePath())
      .addArgument("cs");

    int exitCode = CommandExecutor.create().execute(command, new LogInfoStreamConsumer(), new LogErrorStreamConsumer(), Integer.MAX_VALUE);
    if (exitCode != 0) {
      throw new IllegalStateException("The .NET analyzer failed with exit code: " + exitCode + " - Verify that the .NET Framework version 4.5.2 at least is installed.");
    }
  }

  private static String analysisSettings(boolean includeSettings, boolean ignoreHeaderComments, boolean includeRules, SensorContext context) {
    StringBuilder sb = new StringBuilder();

    appendLine(sb, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    appendLine(sb, "<AnalysisInput>");

    if (includeSettings) {
      appendLine(sb, "  <Settings>");
      appendLine(sb, "    <Setting>");
      appendLine(sb, "      <Key>sonar.cs.ignoreHeaderComments</Key>");
      appendLine(sb, "      <Value>" + (ignoreHeaderComments ? "true" : "false") + "</Value>");
      appendLine(sb, "    </Setting>");
      appendLine(sb, "  </Settings>");
    }

    appendLine(sb, "  <Rules>");
    if (includeRules) {
      Collection<ActiveRule> activeRules = context.activeRules().findByRepository(CSharpSonarRulesDefinition.REPOSITORY_KEY);
      for (ActiveRule activeRule : activeRules) {
        appendLine(sb, "    <Rule>");
        appendLine(sb, "      <Key>" + escapeXml(activeRule.ruleKey().rule()) + "</Key>");
        Map<String, String> parameters = effectiveParameters(activeRule);
        if (!parameters.isEmpty()) {
          appendLine(sb, "      <Parameters>");
          for (Entry<String, String> parameter : parameters.entrySet()) {
            appendLine(sb, "        <Parameter>");
            appendLine(sb, "          <Key>" + escapeXml(parameter.getKey()) + "</Key>");
            appendLine(sb, "          <Value>" + escapeXml(parameter.getValue()) + "</Value>");
            appendLine(sb, "        </Parameter>");
          }
          appendLine(sb, "      </Parameters>");
        }
        appendLine(sb, "    </Rule>");
      }
    }
    appendLine(sb, "  </Rules>");

    appendLine(sb, "  <Files>");
    for (File file : filesToAnalyze(context.fileSystem())) {
      appendLine(sb, "    <File>" + escapeXml(file.getAbsolutePath()) + "</File>");
    }
    appendLine(sb, "  </Files>");

    appendLine(sb, "</AnalysisInput>");

    return sb.toString();
  }

  private static String escapeXml(String str) {
    return str.replace("&", "&amp;").replace("\"", "&quot;").replace("'", "&apos;").replace("<", "&lt;").replace(">", "&gt;");
  }

  private static Map<String, String> effectiveParameters(ActiveRule activeRule) {
    Map<String, String> builder = Maps.newHashMap();

    for (Entry<String, String> param : activeRule.params().entrySet()) {
      builder.put(param.getKey(), param.getValue());
    }

    return ImmutableMap.copyOf(builder);
  }

  private void importResults(SensorContext context) {
    parseProtobuf(context, ProtobufImporters.issuesImporter(CSharpSonarRulesDefinition.REPOSITORY_KEY), ISSUES_OUTPUT_PROTOBUF_NAME);
    parseProtobuf(context, ProtobufImporters.metricsImporter(fileLinesContextFactory, noSonarFilter), METRICS_OUTPUT_PROTOBUF_NAME);
    parseProtobuf(context, ProtobufImporters.highlightImporter(), HIGHLIGHT_OUTPUT_PROTOBUF_NAME);
    parseProtobuf(context, ProtobufImporters.symbolRefsImporter(), SYMBOLREFS_OUTPUT_PROTOBUF_NAME);
    parseProtobuf(context, ProtobufImporters.cpdTokensImporter(), CPDTOKENS_OUTPUT_PROTOBUF_NAME);
  }

  private static void parseProtobuf(SensorContext context, ProtobufImporter importer, String filename) {
    File protobuf = new File(toolOutput(context.fileSystem()), filename);
    if (protobuf.isFile()) {
      importer.accept(context, protobuf);
    } else {
      LOG.warn("Protobuf file not found: " + protobuf);
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

    SarifParserCallback callback = new SarifParserCallbackImpl(context, repositoryKeyByRoslynRuleKey);
    SarifParserFactory.create(new File(reportPath)).parse(callback);
  }

  private static class SarifParserCallbackImpl implements SarifParserCallback {
    private final SensorContext context;
    private final Map<String, String> repositoryKeyByRoslynRuleKey;

    SarifParserCallbackImpl(SensorContext context, Map<String, String> repositoryKeyByRoslynRuleKey) {
      this.context = context;
      this.repositoryKeyByRoslynRuleKey = repositoryKeyByRoslynRuleKey;
    }

    @Override
    public void onProjectIssue(String ruleId, String message) {
      String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
      if (repositoryKey == null) {
        return;
      }
      NewIssue newIssue = context.newIssue();
      newIssue
        .forRule(RuleKey.of(repositoryKey, ruleId))
        .at(newIssue.newLocation()
          .on(context.module())
          .message(message))
        .save();
    }

    @Override
    public void onIssue(String ruleId, String absolutePath, String message, int startLine, int startLineOffset, int endLine, int endLineOffset) {
      String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
      if (repositoryKey == null) {
        return;
      }

      InputFile inputFile = context.fileSystem().inputFile(context.fileSystem().predicates().hasAbsolutePath(absolutePath));
      if (inputFile == null) {
        return;
      }

      NewIssue newIssue = context.newIssue();
      newIssue
        .forRule(RuleKey.of(repositoryKey, ruleId))
        .at(newIssue.newLocation()
          .on(inputFile)
          .at(inputFile.newRange(startLine, startLineOffset, endLine, endLineOffset))
          .message(message))
        .save();
    }
  }

  private static void appendLine(StringBuilder sb, String line) {
    sb.append(line);
    sb.append("\r\n");
  }

  private static class LogInfoStreamConsumer implements StreamConsumer {

    @Override
    public void consumeLine(String line) {
      LOG.info(line);
    }

  }

  private static class LogErrorStreamConsumer implements StreamConsumer {

    @Override
    public void consumeLine(String line) {
      LOG.error(line);
    }

  }

  private static Iterable<File> filesToAnalyze(FileSystem fs) {
    return fs.files(fs.predicates().and(fs.predicates().hasType(Type.MAIN), fs.predicates().hasLanguage(CSharpPlugin.LANGUAGE_KEY)));
  }

  private static File toolInput(FileSystem fs) {
    return new File(fs.workDir(), "SonarLint.xml");
  }

  private static File toolOutput(FileSystem fileSystem) {
    return new File(fileSystem.workDir(), ANALYSIS_OUTPUT_DIRECTORY_NAME);
  }

}
