using System;
using System.Threading;

namespace Monitor_Conditions
{
    class Program
    {
        private readonly static object staticObj = new object();
        private object obj = new object();
        private object other = new object();

        public object PublicObject = new object();

        delegate void VoidDelegate();

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
            Monitor.Enter(obj); // FN, because arg.Length can throw NullReferenceException
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

        public void Method15_SafeConditions(bool a, bool b)
        {
            Monitor.Enter(obj); // Compliant
            if (a)
            {
                Monitor.Exit(obj);
            }
            else if (b)
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
            Monitor.Enter(obj, ref isAcquired);
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

        public void Method24(bool condition)
        {
            bool isAcquired = false;
            var somethingElse = new object();
            Monitor.Enter(obj, ref isAcquired);
            if (!isAcquired)
            {
                Monitor.Enter(somethingElse); // Noncompliant
                if (condition)
                    Monitor.Exit(somethingElse);
            }
        }

        public void Method25()
        {
            bool isAcquired = false;
            Monitor.Enter(lockTaken: ref isAcquired, obj: obj ); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }


        public void Method26()
        {
            bool isAcquired = false;
            Monitor.Enter(lockTaken: ref isAcquired, obj: obj);
            if (isAcquired)
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

        public void Lambda(bool condition)
        {
            Action parenthesizedLambda = () =>
            {
                Monitor.Enter(obj); // Noncompliant
                if (condition)
                    Monitor.Exit(obj);
            };

            Action<int> simpleLambda = x =>
            {
                Monitor.Enter(obj); // Noncompliant
                if (condition)
                    Monitor.Exit(obj);
            };

            void LocalFunction()
            {
                Monitor.Enter(other); // Noncompliant
                if (condition)
                    Monitor.Exit(other);
            }

            static void StaticLocalFunction(bool condition)
            {
                var l = new object();
                Monitor.Enter(l); // Noncompliant
                if (condition)
                    Monitor.Exit(l);
            }

            VoidDelegate anonymousMethod = delegate
            {
                Monitor.Enter(other); // Noncompliant
                if (condition)
                    Monitor.Exit(other);
            };
        }

        public void IsEntered_Compliant()
        {
            Monitor.Enter(obj);
            if (Monitor.IsEntered(obj))
            {
                Monitor.Exit(obj);
            }
        }

        public void IsEntered_Acquired()
        {
            if (Monitor.IsEntered(obj)) // Noncompliant
            {
                if (condition)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void FieldReference_Standalone(string arg)
        {
            Monitor.Enter(obj); // Noncompliant {{Unlock this lock along all executions paths of this method.}}
            if(condition)
                Monitor.Exit(obj);
        }

        public void FieldReference_WithThis(string arg)
        {
            Monitor.Enter(this.obj); // Noncompliant {{Unlock this lock along all executions paths of this method.}}
            if(condition)
                Monitor.Exit(this.obj);
        }

        public void FieldReference_WithThis_Mixed1(string arg)
        {
            Monitor.Enter(this.obj); // Noncompliant {{Unlock this lock along all executions paths of this method.}}
            if(condition)
                Monitor.Exit(obj);
        }

        public void FieldReference_WithThis_Mixed2(string arg)
        {
            Monitor.Enter(obj); // Noncompliant {{Unlock this lock along all executions paths of this method.}}
            if (condition)
                Monitor.Exit(this.obj);
        }

        public void StaticFieldReference(string arg)
        {
            Monitor.Enter(staticObj); // Noncompliant {{Unlock this lock along all executions paths of this method.}}
            if (condition)
                Monitor.Exit(staticObj);
        }

        public void StaticFieldReference_Class(string arg)
        {
            Monitor.Enter(Program.staticObj); // Noncompliant {{Unlock this lock along all executions paths of this method.}}
            if (condition)
                Monitor.Exit(Program.staticObj);
        }

        public void LocalVariable(string arg)
        {
            var l = new object();

            Monitor.Enter(l); // Noncompliant {{Unlock this lock along all executions paths of this method.}}
            if (condition)
                Monitor.Exit(l);
        }

        public void Parameter(object arg)
        {
            Monitor.Enter(arg); // Noncompliant
            if (condition)
                Monitor.Exit(arg);
        }

        public void FirstReleasedThanAcquired_ConditionalEnter()
        {
            Monitor.Exit(obj);
            try
            {
                Console.WriteLine();
            }
            finally
            {
                if (condition)
                {
                    Monitor.Enter(obj); // Not supported by this rule
                }
            }
        }

        public void FirstReleasedThanAcquired_ConditionalExit()
        {
            if (condition)
            {
                Monitor.Exit(obj);
            }
            try
            {
                Console.WriteLine();
            }
            finally
            {
                if (condition)
                {
                    Monitor.Enter(obj); // Compliant, even when the exit is weirdly conditional. Not in scope of this rule.
                }
            }
        }

        public void FirstReleasedThanAcquired_Complex()
        {
            Monitor.Exit(obj);
            try
            {
                Console.WriteLine();
                Monitor.Enter(obj);     // Noncompliant
                Console.WriteLine();
            }
            finally
            {
                if (condition)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void Numbers_Equals()
        {
            Monitor.Enter(obj);     // Compliant, always released
            Monitor.Enter(other);   // Noncompliant
            if (condition)          // Release on some paths
            {
                Monitor.Exit(obj);
                Monitor.Exit(other);
            }
            var value = 42;
            if (value == 42)
            {
                Monitor.Exit(obj);
            }
            else
            {
                Monitor.Exit(other);
            }
        }

        public void Numbers_NotEquals()
        {
            Monitor.Enter(obj);     // Compliant, always released
            Monitor.Enter(other);   // Noncompliant
            if (condition)          // Release on some paths
            {
                Monitor.Exit(obj);
                Monitor.Exit(other);
            }
            var value = 42;
            if (value != 0)
            {
                Monitor.Exit(obj);
            }
            else
            {
                Monitor.Exit(other);
            }
        }

        public void Numbers_CompoundAssignment()
        {
            Monitor.Enter(obj);     // Always released
            Monitor.Enter(other);   // Noncompliant
            if (condition)          // Release on some paths
            {
                Monitor.Exit(obj);
                Monitor.Exit(other);
            }
            var value = 42;
            value += 1;
            if (value == 43)
            {
                Monitor.Exit(obj);
            }
            else
            {
                Monitor.Exit(other);
            }
        }
    }

    class LockOnField
    {
        private object lockObj = new object();

        void M(bool condition)
        {
            Monitor.Enter(lockObj);    // Noncompliant
            this.DoSomething();        // Some constraints get reset on instance invocation (see SymbolConstraint.PreserveOnFieldReset). Make sure, we are not in that boat.
            if (condition)
            {
                Monitor.Exit(lockObj);
            }
        }

        void DoSomething() { }
    }
}
