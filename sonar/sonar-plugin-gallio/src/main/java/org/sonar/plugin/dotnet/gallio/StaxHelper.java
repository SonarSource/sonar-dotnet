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

package org.sonar.plugin.dotnet.gallio;

import javax.xml.namespace.QName;
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
    try{
      return cursor.getAttrValue(attributeName);
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while retrieving attribute value", e);
    }
  }

  public static int findAttributeIntValue(SMInputCursor cursor, String attributeName){
    try{
      return Integer.valueOf(cursor.getAttrValue(attributeName));
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while retrieving attribute value", e);
    }
  }

  public static boolean isAStartElement(SMInputCursor cursor){
    try{
      return cursor.asEvent().isStartElement();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while testing if the cursor were a start element", e);
    }
  }

  public static boolean isAnEndElement(SMInputCursor cursor){
    try{
      return cursor.asEvent().isEndElement();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while testing if the cursor were a end element", e);
    }
  }
  
  public static String findElementName(SMInputCursor cursor){
    try{
      return cursor.getLocalName();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while retrieving element name", e);
    }
  }

  public static void advanceCursor(SMInputCursor cursor){
    try{
      cursor.advance();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to move the cursor", e);
    }
  }

  public static SMEvent nextPosition(SMInputCursor cursor){
    try{
      return cursor.getNext();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to go to the next cursor position", e);
    }
  }

  public static XMLEvent findXMLEvent(SMInputCursor cursor){
    try{
      return cursor.asEvent();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to get XMLEvent", e);
    }
  }

  public static SMInputCursor descendantElements(SMInputCursor cursor){
    try{
      return cursor.descendantElementCursor();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to get descendant elements", e);
    }
  }

  public static SMInputCursor descendantSpecifiedElements(SMInputCursor cursor, String specifiedElements){
    try{
      return cursor.descendantElementCursor(specifiedElements);
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to get descendant specified elements", e);
    }
  }

  public static SMInputCursor descendantSpecifiedElements(SMInputCursor cursor, QName specifiedElements){
    try{
      return cursor.descendantElementCursor(specifiedElements);
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to get descendant specified elements", e);
    }
  }

  public static String findNextElementName(SMInputCursor cursor){
    try{
      return cursor.advance().getLocalName();
    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while trying to get the next element name", e);
    }
  }
}
