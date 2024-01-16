using System;

namespace Tests.Diagnostics
{
    public class FunctionNestingDepth
    {
        public FunctionNestingDepth()
        {
            do
            {
                try
                {
                    while (true)
                    {
                        if (true) { } // Noncompliant {{Refactor this code to not nest more than 3 control flow statements.}}
//                      ^^
                    }
                }
                catch { }
            } while (true);
        }

        ~FunctionNestingDepth()
        {
            do
            {
                try
                {
                    while (true)
                    {
                        foreach (var i in new string[] { }) { } // Noncompliant
                    }
                }
                catch { }
            } while (true);
        }

        public void M1()
        {
            {
                if (true)
                {
                    while (true)
                    {
                        try { }
                        catch { }
                    }
                }
                if (true) { }
                else if (true)
                {
                    if (true) { }
                    else if (true)
                    {
                        do
                        {
                            do // Noncompliant
                            {

                            }
                            while (true);
                        }
                        while (true);
                    }
                }
                else if (true) { }
            }
        }

        public void M2()
        {
            if (true)
            {
                if (true)
                {
                    if (true)
                    {
                        for (; ; ) // Noncompliant
                        {
                        }
                    }
                }
            }
        }

        public int M3() => (new Func<int>(() =>
        {
            if (true)
                if (true)
                    if (true)
                        if (true) // Noncompliant
                            return 0;
            return 42;
        }))();

        public int M4() => (new Func<int>(() =>
        {
            if (true)
                if (true)
                    if (true)
                        return 0;
            return 42;
        }))();

        public void SwitchStatement()
        {
            if (true)
            {
                switch (42)
                {
                    case 1:
                        break;
                    case 2:
                        switch (42)
                        {
                            case 0:
                                break;
                            default:
                                break;
                        }
                }
            }
            if (true)
            {
                switch (42)
                {
                    case 1:
                        break;
                    case 2:
                        switch (42)
                        {
                            case 0:
                                if (true) { }   // Noncompliant
                                break;
                            default:
                                if (true) { }   // Noncompliant
                                break;
                        }
                }
            }
        }

        public int MyProperty
        {
            get
            {
                if (true)
                {
                    if (true)
                    {
                        if (true)
                        {
                            if (true) { } // Noncompliant
                        }
                    }
                }
                return 0;
            }
            set
            {
                if (true)
                {
                    if (true)
                    {
                        if (true)
                        {
                            if (true) { } // Noncompliant
                        }
                    }
                }
            }
        }

        public event EventHandler OnSomething
        {
            add
            {
                if (true)
                {
                    if (true)
                    {
                        if (true)
                        {
                            if (true) { } // Noncompliant
                        }
                    }
                }
            }
            remove
            {
                if (true)
                {
                    if (true)
                    {
                        if (true)
                        {
                            if (true) { } // Noncompliant
                        }
                    }
                }
            }
        }

        public static FunctionNestingDepth operator +(FunctionNestingDepth a)
        {
            if (true)
            {
                if (true)
                {
                    if (true)
                    {
                        if (true) { } // Noncompliant
                    }
                }
            }
            return null;
        }

        public static void M5()
        {
            if (true)
            {
                if (true)
                {
                    if (true)
                    {
                        if (true)  // Noncompliant
                        {
                            if (true) // Compliant
                            {

                            }
                        }
                    }
                }
            }
        }
    }
}
