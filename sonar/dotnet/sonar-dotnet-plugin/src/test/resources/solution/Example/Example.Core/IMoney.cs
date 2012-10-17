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
  public interface IMoney
  {
    
		/// <summary>Adds a money to this money.</summary>
		IMoney Add(IMoney m);

		/// <summary>Adds a simple Money to this money. This is a helper method for
		/// implementing double dispatch.</summary>
		IMoney AddMoney(Money m);

		/// <summary>Adds a MoneyBag to this money. This is a helper method for
		/// implementing double dispatch.</summary>
		IMoney AddMoneyBag(MoneyBag s);

		/// <value>True if this money is zero.</value>
		bool IsZero { get; }

		/// <summary>Multiplies a money by the given factor.</summary>
		IMoney Multiply(int factor);

		/// <summary>Negates this money.</summary>
		IMoney Negate();

		/// <summary>Subtracts a money from this money.</summary>
		IMoney Subtract(IMoney m);
  }
}
