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

import java.io.File;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

import javax.xml.namespace.QName;
import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;

import org.apache.commons.lang.StringUtils;
import org.codehaus.staxmate.SMInputFactory;
import org.codehaus.staxmate.in.SMEvent;
import org.codehaus.staxmate.in.SMHierarchicCursor;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.common.collect.ArrayListMultimap;
import com.google.common.collect.Multimap;

/**
 * Stax implementation of the Gallio result report parser
 * 
 * @author Maxime SCHNEIDER-DUFEUTRELLE
 *
 */
public class GallioResultStaxParser implements GallioResultParser {

  private static final String GALLIO_REPORT_PARSING_ERROR = "gallio report parsing error";
  private static final String GALLIO_URI = "http://www.gallio.org/";
  private static final String ASSEMBLY = "assembly";
  private static final String NAMESPACE = "namespace";
  private static final String TYPE = "type";
  private static final String MEMBER = "member";
  private static final String PATH = "path";
  private static final String LINE = "line";
  /**
   * Outcomes for the test
   */
  private final static String OUTCOME_OK = "passed";
  private final static String OUTCOME_FAILURE = "failed";
  private final static String OUTCOME_SKIPPED = "skipped";

  /**
   * Categories for the test
   */
  private final static String CATEGORY_ERROR = "error";
  private final static Logger log = LoggerFactory.getLogger(GallioResultStaxParser.class);


  private Map<String, TestCaseDetail> testCaseDetailsByTestIds;



  public Set<UnitTestReport> parse(File report) {
    Set<UnitTestReport> reports = new HashSet<UnitTestReport>();
    testCaseDetailsByTestIds = new HashMap<String, TestCaseDetail>();
    try {
      SMInputFactory inf = new SMInputFactory(XMLInputFactory.newInstance());
      SMHierarchicCursor rootCursor = inf.rootElementCursor(report);
      rootCursor.advance();
      log.debug("rootCursor is at : {}", rootCursor.getLocalName());

      // We first get the tests ids and put them in a map to get the details later
      Map<String, TestDescription> testsDetails = new HashMap<String, TestDescription>();

      QName testModelTag = new QName(GALLIO_URI, "testModel");
      SMInputCursor testModelCursor = rootCursor.descendantElementCursor(testModelTag);
      testModelCursor.advance();
      log.debug("TestModelCursor initialized at : {}", testModelCursor.getLocalName());
      testsDetails = recursiveParseTestsIds(testModelCursor, testsDetails, null, null);
      testModelCursor.getStreamReader().closeCompletely();

      // Then we get tests results
      // Due to problems with nested cursors (http://jira.codehaus.org/browse/STAXMATE-40) in staxmate,
      // we create an other root cursor and parse a second time
      SMHierarchicCursor secondRootCursor = inf.rootElementCursor(report);
      secondRootCursor.advance();
      QName testPackageRunTag = new QName(GALLIO_URI, "testPackageRun");
      SMInputCursor testPackageRunCursor = secondRootCursor.descendantElementCursor(testPackageRunTag);
      testPackageRunCursor.advance();
      String testId = "";
      recursiveParseTestsResults(testPackageRunCursor, testId);

      //Finally, we fill the reports
      reports = createUnitTestsReport(testsDetails);
      rootCursor.getStreamReader().closeCompletely();
      log.debug("Parsing ended");


    }catch(XMLStreamException e){
      log.error(GALLIO_REPORT_PARSING_ERROR,e);
    }
    return reports;
  }

