using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
	
	 public bool AltEquals(Object anObject)
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
	  
	  // This is a simple comment for test
      StringBuilder buffer = new StringBuilder();
      // We build the string representation
      buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  buffer.Append("[" + Amount + " " + Currency + "]");
	  
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
