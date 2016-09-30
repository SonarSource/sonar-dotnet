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
import com.google.common.io.Files;
import java.io.File;
import java.io.FileReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.nio.file.StandardOpenOption;
import java.util.Collections;

import org.apache.commons.lang.StringEscapeUtils;
import org.apache.commons.lang.StringUtils;
import org.apache.commons.lang.SystemUtils;
import org.assertj.core.groups.Tuple;
import org.junit.Before;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.rule.internal.ActiveRulesBuilder;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.rule.RuleKey;
import org.sonarsource.dotnet.shared.plugins.SonarAnalyzerScannerExtractor;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.sonar.plugins.csharp.CSharpSonarRulesDefinition.REPOSITORY_KEY;

public class CSharpSensorTest {

  @org.junit.Rule
  public ExpectedException thrown = ExpectedException.none();

  @org.junit.Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private CSharpSensor sensor;
  private Settings settings;
  private DefaultInputFile inputFile;
  private FileLinesContext fileLinesContext;
  private FileLinesContextFactory fileLinesContextFactory;
  private SonarAnalyzerScannerExtractor extractor;
  private NoSonarFilter noSonarFilter;
  private SensorContextTester tester;

  private File workDir;

  @Before
  public void prepare() throws Exception {
    workDir = temp.newFolder();
    Path srcDir = Paths.get("src/test/resources/CSharpSensorTest");
    Path destDir = workDir.toPath();
    java.nio.file.Files.walk(srcDir).forEach(path -> {
      if (java.nio.file.Files.isDirectory(path)) {
        return;
      }
      Path relativized = srcDir.relativize(path);
      try {
        Path destFile = destDir.resolve(relativized);
        if (!java.nio.file.Files.exists(destFile.getParent())) {
          java.nio.file.Files.createDirectories(destFile.getParent());
        }
        java.nio.file.Files.copy(path, destFile, StandardCopyOption.COPY_ATTRIBUTES, StandardCopyOption.REPLACE_EXISTING);
      } catch (Exception e) {
        throw new IllegalStateException(e);
      }
    });
    File csFile = new File("src/test/resources/Program.cs").getAbsoluteFile();

    Path roslynReport = workDir.toPath().resolve("roslyn-report.json");
    java.nio.file.Files.write(roslynReport,
      StringUtils.replace(new String(java.nio.file.Files.readAllBytes(roslynReport), StandardCharsets.UTF_8), "Program.cs",
        StringEscapeUtils.escapeJavaScript(csFile.getAbsolutePath())).getBytes(StandardCharsets.UTF_8),
      StandardOpenOption.WRITE);

    tester = SensorContextTester.create(new File("src/test/resources"));
    tester.fileSystem().setWorkDir(workDir);

    inputFile = new DefaultInputFile(tester.module().key(), "Program.cs")
      .setLanguage(CSharpPlugin.LANGUAGE_KEY)
      .initMetadata(new FileMetadata().readMetadata(new FileReader(csFile)));
    tester.fileSystem().add(inputFile);

    fileLinesContext = mock(FileLinesContext.class);
    fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(inputFile)).thenReturn(fileLinesContext);

    extractor = mock(SonarAnalyzerScannerExtractor.class);
    when(extractor.executableFile(CSharpPlugin.LANGUAGE_KEY)).thenReturn(new File(workDir, SystemUtils.IS_OS_WINDOWS ? "fake.bat" : "fake.sh"));

    noSonarFilter = mock(NoSonarFilter.class);
    settings = new Settings();

