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
 * Created on Jul 2, 2009
 */
package org.sonar.plugin.dotnet.fxcop.xml;

import javax.xml.bind.annotation.XmlElement;

public class ProjectOptions
{
  @XmlElement(name = "SharedProject")
  private String             sharedProject               = "True";

  @XmlElement(name = "Stylesheet")
  private Stylesheet         stylesheet                  = new Stylesheet();

  @XmlElement(name = "SaveMessages")
  private SaveMessages       saveMessages                = new SaveMessages();

  @XmlElement(name = "ProjectFile")
  private ProjectFile        projectFile                 = new ProjectFile();

  @XmlElement(name = "EnableMultithreadedLoad")
  private String             enableMultithreadedLoad     = "True";

  @XmlElement(name = "EnableMultithreadedAnalysis")
  private String             enableMultithreadedAnalysis = "True";

  @XmlElement(name = "SourceLookup")
  private String             sourceLookup                = "True";

  @XmlElement(name = "AnalysisExceptionsThreshold")
  private int                analysisExceptionsThreshold = 10;

  @XmlElement(name = "RuleExceptionsThreshold")
  private int                ruleExceptionsThreshold     = 1;

  @XmlElement(name = "Spelling")
  private Spelling           spelling                    = new Spelling();

  @XmlElement(name = "OverrideRuleVisibilities")
  private String             overrideRuleVisibilities    = "False";

  @XmlElement(name = "CustomDictionaries")
  private CustomDictionaries customDictionaries          = new CustomDictionaries();

  @XmlElement(name = "SearchGlobalAssemblyCache")
  private String             searchGlobalAssemblyCache   = "False";

  @XmlElement(name = "DeadlockDetectionTimeout")
  private int                deadlockDetectionTimeout    = 120;

  @XmlElement(name = "IgnoreGeneratedCode")
  private String             ignoreGeneratedCode         = "False";

  /**
   * Constructs a @link{ProjectOptions}.
   */
  public ProjectOptions()
  {
  }

  /**
   * Returns the sharedProject.
   * 
   * @return The sharedProject to return.
   */
  public String getSharedProject()
  {
    return this.sharedProject;
  }

  /**
   * Sets the sharedProject.
   * 
   * @param sharedProject The sharedProject to set.
   */
  public void setSharedProject(String sharedProject)
  {
    this.sharedProject = sharedProject;
  }

  /**
   * Returns the stylesheet.
   * 
   * @return The stylesheet to return.
   */
  public Stylesheet getStylesheet()
  {
    return this.stylesheet;
  }

  /**
   * Sets the stylesheet.
   * 
   * @param stylesheet The stylesheet to set.
   */
  public void setStylesheet(Stylesheet stylesheet)
  {
    this.stylesheet = stylesheet;
  }

  /**
   * Returns the saveMessages.
   * 
   * @return The saveMessages to return.
   */
  public SaveMessages getSaveMessages()
  {
    return this.saveMessages;
  }

  /**
   * Sets the saveMessages.
   * 
   * @param saveMessages The saveMessages to set.
   */
  public void setSaveMessages(SaveMessages saveMessages)
  {
    this.saveMessages = saveMessages;
  }

  /**
   * Returns the projectFile.
   * 
   * @return The projectFile to return.
   */
  public ProjectFile getProjectFile()
  {
    return this.projectFile;
  }

  /**
   * Sets the projectFile.
   * 
   * @param projectFile The projectFile to set.
   */
  public void setProjectFile(ProjectFile projectFile)
  {
    this.projectFile = projectFile;
  }

  /**
   * Returns the enableMultithreadedLoad.
   * 
   * @return The enableMultithreadedLoad to return.
   */
  public String getEnableMultithreadedLoad()
  {
    return this.enableMultithreadedLoad;
  }

  /**
   * Sets the enableMultithreadedLoad.
   * 
   * @param enableMultithreadedLoad The enableMultithreadedLoad to set.
   */
  public void setEnableMultithreadedLoad(String enableMultithreadedLoad)
  {
    this.enableMultithreadedLoad = enableMultithreadedLoad;
  }

  /**
   * Returns the enableMultithreadedAnalysis.
   * 
   * @return The enableMultithreadedAnalysis to return.
   */
  public String getEnableMultithreadedAnalysis()
  {
    return this.enableMultithreadedAnalysis;
  }

  /**
   * Sets the enableMultithreadedAnalysis.
   * 
   * @param enableMultithreadedAnalysis The enableMultithreadedAnalysis to set.
   */
  public void setEnableMultithreadedAnalysis(String enableMultithreadedAnalysis)
  {
    this.enableMultithreadedAnalysis = enableMultithreadedAnalysis;
  }

