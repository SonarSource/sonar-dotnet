/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import java.io.IOException;
import java.net.URISyntaxException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringEscapeUtils;
import org.apache.commons.lang.StringUtils;
import org.assertj.core.groups.Tuple;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.internal.google.common.collect.ImmutableList;
import org.sonar.api.internal.google.common.collect.ImmutableMap;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.utils.Version;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class RoslynDataImporterTest {
  @Rule
  public ExpectedException exception = ExpectedException.none();
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private RoslynDataImporter roslynDataImporter = new RoslynDataImporter(mock(AbstractConfiguration.class));
  private SensorContextTester tester;
  private Path workDir;

  @Before
  public void setUp() throws IOException, URISyntaxException {
    workDir = temp.getRoot().toPath().resolve("reports");
    Path csFile = Paths.get("src/test/resources/Program.cs").toAbsolutePath();

    // copy test reports to work dir
    FileUtils.copyDirectory(new File("src/test/resources/RoslynDataImporterTest"), workDir.toFile());

    // replace file path in the roslyn report to point to real cs file in the resources
    Path roslynReportPath = workDir.resolve("roslyn-report.json");
    String report = new String(Files.readAllBytes(roslynReportPath), StandardCharsets.UTF_8);
    Files.write(roslynReportPath, StringUtils.replace(report, "Program.cs",
      StringEscapeUtils.escapeJavaScript(csFile.toString())).getBytes(StandardCharsets.UTF_8),
      StandardOpenOption.WRITE);

    roslynReportPath = Paths.get(this.getClass().getResource("/RoslynDataImporterTest").toURI());
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
    Map<String, List<RuleKey>> activeRules = createActiveRules();
    roslynDataImporter.importRoslynReports(Collections.singletonList(new RoslynReport(tester.module(), workDir.resolve("roslyn-report.json"))), tester, activeRules,
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
  }

  @Test
  public void dont_import_external_issues_before_7_4() {
    tester.setRuntime(SonarRuntimeImpl.forSonarQube(Version.create(7, 3), SonarQubeSide.SCANNER));

    Map<String, List<RuleKey>> activeRules = createActiveRules();
    roslynDataImporter.importRoslynReports(Collections.singletonList(new RoslynReport(tester.module(), workDir.resolve("roslyn-report.json"))), tester, activeRules,
      String::toString);

    assertThat(tester.allIssues()).hasSize(4);
    assertThat(tester.allAdHocRules()).isEmpty();
    assertThat(tester.allExternalIssues()).isEmpty();
  }

  @Test
  public void roslynEmptyReportShouldNotFail() {
    Map<String, List<RuleKey>> activeRules = createActiveRules();
    roslynDataImporter.importRoslynReports(Collections.singletonList(new RoslynReport(null, workDir.resolve("roslyn-report-empty.json"))), tester, activeRules, String::toString);
  }

  @Test
  public void failWithDuplicateRuleKey() {
    Map<String, List<RuleKey>> activeRules = ImmutableMap.of(
      "sonaranalyzer-cs", ImmutableList.of(RuleKey.of("csharpsquid", "[parameters_key]")),
      "foo", ImmutableList.of(RuleKey.of("roslyn.foo", "[parameters_key]")));

    exception.expectMessage("Rule keys must be unique, but \"[parameters_key]\" is defined in both the \"csharpsquid\" and \"roslyn.foo\" rule repositories.");
    roslynDataImporter.importRoslynReports(Collections.singletonList(new RoslynReport(null, workDir.resolve("roslyn-report.json"))), tester, activeRules, String::toString);
  }

  private static Map<String, List<RuleKey>> createActiveRules() {
    return ImmutableMap.of(
      "sonaranalyzer-cs", ImmutableList.of(RuleKey.of("csharpsquid", "S1186"), RuleKey.of("csharpsquid", "[parameters_key]")),
      "foo", ImmutableList.of(RuleKey.of("roslyn.foo", "custom-roslyn")));
  }
}
