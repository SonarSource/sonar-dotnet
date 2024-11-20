/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import org.sonar.api.notifications.AnalysisWarnings;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;

/**
 * @deprecated in 8.6 because NCover3 is not supported anymore by NCover LLC
 */
@Deprecated
public class NCover3ReportParser implements CoverageParser {

  private static final String EXCLUDED_ID = "0";
  private static final Logger LOG = LoggerFactory.getLogger(NCover3ReportParser.class);
  private final FileService fileService;
  private final AnalysisWarnings analysisWarnings;

  NCover3ReportParser(FileService fileService, AnalysisWarnings analysisWarnings) {
    this.fileService = fileService;
    this.analysisWarnings = analysisWarnings;
  }

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.debug("The current user dir is '{}'.", lazy(() -> System.getProperty("user.dir")));
    LOG.info("Parsing the NCover3 report {}", file.getAbsolutePath());
    new Parser(file, coverage).parse();
  }

  private class Parser {

    private final File file;
    private final Map<String, String> documents = new HashMap<>();
    private final Coverage coverage;

    Parser(File file, Coverage coverage) {
      this.file = file;
      this.coverage = coverage;
    }

    public void parse() {
      String deprecationMessage ="NCover3 coverage import is deprecated since version 8.6 of the plugin. " +
        "Consider using a different code coverage tool instead.";
      LOG.warn(deprecationMessage);
      analysisWarnings.addUnique(deprecationMessage);

      try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {
        checkRootTag(xmlParserHelper);
        dispatchTags(xmlParserHelper);
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }
    }

    private void dispatchTags(XmlParserHelper xmlParserHelper) {
      String tagName;
      while ((tagName = xmlParserHelper.nextStartTag()) != null) {
        if ("doc".equals(tagName)) {
          handleDocTag(xmlParserHelper);
        } else if ("seqpnt".equals(tagName)) {
          handleSegmentPointTag(xmlParserHelper);
        }
      }
    }

    private void handleDocTag(XmlParserHelper xmlParserHelper) {
      String id = xmlParserHelper.getRequiredAttribute("id");
      String url = xmlParserHelper.getRequiredAttribute("url");

      LOG.trace("Analyzing the doc tag with NCover3 ID '{}' and url '{}'.", id, url);

      if (!isExcludedId(id)) {
        try {
          String path = new File(url).getCanonicalPath();

          LOG.trace("NCover3 ID '{}' with url '{}' is resolved as '{}'.", id, url, path);

          documents.put(id, path);
        } catch (IOException e) {
          LOG.debug("Skipping the import of NCover3 code coverage for the invalid file path: " + url
            + " at line " + xmlParserHelper.stream().getLocation().getLineNumber(), e);
        }
      } else {
        LOG.debug("NCover3 ID '{}' is excluded, so url '{}' was not added.", id, url);
      }
    }

    private boolean isExcludedId(String id) {
      return EXCLUDED_ID.equals(id);
    }

    private void handleSegmentPointTag(XmlParserHelper xmlParserHelper) {
      String doc = xmlParserHelper.getRequiredAttribute("doc");
      int line = xmlParserHelper.getRequiredIntAttribute("l");
      int vc = xmlParserHelper.getRequiredIntAttribute("vc");

      if (documents.containsKey(doc) && !isExcludedLine(line)) {
        String path = documents.get(doc);
        if (fileService.isSupportedAbsolute(path)) {

          LOG.trace("Found coverage for line '{}', vc '{}' when analyzing the doc '{}' with the path '{}'.",
            line, vc, doc, path);

          coverage.addHits(path, line, vc);
        } else {
          LOG.debug("NCover3 doc '{}', line '{}', vc '{}' will be skipped because it has a path '{}'"
              + " which is not indexed or does not have the supported language. " + VERIFY_SONARPROJECTPROPERTIES_MESSAGE,
            doc, line, vc, path);
        }
      } else if (!isExcludedLine(line)) {
        LOG.debug("NCover3 doc '{}' is not contained in documents and will be skipped.", doc);
      }
    }

    private boolean isExcludedLine(Integer line) {
      return 0 == line;
    }

    private void checkRootTag(XmlParserHelper xmlParserHelper) {
      xmlParserHelper.checkRootTag("coverage");
      xmlParserHelper.checkRequiredAttribute("exportversion", 3);
    }

  }

}
