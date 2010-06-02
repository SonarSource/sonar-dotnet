package org.sonar.plugin.dotnet.gendarme;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Plugin;
public class GendarmePlugin implements Plugin
{
  public static final String KEY = "gendarme";

  public String getDescription()
  {
    return "A plugin that collects the Gendarme check results";
  }

  public List<Class<? extends Extension>> getExtensions()
  {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(GendarmeRuleParserImpl.class);
    list.add(GendarmeRuleMarshallerImpl.class);
    list.add(GendarmeRuleRepository.class);
    list.add(GendarmeSensor.class);
    list.add(GendarmePluginHandler.class);
    return list;
  }

  public String getKey()
  {
    return KEY;
  }

  public String getName()
  {
    return "Gendarme Plugin";
  }

  @Override
  public String toString()
  {
    return getKey();
  }
}