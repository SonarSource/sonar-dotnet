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
 * Created on Jul 16, 2009
 */
package org.sonar.plugin.dotnet.core;

import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

import org.apache.commons.io.IOUtils;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.AbstractRulesRepository;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ConfigurationImportable;
import org.sonar.api.rules.Rule;

/**
 * A Base class for .Net rule repositories.
 * 
 * @author Jose CHILLAN Jul 16, 2009
 */
public abstract class AbstractDotNetRuleRepository
  extends AbstractRulesRepository<CSharp, DefaultRuleMapper>
  implements ConfigurationImportable
{
  /**
   * Constructs a @link{AbstractDotNetRuleRepository}.
   */
  public AbstractDotNetRuleRepository()
  {
    super(CSharp.INSTANCE, new DefaultRuleMapper());
  }
  
  /**
   * A map a of profiles to import, The profile name as key, and the xml profile file name in the classpath
   * 
   * @return
   */
  public abstract Map<String, String> getBuiltInProfiles();

  /**
   * Gets all the provided profiles.
   * @return a list of profiles
   */
  public List<RulesProfile> getProvidedProfiles()
  {
    List<RulesProfile> profiles = new ArrayList<RulesProfile>();

    Map<String, String> defaultProfiles = new TreeMap<String, String>(getBuiltInProfiles());
    for (Map.Entry<String, String> entry : defaultProfiles.entrySet())
    {
      RulesProfile providedProfile = loadProvidedProfile(entry.getKey(), getCheckResourcesBase() + entry.getValue());
      profiles.add(providedProfile);
    }
    return profiles;
  }

  /**
   * Loads a provided profile.
   * @param name the profile name
   * @param filePath the path of the file containins the profile
   * @return a provided profile.
   */
  public RulesProfile loadProvidedProfile(String name, String filePath)
  {
    try
    {
      InputStream profileIn = getClass().getResourceAsStream(filePath);
      if (profileIn == null)
      {
        throw new IOException("Resource " + profileIn + " not found");
      }
      RulesProfile profile = new RulesProfile(name, CSharp.KEY);
      List<Rule> initialReferential = getInitialReferential();
      List<ActiveRule> configuration = importConfiguration(IOUtils.toString(profileIn, "UTF-8"), initialReferential);
      profile.setActiveRules(configuration);
      return profile;

    }
    catch (IOException e)
    {
      throw new RuntimeException("Configuration file not found for the profile : " + name, e);
    }
  }
}
