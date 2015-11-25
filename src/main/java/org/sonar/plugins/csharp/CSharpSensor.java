/*
 * SonarQube C# Plugin
 * Copyright (C) 2014 SonarSource
 * sonarqube@googlegroups.com
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

import com.google.common.base.Charsets;
import com.google.common.base.Preconditions;
import com.google.common.base.Throwables;
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableSet;
import com.google.common.collect.Maps;
import com.google.common.collect.Sets;
import com.google.common.io.Closeables;
import com.google.common.io.Files;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.component.ResourcePerspectives;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.Issuable;
import org.sonar.api.issue.Issuable.IssueBuilder;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.PersistenceMode;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ActiveRuleParam;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleParam;
import org.sonar.api.utils.command.Command;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.api.utils.command.StreamConsumer;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;

public class CSharpSensor implements Sensor {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpSensor.class);
  private static final String ROSLYN_REPORT_PATH_PROPERTY_KEY = "sonar.cs.roslyn.reportFilePath";

  private final Settings settings;
  private final RuleRunnerExtractor extractor;
  private final FileSystem fs;
  private final FileLinesContextFactory fileLinesContextFactory;
  private final NoSonarFilter noSonarFilter;
  private final RulesProfile ruleProfile;
  private final ResourcePerspectives perspectives;

  public CSharpSensor(Settings settings, RuleRunnerExtractor extractor, FileSystem fs, FileLinesContextFactory fileLinesContextFactory,
    NoSonarFilter noSonarFilter, RulesProfile ruleProfile,
    ResourcePerspectives perspectives) {
    this.settings = settings;
    this.extractor = extractor;
    this.fs = fs;
    this.fileLinesContextFactory = fileLinesContextFactory;
    this.noSonarFilter = noSonarFilter;
    this.ruleProfile = ruleProfile;
    this.perspectives = perspectives;
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    return filesToAnalyze().iterator().hasNext();
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    String roslynReportPath = settings.getString(ROSLYN_REPORT_PATH_PROPERTY_KEY);
    boolean hasRoslynReportPath = roslynReportPath != null;

    analyze(!hasRoslynReportPath);
    importResults(project, context);

    if (hasRoslynReportPath) {
      importRoslynReport(roslynReportPath);
    }
  }

  private void analyze(boolean includeRules) {
    String analysisSettings = analysisSettings(true, settings.getBoolean("sonar.cs.ignoreHeaderComments"), includeRules, ruleProfile, filesToAnalyze());

    File analysisInput = toolInput();
    File analysisOutput = toolOutput();

    try {
      Files.write(analysisSettings, analysisInput, Charsets.UTF_8);
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }

    File executableFile = extractor.executableFile();

    Command command = Command.create(executableFile.getAbsolutePath())
      .addArgument(analysisInput.getAbsolutePath())
      .addArgument(analysisOutput.getAbsolutePath());

    int exitCode = CommandExecutor.create().execute(command, new LogInfoStreamConsumer(), new LogErrorStreamConsumer(), Integer.MAX_VALUE);
    if (exitCode != 0) {
      throw new IllegalStateException("The .NET analyzer failed with exit code: " + exitCode + " - Verify that the .NET Framework version 4.5.2 at least is installed.");
    }
  }

  public static String analysisSettings(boolean includeSettings, boolean ignoreHeaderComments, boolean includeRules, RulesProfile ruleProfile, Iterable<File> files) {
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
      for (ActiveRule activeRule : ruleProfile.getActiveRulesByRepository(CSharpPlugin.REPOSITORY_KEY)) {
        appendLine(sb, "    <Rule>");
        Rule template = activeRule.getRule().getTemplate();
        String ruleKey = template == null ? activeRule.getRuleKey() : template.getKey();
        appendLine(sb, "      <Key>" + escapeXml(ruleKey) + "</Key>");
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
    for (File file : files) {
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

    if (activeRule.getRule().getTemplate() != null) {
      builder.put("RuleKey", activeRule.getRuleKey());
    }

    for (ActiveRuleParam param : activeRule.getActiveRuleParams()) {
      builder.put(param.getKey(), param.getValue());
    }

    for (RuleParam param : activeRule.getRule().getParams()) {
      if (!builder.containsKey(param.getKey())) {
        builder.put(param.getKey(), param.getDefaultValue());
      }
    }

    return ImmutableMap.copyOf(builder);
  }

  private void importResults(Project project, SensorContext context) {
    File analysisOutput = toolOutput();

    new AnalysisResultImporter(project, context, fs, fileLinesContextFactory, noSonarFilter, perspectives).parse(analysisOutput);
  }

  private void importRoslynReport(String reportPath) {
    String contents;
    try {
      contents = Files.toString(new File(reportPath), Charsets.UTF_8);
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }

    Set<String> activeRuleIds = Sets.newHashSet();
    for (ActiveRule activeRule : ruleProfile.getActiveRulesByRepository(CSharpPlugin.REPOSITORY_KEY)) {
      activeRuleIds.add(activeRule.getRuleKey());
    }

    JsonParser parser = new JsonParser();
    for (JsonElement issueElement : parser.parse(contents).getAsJsonObject().get("issues").getAsJsonArray()) {
      JsonObject issue = issueElement.getAsJsonObject();

      String ruleId = issue.get("ruleId").getAsString();
      if (!activeRuleIds.contains(ruleId)) {
        continue;
      }

      String message = issue.get(issue.has("shortMessage") ? "shortMessage" : "fullMessage").getAsString();
      for (JsonElement locationElement : issue.get("locations").getAsJsonArray()) {
        JsonObject location = locationElement.getAsJsonObject();
        if (location.has("analysisTarget")) {
          for (JsonElement analysisTargetElement : location.get("analysisTarget").getAsJsonArray()) {
            JsonObject analysisTarget = analysisTargetElement.getAsJsonObject();
            String uri = analysisTarget.get("uri").getAsString();
            JsonObject region = analysisTarget.get("region").getAsJsonObject();
            int startLine = region.get("startLine").getAsInt();

            handleRoslynIssue(ruleId, uri, startLine, message);
          }
        }
      }
    }
  }

  private void handleRoslynIssue(String ruleId, String uri, int startLine, String fullMessage) {
    InputFile inputFile = fs.inputFile(fs.predicates().hasAbsolutePath(uri));
    if (inputFile != null) {
      Issuable issuable = perspectives.as(Issuable.class, inputFile);
      if (issuable != null) {
        IssueBuilder builder = issuable.newIssueBuilder();
        builder.ruleKey(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, ruleId));
        builder.line(startLine);
        builder.message(fullMessage);
        issuable.addIssue(builder.build());
      }
    }
  }

  private static class AnalysisResultImporter {

    private final SensorContext context;
    private final FileSystem fs;
    private XMLStreamReader stream;
    private final FileLinesContextFactory fileLinesContextFactory;
    private final NoSonarFilter noSonarFilter;
    private final ResourcePerspectives perspectives;

    public AnalysisResultImporter(Project project, SensorContext context, FileSystem fs, FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter,
      ResourcePerspectives perspectives) {
      this.context = context;
      this.fs = fs;
      this.fileLinesContextFactory = fileLinesContextFactory;
      this.noSonarFilter = noSonarFilter;
      this.perspectives = perspectives;
    }

    public void parse(File file) {
      InputStreamReader reader = null;
      XMLInputFactory xmlFactory = XMLInputFactory.newInstance();

      try {
        reader = new InputStreamReader(new FileInputStream(file), Charsets.UTF_8);
        stream = xmlFactory.createXMLStreamReader(reader);

        while (stream.hasNext()) {
          if (stream.next() == XMLStreamConstants.START_ELEMENT) {
            String tagName = stream.getLocalName();

            if ("File".equals(tagName)) {
              handleFileTag();
            }
          }
        }
      } catch (IOException e) {
        throw Throwables.propagate(e);
      } catch (XMLStreamException e) {
        throw Throwables.propagate(e);
      } finally {
        closeXmlStream();
        Closeables.closeQuietly(reader);
      }

      return;
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
            inputFile = fs.inputFile(fs.predicates().hasAbsolutePath(path));
          } else if ("Metrics".equals(tagName)) {
            // TODO Better message
            Preconditions.checkState(inputFile != null);
            handleMetricsTag(inputFile);
          } else if ("Issues".equals(tagName)) {
            // TODO Better message
            Preconditions.checkState(inputFile != null);
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
          } else if ("Accessors".equals(tagName)) {
            handleAccessorsMetricTag(inputFile);
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

    private void handleLinesMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(inputFile, CoreMetrics.LINES, value);
    }

    private void handleClassesMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(inputFile, CoreMetrics.CLASSES, value);
    }

    private void handleAccessorsMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(inputFile, CoreMetrics.ACCESSORS, value);
    }

    private void handleStatementsMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(inputFile, CoreMetrics.STATEMENTS, value);
    }

    private void handleFunctionsMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(inputFile, CoreMetrics.FUNCTIONS, value);
    }

    private void handlePublicApiMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(inputFile, CoreMetrics.PUBLIC_API, value);
    }

    private void handlePublicUndocumentedApiMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(inputFile, CoreMetrics.PUBLIC_UNDOCUMENTED_API, value);
    }

    private void handleComplexityMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(inputFile, CoreMetrics.COMPLEXITY, value);
    }

    private void handleFileComplexityDistributionMetricTag(InputFile inputFile) throws XMLStreamException {
      String value = stream.getElementText();
      context.saveMeasure(inputFile, new Measure(CoreMetrics.FILE_COMPLEXITY_DISTRIBUTION, value).setPersistenceMode(PersistenceMode.MEMORY));
    }

    private void handleFunctionComplexityDistributionMetricTag(InputFile inputFile) throws XMLStreamException {
      String value = stream.getElementText();
      context.saveMeasure(inputFile, new Measure(CoreMetrics.FUNCTION_COMPLEXITY_DISTRIBUTION, value).setPersistenceMode(PersistenceMode.MEMORY));
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

      noSonarFilter.addComponent(((DefaultInputFile) inputFile).key(), builder.build());
    }

    private void handleNonBlankCommentsMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = 0;
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
      context.saveMeasure(inputFile, CoreMetrics.COMMENT_LINES, value);
    }

    private void handleLinesOfCodeMetricTag(InputFile inputFile) throws XMLStreamException {
      double value = 0;
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
      context.saveMeasure(inputFile, CoreMetrics.NCLOC, value);
    }

    private void handleIssuesTag(InputFile inputFile) throws XMLStreamException {
      Issuable issuable = perspectives.as(Issuable.class, inputFile);

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Issues".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Issue".equals(tagName) && issuable != null) {
            handleIssueTag(issuable);
          }
        }
      }
    }

    private void handleIssueTag(Issuable issuable) throws XMLStreamException {
      IssueBuilder builder = issuable.newIssueBuilder();

      String id = null;
      String message = null;

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Issue".equals(stream.getLocalName())) {
          Preconditions.checkState(!"AnalyzerDriver".equals(id), "The analyzer failed, double check rule parameters or disable failing rules: " + message);

          builder.ruleKey(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, id));
          builder.message(message);

          issuable.addIssue(builder.build());
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Id".equals(tagName)) {
            id = stream.getElementText();
          } else if ("Line".equals(tagName)) {
            builder.line(Integer.parseInt(stream.getElementText()));
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

  private Iterable<File> filesToAnalyze() {
    return fs.files(fs.predicates().and(fs.predicates().hasType(Type.MAIN), fs.predicates().hasLanguage(CSharpPlugin.LANGUAGE_KEY)));
  }

  private File toolInput() {
    return new File(fs.workDir(), "analysis-input.xml");
  }

  private File toolOutput() {
    return toolOutput(fs);
  }

  public static File toolOutput(FileSystem fileSystem) {
    return new File(fileSystem.workDir(), "analysis-output.xml");
  }

}
