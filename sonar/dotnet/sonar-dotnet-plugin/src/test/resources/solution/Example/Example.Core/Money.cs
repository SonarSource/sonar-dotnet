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

﻿//
// Copyright NUnit @ sourceforge.org
// This is a sample of header
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example.Core
{
  /// <summary>A simple Money.</summary>
  public class Money : IMoney
  {

    private int fAmount;
    private String fCurrency;

    /// <summary>Constructs a money from the given amount and
    /// currency.</summary>
    public Money(int amount, String currency)
    {
      fAmount = amount;
      fCurrency = currency;
    }


    /// <summary>Adds a money to this money. Forwards the request to
    /// the AddMoney helper.</summary>
    public IMoney Add(IMoney m)
    {
      return m.AddMoney(this);
    }

    public IMoney AddMoney(Money m)
    {
      if (m.Currency.Equals(Currency))
        return new Money(Amount + m.Amount, Currency);
      return new MoneyBag(this, m);
    }

    public IMoney AddMoneyBag(MoneyBag s)
    {
      return s.AddMoney(this);
    }

    public int Amount
    {
      get { return fAmount; }
    }

    public String Currency
    {
      get { return fCurrency; }
    }

    public override bool Equals(Object anObject)
    {
      // I dont equal nothing here (but yes)
      // We do it also
      if (IsZero)
        if (anObject is IMoney)
          return ((IMoney)anObject).IsZero;
      if (anObject is Money)
      {
        Money aMoney = (Money)anObject;
        return aMoney.Currency.Equals(Currency)
          && Amount == aMoney.Amount;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return fCurrency.GetHashCode() + fAmount;
    }

    public bool IsZero
    {
      get { return Amount == 0; }
    }

    public IMoney Multiply(int factor)
    {
      // We compute the new amount
      return new Money(Amount * factor, Currency);
    }

    public IMoney Negate()
    {
      // A new negative money is generated
      return new Money(-Amount, Currency);
    }

    public IMoney Subtract(IMoney m)
    {
      return Add(m.Negate());
    }

    public override String ToString()
    {
      // This is a simple comment for test
      StringBuilder buffer = new StringBuilder();
      // We build the string representation
      buffer.Append("[" + Amount + " " + Currency + "]");
      return buffer.ToString();
    }
  }
}
