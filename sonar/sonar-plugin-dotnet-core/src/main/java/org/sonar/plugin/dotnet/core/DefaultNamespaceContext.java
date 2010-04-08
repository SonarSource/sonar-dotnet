/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

/*
 * Created on Jun 18, 2009
 */
package org.sonar.plugin.dotnet.core;

import java.util.Iterator;

import javax.xml.XMLConstants;
import javax.xml.namespace.NamespaceContext;

/**
 * A NamespaceContext implementation necessary for the parsing of some XML files.
 * @author Jose CHILLAN Jul 15, 2009
 */
public class DefaultNamespaceContext
  implements NamespaceContext
{
  private final String defaultNamespace;
  private final String defaultPrefix;

  public DefaultNamespaceContext(String defaultPrefix, String defaultNamespace)
  {
    this.defaultNamespace = defaultNamespace;
    this.defaultPrefix = defaultPrefix;
    
  }

  public String getNamespaceURI(String prefix)
  {
    if (defaultPrefix.equals(prefix))
    {
      return defaultNamespace;
    }
    else if ("xml".equals(prefix))
    {
      return XMLConstants.XML_NS_URI;
    }
    return "";
  }

  // This method isn't necessary for XPath processing.
  public String getPrefix(String uri)
  {
    throw new UnsupportedOperationException();
  }

  // This method isn't necessary for XPath processing either.
  public Iterator<?> getPrefixes(String uri)
  {
    throw new UnsupportedOperationException();
  }
}
