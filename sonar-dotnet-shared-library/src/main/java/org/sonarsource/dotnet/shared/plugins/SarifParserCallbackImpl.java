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

import java.nio.file.Paths;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Locale;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.Set;
import java.util.function.Consumer;
import java.util.function.Supplier;
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

/**
 * This class is responsible to report to SonarQube each issue found in the Roslyn reports.
 */
public class SarifParserCallbackImpl implements SarifParserCallback {

  private static final Logger LOG = Loggers.get(SarifParserCallbackImpl.class);

  private static final String EXTERNAL_ENGINE_ID = "roslyn";
  private final SensorContext context;
  private final Map<String, String> repositoryKeyByRoslynRuleKey;
  private final Set<Issue> savedIssues = new HashSet<>();
  private final boolean ignoreThirdPartyIssues;
  private final Set<String> bugCategories;
  private final Set<String> codeSmellCategories;
  private final Set<String> vulnerabilityCategories;
  private final Map<String, String> defaultLevelByRuleId = new HashMap<>();
  private final Map<String, RuleType> ruleTypeByRuleId = new HashMap<>();

  public SarifParserCallbackImpl(SensorContext context, Map<String, String> repositoryKeyByRoslynRuleKey, boolean ignoreThirdPartyIssues, Set<String> bugCategories,
    Set<String> codeSmellCategories, Set<String> vulnerabilityCategories) {
    this.context = context;
    this.repositoryKeyByRoslynRuleKey = repositoryKeyByRoslynRuleKey;
    this.ignoreThirdPartyIssues = ignoreThirdPartyIssues;
    this.bugCategories = bugCategories;
    this.codeSmellCategories = codeSmellCategories;
    this.vulnerabilityCategories = vulnerabilityCategories;
  }

  @Override
  public void onProjectIssue(String ruleId, @Nullable String level, InputModule inputModule, String message) {
    // De-duplicate issues
    Issue issue = new Issue(ruleId, inputModule.toString(), true);
    if (!savedIssues.add(issue)) {
      return;
    }

    String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
    if (repositoryKey != null) {
      createProjectLevelIssue(ruleId, inputModule, message, repositoryKey);
    }
  }

  private void createProjectLevelIssue(String ruleId, InputModule inputModule, String message, String repositoryKey) {
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

    String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
    if (repositoryKey != null) {
      createFileLevelIssue(ruleId, message, repositoryKey, inputFile);
    } else if (shouldCreateExternalIssue(ruleId)) {
      createFileLevelExternalIssue(ruleId, level, message, inputFile);
    }
  }

  private void createFileLevelIssue(String ruleId, String message, String repositoryKey, InputFile inputFile) {
    NewIssue newIssue = context.newIssue();
    newIssue
      .forRule(RuleKey.of(repositoryKey, ruleId))
      .at(newIssue.newLocation()
        .on(inputFile)
        .message(message))
      .save();
  }

