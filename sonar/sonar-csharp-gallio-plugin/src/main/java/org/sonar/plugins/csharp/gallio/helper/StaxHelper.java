/*
 * Sonar C# Plugin :: Gallio
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
package org.sonar.plugins.csharp.gallio.helper;

import javax.xml.namespace.QName;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.events.XMLEvent;

import org.codehaus.staxmate.in.SMEvent;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.SonarException;

/**
 * This class was made to avoid try/catch blocks everywhere 
 * for the XMLStreamException when collecting data and manipulating cursors
 * 
 * @author Maxime SCHNEIDER-DUFEUTRELLE January 31, 2011
 */
public final class StaxHelper {

  private StaxHelper() {
  }
	
  private static final Logger LOG = LoggerFactory.getLogger(StaxHelper.class);

  // Retrieve the value of the given name attribute
  public static String findAttributeValue(SMInputCursor cursor, String attributeName) {
    try {
      final String attributValue;
      if(isAStartElement(cursor)){
        attributValue = cursor.getAttrValue(attributeName);
      } else{
    	// should not happen
    	LOG.warn("Trying to get an attribute value in the wrong position "+cursor);  
        attributValue = null;
      }
      return attributValue;
    } catch (XMLStreamException e) {
      throw new SonarException("Error while retrieving attribute value", e);
    }
  }

  // Retrieve the integer value of the given name attribute
  public static int findAttributeIntValue(SMInputCursor cursor, String attributeName) {
    try {
      return Integer.valueOf(cursor.getAttrValue(attributeName));
    } catch (XMLStreamException e) {
      throw new SonarException("Error while retrieving int attribute value", e);
    }
  }

  // Check if the cursor is positioned at a start element
  public static boolean isAStartElement(SMInputCursor cursor) {
    try {
      return cursor.asEvent().isStartElement();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while testing if the cursor were a start element", e);
    }
  }

  // Check if the cursor is positioned at an end element
  public static boolean isAnEndElement(SMInputCursor cursor) {
    try {
      return cursor.asEvent().isEndElement();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while testing if the cursor were a end element", e);
    }
  }

  // Retrieve current element's name
  public static String findElementName(SMInputCursor cursor) {
    try {
      return cursor.getLocalName();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while retrieving element name", e);
    }
  }

  // Move the cursor forward
  public static void advanceCursor(SMInputCursor cursor) {
    try {
      cursor.advance();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while trying to move the cursor", e);
    }
  }

  // Get the next cursor's position
  public static SMEvent nextPosition(SMInputCursor cursor) {
    try {
      return cursor.getNext();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while trying to go to the next cursor position", e);
    }
  }

  // Retrieve the corresponding element's event
  public static XMLEvent findXMLEvent(SMInputCursor cursor) {
    try {
      return cursor.asEvent();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while trying to get XMLEvent", e);
    }
  }

  // Get the descendant elements
  public static SMInputCursor descendantElements(SMInputCursor cursor) {
    try {
      return cursor.descendantElementCursor();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while trying to get descendant elements", e);
    }
  }

  // Get the descendant elements matching the given name
  public static SMInputCursor descendantSpecifiedElements(SMInputCursor cursor, String specifiedElements) {
    try {
      return cursor.descendantElementCursor(specifiedElements);
    } catch (XMLStreamException e) {
      throw new SonarException("Error while trying to get descendant specified elements", e);
    }
  }

  // Get the descendant elements matching the given QName
  public static SMInputCursor descendantSpecifiedElements(SMInputCursor cursor, QName specifiedElements) {
    try {
      return cursor.descendantElementCursor(specifiedElements);
    } catch (XMLStreamException e) {
      throw new SonarException("Error while trying to get descendant specified elements", e);
    }
  }

  // Retrieve next element's name
  public static String findNextElementName(SMInputCursor cursor) {
    try {
      return cursor.advance().getLocalName();
    } catch (XMLStreamException e) {
      throw new SonarException("Error while trying to get the next element name", e);
    }
  }
}
