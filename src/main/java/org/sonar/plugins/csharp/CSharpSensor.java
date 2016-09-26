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
import com.google.common.base.Preconditions;
import com.google.common.base.Throwables;
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableMultimap;
import com.google.common.collect.ImmutableSet;
import com.google.common.collect.Maps;
import com.google.common.io.Files;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.Serializable;
import java.util.Collection;
import java.util.Map;
import java.util.Map.Entry;
import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import org.apache.commons.lang.SystemUtils;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.measure.Metric;
import org.sonar.api.batch.rule.ActiveRule;
import org.sonar.api.batch.sensor.Sensor;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.batch.sensor.issue.NewIssue;
import org.sonar.api.batch.sensor.issue.NewIssueLocation;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.utils.command.Command;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.api.utils.command.StreamConsumer;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;
import org.sonarsource.dotnet.shared.sarif.SarifParserFactory;

import static java.util.stream.Collectors.toList;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.HIGHLIGHT_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.SYMBOLREFS_OUTPUT_PROTOBUF_NAME;

public class CSharpSensor implements Sensor {

  private static final Logger LOG = Loggers.get(CSharpSensor.class);

  static final String ROSLYN_REPORT_PATH_PROPERTY_KEY = "sonar.cs.roslyn.reportFilePath";

  private static final String ANALYSIS_OUTPUT_DIRECTORY_NAME = "output";

  // Do not change this. SonarAnalyzer defines this filename
  private static final String ANALYSIS_OUTPUT_XML_NAME = "analysis-output.xml";

  private final Settings settings;
  private final RuleRunnerExtractor extractor;
  private final FileLinesContextFactory fileLinesContextFactory;
  private final NoSonarFilter noSonarFilter;

  public CSharpSensor(Settings settings, RuleRunnerExtractor extractor, FileLinesContextFactory fileLinesContextFactory,
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

    File executableFile = extractor.executableFile();

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
      Collection<ActiveRule> activeRules = context.activeRules().findByRepository(CSharpPlugin.REPOSITORY_KEY);
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
    File analysisOutput = new File(toolOutput(context.fileSystem()), ANALYSIS_OUTPUT_XML_NAME);
    new AnalysisResultImporter(context, fileLinesContextFactory, noSonarFilter).parse(analysisOutput);

    File highlightInfoFile = new File(toolOutput(context.fileSystem()), HIGHLIGHT_OUTPUT_PROTOBUF_NAME);
    if (highlightInfoFile.isFile()) {
      ProtobufImporters.highlightImporter().parse(context, highlightInfoFile);
    } else {
      LOG.warn("Syntax highlighting data file not found: " + highlightInfoFile);
    }

    File symbolRefsInfoFile = new File(toolOutput(context.fileSystem()), SYMBOLREFS_OUTPUT_PROTOBUF_NAME);
    if (symbolRefsInfoFile.isFile()) {
      ProtobufImporters.symbolRefsImporter().parse(context, symbolRefsInfoFile);
    } else {
      LOG.warn("Symbol reference data file not found: " + symbolRefsInfoFile);
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

  private static class AnalysisResultImporter {

    private final SensorContext context;
    private XMLStreamReader stream;
    private final FileLinesContextFactory fileLinesContextFactory;
    private final NoSonarFilter noSonarFilter;

    public AnalysisResultImporter(SensorContext context, FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter) {
      this.context = context;
      this.fileLinesContextFactory = fileLinesContextFactory;
      this.noSonarFilter = noSonarFilter;
    }

    public void parse(File file) {
      XMLInputFactory xmlFactory = XMLInputFactory.newInstance();

      try (InputStreamReader reader = new InputStreamReader(new FileInputStream(file), Charsets.UTF_8)) {
        stream = xmlFactory.createXMLStreamReader(reader);

        while (stream.hasNext()) {
          if (stream.next() == XMLStreamConstants.START_ELEMENT) {
            String tagName = stream.getLocalName();

            if ("File".equals(tagName)) {
              handleFileTag();
            }
          }
        }
      } catch (XMLStreamException | IOException e) {
        throw Throwables.propagate(e);
      } finally {
        closeXmlStream();
      }
    }

    private void closeXmlStream() {
      if (stream != null) {
        try {
          stream.close();
        } catch (XMLStreamException e) {
          throw Throwables.propagate(e);
        }
      }
    }

    private void handleFileTag() throws XMLStreamException {
      InputFile inputFile = null;

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "File".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Path".equals(tagName)) {
            String path = stream.getElementText();
            inputFile = context.fileSystem().inputFile(context.fileSystem().predicates().hasAbsolutePath(path));
          } else if ("Metrics".equals(tagName)) {
            Preconditions.checkState(inputFile != null, "No file path for File Tag");
            handleMetricsTag(inputFile);
          } else if ("Issues".equals(tagName)) {
            Preconditions.checkState(inputFile != null, "No file path for File Tag");
            handleIssuesTag(inputFile);
          }
        }
      }
    }

