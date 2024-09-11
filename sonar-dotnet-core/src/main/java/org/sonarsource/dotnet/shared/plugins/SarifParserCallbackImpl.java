/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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

import java.util.Arrays;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Objects;
import java.util.Optional;
import java.util.Set;
import java.util.function.Consumer;
import java.util.function.Supplier;
import javax.annotation.Nullable;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.rule.Severity;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.issue.NewExternalIssue;
import org.sonar.api.batch.sensor.issue.NewIssue;
import org.sonar.api.batch.sensor.issue.NewIssueLocation;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.rules.RuleType;
import org.sonar.api.scanner.fs.InputProject;
import org.sonarsource.dotnet.shared.sarif.Location;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;

/**
 * This class is responsible to report to SonarQube each issue found in the Roslyn reports.
 */
public class SarifParserCallbackImpl implements SarifParserCallback {

  private static final Logger LOG = LoggerFactory.getLogger(SarifParserCallbackImpl.class);

  private static final String EXTERNAL_ENGINE_ID = "roslyn";
  private static final List<String> OWN_REPOSITORIES = Arrays.asList("csharpsquid", "vbnet");
  private final SensorContext context;
  private final Map<String, String> repositoryKeyByRoslynRuleKey;
  private final Set<Issue> savedIssues = new HashSet<>();
  private final Set<ProjectIssue> projectIssues = new HashSet<>();
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
  public void onProjectIssue(String ruleId, @Nullable String level, InputProject inputProject, String message) {
    // Remove duplicate issues.
    // We do not have enough information (other than the message) to distinguish between different dotnet projects.
    // Due to this, project level issues should always mention the assembly in their message (see: S3990, S3992, or S3904)
    if (!projectIssues.add(new ProjectIssue(ruleId, message))) {
      return;
    }

    String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
    if (repositoryKey != null) {
      createProjectLevelIssue(ruleId, inputProject, message, repositoryKey);
    }
  }

  private void createProjectLevelIssue(String ruleId, InputProject inputProject, String message, String repositoryKey) {
    logIssue("project level", ruleId, inputProject.toString());
    NewIssue newIssue = context.newIssue();
    newIssue
      .forRule(RuleKey.of(repositoryKey, ruleId))
      .at(newIssue.newLocation()
        .on(inputProject)
        .message(message))
      .save();
  }

  @Override
  public void onFileIssue(String ruleId, @Nullable String level, String absolutePath, Collection<Location> secondaryLocations, String message) {
    // De-duplicate issues
    Issue issue = new Issue(ruleId, absolutePath);
    if (!savedIssues.add(issue)) {
      return;
    }

    InputFile inputFile = context.fileSystem().inputFile(context.fileSystem().predicates()
      .hasAbsolutePath(absolutePath));
    if (inputFile == null) {
      logMissingInputFile(ruleId, absolutePath);
      return;
    }

    String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
    if (repositoryKey != null) {
      createFileLevelIssue(ruleId, message, repositoryKey, inputFile, secondaryLocations);
    } else if (shouldCreateExternalIssue(ruleId)) {
      createFileLevelExternalIssue(ruleId, level, message, inputFile, secondaryLocations);
    }
  }

  private void createFileLevelIssue(String ruleId, String message, String repositoryKey, InputFile inputFile, Collection<Location> secondaryLocations) {
    logIssue("file level", ruleId, inputFile.toString());
    NewIssue newIssue = context.newIssue();
    newIssue
      .forRule(RuleKey.of(repositoryKey, ruleId))
      .at(newIssue.newLocation()
        .on(inputFile)
        .message(message));
    populateSecondaryLocations(secondaryLocations, newIssue::newLocation, newIssue::addLocation, isSonarSourceRepository(repositoryKey));
    newIssue.save();
  }

  private void createFileLevelExternalIssue(String ruleId, @Nullable String level, String message, InputFile inputFile, Collection<Location> secondaryLocations) {
    logIssue("file level external", ruleId, inputFile.toString());
    NewExternalIssue newIssue = newExternalIssue(ruleId);
    newIssue.at(newIssue.newLocation()
      .on(inputFile)
      .message(message));
    setExternalIssueSeverityAndType(ruleId, level, newIssue);
    populateSecondaryLocations(secondaryLocations, newIssue::newLocation, newIssue::addLocation, true);
    newIssue.save();
  }

