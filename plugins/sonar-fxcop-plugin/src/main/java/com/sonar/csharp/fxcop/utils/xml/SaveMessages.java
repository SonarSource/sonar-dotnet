/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

/*
 * Created on Jul 2, 2009
 */
package com.sonar.csharp.fxcop.utils.xml;

import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

/**
 * SaveMessages option for FxCop
 */
@XmlType(name = "SaveMessages")
public class SaveMessages {

  @XmlElement(name = "Project")
  private Entity project;
  @XmlElement(name = "Report")
  private Entity report;

  /**
   * Returns the project.
   * 
   * @return The project to return.
   */
  public Entity getProject() {
    return this.project;
  }

  /**
   * Sets the project.
   * 
   * @param project
   *          The project to set.
   */
  public void setProject(Entity project) {
    this.project = project;
  }

  /**
   * Returns the report.
   * 
   * @return The report to return.
   */
  public Entity getReport() {
    return this.report;
  }

  /**
   * Sets the report.
   * 
   * @param report
   *          The report to set.
   */
  public void setReport(Entity report) {
    this.report = report;
  }

  /**
   * Constructs the option;
   */
  public SaveMessages() {
    project = new Entity();
    report = new Entity();
    project.setStatus("Active, Excluded");
  }
}