    private void handleMetricsTag(InputFile inputFile) throws XMLStreamException {
      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Metrics".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Lines".equals(tagName)) {
            handleLinesMetricTag(inputFile);
          } else if ("Classes".equals(tagName)) {
            handleClassesMetricTag(inputFile);
          } else if ("Statements".equals(tagName)) {
            handleStatementsMetricTag(inputFile);
          } else if ("Functions".equals(tagName)) {
            handleFunctionsMetricTag(inputFile);
          } else if ("PublicApi".equals(tagName)) {
            handlePublicApiMetricTag(inputFile);
          } else if ("PublicUndocumentedApi".equals(tagName)) {
            handlePublicUndocumentedApiMetricTag(inputFile);
          } else if ("Complexity".equals(tagName)) {
            handleComplexityMetricTag(inputFile);
          } else if ("FileComplexityDistribution".equals(tagName)) {
            handleFileComplexityDistributionMetricTag(inputFile);
          } else if ("FunctionComplexityDistribution".equals(tagName)) {
            handleFunctionComplexityDistributionMetricTag(inputFile);
          } else if ("Comments".equals(tagName)) {
            handleCommentsMetricTag(inputFile);
          } else if ("LinesOfCode".equals(tagName)) {
            handleLinesOfCodeMetricTag(inputFile);
          }
        }
      }
    }

    private <T extends Serializable> void saveMetric(InputFile inputFile, Metric<T> metric, T value) {
      context.<T>newMeasure()
        .on(inputFile)
        .forMetric(metric)
        .withValue(value)
        .save();
    }

    private void parseAndSaveIntegerMetric(InputFile inputFile, Metric<Integer> metric) throws XMLStreamException {
      int value = Integer.parseInt(stream.getElementText());
      saveMetric(inputFile, metric, value);
    }

    private void parseAndSaveStringMetric(InputFile inputFile, Metric<String> metric) throws XMLStreamException {
      String value = stream.getElementText();
      saveMetric(inputFile, metric, value);
    }

    private void handleLinesMetricTag(InputFile inputFile) throws XMLStreamException {
      parseAndSaveIntegerMetric(inputFile, CoreMetrics.LINES);
    }

    private void handleClassesMetricTag(InputFile inputFile) throws XMLStreamException {
      parseAndSaveIntegerMetric(inputFile, CoreMetrics.CLASSES);
    }

    private void handleStatementsMetricTag(InputFile inputFile) throws XMLStreamException {
      parseAndSaveIntegerMetric(inputFile, CoreMetrics.STATEMENTS);
    }

    private void handleFunctionsMetricTag(InputFile inputFile) throws XMLStreamException {
      parseAndSaveIntegerMetric(inputFile, CoreMetrics.FUNCTIONS);
    }

    private void handlePublicApiMetricTag(InputFile inputFile) throws XMLStreamException {
      parseAndSaveIntegerMetric(inputFile, CoreMetrics.PUBLIC_API);
    }

    private void handlePublicUndocumentedApiMetricTag(InputFile inputFile) throws XMLStreamException {
      parseAndSaveIntegerMetric(inputFile, CoreMetrics.PUBLIC_UNDOCUMENTED_API);
    }

    private void handleComplexityMetricTag(InputFile inputFile) throws XMLStreamException {
      parseAndSaveIntegerMetric(inputFile, CoreMetrics.COMPLEXITY);
    }

    private void handleFileComplexityDistributionMetricTag(InputFile inputFile) throws XMLStreamException {
      parseAndSaveStringMetric(inputFile, CoreMetrics.FILE_COMPLEXITY_DISTRIBUTION);
    }

    private void handleFunctionComplexityDistributionMetricTag(InputFile inputFile) throws XMLStreamException {
      parseAndSaveStringMetric(inputFile, CoreMetrics.FUNCTION_COMPLEXITY_DISTRIBUTION);
    }

    private void handleCommentsMetricTag(InputFile inputFile) throws XMLStreamException {
      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Comments".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("NoSonar".equals(tagName)) {
            handleNoSonarCommentsMetricTag(inputFile);
          } else if ("NonBlank".equals(tagName)) {
            handleNonBlankCommentsMetricTag(inputFile);
          }
        }
      }
    }

    private void handleNoSonarCommentsMetricTag(InputFile inputFile) throws XMLStreamException {
      ImmutableSet.Builder<Integer> builder = ImmutableSet.builder();

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "NoSonar".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Line".equals(tagName)) {
            int line = Integer.parseInt(stream.getElementText());
            builder.add(line);
          } else {
            throw new IllegalArgumentException();
          }
        }
      }

      noSonarFilter.noSonarInFile(inputFile, builder.build());
    }

    private void handleNonBlankCommentsMetricTag(InputFile inputFile) throws XMLStreamException {
      int value = 0;
      FileLinesContext fileLinesContext = fileLinesContextFactory.createFor(inputFile);

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "NonBlank".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Line".equals(tagName)) {
            value++;

            int line = Integer.parseInt(stream.getElementText());
            fileLinesContext.setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, line, 1);
          } else {
            throw new IllegalArgumentException();
          }
        }
      }

      fileLinesContext.save();
      saveMetric(inputFile, CoreMetrics.COMMENT_LINES, value);
    }

    private void handleLinesOfCodeMetricTag(InputFile inputFile) throws XMLStreamException {
      int value = 0;
      FileLinesContext fileLinesContext = fileLinesContextFactory.createFor(inputFile);

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "LinesOfCode".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Line".equals(tagName)) {
            value++;

            int line = Integer.parseInt(stream.getElementText());
            fileLinesContext.setIntValue(CoreMetrics.NCLOC_DATA_KEY, line, 1);
          } else {
            throw new IllegalArgumentException();
          }
        }
      }

      fileLinesContext.save();
      saveMetric(inputFile, CoreMetrics.NCLOC, value);
    }

    private void handleIssuesTag(InputFile inputFile) throws XMLStreamException {
      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Issues".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Issue".equals(tagName)) {
            handleIssueTag(inputFile, context);
          }
        }
      }
    }

    private void handleIssueTag(InputFile inputFile, SensorContext context) throws XMLStreamException {

      String id = null;
      String message = null;
      Integer line = null;

      NewIssue newIssue = context.newIssue();

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Issue".equals(stream.getLocalName())) {
          Preconditions.checkState(!"AnalyzerDriver".equals(id), "The analyzer failed, double check rule parameters or disable failing rules: " + message);

          NewIssueLocation newLocation = newIssue
            .newLocation()
            .on(inputFile)
            .message(message);
          if (line != null) {
            newLocation.at(inputFile.selectLine(line));
          }
          newIssue.forRule(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, id))
            .at(newLocation)
            .save();
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Id".equals(tagName)) {
            id = stream.getElementText();
          } else if ("Line".equals(tagName)) {
            line = Integer.parseInt(stream.getElementText());
          } else if ("Message".equals(tagName)) {
            message = stream.getElementText();
          }
        }
      }
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