  private Map<String, TestDescription> recursiveParseTestsIds(SMInputCursor rootCursor, 
      Map<String, TestDescription> testDetails, File sourceFile, String parentAssemblyName){
    try {
      QName testTag = new QName(GALLIO_URI, "test");
      if(!rootCursor.asEvent().isEndElement()){
        // Get all the tests
        SMInputCursor currentTest = rootCursor.descendantElementCursor(testTag);
        String eltName;
        while (null != currentTest.getNext()) {
          TestDescription testDescription = new TestDescription();
          String id = currentTest.getAttrValue("id");
          String name = currentTest.getAttrValue("name");
          String testCase = currentTest.getAttrValue("isTestCase");
          log.debug("Id : {} & isTestCase : {}", id, testCase);
          boolean isTestCase = false;
          if("true".equals(testCase)){
            isTestCase = true;
          }
          else{
            isTestCase = false;
          }
          // We analyse all the tests tags to get usefull informations if the test is a TestCase,
          // and to get their children
          SMInputCursor currentTestChildren = currentTest.descendantElementCursor();
          while (null != currentTestChildren.getNext()){
            eltName = currentTestChildren.getLocalName();
            if(isTestCase){
              testDescription.setMethodName(name);
              String attributeValue = null;
              if("codeReference".equals(eltName)){
                if(null != currentTestChildren.getAttrValue(ASSEMBLY)){
                  attributeValue = currentTestChildren.getAttrValue(ASSEMBLY);
                  log.debug("--{} : {}", ASSEMBLY, attributeValue);
                  testDescription.setAssemblyName(StringUtils.substringBefore(attributeValue, ","));
                  parentAssemblyName = testDescription.getAssemblyName();
                }
                else{
                  //Get the precedent assemblyName if not filled
                  testDescription.setAssemblyName(parentAssemblyName);
                }
                if(null != currentTestChildren.getAttrValue(NAMESPACE)){
                  attributeValue = currentTestChildren.getAttrValue(NAMESPACE);
                  log.debug("--{} : {}", NAMESPACE, attributeValue);
                  testDescription.setNamespace(attributeValue);
                }
                if(null != currentTestChildren.getAttrValue(TYPE)){
                  attributeValue = currentTestChildren.getAttrValue(TYPE);
                  log.debug("--{} : {}", TYPE, attributeValue);
                  testDescription.setClassName(attributeValue);
                }
                if(null != currentTestChildren.getAttrValue(MEMBER)){
                  attributeValue = currentTestChildren.getAttrValue(MEMBER);
                  log.debug("--{} : {}", MEMBER, attributeValue);
                  testDescription.setMethodName(attributeValue);
                }
              }
              currentTestChildren.getNext();
              if("codeLocation".equals(eltName)){
                if(null != currentTestChildren.getAttrValue(PATH)){
                  attributeValue = currentTestChildren.getAttrValue(PATH);
                  log.debug("--{} : {}", PATH, attributeValue);
                  File currentSourceFile = new File(attributeValue);
                  testDescription.setSourceFile(currentSourceFile);
                  sourceFile = currentSourceFile;
                }
                if(null != currentTestChildren.getAttrValue(LINE)){
                  attributeValue = currentTestChildren.getAttrValue(LINE);
                  log.debug("--{} : {}", LINE, attributeValue);
                  int lineNumber = (int) Double.parseDouble(attributeValue);
                  testDescription.setLine(lineNumber);
                }
              }
              if(null == testDescription.getSourceFile()){
                testDescription.setSourceFile(sourceFile);
              }
              testDetails.put(id, testDescription);
            }
            if("codeLocation".equals(eltName) && null != currentTestChildren.getAttrValue(PATH)){
              File currentSourceFile = new File(currentTestChildren.getAttrValue(PATH));
              if(currentSourceFile != null){
                sourceFile = currentSourceFile;
              }								
            }
            if("children".equals(eltName)){
              recursiveParseTestsIds(currentTestChildren, testDetails, sourceFile, parentAssemblyName);
            }
            currentTestChildren.advance();
          }
        }
      }
    }catch(XMLStreamException e){
      log.error(GALLIO_REPORT_PARSING_ERROR,e);
    }
    return testDetails;
  }

  private void recursiveParseTestsResults(SMInputCursor rootCursor, String testId){
    try {

      QName testStepRunTag = new QName(GALLIO_URI, "testStepRun");
      SMInputCursor currentTestStepRun = rootCursor.descendantElementCursor(testStepRunTag);
      String eltName = "";
      while (null != currentTestStepRun.getNext()) {
        // currentTestTags represents the different tests
        SMInputCursor currentTestTags = currentTestStepRun.descendantElementCursor();
        currentTestTags.getNext();
        eltName = currentTestTags.getLocalName();
        if("testStep".equals(eltName)){
          if("true".equals(currentTestTags.getAttrValue("isTestCase"))){
            if(null != currentTestTags.getAttrValue("testId")){
              testId = currentTestTags.getAttrValue("testId");
              log.debug("--testId : {}", testId);
              log.debug("--isTestCase : {}", currentTestTags.getAttrValue("isTestCase"));
              currentTestTags.getNext();
            }
            while (null != currentTestTags.getNext()) {
              TestCaseDetail testCaseDetail = parsingTags(currentTestTags, testId);
              if(null != testCaseDetail) {
                testCaseDetailsByTestIds.put(testId, testCaseDetail);
              }
            }
          }
          else{
            testId = currentTestTags.getAttrValue("testId");
            while (null != currentTestTags.getNext()) {
              parseChildren(testId, currentTestTags);
            }
          }
        }
        currentTestTags.advance();
      }
    }catch(XMLStreamException e){
      log.error(GALLIO_REPORT_PARSING_ERROR,e);
    }
  }

