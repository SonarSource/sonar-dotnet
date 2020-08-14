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

import java.net.URI;
import java.nio.charset.Charset;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.Set;
import java.util.stream.Collectors;
import javax.annotation.Nullable;
import org.sonar.api.batch.Phase;
import org.sonar.api.batch.Phase.Name;
import org.sonar.api.batch.bootstrap.ProjectBuilder;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.protobuf.FileMetadataImporter;

import static java.util.Optional.ofNullable;
import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.getAnalyzerWorkDirProperty;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.FILEMETADATA_OUTPUT_PROTOBUF_NAME;

/**
 * Since SonarQube 7.5, InputFileFilter can only access to global configuration. Use this ProjectBuilder to collect 
 * various data in protobuf files that are in every modules.
 */
@Phase(name = Name.POST)
public abstract class AbstractGlobalProtobufFileProcessor extends ProjectBuilder {

  private static final Logger LOG = Loggers.get(AbstractGlobalProtobufFileProcessor.class);

  private final String languageKey;

  private final Map<URI, Charset> roslynEncodingPerUri = new HashMap<>();
  private final Set<String> generatedFileUpperCaseUris = new HashSet<>();

  public AbstractGlobalProtobufFileProcessor(String languageKey) {
    this.languageKey = languageKey;
  }

  @Override
  public void build(Context context) {
    for (ProjectDefinition p : context.projectReactor().getProjects()) {
      for (Path reportPath : protobufReportPaths(p.properties())) {
        processMetadataReportIfPresent(reportPath);
      }
    }
  }

  private void processMetadataReportIfPresent(Path reportPath) {
    Path metadataReportProtobuf = reportPath.resolve(FILEMETADATA_OUTPUT_PROTOBUF_NAME);
    if (metadataReportProtobuf.toFile().exists()) {
      LOG.debug("Processing {}", metadataReportProtobuf);
      FileMetadataImporter fileMetadataImporter = new FileMetadataImporter();
      fileMetadataImporter.accept(metadataReportProtobuf);
      this.generatedFileUpperCaseUris.addAll(fileMetadataImporter.getGeneratedFileUris().stream().map(x -> x.toString().toUpperCase()).collect(Collectors.toList()));
      this.roslynEncodingPerUri.putAll(fileMetadataImporter.getEncodingPerUri());
    }
  }

  public Map<URI, Charset> getRoslynEncodingPerUri() {
    return Collections.unmodifiableMap(roslynEncodingPerUri);
  }

  public Set<String> getGeneratedFileUppercaseUris() {
    return Collections.unmodifiableSet(generatedFileUpperCaseUris);
  }

  private List<Path> protobufReportPaths(Map<String, String> moduleProps) {
    List<Path> analyzerWorkDirPaths = Arrays.stream(parseAsStringArray(moduleProps.get(getAnalyzerWorkDirProperty(languageKey))))
      .map(Paths::get)
      .collect(Collectors.toList());

    if (analyzerWorkDirPaths.isEmpty()) {
      // fallback to old property
      Optional<String> oldValue = ofNullable(moduleProps.get(AbstractProjectConfiguration.getOldAnalyzerWorkDirProperty(languageKey)));
      analyzerWorkDirPaths = oldValue
        .map(Paths::get)
        .map(Collections::singletonList)
        .orElse(Collections.emptyList());
    }

    return analyzerWorkDirPaths.stream()
      .map(x -> x.resolve(AbstractProjectConfiguration.getAnalyzerReportDir(languageKey)))
      .collect(Collectors.toList());
  }

  /**
   * A very simplified CSV parser, assuming there is no commas nor quotes in the protobuf paths
   */
  private String[] parseAsStringArray(@Nullable String value) {
    if (value == null) {
      return new String[0];
    }
    List<String> escapedValues = Arrays.asList(value.split(","));
    return escapedValues
      .stream()
      .map(String::trim)
      .map(s -> removeStart(s, "\""))
      .map(s -> removeEnd(s, "\""))
      .toArray(String[]::new);
  }

  private static String removeStart(String s, String start) {
    return s.startsWith(start) ? s.substring(start.length()) : s;
  }

  private static String removeEnd(String s, String end) {
    return s.endsWith(end) ? s.substring(0, s.length() - end.length()) : s;
  }
}
