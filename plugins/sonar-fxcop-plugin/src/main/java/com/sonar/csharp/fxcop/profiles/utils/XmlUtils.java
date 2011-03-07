/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

/*
 * Created on May 7, 2009
 */
package com.sonar.csharp.fxcop.profiles.utils;

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
 */
public class XmlUtils {

  /**
   * Marshall an object into a XML Stream
   * 
   * @param serialized
   * @param stream
   * @throws XmlSerializationException
   */
  public static void marshall(Object serialized, OutputStream stream) throws XmlSerializationException {
    try {
      // Establish a jaxb context
      JAXBContext jc = JAXBContext.newInstance(serialized.getClass());

      // Get a marshaller
      Marshaller m = jc.createMarshaller();

      // Enable formatted xml output
      m.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, Boolean.valueOf(true));

      // Marshal to system output: java to xml
      m.marshal(serialized, stream);
    } catch (JAXBException x) {
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
  public static void marshall(Object serialized, Writer writer) throws XmlSerializationException {
    try {
      // Establish a jaxb context
      JAXBContext jc = JAXBContext.newInstance(serialized.getClass());

      // Get a marshaller
      Marshaller m = jc.createMarshaller();

      // Enable formatted xml output
      m.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, Boolean.valueOf(true));

      // Marshal to system output: java to xml
      m.marshal(serialized, writer);
    } catch (JAXBException x) {
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
  public static <T> T unmarshall(InputStream stream, Class<T> type) throws XmlSerializationException {
    try {
      // Establish a jaxb context
      JAXBContext jc = JAXBContext.newInstance(type);

      // Get a unmarshaller
      Unmarshaller m = jc.createUnmarshaller();

      // Marshal to system output: java to xml
      T result = (T) m.unmarshal(stream);
      return result;
    } catch (JAXBException x) {
      throw new XmlSerializationException("Impossible to unmarshall and object of type " + type, x);
    }
  }

  /**
   * Converts an XML element into a string representation.
   * 
   * @param violation
   * @return
   */
  public static String toString(Element violation) {
    try {
      TransformerFactory tf = TransformerFactory.newInstance();
      Transformer trans = tf.newTransformer();
      StringWriter sw = new StringWriter();
      trans.transform(new DOMSource(violation), new StreamResult(sw));
      String xml = sw.toString();
      return xml;
    } catch (Exception e) {
      return "Could not convert : " + e.getMessage();
    }
  }

  /**
   * Disabled constructor.
   */
  private XmlUtils() {
  }
}