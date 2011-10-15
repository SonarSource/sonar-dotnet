/*
 * .NET tools :: Commons
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
package org.sonar.plugins.csharp.core;

import java.io.File;
import java.nio.charset.Charset;

import javax.xml.stream.XMLInputFactory;

import org.codehaus.stax2.XMLInputFactory2;
import org.codehaus.staxmate.SMInputFactory;

public abstract class AbstractStaxParser {

  private Charset encoding;

  public AbstractStaxParser() {
    this.encoding = Charset.defaultCharset();
  }

  /**
   * Parses a processed violation file.
   * 
   * @param file
   *          the file to parse
   */
  public abstract void parse(File file);

  /**
   * Sets the encoding to use to parse the result file
   * 
   * @param encoding
   *          the encoding to set
   */
  public void setEncoding(Charset encoding) {
    this.encoding = encoding;
  }

  protected SMInputFactory initStax() {
    XMLInputFactory xmlFactory = XMLInputFactory2.newInstance();
    xmlFactory.setProperty(XMLInputFactory.IS_COALESCING, Boolean.TRUE);
    xmlFactory.setProperty(XMLInputFactory.IS_NAMESPACE_AWARE, Boolean.FALSE);
    xmlFactory.setProperty(XMLInputFactory.SUPPORT_DTD, Boolean.FALSE);
    xmlFactory.setProperty(XMLInputFactory.IS_VALIDATING, Boolean.FALSE);
    return new SMInputFactory(xmlFactory);
  }

  protected Charset getEncoding() {
    return encoding;
  }

}
