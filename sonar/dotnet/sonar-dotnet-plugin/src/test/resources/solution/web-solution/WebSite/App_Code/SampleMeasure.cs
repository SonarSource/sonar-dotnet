using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
