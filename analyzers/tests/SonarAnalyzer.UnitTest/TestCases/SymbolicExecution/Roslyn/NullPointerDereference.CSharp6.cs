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
            this?._foo1.ToString(); // Not supported, TrackedSymbol() does not resolve flow capture references
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

        // https://github.com/SonarSource/sonar-dotnet/issues/1324
        public void Repro_1324(object o)
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
                var b = o.ToString(); // Unreachable, o? cannot throw ApplicationException
            }

            try
            {
                var a = o?.ToString();
                CanThrow();
            }
            catch (InvalidOperationException) when (o != null)
            {
                var b = o.ToString(); // Compliant, o is checked for null in this branch
            }
            catch (ApplicationException) when (o == null)
            {
                var b = o.ToString(); // FN Suppressed #6117
            }
        }

        public void TryCatch4(object o)
        {
            o = null;
            try
            {
                var a = o?.ToString();
                Console.WriteLine(""); // some call that can throw
            }
            catch (Exception e) when (e.Message != null)
            {
                var b = o.ToString(); // FN Suppressed #6117
            }
        }

        public void Compliant(List<int> list)
        {
            var row = list?.Count;
            if (row != null)
            {
                var type = list.ToArray();  // Compliant Suppressed #6117, nullability is inferred from result relation
            }
        }

        private void CanThrow() { }

        public class A
        {
            public bool booleanVal { get; set; }
        }

        public void Compliant1(List<int> list, A a)
        {
            var row = list?.Count;
            if (a.booleanVal = (row != null))
            {
                var type = list.ToArray();  // Compliant Suppressed #6117, nullability is inferred from result relation
            }
        }

        public void NonCompliant(List<int> list)
        {
            var row = list?.Count;
            if (row == null)
            {
                var type = list.ToArray(); // FN Suppressed #6117
            }
        }

        public void NonCompliant1(List<int> list, A a)
        {
            var row = list?.Count;
            if (a.booleanVal = (row == null))
            {
                var type = list.ToArray(); // FN Suppressed #6117
            }
        }

        void Compliant2(object o)
        {
            switch (o?.GetHashCode())
            {
                case 1:
                    o.ToString(); // Compliant Suppressed #6117, nullability is inferred from result relation
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
                    o.ToString(); // FN Suppressed #6117
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
                    Console.ForegroundColor = obj.Color; // FN Suppressed #6117
                    break;
                case ConsoleColor.Red:
                    Console.ForegroundColor = obj.Color;
                    break;
                default:
                    Console.WriteLine($"Color {obj.Color} is not supported.");
                    break;
            }
        }

        public void Method2(ReproForIssue2338 obj)
        {
            obj = null;
            switch (obj?.Color)
            {
                case ConsoleColor.Red:
                    Console.ForegroundColor = obj.Color;  // Compliant Suppressed #6117, was compliant in the old engine. Should be fixed by MMF-2401
                    break;
                default:
                    Console.WriteLine($"Color {obj.Color} is not supported."); // Non-compliant Suppressed #6117
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
                if (rightName.EndsWith("foo")) // Compliant Suppressed #6117
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
                    return valueHolder.Value;   // Compliant Suppressed #6117, nullability is inferred from result relation
                case MyEnum.TWO:
                case MyEnum.THREE:
                    return valueHolder.Value;   // Compliant Suppressed #6117, nullability is inferred from result relation
                case MyEnum.FOUR:
                    return valueHolder.Value;   // Compliant Suppressed #6117, nullability is inferred from result relation
                case MyEnum.FIVE:
                case null:
                    return valueHolder.Value;   // FN Suppressed #6117
                default:
                    return string.Empty;
            }
        }
    }

}
