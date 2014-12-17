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

import com.google.common.base.Charsets;
import com.google.common.base.Preconditions;
import com.google.common.base.Throwables;
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.ImmutableSet;
import com.google.common.collect.Maps;
import com.google.common.io.ByteStreams;
import com.google.common.io.Closeables;
import com.google.common.io.Files;
import org.sonar.api.batch.DependedUpon;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.checks.NoSonarFilter;
import org.sonar.api.component.ResourcePerspectives;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.Issuable;
import org.sonar.api.issue.Issuable.IssueBuilder;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ActiveRuleParam;
import org.sonar.api.rules.RuleParam;
import org.sonar.api.scan.filesystem.FileQuery;
import org.sonar.api.scan.filesystem.ModuleFileSystem;
import org.sonar.api.utils.command.Command;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.api.utils.command.StreamConsumer;
import org.sonar.plugins.csharp.api.CSharpConstants;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

@DependedUpon("NSonarQubeAnalysis")
public class CSharpSensor implements Sensor {

  private static final String N_SONARQUBE_ANALYZER = "NSonarQubeAnalyzer";
  private static final String N_SONARQUBE_ANALYZER_ZIP = N_SONARQUBE_ANALYZER + ".zip";
  private static final String N_SONARQUBE_ANALYZER_EXE = N_SONARQUBE_ANALYZER + ".exe";

  private static final String REPOSITORY_KEY = "csharpsquid";

  private final Settings settings;
  private final ModuleFileSystem fileSystem;
  private final FileLinesContextFactory fileLinesContextFactory;
  private final NoSonarFilter noSonarFilter;
  private final RulesProfile ruleProfile;
  private final ResourcePerspectives perspectives;

  public CSharpSensor(Settings settings, ModuleFileSystem fileSystem, FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter, RulesProfile ruleProfile,
    ResourcePerspectives perspectives) {
    this.settings = settings;
    this.fileSystem = fileSystem;
    this.fileLinesContextFactory = fileLinesContextFactory;
    this.noSonarFilter = noSonarFilter;
    this.ruleProfile = ruleProfile;
    this.perspectives = perspectives;
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    return !filesToAnalyze().isEmpty();
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    unzipNSonarQubeAnalyzer();

    analyze();
    importResults(project, context);
  }