    sensor = new CSharpSensor(settings, extractor, fileLinesContextFactory, noSonarFilter);
  }

  @Test
  public void metricsAndNoSonar() {
    sensor.executeInternal(tester);

    assertThat(tester.measures(tester.module().key() + ":Program.cs"))
      .extracting("metric.key", "value")
      .containsOnly(
        Tuple.tuple(CoreMetrics.LINES_KEY, 63),
        Tuple.tuple(CoreMetrics.CLASSES_KEY, 4),
        Tuple.tuple(CoreMetrics.NCLOC_KEY, 41),
        Tuple.tuple(CoreMetrics.COMMENT_LINES_KEY, 12),
        Tuple.tuple(CoreMetrics.STATEMENTS_KEY, 6),
        Tuple.tuple(CoreMetrics.FUNCTIONS_KEY, 3),
        Tuple.tuple(CoreMetrics.PUBLIC_API_KEY, 2),
        Tuple.tuple(CoreMetrics.PUBLIC_UNDOCUMENTED_API_KEY, 1),
        Tuple.tuple(CoreMetrics.COMPLEXITY_IN_CLASSES_KEY, 7),
        Tuple.tuple(CoreMetrics.COMPLEXITY_IN_FUNCTIONS_KEY, 7),
        Tuple.tuple(CoreMetrics.FILE_COMPLEXITY_DISTRIBUTION_KEY, "0=0;5=1;10=0;20=0;30=0;60=0;90=0"),
        Tuple.tuple(CoreMetrics.FUNCTION_COMPLEXITY_DISTRIBUTION_KEY, "1=2;2=0;4=1;6=0;8=0;10=0;12=0"),
        Tuple.tuple(CoreMetrics.COMPLEXITY_KEY, 7));

    verify(noSonarFilter).noSonarInFile(inputFile, Collections.singleton(49));

    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 10, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 11, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 12, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 13, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 17, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 18, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 19, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 20, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 21, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 22, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 54, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 56, 1);

    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 1, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 2, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 3, 1);
  }

  @Test
  public void issue() {
    sensor.executeInternal(tester);

    assertThat(tester.allIssues()).extracting("ruleKey", "primaryLocation.textRange.start.line", "primaryLocation.message")
      .containsOnly(
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S1186"), 40,
          "Add a nested comment explaining why this method is empty, throw a \"NotSupportedException\" or complete the implementation."),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S1172"), 40,
          "Remove this unused method parameter \"args\"."),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S1118"), 23,
          "Add a \"protected\" constructor or the \"static\" keyword to the class declaration."),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S101"), 45,
          "Rename class \"IFoo\" to match camel case naming rules, consider using \"Foo\"."),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S101"), 49,
          "Rename class \"IBar\" to match camel case naming rules, consider using \"Bar\".")
        );
  }

  @Test
  public void escapesAnalysisInput() throws Exception {
    tester.setActiveRules(new ActiveRulesBuilder()
      .create(RuleKey.of(REPOSITORY_KEY, "S1186"))
      .setParam("param1", "value1")
      .activate()
      .create(RuleKey.of(REPOSITORY_KEY, "S9999"))
      .setParam("param9", "value9")
      .activate()
      .build());

    sensor.executeInternal(tester);

    assertThat(
      Files.toString(new File(workDir, "SonarLint.xml"), Charsets.UTF_8).replaceAll("\r?\n|\r", "")
        .replaceAll("<File>.*?Program.cs</File>", "<File>Program.cs</File>"))
      .isEqualTo(Files.toString(new File("src/test/resources/CSharpSensorTest/SonarLint-expected.xml"), Charsets.UTF_8).replaceAll("\r?\n|\r", ""));
  }

  @Test
  public void roslynReportIsProcessed() {
    tester.setActiveRules(new ActiveRulesBuilder()
      .create(RuleKey.of(REPOSITORY_KEY, "S1186"))
      .activate()
      .create(RuleKey.of(REPOSITORY_KEY, "[parameters_key]"))
      .activate()
      .create(RuleKey.of("roslyn.foo", "custom-roslyn"))
      .activate()
      .build());

    settings.setProperty(CSharpSensor.ROSLYN_REPORT_PATH_PROPERTY_KEY, new File(workDir, "roslyn-report.json").getAbsolutePath());
    sensor.executeInternal(tester);

    assertThat(tester.allIssues())
      .extracting("ruleKey", "primaryLocation.textRange.start.line", "primaryLocation.message")
      .containsOnly(
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S1186"), 40,
          "Add a nested comment explaining why this method is empty, throw a \"NotSupportedException\" or complete the implementation."),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S1172"), 40,
          "Remove this unused method parameter \"args\"."),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S1118"), 23,
          "Add a \"protected\" constructor or the \"static\" keyword to the class declaration."),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S101"), 45,
          "Rename class \"IFoo\" to match camel case naming rules, consider using \"Foo\"."),
        Tuple.tuple(RuleKey.of(REPOSITORY_KEY, "S101"), 49,
          "Rename class \"IBar\" to match camel case naming rules, consider using \"Bar\"."),
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
  public void roslynRulesNotExecutedTwice() throws Exception {
    settings.setProperty(CSharpSensor.ROSLYN_REPORT_PATH_PROPERTY_KEY, new File(workDir, "roslyn-report.json").getAbsolutePath());
    sensor.executeInternal(tester);

    assertThat(
      Files.toString(new File(workDir, "SonarLint.xml"), Charsets.UTF_8).replaceAll("\r?\n|\r", "")
        .replaceAll("<File>.*?Program.cs</File>", "<File>Program.cs</File>"))
      .isEqualTo(Files.toString(new File("src/test/resources/CSharpSensorTest/SonarLint-expected-with-roslyn.xml"), Charsets.UTF_8).replaceAll("\r?\n|\r", ""));
  }

  @Test
  public void roslynEmptyReportShouldNotFail() {
    settings.setProperty(CSharpSensor.ROSLYN_REPORT_PATH_PROPERTY_KEY, new File(workDir, "roslyn-report-empty.json").getAbsolutePath());
    sensor.executeInternal(tester);
  }

  @Test
  public void failWithDuplicateRuleKey() {
    tester.setActiveRules(new ActiveRulesBuilder()
      .create(RuleKey.of(REPOSITORY_KEY, "[parameters_key]"))
      .activate()
      .create(RuleKey.of("roslyn.foo", "[parameters_key]"))
      .activate()
      .build());

    settings.setProperty(CSharpSensor.ROSLYN_REPORT_PATH_PROPERTY_KEY, new File(workDir, "roslyn-report.json").getAbsolutePath());
    thrown.expectMessage("Rule keys must be unique, but \"[parameters_key]\" is defined in both the \"csharpsquid\" and \"roslyn.foo\" rule repositories.");

    sensor.executeInternal(tester);
  }

  @Test
  public void failWithCustomRoslynRulesAndMSBuild12() {
    tester.setActiveRules(new ActiveRulesBuilder()
      .create(RuleKey.of(REPOSITORY_KEY, "S1186"))
      .activate()
      .create(RuleKey.of("roslyn.foo", "[parameters_key]"))
      .activate()
      .build());

    thrown.expectMessage(
      "Custom and 3rd party Roslyn analyzers are only by MSBuild 14. Either use MSBuild 14, or disable the custom/3rd party Roslyn analyzers in your quality profile.");
    sensor.executeInternal(tester);
  }
}
