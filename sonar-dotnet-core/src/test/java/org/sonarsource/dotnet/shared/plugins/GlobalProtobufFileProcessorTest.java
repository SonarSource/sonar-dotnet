/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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
import java.io.OutputStream;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;
import java.util.Map;
import javax.annotation.Nullable;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.slf4j.event.Level;
import org.sonar.api.batch.bootstrap.ProjectBuilder;
import org.sonar.api.batch.bootstrap.ProjectBuilder.Context;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.bootstrap.ProjectReactor;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.config.Configuration;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.FileMetadataInfo;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.entry;
import static org.assertj.core.api.Assertions.fail;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class GlobalProtobufFileProcessorTest {

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logs = new LogTester();

  private Context context;
  private GlobalProtobufFileProcessor underTest;

  private ProjectDefinition project1;
  private ProjectDefinition project2;

  @Before
  public void prepare() {
    ProjectDefinition rootProject = ProjectDefinition.create();
    project1 = ProjectDefinition.create();
    project2 = ProjectDefinition.create();
    rootProject.addSubProject(project1);
    rootProject.addSubProject(project2);
    context = new ProjectBuilder.Context() {
      @Override
      public ProjectReactor projectReactor() {
        return new ProjectReactor(rootProject);
      }

      @Override
      public Configuration config() {
        return null;
      }
    };
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.languageKey()).thenReturn("foo");
    underTest = new GlobalProtobufFileProcessor(metadata);
  }

  @Test
  public void do_nothing_if_no_properties() {
    underTest.build(context);
    assertThat(underTest.getRoslynEncodingPerUri()).isEmpty();
  }

  @Test
  public void process_generated() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPaths", mockGenerated("generated11") + "," + mockGenerated("generated12"));
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockGenerated("generated2"));

    underTest.build(context);
    assertThat(underTest.isGenerated(mockInputFile("generated11"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("generated12"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("generated2"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("generated3"))).isFalse();
    Map.Entry<String, Charset> expected = entry(toUriString("generated2"), null);
    assertThat(underTest.getRoslynEncodingPerUri()).contains(expected);
  }

  @Test
  public void process_generated_escaped_csv() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPaths",
      "\"" + mockGenerated("generated11") + "\",\"" + mockGenerated("generated12") + "\"");
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockGenerated("generated2"));

    underTest.build(context);
    assertThat(underTest.isGenerated(mockInputFile("generated11"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("generated12"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("generated2"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("generated3"))).isFalse();
    Map.Entry<String, Charset> expected = entry(toUriString("generated2"), null);
    assertThat(underTest.getRoslynEncodingPerUri()).contains(expected);
  }

  @Test
  public void process_generated_is_not_case_sensitive() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPaths", mockGenerated("generated"));
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockGenerated("GENERATED"));

    underTest.build(context);
    assertThat(underTest.getRoslynEncodingPerUri()).hasSize(1);
    assertThat(underTest.isGenerated(mockInputFile("GeNeRaTeD"))).isTrue();
  }

  @Test
  public void process_encoding() throws IOException {
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockEncoding("UTF-8", "encodingutf8"));

    underTest.build(context);
    assertThat(underTest.getRoslynEncodingPerUri()).containsOnly(entry(toUriString("encodingutf8"), StandardCharsets.UTF_8));
    assertThat(underTest.isGenerated(mockInputFile("encodingutf8"))).isFalse();
  }

  @Test
  public void is_not_case_sensitive() throws IOException {
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockGenerated("UPPER/lower/MiXeD"));

    underTest.build(context);
    assertThat(underTest.getRoslynEncodingPerUri())
      .hasSize(1)
      .containsKey(toUriString("UPPER/lower/MiXeD"))
      .containsKey(toUriString("UPPER/LOWER/MIXED"))
      .containsKey(toUriString("upper/lower/mixed"))
      .containsKey(toUriString("upper/LOWER/mIxEd"));

    assertThat(underTest.isGenerated(mockInputFile("UPPER/lower/MiXeD"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("UPPER/LOWER/MIXED"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("upper/lower/mixed"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("upper/LOWER/mIxEd"))).isTrue();
  }

  @Test
  public void warn_about_casing_colission() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPaths", mockEncoding("UTF-8", "collision") + "," + mockEncoding("UTF-16", "COLLISION"));
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockEncoding("ASCII", "CoLLiSioN"));

    underTest.build(context);
    assertThat(underTest.getRoslynEncodingPerUri()).hasSize(1).containsKey(toUriString("collision"));
    assertThat(logs.logs(Level.WARN)).contains(
      String.format("Different encodings UTF-8 vs. UTF-16 were detected for single file %s. Case-Sensitive paths are not supported.", toUriString("COLLISION")));
    assertThat(logs.logs(Level.WARN)).contains(
      String.format("Different encodings UTF-8 vs. US-ASCII were detected for single file %s. Case-Sensitive paths are not supported.", toUriString("CoLLiSioN")));
  }

  @Test
  public void do_not_warn_about_casing_colission_for_same_encoding() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPaths", mockEncoding("UTF-8", "collision") + "," + mockEncoding("UTF-8", "COLLISION"));

    underTest.build(context);
    assertThat(underTest.getRoslynEncodingPerUri()).hasSize(1).containsKey(toUriString("collision"));
    assertThat(logs.logs(Level.WARN).stream().filter(x -> x.contains("COLLISION"))).isEmpty();
  }

  @Test
  public void process_encoding_preserve_null_values() throws IOException {
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockEncoding(null, "encodingnull").toString());

    underTest.build(context);
    assertThat(underTest.getRoslynEncodingPerUri()).containsOnly(entry(toUriString("encodingnull"), null));
    assertThat(underTest.isGenerated(mockInputFile("encodingnull"))).isFalse();
  }

  @Test
  public void ignore_missing_files() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPaths", "notExisiting.pb");
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockGenerated("generated2"));

    underTest.build(context);
    assertThat(underTest.isGenerated(mockInputFile("generated2"))).isTrue();
    assertThat(underTest.isGenerated(mockInputFile("generated3"))).isFalse();
    Map.Entry<String, Charset> expected = entry(toUriString("generated2"), null);
    assertThat(underTest.getRoslynEncodingPerUri()).containsExactly(expected);
  }

  private String mockGenerated(String path) throws IOException {
    File reportPath = temp.newFolder();
    Path analyzerPath = reportPath.toPath().resolve("output-foo");
    Files.createDirectories(analyzerPath);
    try (OutputStream fos = Files.newOutputStream(analyzerPath.resolve("file-metadata.pb"), StandardOpenOption.CREATE)) {
      try {
        FileMetadataInfo.newBuilder().setFilePath(path).setIsGenerated(true).build().writeDelimitedTo(fos);
      } catch (IOException e) {
        fail(e.getMessage(), e);
      }
    }
    return reportPath.toString();
  }

  private String mockEncoding(@Nullable String encoding, String path) throws IOException {
    File reportPath = temp.newFolder();
    Path analyzerPath = reportPath.toPath().resolve("output-foo");
    Files.createDirectories(analyzerPath);
    try (OutputStream fos = Files.newOutputStream(analyzerPath.resolve("file-metadata.pb"), StandardOpenOption.CREATE)) {
      try {
        FileMetadataInfo.Builder builder = FileMetadataInfo.newBuilder().setFilePath(path);
        if (encoding != null) {
          builder.setEncoding(encoding);
        }
        builder.build().writeDelimitedTo(fos);
      } catch (IOException e) {
        fail(e.getMessage(), e);
      }
    }
    return reportPath.toString();
  }

  private String toUriString(String path) {
    return Paths.get(path).toUri().toString();
  }

  private InputFile mockInputFile(String path) {
    InputFile inputFile = mock(InputFile.class);
    when(inputFile.uri()).thenReturn(Paths.get(path).toUri());
    return inputFile;
  }
}
