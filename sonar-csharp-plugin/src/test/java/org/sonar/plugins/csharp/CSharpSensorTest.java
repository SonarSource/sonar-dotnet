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

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.spy;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.sonar.plugins.csharp.CSharpSonarRulesDefinition.REPOSITORY_KEY;

import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.nio.file.StandardOpenOption;

import org.apache.commons.lang.StringEscapeUtils;
import org.apache.commons.lang.StringUtils;
import org.assertj.core.groups.Tuple;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.rule.internal.ActiveRulesBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.utils.MessageException;
import org.sonar.api.utils.System2;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.EncodingInfo;

public class CSharpSensorTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private CSharpSensor sensor;
  private Settings settings;
  private DefaultInputFile inputFile;
  private FileLinesContext fileLinesContext;
  private FileLinesContextFactory fileLinesContextFactory;
  private NoSonarFilter noSonarFilter;
  private SensorContextTester tester;
  private System2 system;

  private Path workDir;

  @Before
  public void prepare() throws Exception {
    workDir = temp.newFolder().toPath();
    Path srcDir = Paths.get("src/test/resources/CSharpSensorTest");
    Files.walk(srcDir).forEach(path -> {
      if (Files.isDirectory(path)) {
        return;
      }
      Path relativized = srcDir.relativize(path);
      try {
        Path destFile = workDir.resolve(relativized);
        if (!Files.exists(destFile.getParent())) {
          Files.createDirectories(destFile.getParent());
        }
        Files.copy(path, destFile, StandardCopyOption.COPY_ATTRIBUTES, StandardCopyOption.REPLACE_EXISTING);
      } catch (Exception e) {
        throw new IllegalStateException(e);
      }
    });
    File csFile = new File("src/test/resources/Program.cs").getAbsoluteFile();

    EncodingInfo msg = EncodingInfo.newBuilder().setEncoding("UTF-8").setFilePath(csFile.getAbsolutePath()).build();
    try (OutputStream output = Files.newOutputStream(workDir.resolve("output-cs\\encoding.pb"))) {
      msg.writeDelimitedTo(output);
    } catch (IOException e) {
      throw new IllegalStateException("could not save message to file", e);
    }

    Path roslynReportPath = workDir.resolve("roslyn-report.json");
    String report = new String(Files.readAllBytes(roslynReportPath), StandardCharsets.UTF_8);
    Files.write(roslynReportPath, StringUtils.replace(report, "Program.cs",
      StringEscapeUtils.escapeJavaScript(csFile.getAbsolutePath())).getBytes(StandardCharsets.UTF_8),
      StandardOpenOption.WRITE);

    tester = SensorContextTester.create(new File("src/test/resources"));
    tester.fileSystem().setWorkDir(workDir.toFile());

    inputFile = new DefaultInputFile(tester.module().key(), "Program.cs")
      .setLanguage(CSharpPlugin.LANGUAGE_KEY)
      .initMetadata(new FileMetadata().readMetadata(new FileReader(csFile)));
    tester.fileSystem().add(inputFile);

    fileLinesContext = mock(FileLinesContext.class);
    fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(inputFile)).thenReturn(fileLinesContext);

    noSonarFilter = mock(NoSonarFilter.class);
    settings = new Settings();

    system = mock(System2.class);

    CSharpConfiguration csConfigConfiguration = new CSharpConfiguration(settings);
    sensor = new CSharpSensor(settings, fileLinesContextFactory, noSonarFilter, csConfigConfiguration, system);
  }

  @Test
  public void roslynReportIsProcessed() throws IOException {
    tester.setActiveRules(new ActiveRulesBuilder()
      .create(RuleKey.of(REPOSITORY_KEY, "S1186"))
      .activate()
      .create(RuleKey.of(REPOSITORY_KEY, "[parameters_key]"))
      .activate()
      .create(RuleKey.of("roslyn.foo", "custom-roslyn"))
      .activate()
      .build());

    settings.setProperty(CSharpConfiguration.ROSLYN_REPORT_PATH_PROPERTY_KEY, workDir.resolve("roslyn-report.json").toString());
    createProtobufOut();

    sensor.executeInternal(tester);

    assertThat(tester.allIssues())
      .extracting("ruleKey", "primaryLocation.textRange.start.line", "primaryLocation.message")
      .containsOnly(
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "[parameters_key]"), 19,
          "Short messages should be used first in Roslyn reports"),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "[parameters_key]"), 1,
          "There only is a full message in the Roslyn report"),
        Tuple.tuple(RuleKey.of("roslyn.foo", "custom-roslyn"), 19,
          "Custom Roslyn analyzer message"),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "[parameters_key]"), null,
          "This is an assembly level Roslyn issue with no location")

    );
  }

  @Test
  public void roslynEmptyReportShouldNotFail() throws IOException {
    settings.setProperty(CSharpConfiguration.ROSLYN_REPORT_PATH_PROPERTY_KEY, workDir.resolve("roslyn-report-empty.json").toString());
    createProtobufOut();
    sensor.executeInternal(tester);
  }

  @Test
  public void failWithDuplicateRuleKey() throws IOException {
    tester.setActiveRules(new ActiveRulesBuilder()
      .create(RuleKey.of(REPOSITORY_KEY, "[parameters_key]"))
      .activate()
      .create(RuleKey.of("roslyn.foo", "[parameters_key]"))
      .activate()
      .build());

    settings.setProperty(CSharpConfiguration.ROSLYN_REPORT_PATH_PROPERTY_KEY, workDir.resolve("roslyn-report.json").toString());
    createProtobufOut();
    thrown.expectMessage("Rule keys must be unique, but \"[parameters_key]\" is defined in both the \"csharpsquid\" and \"roslyn.foo\" rule repositories.");

    sensor.executeInternal(tester);
  }

  private void createProtobufOut() throws IOException {
    Path path = workDir.resolve("report");
    Path outputCs = path.resolve("output-cs");
    Files.createDirectories(outputCs);
    Files.createFile(outputCs.resolve("dummy.pb"));
    settings.setProperty(CSharpConfiguration.ANALYZER_PROJECT_OUT_PATH_PROPERTY_KEY, path.toString());

  }

  @Test
  public void noAnalysisIsExecutedOnEmptyContext() throws Exception {
    tester = SensorContextTester.create(new File("src/test/resources"));

    CSharpSensor spy = spy(sensor);
    spy.execute(tester);

    verify(spy, never()).executeInternal(tester);
  }

  @Test
  public void failWhenOsIsNotWindows() throws MessageException {
    // Arrange
    when(system.isOsWindows()).thenReturn(false);

    // Assert exception
    thrown.expect(MessageException.class);
    thrown.expectMessage("C# analysis is not supported");

    // Act
    sensor.execute(tester);
  }

  @Test
  public void doNotFail_WhenOsIsNotWindows_And_NoCS_FilesDetected() throws MessageException {
    // Arrange
    // Empty context, no C# files to analyze
    tester = SensorContextTester.create(new File("src/test/resources"));
    // Non-windows OS
    when(system.isOsWindows()).thenReturn(false);

    // Act
    sensor.execute(tester);

    // No exceptions thrown
  }
}
