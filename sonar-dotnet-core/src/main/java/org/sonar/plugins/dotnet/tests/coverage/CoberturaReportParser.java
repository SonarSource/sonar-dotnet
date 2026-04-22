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
import javax.annotation.Nullable;
import javax.xml.stream.XMLStreamException;
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
}
