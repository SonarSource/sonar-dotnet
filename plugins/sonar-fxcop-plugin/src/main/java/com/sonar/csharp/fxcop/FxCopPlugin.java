/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;

import com.sonar.csharp.fxcop.maven.FxCopPluginHandler;
import com.sonar.csharp.fxcop.rules.FxCopRuleRepository;

/**
 */
public class FxCopPlugin implements Plugin {

  public static final String KEY = "fxcop";

  public FxCopPlugin() {
  }

  public String getDescription() {
    return "A plugin that collects the FxCop check results";
  }

  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(FxCopSensor.class);
    list.add(FxCopRuleRepository.class);
    list.add(FxCopPluginHandler.class);
    return list;
  }

  public String getKey() {
    return KEY;
  }

  public String getName() {
    return "C# FxCop";
  }

  @Override
  public String toString() {
    return getKey();
  }
}