  private void parseChildren(String testId, SMInputCursor currentTestTags){
    try{
      if("children".equals(currentTestTags.getLocalName())){
        recursiveParseTestsResults(currentTestTags, testId);
        currentTestTags.getNext();
      }
    }catch(XMLStreamException e){
      log.error(GALLIO_REPORT_PARSING_ERROR,e);
    }
  }

  private TestCaseDetail parsingTags(SMInputCursor currentTestTags, String testId){

    try{
      parseChildren(testId, currentTestTags);
      TestCaseDetail detail = new TestCaseDetail();
      if("result".equals(currentTestTags.getLocalName())){
        log.debug("Result for test : {}", testId);

        String assertCount = currentTestTags.getAttrValue("assertCount");
        log.debug("---assertCount : {}", assertCount);
        detail.setCountAsserts((int) Double.parseDouble(assertCount));

        String duration = currentTestTags.getAttrValue("duration");
        log.debug("---duration : {}", currentTestTags.getAttrValue("duration"));
        detail.setTimeMillis((int) Math.round(Double.parseDouble(duration) * 1000.));

        SMInputCursor currentTestOutcomeResultCursor = currentTestTags
        .descendantElementCursor().advance();
        String status = currentTestOutcomeResultCursor.getAttrValue("status");
        String category = null;
        if(null != currentTestOutcomeResultCursor.getAttrValue("category")){
          category = currentTestOutcomeResultCursor.getAttrValue("category");
        }

        log.debug("---status : {}", currentTestOutcomeResultCursor.getAttrValue("status"));
        TestStatus executionStatus = computeStatus(status, category);
        currentTestTags.getNext();
        detail.setStatus(executionStatus);
        if ((executionStatus == TestStatus.FAILED) || 
            (executionStatus == TestStatus.ERROR)) {
          detail = getMessages(currentTestTags, detail);
        }
        return detail;
      }
    }catch(XMLStreamException e){
      log.error(GALLIO_REPORT_PARSING_ERROR,e);
    }
    return null;
  }

  private TestStatus computeStatus(String status, String category) {
    // We convert the Gallio result into 4 status
    TestStatus result = null;
    if (OUTCOME_OK.equals(status)) {
      result = TestStatus.SUCCESS;
    } else if (OUTCOME_SKIPPED.equals(status)) {
      result = TestStatus.SKIPPED;
    } else if (OUTCOME_FAILURE.equals(status)) {
      if (CATEGORY_ERROR.equals(category)) {
        result = TestStatus.ERROR;
      } else {
        result = TestStatus.FAILED;
      }
    }
    return result;
  }

  private TestCaseDetail getMessages(SMInputCursor currentTestTags, TestCaseDetail detail){
    try{
      if("testLog".equals(currentTestTags.getLocalName())){
        SMInputCursor currentTestLogStreamsTags = currentTestTags.descendantElementCursor();
        SMEvent streamsTag = currentTestLogStreamsTags.getNext();
        if(null != streamsTag){
          log.debug("----streams Tag found : {}", currentTestLogStreamsTags.getLocalName());
          if(streamsTag.getEventCode() == SMEvent.START_ELEMENT.getEventCode()){
            log.debug("----Cursor is at <streams> Tag ");
            SMInputCursor currentTestLogStreamTags = currentTestLogStreamsTags
            .descendantElementCursor();
            parseStreams(detail, currentTestLogStreamTags);
          }
        }
      }
    }catch(XMLStreamException e){
      log.error(GALLIO_REPORT_PARSING_ERROR,e);
    }
    return detail;
  }

