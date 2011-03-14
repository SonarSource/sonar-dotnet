/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.gendarme.model;

/**
 * This class was made to modelize issues provided by gendarme reports
 * 
 * @author Maxime SCHNEIDER-DUFEUTRELLE January 31, 2011
 */
public class Issue {

  public Issue(String name, String problem, String solution, String assembly) {
    super();
    this.name = name;
    this.problem = problem;
    this.solution = solution;
    this.assembly = assembly;
  }

  private String name;
  private String problem;
  private String solution;
  private String assembly;
  private String location;
  private String source;
  
  public String getName() {
    return name;
  }
  
  public void setName(String name) {
    this.name = name;
  }
  
  public String getProblem() {
    return problem;
  }
  
  public void setProblem(String problem) {
    this.problem = problem;
  }
  
  public String getSolution() {
    return solution;
  }
  
  public void setSolution(String solution) {
    this.solution = solution;
  }
  
  public String getAssembly() {
    return assembly;
  }
  
  public void setAssembly(String assembly) {
    this.assembly = assembly;
  }
  
  public String getLocation() {
    return location;
  }
  
  public void setLocation(String location) {
    this.location = location;
  }
  
  public String getSource() {
    return source;
  }
  
  public void setSource(String source) {
    this.source = source;
  }

  @Override
  public int hashCode() {
    final int prime = 31;
    int result = 1;
    result = prime * result + ((assembly == null) ? 0 : assembly.hashCode());
    result = prime * result + ((location == null) ? 0 : location.hashCode());
    result = prime * result + ((name == null) ? 0 : name.hashCode());
    result = prime * result + ((problem == null) ? 0 : problem.hashCode());
    result = prime * result + ((solution == null) ? 0 : solution.hashCode());
    result = prime * result + ((source == null) ? 0 : source.hashCode());
    return result;
  }

  @Override
  @SuppressWarnings("all")
  public boolean equals(Object obj) {
    if (this == obj)
      return true;
    if (obj == null)
      return false;
    if (getClass() != obj.getClass())
      return false;
    Issue other = (Issue) obj;
    if (assembly == null) {
      if (other.assembly != null)
        return false;
    } else if ( !assembly.equals(other.assembly))
      return false;
    if (location == null) {
      if (other.location != null)
        return false;
    } else if ( !location.equals(other.location))
      return false;
    if (name == null) {
      if (other.name != null)
        return false;
    } else if ( !name.equals(other.name))
      return false;
    if (problem == null) {
      if (other.problem != null)
        return false;
    } else if ( !problem.equals(other.problem))
      return false;
    if (solution == null) {
      if (other.solution != null)
        return false;
    } else if ( !solution.equals(other.solution))
      return false;
    if (source == null) {
      if (other.source != null)
        return false;
    } else if ( !source.equals(other.source))
      return false;
    return true;
  }

  @Override
  public String toString() {
    return " Issue assembly=" + assembly + "\nlocation=" + location + "\nname=" + name + "\nproblem=" + problem + "\nsolution=" + solution
      + "\nsource=" + source;
  }
  
}
