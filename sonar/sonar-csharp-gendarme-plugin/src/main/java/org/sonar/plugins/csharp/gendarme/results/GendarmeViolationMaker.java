/*
 * Sonar C# Plugin :: Gendarme
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

package org.sonar.plugins.csharp.gendarme.results;

import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.BatchExtension;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.Violation;
import org.sonar.plugins.csharp.api.CSharpResourcesBridge;

import com.google.common.collect.Maps;

/**
 * Creates violations based on a set of information, normally given by the {@link GendarmeResultParser}.
 */
public class GendarmeViolationMaker implements BatchExtension {

  private static final Logger LOG = LoggerFactory.getLogger(GendarmeViolationMaker.class);

  private Project project;
  private SensorContext context;
  private CSharpResourcesBridge resourcesBridge;

  private Map<Rule, String> rulesTypeMap = Maps.newHashMap();
  private static final String TYPE_METHOD = "Method";
  private static final String TYPE_ASSEMBLY = "Assembly";
  private Rule currentRule;
  private String currentDefaultViolationMessage;
  private String currentTargetName;
  private String currentTargetAssembly;
  private String currentLocation;
  private String currentSource;
  private String currentMessage;

  /**
   * Constructs a @link{GendarmeResultParser}.
   * 
   * @param project
   * @param context
   * @param rulesManager
   * @param profile
   */
  public GendarmeViolationMaker(Project project, SensorContext context, CSharpResourcesBridge resourcesBridge) {
    super();
    this.project = project;
    this.context = context;
    this.resourcesBridge = resourcesBridge;
  }

  /**
   * Creates a violation, given the elements that the class has in hands.
   * 
   * @return the created violation
   */
  public Violation createViolation() {
    Resource<?> resource = null;
    Integer line = null;
    if (StringUtils.isEmpty(currentSource)) {
      // No source information, need to detect the corresponding resource through the CSharpResourceBridge
      resource = detectResource();
      if (resource == null) {
        LOG.info("Could not find corresponding resource for the violation on {}.", currentTargetName);
      }
    } else {
      // Too easy, let's use the source information
      DefectLocation defectLocation = DefectLocation.parse(currentSource);
      resource = File.fromIOFile(new java.io.File(defectLocation.getPath()), project);
      line = defectLocation.getLineNumber();
    }
    return createViolationOnResource(resource, line);
  }

  private Resource<?> detectResource() {
    Resource<?> foundResource = project;
    if (TYPE_ASSEMBLY.equals(rulesTypeMap.get(currentRule))) {
      // Do nothing, this will be created on the project
      LOG.trace("Violation on assembly: {}", currentTargetAssembly);
    } else {
      // Type or method
      foundResource = tryDetection(currentLocation);
      if (foundResource == null) {
        foundResource = tryDetection(currentTargetName);
      }
    }
    return foundResource;
  }

  private Resource<?> tryDetection(String resourceIdentifier) {
    // For types, the currentLocation is the key
    String resourceKey = resourceIdentifier.replaceAll("/", ".");
    if (TYPE_METHOD.equals(rulesTypeMap.get(currentRule))) {
      resourceKey = StringUtils.substringBefore(resourceKey, "::");
      resourceKey = StringUtils.substringAfterLast(resourceKey, " ");
    }
    return resourcesBridge.getFromTypeName(resourceKey);
  }

  private Violation createViolationOnResource(Resource<?> resource, Integer lineNumber) {
    Violation violation = Violation.create(currentRule, resource);
    if (lineNumber != null) {
      violation.setLineId(lineNumber);
    }
    if (StringUtils.isEmpty(currentMessage)) {
      violation.setMessage(currentDefaultViolationMessage);
    } else {
      violation.setMessage(currentMessage);
    }
    violation.setSeverity(currentRule.getSeverity());
    context.saveViolation(violation);
    return violation;
  }

  protected void registerRuleType(Rule rule, String type) {
    rulesTypeMap.put(rule, type);
  }

  /**
   * @param currentRule
   *          the currentRule to set
   */
  protected void setCurrentRule(Rule currentRule) {
    this.currentRule = currentRule;
  }

  /**
   * @param currentDefaultViolationMessage
   *          the currentDefaultViolationMessage to set
   */
  protected void setCurrentDefaultViolationMessage(String currentDefaultViolationMessage) {
    this.currentDefaultViolationMessage = currentDefaultViolationMessage;
  }

  /**
   * @param currentTargetName
   *          the currentTargetName to set
   */
  protected void setCurrentTargetName(String currentTargetName) {
    this.currentTargetName = currentTargetName;
  }

  /**
   * @param currentTargetAssembly
   *          the currentTargetAssembly to set
   */
  protected void setCurrentTargetAssembly(String currentTargetAssembly) {
    this.currentTargetAssembly = currentTargetAssembly;
  }

  /**
   * @param currentLocation
   *          the currentLocation to set
   */
  protected void setCurrentLocation(String currentLocation) {
    this.currentLocation = currentLocation;
  }

  /**
   * @param currentSource
   *          the currentSource to set
   */
  protected void setCurrentSource(String currentSource) {
    this.currentSource = currentSource;
  }

  /**
   * @param currentMessage
   *          the currentMessage to set
   */
  protected void setCurrentMessage(String currentMessage) {
    this.currentMessage = currentMessage;
  }

}
