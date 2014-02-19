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
using Example.Core;

namespace Example.Application
{
  class Program
  {
    static void Main(string[] args)
    {
      MoneyBag bag = new MoneyBag();
      Money moneyA = new Money(10, "EUR");
      Money moneyB = new Money(20, "USD");
      Money moneyC = new Money(15, "EUR");
      Money moneyD = new Money(25, "JPY");
      IMoney money = bag.AddMoney(moneyA);
      money = money.AddMoney(moneyB);
      money = money.AddMoney(moneyC);
      money = money.AddMoney(moneyD);
      Console.WriteLine("My Portfolio : " + money.ToString());
      Console.ReadLine();
    }
  }
}
