/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.rules;

/**
 * Definition of a FXCop rule.
 */
public class FxCopRule {

  private String category;
  private String name;
  private boolean enabled;
  private String fileName;
  private String priority;

  /**
   * Constructs a @link{FxCopRule}.
   */
  public FxCopRule() {
  }

  /**
   * Returns the name.
   * 
   * @return The name to return.
   */
  public String getName() {
    return this.name;
  }

  /**
   * Sets the name.
   * 
   * @param name
   *          The name to set.
   */
  public void setName(String name) {
    this.name = name;
  }

  /**
   * Returns the enabled.
   * 
   * @return The enabled to return.
   */
  public boolean isEnabled() {
    return this.enabled;
  }

  /**
   * Sets the enabled.
   * 
   * @param enabled
   *          The enabled to set.
   */
  public void setEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  @Override
  public String toString() {
    return "FxCopRule(name=" + name + ", enabled=" + enabled + ", category=" + category + ")";
  }

  /**
   * Returns the category.
   * 
   * @return The category to return.
   */
  public String getCategory() {
    return this.category;
  }

  /**
   * Sets the category.
   * 
   * @param category
   *          The category to set.
   */
  public void setCategory(String category) {
    this.category = category;
  }

  /**
   * Returns the fileName.
   * 
   * @return The fileName to return.
   */
  public String getFileName() {
    return this.fileName;
  }

  /**
   * Sets the fileName.
   * 
   * @param fileName
   *          The fileName to set.
   */
  public void setFileName(String fileName) {
    this.fileName = fileName;
  }

  /**
   * Get the sonar priority of this rule
   * 
   * @return the sonar priority
   */
  public String getPriority() {
    return priority;
  }

  /**
   * Set the sonar priority of this rule
   * 
   * @param priority
   *          sonar priority
   */
  public void setPriority(String priority) {
    this.priority = priority;
  }
}
