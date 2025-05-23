﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Foo
    {
        private string field;

        string Method(string s) =>
            s != null
            ? null
            : s.ToLower(); // Noncompliant

        string Prop =>
            field != null
            ? null
            : field.ToLower(); // Noncompliant

        string PropGet
        {
            get =>
                field != null
                ? null
                : field.ToLower(); // Noncompliant
        }

        void ConstantPattern(object o)
        {
            if (o is null)
            {
                o.ToString(); // Noncompliant
            }
            else
            {
                o.ToString(); // Compliant
            }
        }

        void VariableDesignationPattern_Variable(object o)
        {
            if (o is string s)
            {
                if (s == null)
                {
                    // This is unreachable, s has NotNull constraint from the outer if condition
                    s.ToString(); // Compliant
                }

                // s still has NotNull constraint from the outer if statement
                s.ToString(); // Compliant
            }
        }

        void VariableDesignationPattern_Source(object o)
        {
            // We can set NotNull constraint only for one of the variables in the if condition
            // and we choose the declared variable because it is more likely to have usages of
            // it inside the statement body.
            if (o is string s)
            {
                if (o == null)
                {
                    o.ToString(); // Compliant
                }
            }
        }

        void VariableDesignationPattern_Discard(object o)
        {
            if (o is string _) // Ensure that the discard does not throw exception when processed
            {
                if (o == null)
                {
                    o.ToString(); // Compliant
                }
            }
        }

        void Patterns_In_Loops(object o, object[] items, int length)
        {
            while (o is string s)
            {
                if (s == null)
                {
                    // This is unreachable, s has NotNull constraint from the while condition
                    s.ToString(); // Compliant
                }
            }

            do
            {
                // The condition is evaluated after the first execution, so we cannot test s
            }
            while (o is string s);

            for (int i = 0; i < length && items[i] is string s; i++)
            {
                if (s == null)
                {
                    // This is unreachable, s has NotNull constraint from the for condition
                    s.ToString();
                }
            }
        }

        void Patterns_In_Loops_With_Discard(object o, object[] items)
        {
            // The following should not throw exceptions
            while (o is string _)
            { }

            do
            { } while (o is string _);

            for (int i = 0; i < items.Length && items[i] is string _; i++)
            { }
        }

        void Switch_Pattern_Source(object o)
        {
            switch (o)
            {
                case string s:
                    // We don't set constraints on the switch expression
                    if (o == null)
                    {
                        o.ToString(); // Compliant
                    }
                    break;

                default:
                    break;
            }
        }

        void Switch_Pattern(object o)
        {
            switch (o)
            {
                case string s:
                    if (s == null)
                    {
                        // This is unreachable, s has NotNull constraint from the outer if condition
                        s.ToString(); // Compliant
                    }
                    // s still has NotNull constraint from the outer if statement
                    s.ToString(); // Compliant
                    break;

                case Foo f when f == null:
                    if (f == null)
                    {
                        f.ToString(); // Compliant, this code is not reachable
                    }
                    break;

                case int _: // The discard is redundant, but still allowed
                    o.ToString();
                    break;

                case null:
                    o.ToString(); // Noncompliant
                    break;

                default:
                    break;
            }
        }

        void Compliant1()
        {
            Exception exception = null;
            if (exception?.Data is IDictionary data)
            {
                if (exception?.InnerException?.Data is IDictionary innerexceptiondata)
                {

                }
            }
        }

        void NonCompliant1()
        {
            Exception exception = null;
            if (exception.Data is IDictionary data) // Noncompliant
            {
                if (exception.InnerException?.Data is IDictionary innerexceptiondata)
                {

                }
            }
        }

        void Repro_2361(Exception exception)
        {
            if (exception?.Data is IDictionary data)
            {
                if (exception.InnerException?.Data is IDictionary innerexceptiondata)
                {

                }
            }
        }

        void Null_Conditional_Is_Null(Exception exception)
        {
            if (exception?.Data is null)
            {
                if (exception.InnerException?.Data is IDictionary innerexceptiondata) // Noncompliant
                {

                }
            }
        }

        public void Method1(object obj)
        {
            switch (obj)
            {
                case string s1:
                    break;
                default:
                    return;
            }
            var s = obj.ToString();
        }

        public void Method2(object obj)
        {
            switch (obj)
            {
                case null:
                    break;
                default:
                    return;
            }
            var s1 = obj.ToString(); // Noncompliant
        }

        public void Method3(object obj)
        {
            obj = null;
            switch (obj)
            {
                case string s1:
                    return;
                default:
                    break;
            }
            var s = obj.ToString(); // Noncompliant
        }

        public void Method4(object obj)
        {
            obj = null;
            switch (obj)
            {
                case null:
                    return;
                default:
                    break;
            }
            var s = obj.ToString();
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/2528
        public int Repro_2528(Dictionary<string, string> dict)
        {
            if ((dict?.Count ?? 0) == 0)
            {
                return -1;
            }

            return dict.Count; // Compliant
        }
    }
}
