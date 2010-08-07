package org.sonar.plugin.dotnet.gendarme;

import org.sonar.api.rules.RulePriority;

public class GendarmeRule {

	private RulePriority priority = RulePriority.MAJOR;
	
	private String id;
	
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

	@Override
  public int hashCode() {
	  final int prime = 31;
	  int result = 1;
	  result = prime * result + ((id == null) ? 0 : id.hashCode());
	  return result;
  }

	@Override
  public boolean equals(Object obj) {
	  if (this == obj)
		  return true;
	  if (obj == null)
		  return false;
	  if (getClass() != obj.getClass())
		  return false;
	  GendarmeRule other = (GendarmeRule) obj;
	  if (id == null) {
		  if (other.id != null)
			  return false;
	  } else if (!id.equals(other.id))
		  return false;
	  return true;
  }

	
	
}
