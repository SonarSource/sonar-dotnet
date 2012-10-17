/*
 * Sonar .NET Plugin :: Core
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
package org.sonar.plugins.dotnet.api;

import com.google.common.base.Joiner;
import com.google.common.collect.Lists;
import com.google.common.collect.Maps;
import org.apache.commons.configuration.Configuration;
import org.apache.commons.lang.StringUtils;
import org.sonar.api.BatchExtension;
import org.sonar.api.ServerExtension;
import org.sonar.api.batch.InstantiationStrategy;
import org.sonar.api.config.Settings;
import org.sonar.api.utils.Logs;

import java.util.Arrays;
import java.util.Collection;
import java.util.Map;

/**
 * Class that reads configuration related to all the .NET plugins. It can be injected via the constructor.<br/>
 * <br/>
 * <b>Important</b>: It should be used over the original {@link Settings} as it takes care to maintain backward compatibility with the
 * previous .NET plugin parameter names.
 */
@InstantiationStrategy(InstantiationStrategy.PER_BATCH)
public class DotNetConfiguration implements BatchExtension, ServerExtension {

  private Settings settings;

  private Map<String, Object> newToPreviousParamMap = Maps.newHashMap();

  /**
   * Creates a new {@link DotNetConfiguration} object that will use the inner {@link Configuration} object to retrieve the required key
   * values, taking into account the name of the previous .NET plugin parameters.
   * 
   * @param configuration
   *          the configuration
   */
  public DotNetConfiguration(Settings settings) {
    this.settings = settings;

    // Core OLD/Maven parameters
    newToPreviousParamMap.put(DotNetConstants.TEST_PROJECT_PATTERN_KEY, Lists.newArrayList("visual.test.project.pattern", "sonar.donet.visualstudio.testProjectPattern"));
    newToPreviousParamMap.put(DotNetConstants.SOLUTION_FILE_KEY, "visual.studio.solution");
    newToPreviousParamMap.put(DotNetConstants.BUILD_CONFIGURATION_KEY, Lists.newArrayList("msbuild.configurations", "sonar.dotnet.buildConfigurations"));
    newToPreviousParamMap.put(DotNetConstants.DOTNET_2_0_SDK_DIR_KEY, DotNetConstants.MVN_DOTNET_2_0_SDK_DIR_KEY);
    newToPreviousParamMap.put(DotNetConstants.DOTNET_3_5_SDK_DIR_KEY, DotNetConstants.MVN_DOTNET_3_5_SDK_DIR_KEY);
    newToPreviousParamMap.put(DotNetConstants.DOTNET_4_0_SDK_DIR_KEY, DotNetConstants.MVN_DOTNET_4_0_SDK_DIR_KEY);
    newToPreviousParamMap.put(DotNetConstants.SILVERLIGHT_3_MSCORLIB_LOCATION_KEY, DotNetConstants.MVN_SILVERLIGHT_3_MSCORLIB_LOCATION_KEY);
    newToPreviousParamMap.put(DotNetConstants.SILVERLIGHT_4_MSCORLIB_LOCATION_KEY, DotNetConstants.MVN_SILVERLIGHT_4_MSCORLIB_LOCATION_KEY);
    newToPreviousParamMap.put(DotNetConstants.DOTNET_VERSION_KEY, DotNetConstants.MVN_DOTNET_VERSION_KEY);
    newToPreviousParamMap.put(DotNetConstants.SILVERLIGHT_VERSION_KEY, DotNetConstants.MVN_SILVERLIGHT_VERSION_KEY);

    // FxCop OLD parameters
    newToPreviousParamMap.put("sonar.fxcop.installDirectory", "fxcop.directory");
    newToPreviousParamMap.put("sonar.fxcop.mode", "sonar.dotnet.fxcop");
    newToPreviousParamMap.put("sonar.fxcop.reports.path", "sonar.dotnet.fxcop.reportsPath");
    newToPreviousParamMap.put("sonar.fxcop.assemblyDependencyDirectories", "fxcop.additionalDirectories");
    newToPreviousParamMap.put("sonar.fxcop.ignoreGeneratedCode", "fxcop.ignore.generated.code");
    newToPreviousParamMap.put("sonar.dotnet.assemblies", "sonar.fxcop.assemblies");

    // Gendarme OLD parameters
    newToPreviousParamMap.put("sonar.gendarme.installDirectory", "gendarme.directory");
    newToPreviousParamMap.put("sonar.gendarme.mode", "sonar.dotnet.gendarme");
    newToPreviousParamMap.put("sonar.gendarme.reports.path", "sonar.dotnet.gendarme.reportPath");
    newToPreviousParamMap.put("sonar.gendarme.confidence", "gendarme.confidence");

    // StyleCop OLD parameters
    newToPreviousParamMap.put("sonar.stylecop.installDirectory", "stylecop.directory");
    newToPreviousParamMap.put("sonar.stylecop.mode", "sonar.dotnet.stylecop");
    newToPreviousParamMap.put("sonar.stylecop.reports.path", "sonar.dotnet.stylecop.reportPath");

    // Gallio OLD parameters
    newToPreviousParamMap.put("sonar.gallio.installDirectory", "gallio.directory");
    newToPreviousParamMap.put("sonar.gallio.mode", "sonar.dotnet.gallio");
    newToPreviousParamMap.put("sonar.gallio.reports.path", "sonar.dotnet.gallio.reportPath");
    newToPreviousParamMap.put("sonar.gallio.filter", "gallio.filter");

    // PartCover OLD parameters
    newToPreviousParamMap.put("sonar.partcover.installDirectory", "partcover.directory");

  }

