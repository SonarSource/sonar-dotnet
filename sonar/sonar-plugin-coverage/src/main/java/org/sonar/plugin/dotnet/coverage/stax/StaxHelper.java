/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.coverage.stax;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.events.XMLEvent;

import org.codehaus.staxmate.in.SMEvent;
import org.codehaus.staxmate.in.SMInputCursor;
import org.sonar.plugin.dotnet.core.SonarPluginException;

/**
 * This class was made to avoid try/catch blocks everywhere for
 * the XMLStreamException when collecting datas and manipulating cursors
 * 
 * @author Maxime SCHNEIDER-DUFEUTRELLE January 31, 2011
 */
public class StaxHelper {

  public static String findAttributeValue(SMInputCursor cursor, String attributeName){
    String attributeValue;
    try{
      attributeValue = cursor.getAttrValue(attributeName);
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while retrieving attribute value", e);
    }
    return attributeValue;
  }

  public static int findAttributeIntValue(SMInputCursor cursor, String attributeName){
    int attributeValue;
    try{
      attributeValue = Integer.valueOf(cursor.getAttrValue(attributeName));
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while retrieving attribute value", e);
    }
    return attributeValue;
  }
  
  public static boolean isAStartElement(SMInputCursor cursor){
    boolean isStartElement = false;
    try{
      isStartElement = cursor.asEvent().isStartElement();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while testing if the cursor were a start element", e);
    }
    return isStartElement;
  }

  public static String findElementName(SMInputCursor cursor){
    String elementValue;
    try{
      elementValue = cursor.getLocalName();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while retrieving element name", e);
    }
    return elementValue;
  }

  public static void advanceCursor(SMInputCursor cursor){
    try{
      cursor.advance();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to move the cursor", e);
    }
  }

  public static SMEvent nextPosition(SMInputCursor cursor){
    SMEvent nextEvent;
    try{
      nextEvent = cursor.getNext();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to go to the next cursor position", e);
    }
    return nextEvent;
  }

  public static XMLEvent findXMLEvent(SMInputCursor cursor){
    XMLEvent itsEvent;
    try{
      itsEvent = cursor.asEvent();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to get XMLEvent", e);
    }
    return itsEvent;
  }

  public static SMInputCursor descendantElements(SMInputCursor cursor){
    SMInputCursor pointTag;
    try{
      pointTag = cursor.descendantElementCursor();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to get descendant elements", e);
    }
    return pointTag;
  }

  public static String findNextElementName(SMInputCursor cursor){
    String nextElementName;
    try{
      nextElementName = cursor.advance().getLocalName();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to get the next element name", e);
    }
    return nextElementName;
  }
}
