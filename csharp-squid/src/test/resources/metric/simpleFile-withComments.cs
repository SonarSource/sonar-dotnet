// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org.
// ****************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example.Core
{
  /// <summary>A simple Money.</summary>
  public class Money : IMoney
  {

    private int fAmount; //NOSONAR
    private String fCurrency;

    /// <summary>Constructs a money from the given amount and
    /// currency.</summary>
    //
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

	/*
    public IMoney AddMoney(Money m)
    {
      if (m.Currency.Equals(Currency))
        return new Money(Amount + m.Amount, Currency);
      return new MoneyBag(this, m);
    }
    */

    public IMoney AddMoneyBag(MoneyBag s)
    {
      return s.AddMoney(this); //NOSONAR
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
      /*
      	And this is the end.
      	
      	Full stop.
      */
      return false;
    }
  }
}