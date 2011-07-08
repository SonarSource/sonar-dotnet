using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example.Core
{
  public class TooLongMethodClass
  {

    public override bool Equals(Object anObject)
    {
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

  }
}