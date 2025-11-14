/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;
import java.util.Collections;
import java.util.List;
import org.apache.commons.io.FileUtils;
import org.assertj.core.groups.Tuple;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.rule.internal.ActiveRulesBuilder;
import org.sonar.api.batch.rule.internal.NewActiveRule;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.internal.apachecommons.text.StringEscapeUtils;
import org.sonar.api.internal.apachecommons.lang3.StringUtils;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class RoslynDataImporterTest {
  @Rule
  public ExpectedException exception = ExpectedException.none();
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();
  @Rule
  public LogTester logTester = new LogTester();

  private PluginMetadata metadata = csPluginMetadata();
  private RoslynDataImporter roslynDataImporter = new RoslynDataImporter(metadata, mock(AbstractLanguageConfiguration.class));
  private SensorContextTester tester;
  private Path workDir;

  @Before
  public void setUp() throws IOException {
    logTester.setLevel(Level.DEBUG);
    workDir = temp.getRoot().toPath().resolve("reports");
    Path csFile = Paths.get("src/test/resources/Program.cs").toAbsolutePath();

    // copy test reports to work dir
    FileUtils.copyDirectory(new File("src/test/resources/RoslynDataImporterTest"), workDir.toFile());

    // replace file path in the roslyn report to point to real cs file in the resources
    updateCodeFilePathsInReport("roslyn-report.json", false);

    tester = SensorContextTester.create(csFile.getParent());

    DefaultInputFile inputFile = new TestInputFileBuilder(tester.module().key(), csFile.getParent().toFile(), csFile.toFile())
      .setLanguage("cs")
      .initMetadata(new String(Files.readAllBytes(csFile), StandardCharsets.UTF_8))
      .build();

    tester.fileSystem().setWorkDir(workDir);
    tester.fileSystem().add(inputFile);
  }

  @Test
  public void roslynReportIsProcessed() {
    addActiveRules();
    roslynDataImporter.importRoslynReports(Collections.singletonList(new RoslynReport(tester.project(), workDir.resolve("roslyn-report.json"))), tester,
      String::toString);

    assertThat(tester.allIssues())
      .extracting("ruleKey", "primaryLocation.textRange.start.line", "primaryLocation.message")
      .containsOnly(
        Tuple.tuple(RuleKey.of("csharpsquid", "[parameters_key]"), 19,
          "Short messages should be used first in Roslyn reports"),
        Tuple.tuple(RuleKey.of("csharpsquid", "[parameters_key]"), 1,
          "There only is a full message in the Roslyn report"),
        Tuple.tuple(RuleKey.of("roslyn.foo", "custom-roslyn"), 19,
          "Custom Roslyn analyzer message"),
        Tuple.tuple(RuleKey.of("csharpsquid", "[parameters_key]"), null,
          "This is an assembly level Roslyn issue with no location"));

    assertThat(tester.allAdHocRules()).isEmpty();
    assertThat(tester.allExternalIssues()).hasSize(1);
    assertThat(logTester.logs(Level.DEBUG)).contains("Processing Roslyn report: " + workDir.resolve("roslyn-report.json"));
  }

  @Test
  public void roslynEmptyReportShouldNotFail() {
    addActiveRules();
    roslynDataImporter.importRoslynReports(Collections.singletonList(new RoslynReport(null, workDir.resolve("roslyn-report-empty.json"))), tester, String::toString);

    assertThat(tester.allIssues()).isEmpty();
    assertThat(tester.allExternalIssues()).isEmpty();
    assertThat(logTester.logs(Level.INFO)).containsExactly("Importing 1 Roslyn report");
  }

  @Test
  public void failWithDuplicateRuleKey() {
    tester.setActiveRules(new ActiveRulesBuilder()
      .addRule(createRule("csharpsquid", "[parameters_key]"))
      .addRule(createRule("roslyn.foo", "[parameters_key]"))
      .build());

    exception.expectMessage("Rule keys must be unique, but \"[parameters_key]\" is defined in both the \"roslyn.foo\" and \"csharpsquid\" rule repositories.");
    roslynDataImporter.importRoslynReports(Collections.singletonList(new RoslynReport(null, workDir.resolve("roslyn-report.json"))), tester, String::toString);
  }

  @Test
  public void internalIssuesFromExternalRepositoriesWithInvalidLocationShouldNotFail() throws IOException {
    final String repositoryName = "roslyn.stylecop.analyzers.cs";
    tester.setActiveRules(new ActiveRulesBuilder()
      .addRule(createRule(repositoryName, "SA1629"))
      .build());
    Path reportPath = updateCodeFilePathsInReport("roslyn-report-invalid-location.json", true);
    RoslynReport report = new RoslynReport(tester.project(), reportPath);
    roslynDataImporter.importRoslynReports(Collections.singletonList(report), tester, String::toString);

    assertThat(tester.allIssues()).hasSize(2);
    assertThat(tester.allAdHocRules()).isEmpty();
    assertThat(tester.allExternalIssues()).isEmpty();

    List<String> logs = logTester.logs();
    assertThat(logs.get(0)).isEqualTo("Importing 1 Roslyn report");
    assertThat(logs.get(1)).isEqualTo("Processing Roslyn report: " + workDir.resolve("roslyn-report-invalid-location.json"));
    assertThat(logs.get(2))
      .startsWith("Adding normal issue SA1629:")
      .endsWith("\\resources\\Program.cs");
    assertThat(logs.get(3))
      .startsWith("Precise issue location cannot be found! Location:")
      .endsWith("\\resources\\Program.cs, message=Documentation text should end with a period, startLine=13, startColumn=99, endLine=13, endColumn=100]");
    assertThat(logs.get(4))
      .startsWith("Adding normal issue SA1629:")
      .endsWith("\\resources\\Program.cs");
    assertThat(logs.get(5))
      .startsWith("Precise issue location cannot be found! Location:")
      .endsWith("\\resources\\Program.cs, message=Documentation text should end with a period, startLine=100, startColumn=0, endLine=100, endColumn=1]");
    assertThat(logs.get(6))
      .startsWith("Line issue location cannot be found! Location:")
      .endsWith("\\resources\\Program.cs, message=Documentation text should end with a period, startLine=100, startColumn=0, endLine=100, endColumn=1]");
  }

  @Test
  public void internalIssuesFromCSharpRepositoryWithInvalidLocationShouldFail() throws IOException {
    assertInvalidLocationFail("csharpsquid", roslynDataImporter);
  }

  @Test
  public void internalIssuesFromVBNetRepositoryWithInvalidLocationShouldFail() throws IOException {
    PluginMetadata vbPluginMetadata = mock(PluginMetadata.class);
    when(vbPluginMetadata.repositoryKey()).thenReturn("vbnet");

    RoslynDataImporter vbNetRoslynDataImporter = new RoslynDataImporter(vbPluginMetadata, mock(AbstractLanguageConfiguration.class));
    assertInvalidLocationFail("vbnet", vbNetRoslynDataImporter);
  }

  private void assertInvalidLocationFail(String repositoryName, RoslynDataImporter sut) throws IOException {
    tester.setActiveRules(new ActiveRulesBuilder()
      .addRule(createRule(repositoryName, "SA1629"))
      .build());

    Path reportPath = updateCodeFilePathsInReport("roslyn-report-invalid-location.json", true);
    RoslynReport report = new RoslynReport(tester.project(), reportPath);

    exception.expectMessage("99 is not a valid line offset for pointer. File Program.cs has 15 character(s) at line 13");
    sut.importRoslynReports(Collections.singletonList(report), tester, String::toString);
  }

  private void addActiveRules() {
    tester.setActiveRules(new ActiveRulesBuilder()
      .addRule(createRule("csharpsquid", "[parameters_key]"))
      .addRule(createRule("csharpsquid", "S1186"))
      .addRule(createRule("roslyn.foo", "custom-roslyn"))
      .build());
  }

  // Updates the file paths in the roslyn report to point to real cs file in the resources.
  private Path updateCodeFilePathsInReport(String reportFileName, boolean useUriPath) throws IOException {
    Path reportPath = workDir.resolve(reportFileName);
    Path csFile = Paths.get("src/test/resources/Program.cs").toAbsolutePath();

    // In SARIF Version 0.1 the path is an absolute path but in 1.0.0 it is serialized in URI format.
    String csFilePath;
    if (useUriPath) {
      csFilePath = csFile.toUri().toString();
    } else {
      csFilePath = csFile.toString();
    }

    String reportContent = new String(Files.readAllBytes(reportPath), StandardCharsets.UTF_8);
    reportContent = StringUtils.replace(reportContent, "Program.cs", StringEscapeUtils.escapeEcmaScript(csFilePath));
    Files.write(reportPath, reportContent.getBytes(StandardCharsets.UTF_8), StandardOpenOption.WRITE);
    return reportPath;
  }
  private NewActiveRule createRule(String repositoryKey, String ruleKey) {
    return new NewActiveRule.Builder()
      .setRuleKey(RuleKey.of(repositoryKey, ruleKey))
      .build();
  }

  private static PluginMetadata csPluginMetadata() {
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.repositoryKey()).thenReturn("csharpsquid");
    return metadata;
  }
}
