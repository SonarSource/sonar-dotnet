/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2018 SonarSource SA
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

import java.nio.file.Paths;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Locale;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.Set;
import javax.annotation.Nullable;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputModule;
import org.sonar.api.batch.rule.Severity;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.issue.NewExternalIssue;
import org.sonar.api.batch.sensor.issue.NewIssue;
import org.sonar.api.batch.sensor.issue.NewIssueLocation;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.rules.RuleType;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.sarif.Location;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;

public class SarifParserCallbackImpl implements SarifParserCallback {

  private static final Logger LOG = Loggers.get(SarifParserCallbackImpl.class);

  private static final String EXTERNAL_ENGINE_ID = "roslyn";
  private final SensorContext context;
  private final Map<String, String> repositoryKeyByRoslynRuleKey;
  private final Set<Issue> savedIssues = new HashSet<>();
  private final boolean importAllIssues;
  private final Set<String> bugCategories;
  private final Set<String> codeSmellCategories;
  private final Set<String> vulnerabilityCategories;
  private final Map<String, String> defaultLevelByRuleId = new HashMap<>();
  private final Map<String, RuleType> ruleTypeByRuleId = new HashMap<>();

  public SarifParserCallbackImpl(SensorContext context, Map<String, String> repositoryKeyByRoslynRuleKey, boolean importAllIssues, Set<String> bugCategories,
    Set<String> codeSmellCategories, Set<String> vulnerabilityCategories) {
    this.context = context;
    this.repositoryKeyByRoslynRuleKey = repositoryKeyByRoslynRuleKey;
    this.importAllIssues = importAllIssues;
    this.bugCategories = bugCategories;
    this.codeSmellCategories = codeSmellCategories;
    this.vulnerabilityCategories = vulnerabilityCategories;
  }

  @Override
  public void onProjectIssue(String ruleId, @Nullable String level, InputModule inputModule, String message) {
    String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
    if (repositoryKey == null) {
      return;
    }

    // De-duplicate issues
    Issue issue = new Issue(ruleId, inputModule.toString(), true);
    if (!savedIssues.add(issue)) {
      return;
    }

    NewIssue newIssue = context.newIssue();
    newIssue
      .forRule(RuleKey.of(repositoryKey, ruleId))
      .at(newIssue.newLocation()
        .on(inputModule)
        .message(message))
      .save();
  }

