using System;
using System.Threading;

namespace Monitor_Conditions
{
    class Program
    {
        private object obj = new object();
        private object other = new object();

        public object PublicObject = new object();

        private bool condition;

        public void Method1()
        {
            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method2()
        {
            Monitor.Enter(obj); // Noncompliant
            switch (condition)
            {
                case true:
                    Monitor.Exit(obj);
                    break;
                default:
                    break;
            }
        }

        public void Method3()
        {
            bool isAcquired = false;
            Monitor.Enter(obj, ref isAcquired); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method4()
        {
            Monitor.Enter(obj);     // Noncompliant
            Monitor.Enter(other);   // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
            else
            {
                Monitor.Exit(other);
            }
        }

        public void Method5()
        {
            Monitor.Enter(obj); // Compliant
            if (condition)
            {
                Monitor.Exit(other);
            }
        }

        public void Method6(string arg)
        {
            var localObj = obj;
            Monitor.Enter(localObj); // Noncompliant
            Console.WriteLine(arg.Length);
            if (condition)
            {
                Monitor.Exit(localObj);
            }
        }

        public void Method7(string arg)
        {
            var localObj = obj;
            Monitor.Enter(obj); // FN
            Console.WriteLine(arg.Length);
            if (condition)
            {
                Monitor.Exit(localObj);
            }
        }

        public void Method8(string arg, object paramObj)
        {
            paramObj = obj;
            Monitor.Enter(obj); // FN
            Console.WriteLine(arg.Length);
            if (condition)
            {
                Monitor.Exit(paramObj);
            }
        }

        public void Method9(string arg, object paramObj)
        {
            Monitor.Enter(obj);
            Console.WriteLine(arg.Length);
            if (condition)
            {
                Monitor.Exit(paramObj);
            }
        }

        public void Method10(string arg, Program p1)
        {
            Monitor.Enter(p1.PublicObject); // FN
            Console.WriteLine(arg.Length);
            if (condition)
            {
                Monitor.Exit(p1.PublicObject);
            }
        }

        public void Method11(string arg, Program p1, Program p2)
        {
            Monitor.Enter(p1.PublicObject);
            Console.WriteLine(arg.Length);
            if (condition)
            {
                Monitor.Exit(p2.PublicObject);
            }
        }

        public void Method12()
        {
            var getObj = new Func<object>(() =>
            {
                return obj;
            });

            Monitor.Enter(getObj()); // FN
            if (condition)
            {
                Monitor.Exit(getObj());
            }
        }

        public void Method13()
        {
            Monitor.Enter(obj); // FN
            var a = new Action(() =>
            {
                Monitor.Exit(obj);
            });

            if (condition)
            {
                a();
            }
        }

        public void Method14()
        {
            Monitor.Enter(obj); // Compliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void Method15(string arg)
        {
            Monitor.Enter(obj); // Compliant
            if (arg.Length == 16)
            {
                Monitor.Exit(obj);
            }
            else if (arg.Length == 23)
            {
                Monitor.Exit(obj);
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void Method16(string arg)
        {
            Monitor.Enter(obj); // Noncompliant
            if (arg.Length == 16)
            {
                Monitor.Exit(obj);
            }
            else if (arg.Length == 23)
            {
                Monitor.Exit(obj);
            }
            else
            {
            }
        }

        public void Method17(bool anotherCondition)
        {
            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                if (!anotherCondition)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void Method18(bool condition1, bool condition2)
        {
            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                switch (condition1)
                {
                    case true:
                        {
                            if (!condition2)
                            {
                                Monitor.Exit(obj);
                            }
                            else
                            {
                                Monitor.Exit(obj);
                            }
                        }
                        break;
                }
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void Method19()
        {
            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
            else
            {
                Monitor.Exit(other);
            }
        }

        public void Method20(Program first, Program second)
        {
            Monitor.Enter(first.obj); // FN
            Monitor.Exit(second.obj);
            if (condition)
            {
                Monitor.Exit(first.obj);
            }
        }

        public void Method21(string arg)
        {
            Monitor.Enter(obj); // Noncompliant
            if (arg.Length == 16)
            {
                Monitor.Exit(obj);
            }
            else if (arg.Length == 23)
            {
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void Method22()
        {
            bool isAcquired = false;
            Monitor.Enter(obj, ref isAcquired); // Noncompliant FP, the isAcquired is not tracked
            if (isAcquired)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method23(bool condition1)
        {
            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                if (!condition1)
                {
                    Monitor.Exit(obj);
                }
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void SameObject_SameField(Program arg, bool condition)
        {
            Monitor.Enter(arg.obj); // FN, because we are not field sensitive
            if (condition)
                Monitor.Exit(arg.obj);
        }

        public int MyProperty
        {
            set
            {
                Monitor.Enter(obj); // Noncompliant
                if (value == 42)
                {
                    Monitor.Exit(obj);
                }
            }
        }
    }
}
