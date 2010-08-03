/**
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

/*
 * Created on Jun 4, 2009
 */
package org.sonar.plugin.dotnet.gallio;

import java.io.File;
import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

import org.apache.commons.lang.StringUtils;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.w3c.dom.Element;

/**
 * Parses Gallio test reports.
 * 
 * @author Jose CHILLAN Jun 4, 2009
 */
public class GallioResultParser
  extends AbstractXmlParser
{

  /**
   * Outcomes for the test
   */
  public final static String OUTCOME_OK           = "passed";
  public final static String OUTCOME_FAILURE      = "failed";
  public final static String OUTCOME_SKIPPED      = "skipped";

  /**
   * Categories for the test
   */
  public final static String CATEGORY_IGNORED     = "ignored";
  public final static String CATEGORY_ERROR       = "error";
  public final static String GALLIO_XML_NAMESPACE = "http://www.gallio.org/";

  /**
   * Constructs a empty parser.
   */
  public GallioResultParser()
  {
    super("ga", GALLIO_XML_NAMESPACE);
  }

  /**
   * Extracts the list of unit tests reports.
   * @param file the Gallio result file
   * @return
   */
  public Set<UnitTestReport> parse(File file)
  {
    try
    {
      URL url = file.toURI().toURL();
      return parse(url);
    }
    catch (Exception e)
    {
      throw new IllegalArgumentException("Could not load report File : " + file, e);
    }
  }

  /**
   * Parses a file identified by its url
   * @param reportURL
   * @return
   */
  public Set<UnitTestReport> parse(URL reportURL)
  {
    Map<String, UnitTestReport> reportMap = new HashMap<String, UnitTestReport>();

    // We first build a map of the test ids
    Map<String, TestDescription> tests = new HashMap<String, TestDescription>();
    try
    {
      List<Element> testElements = extractElements(reportURL, "//ga:testModel//ga:test[@isTestCase='true']");
      for (Element testElement : testElements)
      {
        Element codeReferenceElement = getUniqueSubElement(testElement, "codeReference");
        Element codeLocationElement = getUniqueSubElement(testElement, "codeLocation");
        if (codeReferenceElement == null)
        {
          continue;
        }
        String id = testElement.getAttribute("id");
        String fullAssembly = codeReferenceElement.getAttribute("assembly");
        if (StringUtils.isBlank(fullAssembly))
        {
          Element parentNode = (Element) testElement.getParentNode();
          codeReferenceElement = getUniqueSubElement(parentNode, "codeReference");
          codeLocationElement = getUniqueSubElement(parentNode, "codeLocation");
          fullAssembly = codeReferenceElement.getAttribute("assembly");
          if (fullAssembly == null)
          {
            // Can't find the location : we skip it
            continue;
          }
        }
        String assemblyName = StringUtils.substringBefore(fullAssembly, ",");
        String namespace = codeReferenceElement.getAttribute("namespace");
        String className = codeReferenceElement.getAttribute("type");
        String methodName = codeReferenceElement.getAttribute("member");
        File sourceLocation = null;
        int lineNumber = 0;
        if (codeLocationElement != null)
        {
          sourceLocation = new File(codeLocationElement.getAttribute("path"));
          lineNumber = getIntAttribute(codeLocationElement, "line");
        }
        // The test model is populated
        TestDescription testDescription = new TestDescription();
        testDescription.setAssemblyName(assemblyName);
        testDescription.setNamespace(namespace);
        testDescription.setClassName(className);
        testDescription.setMethodName(methodName);
        testDescription.setLine(lineNumber);
        testDescription.setSourceFile(sourceLocation);

        // We keep it in a map for future reuse
        tests.put(id, testDescription);
      }

      // Then we parse the results fo the tests.
      List<Element> testStepsElements = extractElements(reportURL, "//ga:testStepRun/ga:testStep[@isTestCase='true']");
      for (Element testStepElt : testStepsElements)
      {
        String testId = testStepElt.getAttribute("testId");
        Element testStepRunElement = (Element) testStepElt.getParentNode();
        // We retrieve the description already parsed
        // NOTE : we can do this in ONE PASS !!! (the data are duplicated from the model)
        TestDescription description = tests.get(testId);
        if (description == null)
        {
          // No associated description : we skip it
          continue;
        }
        Element resultElement = getUniqueSubElement(testStepRunElement, "result");
        int countAsserts = getIntAttribute(resultElement, "assertCount");
        double duration = getDoubleAttribute(resultElement, "duration");
        String status = evaluateAttribute(resultElement, "ga:outcome/@status");
        String category = evaluateAttribute(resultElement, "ga:outcome/@category");
        TestStatus executionStatus = computeStatus(status, category);
        String message = null;
        String stackTrace = null;
        if ((executionStatus == TestStatus.FAILED) || (executionStatus == TestStatus.ERROR))
        {
          message = evaluateAttribute(testStepRunElement, "ga:testLog//ga:section[@name='Message']/ga:contents/ga:text");
          stackTrace = evaluateAttribute(testStepRunElement, "ga:testLog//ga:section//ga:marker[@class='StackTrace']/ga:contents/ga:text");
        }
        File sourceFile = description.getSourceFile();

        // We build and populate the detail
        TestCaseDetail detail = new TestCaseDetail();
        detail.setSourceFile(sourceFile);

        detail.setCountAsserts(countAsserts);
        detail.setName(description.getMethodName());
        detail.setStatus(executionStatus);
        detail.setTimeMillis((int) Math.round(duration * 1000.));
        detail.setStackTrace(stackTrace);
        detail.setErrorMessage(message);
        String assemblyName = description.getAssemblyName();
        String sourcePath = sourceFile.getPath();
        UnitTestReport report = getReport(reportMap, assemblyName, sourcePath);
        report.addDetail(detail);
      }
    }
    catch (Exception discarded)
    {
      // Nothing
    }

    // We build the result.
    Set<UnitTestReport> result = new HashSet<UnitTestReport>(reportMap.values());
    return result;
  }

  /**
   * Reads or generate a test report.
   * 
   * @param map
   * @param assemblyName
   * @return
   */
  private UnitTestReport getReport(Map<String, UnitTestReport> map, String assemblyName, String testPath)
  {
    String key = "[" + assemblyName + "]" + testPath;
    UnitTestReport report = map.get(key);
    if (report == null)
    {
      report = new UnitTestReport();
      report.setAssemblyName(assemblyName);
      report.setSourceFile(new File(testPath));
      map.put(key, report);
    }
    return report;
  }

  private TestStatus computeStatus(String status, String category)
  {
    // We convert the Gallio result into 4 status
    TestStatus result = null;
    if (OUTCOME_OK.equals(status))
    {
      result = TestStatus.SUCCESS;
    }
    else if (OUTCOME_SKIPPED.equals(status))
    {
      result = TestStatus.SKIPPED;
    }
    else if (OUTCOME_FAILURE.equals(status))
    {
      if (CATEGORY_ERROR.equals(category))
      {
        result = TestStatus.ERROR;
      }
      else
      {
        result = TestStatus.FAILED;
      }
    }

    return result;
  }

}