  /**
   * Returns the sourceLookup.
   * 
   * @return The sourceLookup to return.
   */
  public String getSourceLookup()
  {
    return this.sourceLookup;
  }

  /**
   * Sets the sourceLookup.
   * 
   * @param sourceLookup The sourceLookup to set.
   */
  public void setSourceLookup(String sourceLookup)
  {
    this.sourceLookup = sourceLookup;
  }

  /**
   * Returns the analysisExceptionsThreshold.
   * 
   * @return The analysisExceptionsThreshold to return.
   */
  public int getAnalysisExceptionsThreshold()
  {
    return this.analysisExceptionsThreshold;
  }

  /**
   * Sets the analysisExceptionsThreshold.
   * 
   * @param analysisExceptionsThreshold The analysisExceptionsThreshold to set.
   */
  public void setAnalysisExceptionsThreshold(int analysisExceptionsThreshold)
  {
    this.analysisExceptionsThreshold = analysisExceptionsThreshold;
  }

  /**
   * Returns the ruleExceptionsThreshold.
   * 
   * @return The ruleExceptionsThreshold to return.
   */
  public int getRuleExceptionsThreshold()
  {
    return this.ruleExceptionsThreshold;
  }

  /**
   * Sets the ruleExceptionsThreshold.
   * 
   * @param ruleExceptionsThreshold The ruleExceptionsThreshold to set.
   */
  public void setRuleExceptionsThreshold(int ruleExceptionsThreshold)
  {
    this.ruleExceptionsThreshold = ruleExceptionsThreshold;
  }

  /**
   * Returns the spelling.
   * 
   * @return The spelling to return.
   */
  public Spelling getSpelling()
  {
    return this.spelling;
  }

  /**
   * Sets the spelling.
   * 
   * @param spelling The spelling to set.
   */
  public void setSpelling(Spelling spelling)
  {
    this.spelling = spelling;
  }

  /**
   * Returns the overrideRuleVisibilities.
   * 
   * @return The overrideRuleVisibilities to return.
   */
  public String getOverrideRuleVisibilities()
  {
    return this.overrideRuleVisibilities;
  }

  /**
   * Sets the overrideRuleVisibilities.
   * 
   * @param overrideRuleVisibilities The overrideRuleVisibilities to set.
   */
  public void setOverrideRuleVisibilities(String overrideRuleVisibilities)
  {
    this.overrideRuleVisibilities = overrideRuleVisibilities;
  }

  /**
   * Returns the customDictionaries.
   * 
   * @return The customDictionaries to return.
   */
  public CustomDictionaries getCustomDictionaries()
  {
    return this.customDictionaries;
  }

  /**
   * Sets the customDictionaries.
   * 
   * @param customDictionaries The customDictionaries to set.
   */
  public void setCustomDictionaries(CustomDictionaries customDictionaries)
  {
    this.customDictionaries = customDictionaries;
  }

  /**
   * Returns the searchGlobalAssemblyCache.
   * 
   * @return The searchGlobalAssemblyCache to return.
   */
  public String getSearchGlobalAssemblyCache()
  {
    return this.searchGlobalAssemblyCache;
  }

  /**
   * Sets the searchGlobalAssemblyCache.
   * 
   * @param searchGlobalAssemblyCache The searchGlobalAssemblyCache to set.
   */
  public void setSearchGlobalAssemblyCache(String searchGlobalAssemblyCache)
  {
    this.searchGlobalAssemblyCache = searchGlobalAssemblyCache;
  }

  /**
   * Returns the deadlockDetectionTimeout.
   * 
   * @return The deadlockDetectionTimeout to return.
   */
  public int getDeadlockDetectionTimeout()
  {
    return this.deadlockDetectionTimeout;
  }

  /**
   * Sets the deadlockDetectionTimeout.
   * 
   * @param deadlockDetectionTimeout The deadlockDetectionTimeout to set.
   */
  public void setDeadlockDetectionTimeout(int deadlockDetectionTimeout)
  {
    this.deadlockDetectionTimeout = deadlockDetectionTimeout;
  }

  /**
   * Returns the ignoreGeneratedCode.
   * 
   * @return The ignoreGeneratedCode to return.
   */
  public String getIgnoreGeneratedCode()
  {
    return this.ignoreGeneratedCode;
  }

  /**
   * Sets the ignoreGeneratedCode.
   * 
   * @param ignoreGeneratedCode The ignoreGeneratedCode to set.
   */
  public void setIgnoreGeneratedCode(String ignoreGeneratedCode)
  {
    this.ignoreGeneratedCode = ignoreGeneratedCode;
  }
}