  /**
   * Sets a property on the underlying {@link Settings} object
   */
  public void setProperty(String key, String value) {
    settings.setProperty(key, value);
  }

  /**
   * Sets a string-array property on the underlying {@link Settings} object
   */
  public void setProperty(String key, String[] values) {
    settings.setProperty(key, Joiner.on(',').join(values));
  }

  /**
   * @see Settings#getString(String)
   */
  public String getString(String key) {
    // look if this key existed before
    Object rawPreviousKeys = newToPreviousParamMap.get(key);
    if (rawPreviousKeys instanceof String) {
      String conf = getConfig((String) rawPreviousKeys);
      if (StringUtils.isNotBlank(conf)) {
        return conf;
      }
    } else if (rawPreviousKeys instanceof Collection<?>) {
      // should be a collection
      Collection<String> prevousKeys = (Collection<String>) rawPreviousKeys;
      for (String previousKey : prevousKeys) {
        String conf = getConfig((String) previousKey);
        if (StringUtils.isNotBlank(conf)) {
          return conf;
        }
      }
    }
    // if this key wasn't used before, or if no value for was for it, use the value of the current key
    return settings.getString(key);
  }

  private String getConfig(String previousKey) {
    if (StringUtils.isNotBlank(previousKey)) {
      String result = settings.getString(previousKey);
      if (StringUtils.isNotBlank(result)) {
        // a former parameter has been specified, let's take this value
        logInfo(result, previousKey);
        return result;
      }
    }
    return null;
  }

  /**
   * @see Settings#getStringArray(String)
   */
  public String[] getStringArray(String key) {
    // look if this key existed before
    String previousKey = (String) newToPreviousParamMap.get(key);
    final String[] resultArray;
    if (StringUtils.isBlank(previousKey)) {
      // if this key wasn't used before, or if no value for was for it,
      // use the value of the current key
      String[] result = settings.getStringArray(key);
      if (result.length == 0) {
        resultArray = result;
      } else {
        // in the previous .NET plugin, parameters used to be split with a semi-colon
        resultArray = splitUsingSemiColon(result);
      }
    } else {
      String[] result = settings.getStringArray(previousKey);
      if (result.length == 0) {
        result = settings.getStringArray(key);
        resultArray = splitUsingSemiColon(result);
      } else {
        // a former parameter has been specified, let's take this value
        logInfo(result, previousKey);
        // in the previous .NET plugin, parameters used to be split with a semi-colon
        resultArray = splitUsingSemiColon(result);
      }
    }

    return resultArray;
  }

  public String[] getStringArray(String key, String defaultValue) {
    String[] result = getStringArray(key);
    if (result.length == 0) {
      if (StringUtils.isEmpty(defaultValue)) {
        result = new String[] {};
      } else {
        result = new String[] {defaultValue};
      }

    }
    return result;
  }

  private String[] splitUsingSemiColon(String[] strings) {
    Collection<String> resultCollection = Lists.newArrayList();
    for (int i = 0; i < strings.length; i++) {
      resultCollection.addAll(Arrays.asList(StringUtils.split(strings[i], ';')));
    }
    return resultCollection.toArray(new String[resultCollection.size()]);
  }

  /**
   * @see Settings#getBoolean(String, Boolean)
   */
  public boolean getBoolean(String key) {
    boolean result = false;
    // look if this key existed before
    String previousKey = (String) newToPreviousParamMap.get(key);
    if (StringUtils.isNotBlank(previousKey) && settings.hasKey(previousKey)) {
      result = settings.getBoolean(previousKey);
      // a former parameter has been specified, let's take this value
      logInfo(result, previousKey);
      return result;
    }
    // if this key wasn't used before, or if no value for was for it, use the value of the current key
    return settings.getBoolean(key);
  }

  /**
   * @see Settings#getInteger(String, Integer)
   */
  public int getInt(String key) {
    int result = -1;
    // look if this key existed before
    String previousKey = (String) newToPreviousParamMap.get(key);
    if (StringUtils.isNotBlank(previousKey) && settings.hasKey(previousKey)) {
      result = settings.getInt(previousKey);
      // a former parameter has been specified, let's take this value
      logInfo(result, previousKey);
      return result;
    }
    // if this key wasn't used before, or if no value for was for it, use the value of the current key
    return settings.getInt(key);
  }

  protected void logInfo(Object result, String previousKey) {
    Logs.INFO.info("The old .NET parameter '{}' has been found and will be used. Its value: '{}'", previousKey, result);
  }

}
