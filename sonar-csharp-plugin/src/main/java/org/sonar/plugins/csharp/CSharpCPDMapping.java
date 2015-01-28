/*
 * Sonar C# Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp;

import com.google.common.base.Charsets;
import com.google.common.base.Preconditions;
import com.google.common.base.Throwables;
import com.google.common.io.Closeables;
import net.sourceforge.pmd.cpd.SourceCode;
import net.sourceforge.pmd.cpd.TokenEntry;
import net.sourceforge.pmd.cpd.Tokenizer;
import net.sourceforge.pmd.cpd.Tokens;
import org.apache.commons.codec.binary.Base64;
import org.sonar.api.batch.AbstractCpdMapping;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.resources.Language;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;

@DependsUpon("NSonarQubeAnalysis")
public class CSharpCPDMapping extends AbstractCpdMapping {

  private final CSharp csharp;
  private final FileSystem fs;

  public CSharpCPDMapping(CSharp csharp, FileSystem fs) {
    this.csharp = csharp;
    this.fs = fs;
  }

  @Override
  public Language getLanguage() {
    return csharp;
  }

  @Override
  public Tokenizer getTokenizer() {
    return new Tokenizer() {

      @Override
      public void tokenize(SourceCode source, Tokens cpdTokens) {
        File toolOutput = CSharpSensor.toolOutput(fs);
        new AnalysisResultImporter(cpdTokens, source.getFileName()).parse(toolOutput);
        cpdTokens.add(TokenEntry.EOF);
      }

    };
  }

  private static class AnalysisResultImporter {

    private final Tokens cpdTokens;
    private final String path;

    private File file;
    private XMLStreamReader stream;

    public AnalysisResultImporter(Tokens cpdTokens, String path) {
      this.cpdTokens = cpdTokens;
      this.path = path;
    }

    public void parse(File file) {
      this.file = file;
      InputStreamReader reader = null;
      XMLInputFactory xmlFactory = XMLInputFactory.newInstance();

      try {
        reader = new InputStreamReader(new FileInputStream(file), Charsets.UTF_8);
        stream = xmlFactory.createXMLStreamReader(reader);

        while (stream.hasNext()) {
          if (stream.next() == XMLStreamConstants.START_ELEMENT) {
            String tagName = stream.getLocalName();

            if ("File".equals(tagName)) {
              handleFileTag();
            }
          }
        }
      } catch (IOException e) {
        throw Throwables.propagate(e);
      } catch (XMLStreamException e) {
        throw Throwables.propagate(e);
      } finally {
        closeXmlStream();
        Closeables.closeQuietly(reader);
      }

      return;
    }

    private void closeXmlStream() {
      if (stream != null) {
        try {
          stream.close();
        } catch (XMLStreamException e) {
          throw Throwables.propagate(e);
        }
      }
    }

    private void handleFileTag() throws XMLStreamException {
      boolean pathMatches = false;

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "File".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Path".equals(tagName) && path.equals(stream.getElementText())) {
            pathMatches = true;
          } else if (pathMatches && "CopyPasteTokens".equals(tagName)) {
            handleCopyPasteTokensTag();
          }
        }
      }
    }

    private void handleCopyPasteTokensTag() throws XMLStreamException {
      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "CopyPasteTokens".equals(stream.getLocalName())) {
          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Token".equals(tagName)) {
            handleTokenTag();
          } else {
            throw new IllegalArgumentException();
          }
        }
      }
    }

    private void handleTokenTag() throws XMLStreamException {
      String value = null;
      int line = -1;

      while (stream.hasNext()) {
        int next = stream.next();

        if (next == XMLStreamConstants.END_ELEMENT && "Token".equals(stream.getLocalName())) {
          Preconditions.checkState(value != null);
          Preconditions.checkState(line != -1);

          cpdTokens.add(new TokenEntry(value, file.getAbsolutePath(), line));

          break;
        } else if (next == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("Value".equals(tagName)) {
            value = new String(Base64.decodeBase64(stream.getElementText()), Charsets.UTF_8);
          } else if ("Line".equals(tagName)) {
            line = Integer.parseInt(stream.getElementText());
          } else {
            throw new IllegalArgumentException();
          }
        }
      }
    }

  }

}
