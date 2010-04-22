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
 * Created on May 7, 2009
 */
package org.sonar.plugin.dotnet.core;

import java.io.InputStream;
import java.io.OutputStream;
import java.io.StringWriter;
import java.io.Writer;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Marshaller;
import javax.xml.bind.Unmarshaller;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import org.w3c.dom.Element;

/**
 * XML serialization utility class based on JAXB 2.x .
 * 
 * @author Jose CHILLAN (jose.chillan($)heavenize.org) , 15 august 2006
 */
public class XmlUtils
{
  /**
   * Marshall an object into a XML Stream
   * 
   * @param serialized
   * @param stream
   * @throws XmlSerializationException
   */
  public static void marshall(Object serialized, OutputStream stream) throws XmlSerializationException
  {
    try
    {
      // Establish a jaxb context
      JAXBContext jc = JAXBContext.newInstance(serialized.getClass());

      // Get a marshaller
      Marshaller m = jc.createMarshaller();

      // Enable formatted xml output
      m.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, Boolean.valueOf(true));

      // Marshal to system output: java to xml
      m.marshal(serialized, stream);
    }
    catch (JAXBException x)
    {
      throw new XmlSerializationException(x);
    }
  }

  /**
   * Marshall an object into a XML Stream
   * 
   * @param serialized
   * @param stream
   * @throws XmlSerializationException
   */
  public static void marshall(Object serialized, Writer writer) throws XmlSerializationException
  {
    try
    {
      // Establish a jaxb context
      JAXBContext jc = JAXBContext.newInstance(serialized.getClass());

      // Get a marshaller
      Marshaller m = jc.createMarshaller();

      // Enable formatted xml output
      m.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, Boolean.valueOf(true));

      // Marshal to system output: java to xml
      m.marshal(serialized, writer);
    }
    catch (JAXBException x)
    {
      throw new XmlSerializationException(x);
    }
  }
  /**
   * Unmarshall an xml stream into an object of given type
   * 
   * @param <T>
   * @param stream
   * @param type
   * @return
   * @throws XmlSerializationException
   */
  @SuppressWarnings("unchecked")
  public static <T> T unmarshall(InputStream stream, Class<T> type) throws XmlSerializationException
  {
    try
    {
      // Establish a jaxb context
      JAXBContext jc = JAXBContext.newInstance(type);

      // Get a unmarshaller
      Unmarshaller m = jc.createUnmarshaller();

      // Marshal to system output: java to xml
      T result = (T) m.unmarshal(stream);
      return result;
    }
    catch (JAXBException x)
    {
      throw new XmlSerializationException("Impossible to unmarshall and object of type " + type, x);
    }
  }

  /**
   * Converts an XML element into a string representation.
   * @param violation
   * @return
   */
  public static String toString(Element violation)
  {
    try
    {
      TransformerFactory tf = TransformerFactory.newInstance();
      Transformer trans = tf.newTransformer();
      StringWriter sw = new StringWriter();
      trans.transform(new DOMSource(violation), new StreamResult(sw));
      String xml = sw.toString();
      return xml;
    }
    catch (Exception e)
    {
      return "Could not convert : " + e.getMessage();
    }
  }

  /**
   * Disabled constructor.
   */
  private XmlUtils()
  {
  }
}