  @Override
  public void onFileIssue(String ruleId, @Nullable String level, String absolutePath, String message) {
    String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
    if (repositoryKey == null) {
      return;
    }

    // De-duplicate issues
    Issue issue = new Issue(ruleId, absolutePath, false);
    if (!savedIssues.add(issue)) {
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
  public void onIssue(String ruleId, @Nullable String level, Location primaryLocation, Collection<Location> secondaryLocations) {
    // De-duplicate issues
    Issue issue = new Issue(ruleId, primaryLocation);
    if (!savedIssues.add(issue)) {
      return;
    }

    InputFile inputFile = context.fileSystem().inputFile(context.fileSystem().predicates()
      .hasAbsolutePath(primaryLocation.getAbsolutePath()));
    if (inputFile == null) {
      return;
    }

    String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
    if (repositoryKey != null) {
      createIssue(inputFile, ruleId, primaryLocation, secondaryLocations, repositoryKey);
    } else if (importAllIssues) {
      createExternalIssue(inputFile, ruleId, level, primaryLocation, secondaryLocations);
    }
  }

  private void createExternalIssue(InputFile inputFile, String ruleId, @Nullable String level, Location primaryLocation, Collection<Location> secondaryLocations) {
    NewExternalIssue newIssue = context.newExternalIssue();
    newIssue
      .engineId(EXTERNAL_ENGINE_ID)
      .ruleId(ruleId)
      .at(newIssue.newLocation()
        .on(inputFile)
        .at(inputFile.newRange(primaryLocation.getStartLine(), primaryLocation.getStartColumn(),
          primaryLocation.getEndLine(), primaryLocation.getEndColumn()))
        .message(primaryLocation.getMessage()));
    if (level != null) {
      newIssue.severity(mapSeverity(level));
    } else if (defaultLevelByRuleId.containsKey(ruleId)) {
      newIssue.severity(mapSeverity(defaultLevelByRuleId.get(ruleId)));
    } else {
      LOG.warn("Rule {} was not found in the SARIF report, assuming default severity", ruleId);
      newIssue.severity(Severity.MAJOR);
    }

    newIssue.type(Optional.ofNullable(ruleTypeByRuleId.get(ruleId)).orElse(RuleType.CODE_SMELL));

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

  private void createIssue(InputFile inputFile, String ruleId, Location primaryLocation, Collection<Location> secondaryLocations, String repositoryKey) {
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

  @Override
  public void onRule(String ruleId, @Nullable String shortDescription, @Nullable String fullDescription, String defaultLevel, @Nullable String category) {
    if (!importAllIssues || repositoryKeyByRoslynRuleKey.containsKey(ruleId)) {
      // This is not an external rule
      return;
    }
    defaultLevelByRuleId.put(ruleId, defaultLevel);
    RuleType ruleType = mapRuleType(category, defaultLevel);
    ruleTypeByRuleId.put(ruleId, ruleType);
    context.newAdHocRule()
      .engineId(EXTERNAL_ENGINE_ID)
      .ruleId(ruleId)
      .severity(mapSeverity(defaultLevel))
      .name(shortDescription != null ? shortDescription : ruleId)
      .description(fullDescription != null ? fullDescription : null)
      .type(ruleType)
      .save();
  }

  private RuleType mapRuleType(String category, String defaultLevel) {
    if (bugCategories.contains(category)) {
      return RuleType.BUG;
    }
    if (codeSmellCategories.contains(category)) {
      return RuleType.CODE_SMELL;
    }
    if (vulnerabilityCategories.contains(category)) {
      return RuleType.VULNERABILITY;
    }
    return "Error".equalsIgnoreCase(defaultLevel) ? RuleType.BUG : RuleType.CODE_SMELL;
  }

  private Severity mapSeverity(String defaultLevel) {
    switch (defaultLevel.toLowerCase(Locale.ENGLISH)) {
      case "error":
        return Severity.CRITICAL;
      case "warning":
        return Severity.MAJOR;
      default:
        return Severity.INFO;
    }
  }

  private static class Issue {
    private String ruleId;
    private String moduleId;
    private String absolutePath;
    private int startLine;
    private int startColumn;
    private int endLine;
    private int endColumn;

    Issue(String ruleId, String moduleOrPath, boolean isModuleId) {
      this.ruleId = ruleId;
      if (isModuleId) {
        this.moduleId = moduleOrPath;
        this.absolutePath = "";
      } else {
        this.absolutePath = moduleOrPath;
      }
    }

    Issue(String ruleId, Location location) {
      this.ruleId = ruleId;
      this.absolutePath = location.getAbsolutePath();
      this.startLine = location.getStartLine();
      this.startColumn = location.getStartColumn();
      this.endLine = location.getEndLine();
      this.endColumn = location.getEndColumn();
    }

    @Override
    public int hashCode() {
      return Objects.hash(ruleId, moduleId, absolutePath, startLine, startColumn, endLine, endColumn);
    }

    @Override
    public boolean equals(Object other) {
      if (!(other instanceof Issue)) {
        return false;
      }
      Issue o = (Issue) other;

      // note that comparison of absolute path is done using Path.
      return Objects.equals(ruleId, o.ruleId)
        && Objects.equals(moduleId, o.moduleId)
        && Objects.equals(startLine, o.startLine)
        && Objects.equals(startColumn, o.startColumn)
        && Objects.equals(endLine, o.endLine)
        && Objects.equals(endColumn, o.endColumn)
        && Paths.get(absolutePath).equals(Paths.get(o.absolutePath));
    }
  }
}
