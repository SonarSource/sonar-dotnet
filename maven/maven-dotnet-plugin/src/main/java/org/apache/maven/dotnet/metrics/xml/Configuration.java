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
 * Created on Apr 7, 2009
 */
package org.apache.maven.dotnet.metrics.xml;

import java.util.List;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;

@XmlAccessorType(XmlAccessType.FIELD)
@XmlRootElement(name = "sourcemonitor_commands")
public class Configuration
{
  @XmlElement(name = "write_log")
  private boolean       log;

  @XmlElement(name = "command", type = Command.class)
  private List<Command> commands;

  public Configuration()
  {
  }

  /**
   * Returns the log.
   * 
   * @return The log to return.
   */
  public boolean isLog()
  {
    return this.log;
  }

  /**
   * Sets the log.
   * 
   * @param log The log to set.
   */
  public void setLog(boolean log)
  {
    this.log = log;
  }

  /**
   * Returns the commands.
   * 
   * @return The commands to return.
   */
  public List<Command> getCommands()
  {
    return this.commands;
  }

  /**
   * Sets the commands.
   * 
   * @param commands The commands to set.
   */
  public void setCommands(List<Command> commands)
  {
    this.commands = commands;
  }
}
