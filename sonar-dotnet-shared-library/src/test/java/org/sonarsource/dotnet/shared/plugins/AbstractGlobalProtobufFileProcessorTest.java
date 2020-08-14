/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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
import java.io.OutputStream;
import java.net.URI;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;
import java.util.HashMap;
import java.util.Map;
import java.util.stream.Stream;
import javax.annotation.Nullable;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.bootstrap.ProjectBuilder;
import org.sonar.api.batch.bootstrap.ProjectBuilder.Context;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.bootstrap.ProjectReactor;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.FileMetadataInfo;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.entry;
import static org.assertj.core.api.Assertions.fail;

public class AbstractGlobalProtobufFileProcessorTest {

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private Context context;
  private AbstractGlobalProtobufFileProcessor underTest;

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
    };
    underTest = new AbstractGlobalProtobufFileProcessor("foo") {

    };
  }

  @Test
  public void do_nothing_if_no_properties() {
    underTest.build(context);
    assertThat(underTest.getGeneratedFileUppercaseUris()).isEmpty();
    assertThat(underTest.getRoslynEncodingPerUri()).isEmpty();
  }

  @Test
  public void process_generated_old_prop() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPath", mockMetadataProtoReport("generated1").toString());
    project2.setProperty("sonar.foo.analyzer.projectOutPath", mockMetadataProtoReport("generated2").toString());

    underTest.build(context);
    assertThat(underTest.getGeneratedFileUppercaseUris()).containsExactlyInAnyOrder(toUpperCaseUriString("GENERATED1"), toUpperCaseUriString("GENERATED2"));
    Map.Entry<URI, Charset> expected1 = new HashMap.SimpleEntry<>(toUri("generated1"), null);
    Map.Entry<URI, Charset> expected2 = new HashMap.SimpleEntry<>(toUri("generated2"), null);
    assertThat(underTest.getRoslynEncodingPerUri()).containsExactly(expected2, expected1);
  }

  @Test
  public void process_generated_new_prop() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPaths", mockMetadataProtoReport("generated11").toString() + "," + mockMetadataProtoReport("generated12").toString());
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockMetadataProtoReport("generated2").toString());

    underTest.build(context);
    assertThat(underTest.getGeneratedFileUppercaseUris()).containsExactlyInAnyOrder(toUpperCaseUriString("GENERATED11"), toUpperCaseUriString("GENERATED12"),
      toUpperCaseUriString("GENERATED2"));
    Map.Entry<URI, Charset> expected = new HashMap.SimpleEntry<>(Paths.get("generated2").toUri(), null);
    assertThat(underTest.getRoslynEncodingPerUri()).contains(expected);
  }

  @Test
  public void process_generated_new_prop_escaped_csv() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPaths",
      "\"" + mockMetadataProtoReport("generated11").toString() + "\",\"" + mockMetadataProtoReport("generated12").toString() + "\"");
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockMetadataProtoReport("generated2").toString());

    underTest.build(context);
    assertThat(underTest.getGeneratedFileUppercaseUris()).containsExactlyInAnyOrder(toUpperCaseUriString("GENERATED11"), toUpperCaseUriString("GENERATED12"),
      toUpperCaseUriString("GENERATED2"));
    Map.Entry<URI, Charset> expected = new HashMap.SimpleEntry<>(toUri("generated2"), null);
    assertThat(underTest.getRoslynEncodingPerUri()).contains(expected);
  }

  @Test
  public void process_encoding_new_prop() throws IOException {
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockEncodingProtoReport("UTF-8", "encodingutf8").toString());

    underTest.build(context);
    assertThat(underTest.getRoslynEncodingPerUri()).containsOnly(entry(toUri("encodingutf8"), StandardCharsets.UTF_8));
    assertThat(underTest.getGeneratedFileUppercaseUris()).isEmpty();
  }

  @Test
  public void process_encoding_preserve_null_values() throws IOException {
    project2.setProperty("sonar.foo.analyzer.projectOutPaths", mockEncodingProtoReport(null, "encodingnull").toString());

    underTest.build(context);
    assertThat(underTest.getRoslynEncodingPerUri()).containsOnly(entry(toUri("encodingnull"), null));
    assertThat(underTest.getGeneratedFileUppercaseUris()).isEmpty();
  }

  @Test
  public void ignore_missing_files() throws IOException {
    project1.setProperty("sonar.foo.analyzer.projectOutPath", "notExisiting.pb");
    project2.setProperty("sonar.foo.analyzer.projectOutPath", mockMetadataProtoReport("generated2").toString());

    underTest.build(context);
    assertThat(underTest.getGeneratedFileUppercaseUris()).containsExactlyInAnyOrder(toUpperCaseUriString("GENERATED2"));
    Map.Entry<URI, Charset> expected = new HashMap.SimpleEntry<>(toUri("generated2"), null);
    assertThat(underTest.getRoslynEncodingPerUri()).containsExactly(expected);
  }

  private File mockMetadataProtoReport(String... paths) throws IOException {
    File reportPath = temp.newFolder();
    Path analyzerPath = reportPath.toPath().resolve("output-foo");
    Files.createDirectories(analyzerPath);
    try (OutputStream fos = Files.newOutputStream(analyzerPath.resolve("file-metadata.pb"), StandardOpenOption.CREATE)) {
      Stream.of(paths).forEach(p -> {
        try {
          FileMetadataInfo.newBuilder().setFilePath(p).setIsGenerated(true).build().writeDelimitedTo(fos);
        } catch (IOException e) {
          fail(e.getMessage(), e);
        }
      });
    }
    return reportPath;
  }

  private File mockEncodingProtoReport(@Nullable String encoding, String... paths) throws IOException {
    File reportPath = temp.newFolder();
    Path analyzerPath = reportPath.toPath().resolve("output-foo");
    Files.createDirectories(analyzerPath);
    try (OutputStream fos = Files.newOutputStream(analyzerPath.resolve("file-metadata.pb"), StandardOpenOption.CREATE)) {
      Stream.of(paths).forEach(p -> {
        try {
          FileMetadataInfo.Builder builder = FileMetadataInfo.newBuilder().setFilePath(p);
          if (encoding != null) {
            builder.setEncoding(encoding);
          }
          builder.build().writeDelimitedTo(fos);
        } catch (IOException e) {
          fail(e.getMessage(), e);
        }
      });
    }
    return reportPath;
  }

  private String toUpperCaseUriString(String path) {
    return Paths.get(path).toUri().toString().toUpperCase();
  }

  // FIXME: Remove after encoding update
  private URI toUri(String path) {
    return Paths.get(path).toUri();
  }
}
