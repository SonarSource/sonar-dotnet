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
package org.sonar.plugins.dotnet.tests;

import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.io.File;
import java.io.IOException;

public class XUnitTestResultsFileParser implements UnitTestResultsParser {

  private static final Logger LOG = Loggers.get(XUnitTestResultsFileParser.class);

  @Override
  public void accept(File file, UnitTestResults unitTestResults) {
    LOG.info("Parsing the XUnit Test Results file " + file.getAbsolutePath());
    new Parser(file, unitTestResults).parse();
  }

  private static class Parser {

    private final File file;
    private final UnitTestResults unitTestResults;

    Parser(File file, UnitTestResults unitTestResults) {
      this.file = file;
      this.unitTestResults = unitTestResults;
    }

    public void parse() {
      try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {

        String tag = xmlParserHelper.nextStartTag();
        if (!"assemblies".equals(tag) && !"assembly".equals(tag)) {
          throw xmlParserHelper.parseError("Expected either an <assemblies> or an <assembly> root tag, but got <" + tag + "> instead.");
        }

        do {
          if ("assembly".equals(tag)) {
            handleAssemblyTag(xmlParserHelper);
          }
        } while ((tag = xmlParserHelper.nextStartTag()) != null);
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }
    }

    private void handleAssemblyTag(XmlParserHelper xmlParserHelper) {

      String totalString = xmlParserHelper.getAttribute("total");
      if (totalString == null) {
        LOG.warn("One of the assemblies contains no test result, please make sure this is expected.");
        return;
      }

      int total = xmlParserHelper.tagToIntValue("total", totalString);
      int failed = xmlParserHelper.getRequiredIntAttribute("failed");
      int skipped = xmlParserHelper.getRequiredIntAttribute("skipped");
      int errors = xmlParserHelper.getIntAttributeOrZero("errors");

      Double time = xmlParserHelper.getDoubleAttribute("time");
      Long executionTime = time != null ? (long) (time * 1000) : null;

      unitTestResults.add(total, skipped, failed, errors, executionTime);
    }

  }

}
