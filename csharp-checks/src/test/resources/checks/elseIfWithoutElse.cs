using System;

class Program
{

    static void Main(string[] args)
    {
      if (a)
      {
      }

      if (a)
      {
      } else
      {
      }

      if (a)
      {
      } else if (a)
      {
      } else if (a)  // Noncompliant
      {
      }


      if (a)
      {
      } else if (a)
      {
      } else if (a)  // Compliant
      {
      } else {
      }

      if (a)
      {
      } else
      {
        if (a)
        {
        }
      }
    }
}
