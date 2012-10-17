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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example.Core
{
  public class SampleMeasure
  {
    public int Size { get; set; }

    private Possible actual;

    private enum Possible
    {
      A,
      B,
      C,
      D
    } ;

    public SampleMeasure()
    {
      Size = 3;
      actual = Possible.C;
    }

    public String Compute()
    {
      for (int idx = 0; idx < Size; idx++)
      {
        switch (actual)
        {
          case Possible.A:
            return actual.ToString();
          case Possible.B:
            return actual.ToString();
          case Possible.C:
            return actual.ToString();
          case Possible.D:
            return actual.ToString();
          default:
            return "null";
        }
      }
      return "done";
    }

    public String Compute(int maxSize)
    {
      return "done".Substring(0, maxSize);
    }

    public String Compute(String input)
    {
      return input.Substring(0, Size);
    }

    public bool Equals(SampleMeasure obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      return Equals(obj.actual, actual) && obj.Size == Size;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != typeof (SampleMeasure)) return false;
      return Equals((SampleMeasure) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (actual.GetHashCode()*397) ^ Size;
      }
    }
  }
}
