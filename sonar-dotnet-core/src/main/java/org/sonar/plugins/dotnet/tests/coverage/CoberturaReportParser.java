/*
 * SonarSource :: .NET :: Core
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonar.plugins.dotnet.tests.coverage;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import javax.annotation.Nullable;
import javax.xml.stream.XMLStreamException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugins.dotnet.tests.FileService;
import org.sonar.plugins.dotnet.tests.XmlParserHelper;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;

public final class CoberturaReportParser implements CoverageParser {

  private static final Logger LOG = LoggerFactory.getLogger(CoberturaReportParser.class);
  private static final Pattern CONDITION_COVERAGE_PATTERN = Pattern.compile("\\d+%\\s*\\((\\d+)/(\\d+)\\)");
  private final FileService fileService;

  public CoberturaReportParser(FileService fileService) {
    this.fileService = fileService;
  }

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.debug("The current user dir is '{}'.", lazy(() -> System.getProperty("user.dir")));
    LOG.info("Parsing the Cobertura report {}", file.getAbsolutePath());
    new Parser(file, coverage).parse();
  }

  private class Parser {
    private final File file;
    private final Coverage coverage;
    private final List<String> sources = new ArrayList<>();
    private final Map<String, CoveredFile> filesByFilename = new HashMap<>();
    private String currentClassFilename;

    private Parser(File file, Coverage coverage) {
      this.file = file;
      this.coverage = coverage;
    }

    void parse() {
      try (var xmlParserHelper = new XmlParserHelper(file)) {
        xmlParserHelper.checkRootTag("coverage");
        dispatchTags(xmlParserHelper);
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }
    }

    private void dispatchTags(XmlParserHelper xmlParserHelper) {
      String tagName;
      while ((tagName = xmlParserHelper.nextStartTag()) != null) {
        if ("source".equals(tagName)) {
          handleSourceTag(xmlParserHelper);
        } else if ("class".equals(tagName)) {
          handleClassTag(xmlParserHelper);
        } else if ("line".equals(tagName)) {
          handleLineTag(xmlParserHelper);
        }
      }
    }

    private void handleSourceTag(XmlParserHelper xmlParserHelper) {
      try {
        String sourceText = xmlParserHelper.stream().getElementText();
        if (sourceText != null && !sourceText.isBlank()) {
          sources.add(sourceText);
          LOG.debug("Cobertura parser: found source '{}'.", sourceText);
        }
      } catch (XMLStreamException e) {
        LOG.debug("Cobertura parser: failed to read <source> element text at line {}.",
          xmlParserHelper.stream().getLocation().getLineNumber(), e);
      }
    }

    private void handleClassTag(XmlParserHelper xmlParserHelper) {
      String filename = xmlParserHelper.getRequiredAttribute("filename");
      currentClassFilename = filename;
      if (!filesByFilename.containsKey(filename)) {
        CoveredFile coveredFile = resolveFile(filename);
        LOG.debug("CoveredFile created: {}.", coveredFile);
        filesByFilename.put(filename, coveredFile);
      }
    }

    private void handleLineTag(XmlParserHelper xmlParserHelper) {
      CoveredFile coveredFile = filesByFilename.get(currentClassFilename);
      if (coveredFile == null || !coveredFile.isIndexed()) {
        return;
      }

      int line = xmlParserHelper.getRequiredIntAttribute("number");
      int hits = xmlParserHelper.getRequiredIntAttribute("hits");
      coverage.addHits(coveredFile.indexedPath(), line, hits);
      LOG.trace("Cobertura parser: add hits for file {}, line '{}', hits '{}'.", coveredFile, line, hits);

      String branchAttr = xmlParserHelper.getAttribute("branch");
      String conditionCoverage = xmlParserHelper.getAttribute("condition-coverage");
      int[] coveredAndTotal = parseConditionCoverage(conditionCoverage);
      if (!"True".equalsIgnoreCase(branchAttr) || coveredAndTotal == null) {
        return;
      }

      String filePath = coveredFile.indexedPath();
      int covered = coveredAndTotal[0];
      int total = coveredAndTotal[1];
      String coverageIdentifier = file.getPath();

      List<ParsedCondition> conditions = handleConditions(xmlParserHelper);

      if (conditions.isEmpty()) {
        addUnmergeableConditions(filePath, line, covered, total, coverageIdentifier);
      } else {
        addMergeableConditions(filePath, line, covered, total, conditions, coverageIdentifier);
      }
    }

    private List<ParsedCondition> handleConditions(XmlParserHelper xmlParserHelper) {
      List<ParsedCondition> conditions = new ArrayList<>();
      String tag;
      while ((tag = xmlParserHelper.nextStartOrEndTag()) != null) {
        if ("</line>".equals(tag)) {
          break;
        } else if ("<condition>".equals(tag)) {
          int number = xmlParserHelper.getRequiredIntAttribute("number");
          String type = xmlParserHelper.getRequiredAttribute("type");
          String coverageAttr = xmlParserHelper.getRequiredAttribute("coverage");
          conditions.add(new ParsedCondition(number, type, coverageAttr));
        }
      }
      return conditions;
    }

    private void addMergeableConditions(String filePath, int line, int covered, int total,
      List<ParsedCondition> conditions, String coverageIdentifier) {
      // Two-pass approach: process jump conditions first, then switches with remaining totals.
      // The line-level condition-coverage counts all branches (jump + switch combined).
      // Each jump condition accounts for 2 branches, so we subtract those before processing switches.
      int remainingTotal = total;
      int remainingCovered = covered;
      for (ParsedCondition condition : conditions) {
        if ("jump".equals(condition.type)) {
          int jumpCovered = addJumpConditions(filePath, line, condition, coverageIdentifier);
          remainingTotal -= 2;
          remainingCovered -= jumpCovered;
        } else if (!"switch".equals(condition.type)) {
          LOG.debug("Cobertura parser: unknown condition type '{}' on file '{}', line '{}'. Skipping.", condition.type, filePath, line);
        }
      }

      remainingTotal = Math.max(0, remainingTotal);
      remainingCovered = Math.max(0, remainingCovered);

      for (ParsedCondition condition : conditions) {
        if ("switch".equals(condition.type)) {
          addSwitchConditions(filePath, line, remainingCovered, remainingTotal, condition.number, coverageIdentifier);
        }
      }
    }

    private void addUnmergeableConditions(String filePath, int line, int covered, int total, String coverageIdentifier) {
      for (int i = 0; i < total; i++) {
        coverage.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, line, new ConditionData.Location(i, 0), i, i < covered ? 1 : 0, coverageIdentifier));
      }
      LOG.trace("Cobertura parser: add {} unmergeable branch conditions for file '{}', line '{}'.", total, filePath, line);
    }

    private void addSwitchConditions(String filePath, int line, int covered, int total, int conditionNumber, String coverageIdentifier) {
      for (int i = 0; i < total; i++) {
        coverage.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, line, new ConditionData.Location(conditionNumber, 0), i, i < covered ? 1 : 0, coverageIdentifier));
      }
      LOG.trace("Cobertura parser: add {} switch branch conditions for file '{}', line '{}', condition number '{}'.", total, filePath, line, conditionNumber);
    }

    private int addJumpConditions(String filePath, int line, ParsedCondition condition, String coverageIdentifier) {
      int percentage = parsePercentage(condition.coverage);
      var location = new ConditionData.Location(condition.number, 0);
      int hitPath0 = percentage > 0 ? 1 : 0;
      int hitPath1 = percentage == 100 ? 1 : 0;
      coverage.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, line, location, 0, hitPath0, coverageIdentifier));
      coverage.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, line, location, 1, hitPath1, coverageIdentifier));
      LOG.trace("Cobertura parser: add jump branch conditions for file '{}', line '{}', condition number '{}', coverage '{}%'.", filePath, line, condition.number, percentage);
      return hitPath0 + hitPath1;
    }

    private CoveredFile resolveFile(String filename) {
      var filenameAsFile = new File(filename);
      if (filenameAsFile.isAbsolute()) {
        return createCoveredFile(filenameAsFile);
      }
      for (String source : sources) {
        CoveredFile result = createCoveredFile(new File(source, filename));

        // Potential file resolution ambiguity. There is no feasible fix on our end.
        if (result.isIndexed()) {
          return result;
        }
      }
      LOG.debug("Cobertura parser: could not resolve relative filename '{}' with any source prefix. Skipping. " + VERIFY_SONARPROJECTPROPERTIES_MESSAGE, filename);
      return new CoveredFile(filename, null);
    }

    private CoveredFile createCoveredFile(File fileToResolve) {
      String path = fileToResolve.getPath();
      String canonicalPath;
      try {
        canonicalPath = fileToResolve.getCanonicalPath();
      } catch (IOException e) {
        LOG.debug("Skipping the import of Cobertura code coverage for the invalid file path: {} in report {}. {}",
          path, file.getPath(), VERIFY_SONARPROJECTPROPERTIES_MESSAGE, e);
        return new CoveredFile(path, null);
      }

      if (fileService.isSupportedAbsolute(canonicalPath)) {
        return new CoveredFile(path, canonicalPath);
      } else {
        return fileService.getAbsolutePath(path)
          .map(resolved -> new CoveredFile(path, resolved))
          .orElseGet(() -> new CoveredFile(path, null));
      }
    }
  }

  record ParsedCondition(int number, String type, String coverage) { }

  record CoveredFile(String originalFilename, @Nullable String indexedPath) {
    boolean isIndexed() {
      return indexedPath != null;
    }

    @Override
    public String toString() {
      return indexedPath == null
        ? String.format("(path '%s', NO INDEXED PATH)", originalFilename)
        : String.format("(path '%s', indexed as '%s')", originalFilename, indexedPath);
    }
  }

  @Nullable
  private static int[] parseConditionCoverage(@Nullable String conditionCoverage) {
    if (conditionCoverage == null) {
      return null;
    }
    Matcher matcher = CONDITION_COVERAGE_PATTERN.matcher(conditionCoverage);
    if (!matcher.find()) {
      LOG.debug("Cobertura parser: could not parse condition-coverage '{}'.", conditionCoverage);
      return null;
    }
    return new int[]{Integer.parseInt(matcher.group(1)), Integer.parseInt(matcher.group(2))};
  }

  private static int parsePercentage(String coverage) {
    String trimmed = coverage.trim().replace("%", "");
    try {
      return Integer.parseInt(trimmed);
    } catch (NumberFormatException e) {
      LOG.debug("Cobertura parser: could not parse coverage percentage '{}'.", coverage);
      return 0;
    }
  }
}
