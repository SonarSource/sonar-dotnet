using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class A { }

    class NullPointerDereferenceWithFieldsCSharp6 : A
    {
        private object _foo1;

        void ConditionalThisFieldAccess()
        {
            object o = null;
            this._foo1 = o;
            this?._foo1.ToString(); // Noncompliant
        }

        string TryCatch3()
        {
            object o = null;
            try
            {
                o = new object();
            }
            catch (Exception e) when (e.Message != null)
            {
                o = new object();
            }
            return o.ToString(); // If e.Message is null, the exception won't be caught and this is not reachable
        }

        // https://github.com/SonarSource/sonar-csharp/issues/1324
        public void FlasePositive(object o)
        {
            try
            {
                var a = o?.ToString();
            }
            catch (InvalidOperationException) when (o != null)
            {
                var b = o.ToString(); // Compliant, o is checked for null in this branch
            }
            catch (ApplicationException) when (o == null)
            {
                var b = o.ToString(); // Noncompliant
            }
        }

        public void TryCatch4(object o)
        {
            o = null;
            try
            {
                var a = o?.ToString();
            }
            catch (Exception e) when (e.Message != null)
            {
                var b = o.ToString(); // Noncompliant, o could be null here
            }
        }

    public void Compliant(List<int> list)
    {
      var row = list?.Count;
      if (row != null)
      {
        var type = list.ToArray();
      }
    }

    public class A
    {
      public bool booleanVal { get; set; }
    }

    public void Compliant1(List<int> list, A a)
    {
      var row = list?.Count;
      if (a.booleanVal = (row != null))
      {
        var type = list.ToArray();
      }
    }

    public void NonCompliant(List<int> list)
    {
      var row = list?.Count;
      if (row == null)
      {
        var type = list.ToArray(); // Noncompliant
      }
    }

    public void NonCompliant1(List<int> list, A a)
    {
      var row = list?.Count;
      if (a.booleanVal = (row == null))
      {
        var type = list.ToArray(); // Noncompliant
      }
    }

    void Compliant2(object o)
    {
      switch (o?.GetHashCode())
      {
        case 1:
          o.ToString();
          break;
        default:
          break;
      }
    }

    void NonCompliant2()
    {
      object o = null;
      switch (o?.GetHashCode())
      {
        case null:
          o.ToString(); // Noncompliant
          break;
        default:
          break;
      }
    }
  }

  public class ReproForIssue2338
  {
    public ConsoleColor Color { get; set; }

    public void Method1(ReproForIssue2338 obj)
    {
      switch (obj?.Color)
      {
        case null:
          Console.ForegroundColor = obj.Color; // Noncompliant
          break;
        case ConsoleColor.Red:
          Console.ForegroundColor = obj.Color; //compliant
          break;
        default:
          Console.WriteLine($"Color {obj.Color} is not supported."); //compliant
          break;
      }
    }

    public void Method2(ReproForIssue2338 obj)
    {
      obj = null;
      switch (obj?.Color)
      {
        case ConsoleColor.Red:
          Console.ForegroundColor = obj.Color; //compliant
          break;
        default:
          Console.WriteLine($"Color {obj.Color} is not supported."); // Noncompliant
          break;
      }
    }
  }

    public class ReproFor2593
    {
        public int id;
        public string name;

        public void Repro(ReproFor2593 obj)
        {
            obj = null;
            var objId = obj?.id;
            if (objId.HasValue)
            {
                var objName = obj.name; // Ok
            }
        }
    }


    public enum MyEnum
    {
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE
    }

    public class ValueHolder
    {
        public string Value { get; }
        public MyEnum MyEnum { get; }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3416
    public class ReproCommunityIssue3416
    {
        public int Compare(ValueHolder left, ValueHolder right)
        {
            string leftName = left?.Value;
            string rightName = right?.Value;

            if (string.Equals(leftName, rightName))
                // this will return if both are NULL or if they have equal non-null values
                return 0;

            // at this point, leftName can be NULL if rightName is not NULL
            if (leftName == null)
            {
                // rightName is not null
                if (rightName.EndsWith("foo")) // Noncompliant FP
                    return 1;
                return 0;
            }

            return 0;
        }

        public string Foo(ValueHolder valueHolder)
        {
            switch (valueHolder?.MyEnum)
            {
                case MyEnum.ONE:
                    return valueHolder.Value;
                case MyEnum.TWO:
                case MyEnum.THREE:
                    return valueHolder.Value;
                case MyEnum.FOUR:
                    return valueHolder.Value;
                case MyEnum.FIVE:
                case null:
                    return valueHolder.Value; // Noncompliant Ok
                default:
                    return string.Empty;
            }
        }
    }

}