  @Override
  public void onIssue(String ruleId, @Nullable String level, Location primaryLocation, Collection<Location> secondaryLocations, boolean withExecutionFlow) {
    // De-duplicate issues
    Issue issue = new Issue(ruleId, primaryLocation);
    if (!savedIssues.add(issue)) {
      return;
    }

    InputFile inputFile = context.fileSystem().inputFile(context.fileSystem().predicates()
      .hasAbsolutePath(primaryLocation.getAbsolutePath()));
    if (inputFile == null) {
      logMissingInputFile(ruleId, primaryLocation.getAbsolutePath());
      return;
    }

    String repositoryKey = repositoryKeyByRoslynRuleKey.get(ruleId);
    if (repositoryKey != null) {
      createIssue(inputFile, ruleId, primaryLocation, secondaryLocations, repositoryKey, withExecutionFlow);
    } else if (shouldCreateExternalIssue(ruleId)) {
      createExternalIssue(inputFile, ruleId, level, primaryLocation, secondaryLocations);
    }
  }

  private void createExternalIssue(InputFile inputFile, String ruleId, @Nullable String level, Location primaryLocation, Collection<Location> secondaryLocations) {
    logIssue("external", ruleId, primaryLocation.getAbsolutePath());
    NewExternalIssue newIssue = newExternalIssue(ruleId);
    newIssue.at(createIssueLocation(inputFile, primaryLocation, newIssue::newLocation, true));
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

  private void createIssue(InputFile inputFile, String ruleId, Location primaryLocation, Collection<Location> secondaryLocations, String repositoryKey, boolean withExecutionFlow) {
    boolean isSonarSourceRepository = isSonarSourceRepository(repositoryKey);
    logIssue("normal", ruleId, primaryLocation.getAbsolutePath());
    NewIssue newIssue = context.newIssue();
    newIssue
      .forRule(RuleKey.of(repositoryKey, ruleId))
      .at(createIssueLocation(inputFile, primaryLocation, newIssue::newLocation, !isSonarSourceRepository));
    if (withExecutionFlow) {
      populateExecutionFlow(secondaryLocations, newIssue, !isSonarSourceRepository);
    } else {
      populateSecondaryLocations(secondaryLocations, newIssue::newLocation, newIssue::addLocation, !isSonarSourceRepository);
    }
    newIssue.save();
  }

  private void populateExecutionFlow(Collection<Location> locations, NewIssue newIssue, boolean isLocationResilient) {
    List<NewIssueLocation> newIssueLocations = locations.stream()
      .map(x -> context.fileSystem().inputFile(context.fileSystem().predicates().hasAbsolutePath(x.getAbsolutePath())))
      .filter(Objects::nonNull)
      .map(x -> createIssueLocation(x, locations.iterator().next(), newIssue::newLocation, isLocationResilient))
      .toList();
    if (!newIssueLocations.isEmpty()) {
      newIssue.addFlow(newIssueLocations, NewIssue.FlowType.EXECUTION, "Execution Flow");
    }
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

    NewIssueLocation newIssueLocation = newIssueLocationSupplier.get().on(inputFile);

    try {
      // First, we try to report the issue with the precise location...
      newIssueLocation = newIssueLocation.at(inputFile.newRange(location.getStartLine(), location.getStartColumn(),
        location.getEndLine(), location.getEndColumn()));

    } catch (IllegalArgumentException ex1) {
      LOG.debug("Precise issue location cannot be found! Location: {}", location);

      if (!isLocationResilient && !isLocationInsideRazorFile(location)) {
        // Our rules should fail if they report on an invalid location except for .razor and .cshtml files where we expect invalid locations due to issues on razor/roslyn side
        throw ex1;
      }

      try {
        // Precise location failed, now try the line...
        newIssueLocation = newIssueLocation.at(inputFile.selectLine(location.getStartLine()));
      } catch (IllegalArgumentException ex2) {
        // Line location failed so let's report at file level (we are sure the file exists).
        // As the file was already registered previously, there is nothing to do here.

        LOG.debug("Line issue location cannot be found! Location: {}", location);
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

  private static boolean isSonarSourceRepository(String repositoryKey) {
    return OWN_REPOSITORIES.contains(repositoryKey);
  }

  private static boolean isLocationInsideRazorFile(Location location) {
    String absolutePath = location.getAbsolutePath();

    return absolutePath.endsWith(".razor")
      || absolutePath.endsWith(".cshtml");
  }

  private void logIssue(String issueType, String ruleId, String location) {
    LOG.debug("Adding {} issue {}: {}", issueType, ruleId, location);
  }

  private void logMissingInputFile(String ruleId, String filePath) {
    LOG.debug("Skipping issue {}, input file not found or excluded: {}", ruleId, filePath);
  }

  private record ProjectIssue(String ruleId, String message) {
  }

  private record Issue(String ruleId, String absolutePath, int startLine, int startColumn, int endLine, int endColumn) {
    Issue(String ruleId, String path) {
      this(ruleId, path, 0, 0, 0, 0);
    }

    Issue(String ruleId, Location location) {
      this(ruleId,
        location.getAbsolutePath(),
        location.getStartLine(),
        location.getStartColumn(),
        location.getEndLine(),
        location.getEndColumn());
    }
  }
}
