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

import java.net.URI;
import java.nio.charset.Charset;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.TreeMap;
import java.util.TreeSet;
import javax.annotation.Nullable;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Phase;
import org.sonar.api.batch.Phase.Name;
import org.sonar.api.batch.bootstrap.ProjectBuilder;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.fs.InputFile;
import org.sonarsource.dotnet.shared.plugins.protobuf.FileMetadataImporter;

import static org.sonarsource.dotnet.shared.plugins.AbstractPropertyDefinitions.analyzerWorkDirProperty;
import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.FILEMETADATA_FILENAME;

/**
 * Since SonarQube 7.5, InputFileFilter can only access to global configuration. Use this ProjectBuilder to collect
 * various data in protobuf files that are in every modules.
 */
@Phase(name = Name.POST)
public class GlobalProtobufFileProcessor extends ProjectBuilder {

  private static final Logger LOG = LoggerFactory.getLogger(GlobalProtobufFileProcessor.class);

  private final PluginMetadata metadata;

  // We need case-insensitive string matching for Uri, because Uri for "file://D:/Something" is not equal to "file://d:/something"
  private final Map<String, Charset> roslynEncodingPerUri = new TreeMap<>(String.CASE_INSENSITIVE_ORDER);
  private final Set<String> generatedFileUris = new TreeSet<>(String.CASE_INSENSITIVE_ORDER);

  public GlobalProtobufFileProcessor(PluginMetadata metadata) {
    this.metadata = metadata;
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
    Path metadataReportProtobuf = reportPath.resolve(FILEMETADATA_FILENAME);
    if (metadataReportProtobuf.toFile().exists()) {
      LOG.debug("Processing {}", metadataReportProtobuf);
      FileMetadataImporter fileMetadataImporter = new FileMetadataImporter();
      fileMetadataImporter.accept(metadataReportProtobuf);
      this.generatedFileUris.addAll(fileMetadataImporter.getGeneratedFileUris().stream().map(URI::toString).toList());
      for (Map.Entry<URI, Charset> entry : fileMetadataImporter.getEncodingPerUri().entrySet()) {
        String key = entry.getKey().toString();
        if (!this.roslynEncodingPerUri.containsKey(key)) {
          this.roslynEncodingPerUri.put(key, entry.getValue());
        } else if (this.roslynEncodingPerUri.get(key) != entry.getValue()) {
          LOG.warn("Different encodings {} vs. {} were detected for single file {}. Case-Sensitive paths are not supported.",
            this.roslynEncodingPerUri.get(key), entry.getValue(), key);
        }
      }
    }
  }

  public Map<String, Charset> getRoslynEncodingPerUri() {
    return Collections.unmodifiableMap(roslynEncodingPerUri);
  }

  /**
   * Uri check is Case-Insensitive.
   */
  public boolean isGenerated(InputFile inputFile) {
    return generatedFileUris.contains(inputFile.uri().toString());
  }

  private List<Path> protobufReportPaths(Map<String, String> moduleProps) {
    return Arrays.stream(parseAsStringArray(moduleProps.get(analyzerWorkDirProperty(metadata.languageKey()))))
      .map(x -> Paths.get(x).resolve(ModuleConfiguration.getAnalyzerReportDir(metadata.languageKey())))
      .toList();
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
