package org.sonar.plugin.dotnet.gendarme;

import org.sonar.api.rules.RulePriority;

public class GendarmeRule {

	private RulePriority priority = RulePriority.MAJOR;
	
	public RulePriority getPriority() {
  	return priority;
  }

	public void setPriority(RulePriority priority) {
  	this.priority = priority;
  }

	public String getId() {
  	return id;
  }

	public void setId(String id) {
  	this.id = id;
  }

	private String id;
	
}
