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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugins.dotnet.tests.FileService;
import org.sonar.plugins.dotnet.tests.XmlParserHelper;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;

public final class CoberturaReportParser implements CoverageParser {

  private static final Logger LOG = LoggerFactory.getLogger(CoberturaReportParser.class);
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

    private static void dispatchTags(XmlParserHelper xmlParserHelper) {
      String tagName;
      while ((tagName = xmlParserHelper.nextStartTag()) != null) {
        // TODO: NET-3613
      }
    }
  }
}