  private void createFileLevelExternalIssue(String ruleId, @Nullable String level, String message, InputFile inputFile) {
    NewExternalIssue newIssue = newExternalIssue(ruleId);
    newIssue.at(newIssue.newLocation()
      .on(inputFile)
      .message(message));
    setExternalIssueSeverityAndType(ruleId, level, newIssue);
    newIssue.save();
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
    } else if (shouldCreateExternalIssue(ruleId)) {
      createExternalIssue(inputFile, ruleId, level, primaryLocation, secondaryLocations);
    }
  }

  private void createExternalIssue(InputFile inputFile, String ruleId, @Nullable String level, Location primaryLocation, Collection<Location> secondaryLocations) {
    NewExternalIssue newIssue = newExternalIssue(ruleId);
    newIssue.at(createPrimaryLocation(inputFile, primaryLocation, newIssue::newLocation, true));
    setExternalIssueSeverityAndType(ruleId, level, newIssue);

    populateSecondaryLocations(secondaryLocations, newIssue::newLocation, newIssue::addLocation, true);

    newIssue.save();
  }

  private void setExternalIssueSeverityAndType(String ruleId, @Nullable String level, NewExternalIssue newIssue) {
    if (level != null) {
      newIssue.severity(mapSeverity(level));
    } else if (defaultLevelByRuleId.containsKey(ruleId)) {
      newIssue.severity(mapSeverity(defaultLevelByRuleId.get(ruleId)));
    } else {
      LOG.warn("Rule {} was not found in the SARIF report, assuming default severity", ruleId);
      newIssue.severity(Severity.MAJOR);
    }

    newIssue.type(Optional.ofNullable(ruleTypeByRuleId.get(ruleId)).orElse(RuleType.CODE_SMELL));
  }

  private NewExternalIssue newExternalIssue(String ruleId) {
    NewExternalIssue newIssue = context.newExternalIssue();
    newIssue
      .engineId(EXTERNAL_ENGINE_ID)
      .ruleId(ruleId);
    return newIssue;
  }

  private void createIssue(InputFile inputFile, String ruleId, Location primaryLocation, Collection<Location> secondaryLocations, String repositoryKey) {
    NewIssue newIssue = context.newIssue();
    newIssue
      .forRule(RuleKey.of(repositoryKey, ruleId))
      .at(createPrimaryLocation(inputFile, primaryLocation, newIssue::newLocation, false));

    populateSecondaryLocations(secondaryLocations, newIssue::newLocation, newIssue::addLocation, false);

    newIssue.save();
  }

  private static NewIssueLocation createPrimaryLocation(InputFile inputFile, Location primaryLocation, Supplier<NewIssueLocation> newIssueLocationSupplier,
    boolean isLocationResilient) {

    return createIssueLocation(inputFile, primaryLocation, newIssueLocationSupplier, isLocationResilient);
  }

  private void populateSecondaryLocations(Collection<Location> secondaryLocations, Supplier<NewIssueLocation> newIssueLocationSupplier,
    Consumer<NewIssueLocation> newIssueLocationConsumer, boolean isLocationResilient) {
    for (Location secondaryLocation : secondaryLocations) {
      InputFile inputFile = context.fileSystem().inputFile(context.fileSystem().predicates()
        .hasAbsolutePath(secondaryLocation.getAbsolutePath()));
      if (inputFile == null) {
        continue;
      }

      NewIssueLocation newIssueLocation = createIssueLocation(inputFile, secondaryLocation, newIssueLocationSupplier, isLocationResilient);
      newIssueLocationConsumer.accept(newIssueLocation);
    }
  }

  private static NewIssueLocation createIssueLocation(InputFile inputFile, Location location, Supplier<NewIssueLocation> newIssueLocationSupplier,
    boolean isLocationResilient) {

    NewIssueLocation newIssueLocation = newIssueLocationSupplier.get()
      .on(inputFile);

    try {
      // First, we try to report the issue with the precise location...
      newIssueLocation = newIssueLocation.at(inputFile.newRange(location.getStartLine(), location.getStartColumn(),
        location.getEndLine(), location.getEndColumn()));

    } catch (IllegalArgumentException ex1) {

      if (!isLocationResilient) {
        // Our rules should fail if they report on an invalid location
        throw ex1;
      }

      try {
        // Precise location failed, now try the line...
        newIssueLocation = newIssueLocation.at(inputFile.selectLine(location.getStartLine()));
      } catch (IllegalArgumentException ex2) {
        // Line location failed so let's report at file level (we are sure the file exists).
        // As the file was already registered previously, there is nothing to do here.
      }
    }

    String message = location.getMessage();
    if (message != null) {
      newIssueLocation.message(message);
    }

    return newIssueLocation;
  }

  @Override
  public void onRule(String ruleId, @Nullable String shortDescription, @Nullable String fullDescription, String defaultLevel, @Nullable String category) {
    if (repositoryKeyByRoslynRuleKey.containsKey(ruleId) || !shouldCreateExternalIssue(ruleId)) {
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
      .description(fullDescription)
      .type(ruleType)
      .save();
  }

  private boolean shouldCreateExternalIssue(String ruleId) {
    return !ignoreThirdPartyIssues && !ruleId.matches("^S\\d{3,4}$");
  }

  private RuleType mapRuleType(@Nullable String category, String defaultLevel) {
    if (category != null) {
      if (bugCategories.contains(category)) {
        return RuleType.BUG;
      }
      if (codeSmellCategories.contains(category)) {
        return RuleType.CODE_SMELL;
      }
      if (vulnerabilityCategories.contains(category)) {
        return RuleType.VULNERABILITY;
      }
    }
    return "Error".equalsIgnoreCase(defaultLevel) ? RuleType.BUG : RuleType.CODE_SMELL;
  }

  private static Severity mapSeverity(String defaultLevel) {
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
