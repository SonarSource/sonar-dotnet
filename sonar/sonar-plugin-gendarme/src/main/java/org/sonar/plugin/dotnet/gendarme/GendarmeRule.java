/*
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

package org.sonar.plugin.dotnet.gendarme;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.rules.RulePriority;

public class GendarmeRule {

	private RulePriority priority = RulePriority.MAJOR;
	
	private String id;
	
	private List<RuleProperty> properties = new ArrayList<RuleProperty>();
	
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

	public List<RuleProperty> getProperties() {
    return properties;
  }

  public void setProperties(List<RuleProperty> properties) {
    this.properties = properties;
  }
  
  public void addProperty(RuleProperty property) {
    properties.add(property);
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
