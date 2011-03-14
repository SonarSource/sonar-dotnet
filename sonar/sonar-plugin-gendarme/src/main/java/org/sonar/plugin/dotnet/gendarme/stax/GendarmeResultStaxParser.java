/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.gendarme.stax;

import static org.sonar.plugin.dotnet.core.StaxHelper.advanceCursor;
import static org.sonar.plugin.dotnet.core.StaxHelper.descendantElements;
import static org.sonar.plugin.dotnet.core.StaxHelper.descendantSpecifiedElements;
import static org.sonar.plugin.dotnet.core.StaxHelper.findAttributeValue;
import static org.sonar.plugin.dotnet.core.StaxHelper.isAStartElement;
import static org.sonar.plugin.dotnet.core.StaxHelper.nextPosition;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;

import org.codehaus.staxmate.SMInputFactory;
import org.codehaus.staxmate.in.SMFilterFactory;
import org.codehaus.staxmate.in.SMHierarchicCursor;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.sonar.plugin.dotnet.core.SonarPluginException;
import org.sonar.plugin.dotnet.gendarme.model.Issue;

/**
 * Parser Gendarme using Stax
 * 
 * @author Maxime Schneider-Dufeutrelle
 *
 */
public class GendarmeResultStaxParser extends AbstractXmlParser {

  private final static Logger log = LoggerFactory.getLogger(GendarmeResultStaxParser.class);

  /**
   * Parses a Gendarme violation file.
   * 
   * @param file
   * @return a list of issues corresponding to the reported violations
   */
  public List<Issue> parse(File file) {

    try{
      SMInputFactory inf = new SMInputFactory(XMLInputFactory.newInstance());
      SMHierarchicCursor rootCursor = inf.rootElementCursor(file);
      advanceCursor(rootCursor);    
      SMInputCursor results = descendantSpecifiedElements(rootCursor, "results");
      advanceCursor(results);
      SMInputCursor rules = descendantElements(results);
      List<Issue> issues = new ArrayList<Issue>();

      while( nextPosition(rules) != null && isAStartElement(rules) ){

        String name = findAttributeValue(rules, "Name");
        log.debug("Retrieving details for rule {}", name);
        SMInputCursor details = descendantElements(rules);

        //Collect problem message
        advanceCursor(details);
        String problem = details.collectDescendantText();
        log.debug("-Problem message : {}", problem);

        //Collect solution message
        advanceCursor(details);
        String solution = details.collectDescendantText();
        log.debug("-Solution message : {}", solution);

        details.setFilter( SMFilterFactory.getElementOnlyFilter("target") );
        log.debug("--Location(s) :");
        while( nextPosition(details) != null && isAStartElement(details) ){

          String assembly = findAttributeValue(details, "Assembly");
          SMInputCursor defect = descendantElements(details);

          while( nextPosition(defect) != null && isAStartElement(defect) ){

            Issue issue = new Issue(name, problem, solution, assembly);
            String location = findAttributeValue(defect, "Location");
            issue.setLocation(location);
            log.debug("--------------- {}", location);
            issue.setSource( findAttributeValue(defect, "Source") );

            log.debug("Adding new issue :\n"+ issue.toString() +"\n\n");

            issues.add(issue);
            advanceCursor(defect);
          }
        }
      }
      return issues;

    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while parsing gendarme report", e);
    }
  }


}
