package org.sonar.plugin.dotnet.gendarme;

import java.util.List;

import org.sonar.api.BatchExtension;
import org.sonar.api.ServerExtension;
import org.sonar.api.rules.ActiveRule;

/**
 * Implementations of this interfaces have the 
 * responsibility of parsing gendarme xml result reports
 * 
 * @author Alexandre Victoor
 *
 */
public interface GendarmeRuleMarshaller extends ServerExtension, BatchExtension {

  public String marshall(List<ActiveRule> rules);
	
}
