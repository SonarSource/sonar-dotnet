/*
 * SonarSource :: .NET :: Shared library
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
package org.sonarsource.dotnet.shared.plugins;

import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.CPDTOKENS_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.HIGHLIGHT_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.ISSUES_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.METRICS_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.SYMBOLREFS_OUTPUT_PROTOBUF_NAME;

import java.nio.file.Path;
import java.util.Collection;
import java.util.Map;

import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.issue.NewIssue;
import org.sonar.api.batch.sensor.issue.NewIssueLocation;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.utils.command.StreamConsumer;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters;
import org.sonarsource.dotnet.shared.plugins.protobuf.RawProtobufImporter;
import org.sonarsource.dotnet.shared.sarif.Location;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;

public abstract class AbstractSensor {
  private static final Logger LOG = Loggers.get(AbstractSensor.class);
  private final FileLinesContextFactory fileLinesContextFactory;
  private final NoSonarFilter noSonarFilter;
  private final AbstractConfiguration config;
  private final String repositoryKey;

  protected AbstractSensor(FileLinesContextFactory fileLinesContextFactory, NoSonarFilter noSonarFilter, AbstractConfiguration config,
    String repositoryKey) {
    this.fileLinesContextFactory = fileLinesContextFactory;
    this.noSonarFilter = noSonarFilter;
    this.config = config;
    this.repositoryKey = repositoryKey;
  }

  public void importResults(SensorContext context, Path protobufReportsDirectory, boolean importIssues) {
    if (config.areProtobufReportsPresent()) {
      // Note: the no-sonar "measure" must be imported before issues, otherwise the affected issues won't get excluded!
      parseProtobuf(ProtobufImporters.metricsImporter(context, fileLinesContextFactory, noSonarFilter), protobufReportsDirectory, METRICS_OUTPUT_PROTOBUF_NAME);
      if (importIssues) {
        parseProtobuf(ProtobufImporters.issuesImporter(context, repositoryKey), protobufReportsDirectory, ISSUES_OUTPUT_PROTOBUF_NAME);
      }
      parseProtobuf(ProtobufImporters.highlightImporter(context), protobufReportsDirectory, HIGHLIGHT_OUTPUT_PROTOBUF_NAME);
      parseProtobuf(ProtobufImporters.symbolRefsImporter(context), protobufReportsDirectory, SYMBOLREFS_OUTPUT_PROTOBUF_NAME);
      parseProtobuf(ProtobufImporters.cpdTokensImporter(context), protobufReportsDirectory, CPDTOKENS_OUTPUT_PROTOBUF_NAME);
    }
  }

  private static void parseProtobuf(RawProtobufImporter<?> importer, Path workDirectory, String filename) {
    Path protobuf = workDirectory.resolve(filename);
    if (protobuf.toFile().exists()) {
      importer.accept(protobuf);
    } else {
      LOG.warn("Protobuf file not found: " + protobuf);
    }
  }

  protected static final class SarifParserCallbackImplementation implements SarifParserCallback {
    private final SensorContext context;
    private final Map<String, String> repositoryKeyByRoslynRuleKey;

    public SarifParserCallbackImplementation(SensorContext context, Map<String, String> repositoryKeyByRoslynRuleKey) {
      this.context = context;
      this.repositoryKeyByRoslynRuleKey = repositoryKeyByRoslynRuleKey;
    }

    @Override
    public void onProjectIssue(String ruleId, String message) {
      String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
      if (repositoryKey == null) {
        return;
      }
      NewIssue newIssue = context.newIssue();
      newIssue
        .forRule(RuleKey.of(repositoryKey, ruleId))
        .at(newIssue.newLocation()
          .on(context.module())
          .message(message))
        .save();
    }

    @Override
    public void onFileIssue(String ruleId, String absolutePath, String message) {
      String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
      if (repositoryKey == null) {
        return;
      }

      InputFile inputFile = context.fileSystem().inputFile(context.fileSystem().predicates()
        .hasAbsolutePath(absolutePath));
      if (inputFile == null) {
        return;
      }

      NewIssue newIssue = context.newIssue();
      newIssue
        .forRule(RuleKey.of(repositoryKey, ruleId))
        .at(newIssue.newLocation()
          .on(inputFile)
          .message(message))
        .save();
    }

    @Override
    public void onIssue(String ruleId, Location primaryLocation, Collection<Location> secondaryLocations) {
      String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
      if (repositoryKey == null) {
        return;
      }

      InputFile inputFile = context.fileSystem().inputFile(context.fileSystem().predicates()
        .hasAbsolutePath(primaryLocation.getAbsolutePath()));
      if (inputFile == null) {
        return;
      }

      NewIssue newIssue = context.newIssue();
      newIssue
        .forRule(RuleKey.of(repositoryKey, ruleId))
        .at(newIssue.newLocation()
          .on(inputFile)
          .at(inputFile.newRange(primaryLocation.getStartLine(), primaryLocation.getStartColumn(),
            primaryLocation.getEndLine(), primaryLocation.getEndColumn()))
          .message(primaryLocation.getMessage()));

      for (Location secondaryLocation : secondaryLocations) {
        if (!inputFile.absolutePath().equalsIgnoreCase(secondaryLocation.getAbsolutePath())) {
          inputFile = context.fileSystem().inputFile(context.fileSystem().predicates()
            .hasAbsolutePath(secondaryLocation.getAbsolutePath()));
          if (inputFile == null) {
            return;
          }
        }

        NewIssueLocation newIssueLocation = newIssue.newLocation()
          .on(inputFile)
          .at(inputFile.newRange(secondaryLocation.getStartLine(), secondaryLocation.getStartColumn(),
            secondaryLocation.getEndLine(), secondaryLocation.getEndColumn()));

        String secondaryLocationMessage = secondaryLocation.getMessage();
        if (secondaryLocationMessage != null) {
          newIssueLocation.message(secondaryLocationMessage);
        }

        newIssue.addLocation(newIssueLocation);
      }

      newIssue.save();
    }
  }

  public static class LogInfoStreamConsumer implements StreamConsumer {
    @Override
    public void consumeLine(String line) {
      LOG.info(line);
    }
  }

  public static class LogErrorStreamConsumer implements StreamConsumer {
    @Override
    public void consumeLine(String line) {
      LOG.error(line);
    }
  }
}