  private void analyze() {
    StringBuilder sb = new StringBuilder();
    appendLine(sb, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    appendLine(sb, "<AnalysisInput>");
    appendLine(sb, "  <Settings>");
    appendLine(sb, "    <Setting>");
    appendLine(sb, "      <Key>sonar.cs.ignoreHeaderComments</Key>");
    appendLine(sb, "      <Value>" + (settings.getBoolean("sonar.cs.ignoreHeaderComments") ? "true" : "false") + "</Value>");
    appendLine(sb, "    </Setting>");
    appendLine(sb, "  </Settings>");
    appendLine(sb, "  <Rules>");
    for (ActiveRule activeRule : ruleProfile.getActiveRulesByRepository(REPOSITORY_KEY)) {
      appendLine(sb, "    <Rule>");
      appendLine(sb, "      <Key>" + activeRule.getRuleKey() + "</Key>");
      if (activeRule.getRule().getParent() != null) {
        appendLine(sb, "      <ParentKey>" + activeRule.getRule().getParent().getKey() + "</ParentKey>");
      }
      Map<String, String> parameters = effectiveParameters(activeRule);
      if (!parameters.isEmpty()) {
        appendLine(sb, "      <Parameters>");
        for (Entry<String, String> parameter : parameters.entrySet()) {
          appendLine(sb, "        <Parameter>");
          appendLine(sb, "          <Key>" + parameter.getKey() + "</Key>");
          appendLine(sb, "          <Value>" + parameter.getValue() + "</Value>");
          appendLine(sb, "        </Parameter>");
        }
        appendLine(sb, "      </Parameters>");
      }
      appendLine(sb, "    </Rule>");
    }
    appendLine(sb, "  </Rules>");
    appendLine(sb, "  <Files>");
    for (File file : filesToAnalyze()) {
      appendLine(sb, "    <File>" + file.getAbsolutePath() + "</File>");
    }
    appendLine(sb, "  </Files>");
    appendLine(sb, "</AnalysisInput>");

    File analysisInput = new File(fileSystem.workingDir(), "analysis-input.xml");
    File analysisOutput = new File(fileSystem.workingDir(), "analysis-output.xml");

    try {
      Files.write(sb, analysisInput, Charsets.UTF_8);
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }

    // FIXME duplicated
    File workingDir = new File(fileSystem.workingDir(), N_SONARQUBE_ANALYZER);
    File executableFile = new File(workingDir, N_SONARQUBE_ANALYZER_EXE);

    Command command = Command.create(executableFile.getAbsolutePath())
      .addArgument(analysisInput.getAbsolutePath())
      .addArgument(analysisOutput.getAbsolutePath());

    CommandExecutor.create().execute(command, new SinkStreamConsumer(), new SinkStreamConsumer(), Integer.MAX_VALUE);
  }

  private static Map<String, String> effectiveParameters(ActiveRule activeRule) {
    Map<String, String> builder = Maps.newHashMap();

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
    // FIXME duplicated
    File analysisOutput = new File(fileSystem.workingDir(), "analysis-output.xml");

    new AnalysisResultImporter(project, context, fileLinesContextFactory, noSonarFilter, perspectives).parse(analysisOutput);
  }

  private static class AnalysisResultImporter {

    private final Project project;
    private final SensorContext context;
    private XMLStreamReader stream;
    private final FileLinesContextFactory fileLinesContextFactory;
    private final NoSonarFilter noSonarFilter;
    private final ResourcePerspectives perspectives;

    public AnalysisResultImporter(Project project, SensorContext context, FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter,
      ResourcePerspectives perspectives) {
      this.project = project;
      this.context = context;
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
      org.sonar.api.resources.File sonarFile = null;

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "File".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Path".equals(tagName)) {
            String path = stream.getElementText();
            sonarFile = org.sonar.api.resources.File.fromIOFile(new File(path), project);
          } else if ("Metrics".equals(tagName)) {
            // TODO Better message
            Preconditions.checkState(sonarFile != null);
            handleMetricsTag(sonarFile);
          } else if ("Issues".equals(tagName)) {
            // TODO Better message
            Preconditions.checkState(sonarFile != null);
            handleIssuesTag(sonarFile);
          }
        }
      }
    }

    private void handleMetricsTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Metrics".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Lines".equals(tagName)) {
            handleLinesMetricTag(sonarFile);
          } else if ("Classes".equals(tagName)) {
            handleClassesMetricTag(sonarFile);
          } else if ("Accessors".equals(tagName)) {
            handleAccessorsMetricTag(sonarFile);
          } else if ("Statements".equals(tagName)) {
            handleStatementsMetricTag(sonarFile);
          } else if ("Functions".equals(tagName)) {
            handleFunctionsMetricTag(sonarFile);
          } else if ("PublicApi".equals(tagName)) {
            handlePublicApiMetricTag(sonarFile);
          } else if ("PublicUndocumentedApi".equals(tagName)) {
            handlePublicUndocumentedApiMetricTag(sonarFile);
          } else if ("Complexity".equals(tagName)) {
            handleComplexityMetricTag(sonarFile);
          } else if ("Comments".equals(tagName)) {
            handleCommentsMetricTag(sonarFile);
          } else if ("LinesOfCode".equals(tagName)) {
            handleLinesOfCodeMetricTag(sonarFile);
          }
        }
      }
    }

    private void handleLinesMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(sonarFile, CoreMetrics.LINES, value);
    }

    private void handleClassesMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(sonarFile, CoreMetrics.CLASSES, value);
    }

    private void handleAccessorsMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(sonarFile, CoreMetrics.ACCESSORS, value);
    }

    private void handleStatementsMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(sonarFile, CoreMetrics.STATEMENTS, value);
    }

    private void handleFunctionsMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(sonarFile, CoreMetrics.FUNCTIONS, value);
    }

    private void handlePublicApiMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(sonarFile, CoreMetrics.PUBLIC_API, value);
    }

    private void handlePublicUndocumentedApiMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(sonarFile, CoreMetrics.PUBLIC_UNDOCUMENTED_API, value);
    }

    private void handleComplexityMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = Double.parseDouble(stream.getElementText());
      context.saveMeasure(sonarFile, CoreMetrics.COMPLEXITY, value);
    }

    private void handleCommentsMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Comments".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("NoSonar".equals(tagName)) {
            handleNoSonarCommentsMetricTag(sonarFile);
          } else if ("NonBlank".equals(tagName)) {
            handleNonBlankCommentsMetricTag(sonarFile);
          }
        }
      }
    }

    private void handleNoSonarCommentsMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
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

      noSonarFilter.addResource(sonarFile, builder.build());
    }

    private void handleNonBlankCommentsMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = 0;
      FileLinesContext fileLinesContext = fileLinesContextFactory.createFor(sonarFile);

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
      context.saveMeasure(sonarFile, CoreMetrics.COMMENT_LINES, value);
    }

    private void handleLinesOfCodeMetricTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      double value = 0;
      FileLinesContext fileLinesContext = fileLinesContextFactory.createFor(sonarFile);

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
      context.saveMeasure(sonarFile, CoreMetrics.NCLOC, value);
    }

    private void handleIssuesTag(org.sonar.api.resources.File sonarFile) throws XMLStreamException {
      Issuable issuable = perspectives.as(Issuable.class, sonarFile);

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Issues".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Issue".equals(tagName)) {
            if (issuable != null) {
              handleIssueTag(issuable);
            }
          }
        }
      }
    }

    private void handleIssueTag(Issuable issuable) throws XMLStreamException {
      IssueBuilder builder = issuable.newIssueBuilder();

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Issue".equals(stream.getLocalName())) {
          issuable.addIssue(builder.build());
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Id".equals(tagName)) {
            builder.ruleKey(RuleKey.of(REPOSITORY_KEY, stream.getElementText()));
          } else if ("Line".equals(tagName)) {
            builder.line(Integer.parseInt(stream.getElementText()));
          } else if ("Message".equals(tagName)) {
            builder.message(stream.getElementText());
          }
        }
      }
    }

  }

  private void appendLine(StringBuilder sb, String line) {
    sb.append(line);
    sb.append("\r\n");
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