  private void parseStreams(TestCaseDetail detail,
      SMInputCursor currentTestLogStreamTags) {
    try{
      while (null != currentTestLogStreamTags.getNext()) {
        log.debug("----Cursor is at <stream> Tag ");
        String streamName = currentTestLogStreamTags.getAttrValue("name");
        log.debug("----stream name : {}", streamName);
        SMInputCursor currentTestLogStreamSectionsTags = currentTestLogStreamTags
        .descendantElementCursor().advance()
        .descendantElementCursor().advance()
        .descendantElementCursor();
        while (null != currentTestLogStreamSectionsTags.getNext()) {
          String sectionName = currentTestLogStreamSectionsTags.getAttrValue("name");
          log.debug("----section name : {}", sectionName);
          SMInputCursor sectionContentsChild = currentTestLogStreamSectionsTags
          .descendantElementCursor().advance()
          .descendantElementCursor().advance();
          if("text".equals(sectionContentsChild.getLocalName())){
            String message = sectionContentsChild.collectDescendantText();
            log.debug("Error Message is : {}", message);
            detail.setErrorMessage(message);
          }
          else if("marker".equals(sectionContentsChild.getLocalName())) {
            log.debug("-------Marker found ! ");
            if("StackTrace".equals(sectionContentsChild.getAttrValue("class"))){
              SMInputCursor sectionMarkerTextContent = sectionContentsChild
              .descendantElementCursor().advance()
              .descendantElementCursor().advance();
              String stackTrace = sectionMarkerTextContent.collectDescendantText();
              log.debug("StackTrace is : {}", stackTrace);
              detail.setStackTrace(stackTrace);
            }
          }
        }
      }
    }catch(XMLStreamException e){
      log.error(GALLIO_REPORT_PARSING_ERROR,e);
    }
  }

  private Set<UnitTestReport> createUnitTestsReport(Map<String, TestDescription> testsDescriptionByTestIds){

    Set<UnitTestReport> result = new HashSet<UnitTestReport>();
    Set<String> testIds = testCaseDetailsByTestIds.keySet();
    //We associate the descriptions with the test details
    for (String testId : testIds) {
      TestDescription description = testsDescriptionByTestIds.get(testId);
      TestCaseDetail testCaseDetail = testCaseDetailsByTestIds.get(testId);
      testCaseDetail.merge(description);
      testCaseDetailsByTestIds.put(testId, testCaseDetail);
    }

    Collection<TestCaseDetail> testCases = testCaseDetailsByTestIds.values();
    Multimap<String, TestCaseDetail> testCaseDetailsBySrcKey = ArrayListMultimap.create();
    for (TestCaseDetail testCaseDetail : testCases) {
      String sourceKey = testCaseDetail.createSourceKey();
      testCaseDetailsBySrcKey.put(sourceKey, testCaseDetail);
    }

    Map<String, UnitTestReport> unitTestsReports = new HashMap<String, UnitTestReport>();
    log.debug("testCaseDetails size : {}", String.valueOf(testCaseDetailsByTestIds.size()));

    Set<String> pathKeys = testCaseDetailsBySrcKey.keySet();
    log.debug("There are {} different pathKeys",String.valueOf(pathKeys.size()));
    for (String pathKey : pathKeys) {
      //If the Key already exists in the map, we add the details
      if(unitTestsReports.containsKey(pathKey)){
        UnitTestReport unitTest = unitTestsReports.get(pathKey);
        for (TestCaseDetail testDetail : testCaseDetailsBySrcKey.get(pathKey)) {
          log.debug("Adding testDetail {} to the unitTestReport", testDetail.getName());
          unitTest.addDetail(testDetail);
        }
        unitTestsReports.put(pathKey, unitTest);
      }
      else{
        //Else we create a new report
        UnitTestReport unitTest = new UnitTestReport();
        unitTest.setAssemblyName(testCaseDetailsBySrcKey.get(pathKey).iterator().next().getAssemblyName());
        unitTest.setSourceFile(testCaseDetailsBySrcKey.get(pathKey).iterator().next().getSourceFile());
        log.debug("Create new unitTest for path : {}", unitTest.getSourceFile().getPath());
        for (TestCaseDetail testDetail : testCaseDetailsBySrcKey.get(pathKey)) {
          log.debug("+ and add details : {}", testDetail.getName());
          unitTest.addDetail(testDetail);
        }
        unitTestsReports.put(pathKey, unitTest);
      }
    }
    result.addAll(unitTestsReports.values());
    log.debug("UnitTestReports contains " + unitTestsReports.size() + " report(s)");
    log.debug("The result Set contains " + result.size() + " report(s)");

    return result;
  }
}