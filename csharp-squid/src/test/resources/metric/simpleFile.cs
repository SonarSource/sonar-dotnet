public class Test
{
    private int fAmount;
    private String fCurrency;

	private void test()
	{
		// The following comment must not be used for CPD
		string message = "Hello World";
	}
	
    /// <summary>
    /// Base implementation of Single operator.
    /// </summary>
    
    private static TSource SingleImpl<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource> empty)
    {
        CheckNotNull(source, "source");

        using (var e = source.GetEnumerator())
        {
            if (e.MoveNext())
            {
                var single = e.Current;
                if (!e.MoveNext())
                    return single;

                throw new InvalidOperationException();
            }

            return empty();
        }
    }

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
	
}
