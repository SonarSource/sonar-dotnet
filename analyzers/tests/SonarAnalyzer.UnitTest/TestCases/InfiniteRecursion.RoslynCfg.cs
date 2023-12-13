using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    class InfiniteRecursion
    {
        int Pow(int num, int exponent)   // Noncompliant; no condition under which pow isn't re-called
//          ^^^
        {
            num = num * Pow(num, exponent - 1);
            return num;  // this is never reached
        }

        void Test1() // Noncompliant {{Add a way to break out of this method's recursion.}}
        {
            var i = 10;
            if (i == 10)
            {
                Test1();
            }
            else
            {
                switch (i)
                {
                    case 1:
                        Test1();
                        break;
                    default:
                        Test1();
                        break;
                }
            }
        }

        void Test2()  // Noncompliant
        {
            var i = 10;
            switch (i)
            {
                case 1:
                    goto default;  // Missing secondary location
                default:
                    goto case 1;   // Missing secondary location
            }
        }

        void Test3()
        {
            var i = 10;
            switch (i)
            {
                case 1:
                    goto default;
                case 2:
                    break;
                default:
                    goto case 1; // FN
            }

            switch (i)
            {
                case 1:
                    goto default;
                case 2:
                    break;
                default:
                    goto case 1; // FN
            }
        }

        int Test4() => Test4(); // Noncompliant

        int Test5() => 42;

        int Prop
        {
            get // Noncompliant {{Add a way to break out of this property accessor's recursion.}}
            {
                return Prop;
            }
        }

        string Prop0
        {
            get
            {
                return nameof(Prop0);
            }
        }

        object Prop1
        {
            get // FN - analyzer currently only checks for property access on the 'this' object
            {
                return (new InfiniteRecursion())?.Prop1;
            }
        }

        int Prop2
        {
            get // Not recognized, but the accessors are circularly infinitely recursive
            {
                (Prop2) = 10;
                return 10;
            }
            set
            {
                var x = Prop2;
            }
        }

        int Prop3 => 42;

        int Prop5 => Prop5; // Noncompliant

        int Prop6
        {
            get => 42;
        }

        int Prop7
        {
            get => Prop6;
        }

        private InfiniteRecursion infiniteRecursionField;

        object Prop8
        {
            get // FN - analyzer currently only checks for property access on the 'this' object
            {
                return infiniteRecursionField?.Prop1;
            }
        }

        int RecursiveProp1
        {
            get  // Noncompliant
            {
                start:
                    goto end;
                end:
                    goto start;
                return 42;
            }
        }

        private int backing;

        int RecursiveProp2
        {
            set  // Noncompliant
            {
                start:
                    goto end;
                end:
                    goto start;
                backing = value;
            }
        }

        void InternalRecursion(int i)  // Noncompliant
        {
        start:
            goto end;    // Missing secondary location
        end:
            goto start;  // Missing secondary location

            switch (i)
            {
                case 1:
                    goto default;
                case 2:
                    break;
                default:
                    goto case 1; // Compliant, already not reachable
            }
        }

        int Pow2(int num, int exponent)
        {
            if (exponent > 1)
            {
                num = num * Pow2(num, exponent - 1);
            }
            return num;
        }

        void Generic<T>() // Noncompliant
        {
            Generic<T>();
        }

        void Generic2<T>() // Compliant
        {
            Generic2<int>();
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2342
        int Power(int num, int exponent) // Noncompliant
        {
            try
            {
                num = num * Power(num, exponent - 1);
                return num;  // this is never reached
            }
            catch (Exception)
            {
                throw;
            }
        }

        int FixIt(int a)    // FN - Two methods calling each other are not recognized
        {
            return UpdateIt(a);
        }

        int UpdateIt(int a)
        {
            return FixIt(a);
        }

        int CSharp8_SwitchExpressions_OK(int a)
        {
            return a switch
            {
                0 => 1,
                1 => 0,
                _ => CSharp8_SwitchExpressions_OK(a) % 2
            };
        }

        int CSharp8_SwitchExpressions_Bad(int a)    // Noncompliant
        {
            return a switch
            {
                0 => CSharp8_SwitchExpressions_Bad(a + 1),
                1 => CSharp8_SwitchExpressions_Bad(a - 1),
                _ => CSharp8_SwitchExpressions_Bad(a) % 2
            };
        }

        int CSharp8_StaticLocalFunctions_OK(int a)
        {
            static int Calculate(int a, int b) => a + b + 1;

            return Calculate(a, 1);
        }

        int CSharp8_StaticLocalFunctions_Bad(int a, int b)
        {
            static int Calculate(int a, int b) => Calculate(a, b) + 1;  //Noncompliant

            return Calculate(a, b);
        }

        int CSharp8_StaticLocalFunctions_FN(int a, int b)
        {
            static int Add(int a, int b) => Fix(a, b);  // FN - Two methods calling each other are not recognized
            static int Fix(int a, int b) => Add(a, b);

            return Add(a, b);
        }

        void MethodWithNestedLocalFunctions()
        {
            void LocalFunction()
            {
                void NestedLocalFunction()     // Noncompliant
                {
                    NestedLocalFunction();
                }
            }
        }

        void MethodWithNestedLocalFunctions2()          // FN
        {
            LocalFunction();
            void LocalFunction()
            {
                InvokingEnclosingMethod();
                void InvokingEnclosingMethod()
                {
                    MethodWithNestedLocalFunctions2();  // gets invoked here
                }
            }
        }

        void MethodWithNestedLocalFunctions3()
        {
            LocalFunction();
            void LocalFunction()                       // FN
            {
                InvokingParentLocalFunction();
                void InvokingParentLocalFunction()
                {
                    LocalFunction();                   // gets invoked here
                }
            }
        }

        private Action<T> SimpleLambdaExpression<T>(Func<T, Task> asyncHandler)
        {
            return m =>
            {
                Task Foo1() => asyncHandler(m);
                Task Foo2() => Foo2();  // Noncompliant
            };
        }

        private Action<T> ParenthesizedLambdaExpression<T>(Func<T, Task> asyncHandler)
        {
            return (m) =>
            {
                Task Bar1() => asyncHandler(m);
                Task Bar2() => Bar2();  // Noncompliant
            };
        }

        delegate void MethodExpressionDelegate();

        private void MethodWithNestedLocalFunctionsAndDelegate()
        {
            void LocalFunctionWithDelegate()
            {
                MethodExpressionDelegate del = delegate
                {
                    void LocalFunctionInDelegate()  // Noncompliant
                    {
                        LocalFunctionInDelegate();
                    }
                };
            }
        }

        MethodExpressionDelegate delMain = delegate {
            void Foo()  // Noncompliant
            {
                Foo();
            }
        };

        private void LamdaInIf(List<string> input)
        {
            if (input.Where(x =>
                            {
                                return Foo();
                                bool Foo() { return Foo(); }  // Noncompliant
                            }).SingleOrDefault() != null)
            {
            }
        }
    }

    class MoreCases
    {
        void CallsOnThis()    // Noncompliant
        {
            this.CallsOnThis();
        }

        void CallsOnObject()  // Noncompliant
        {
            var x = new MoreCases();
            x.CallsOnObject();
        }

        public virtual void CallsOnObjectVirtual(MoreCases arg)  // Compliant
        {
            arg.CallsOnObjectVirtual(arg);
        }

        static int PassItself(int a)
        {
            Receiver(PassItself);
            return 42;
        }

        static void Receiver(Func<int, int> func)
        {

        }
    }

    class Exceptions
    {
        public int Property1 => throw new NotImplementedException();

        public int Property2
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int Property3
        {
            get  // Noncompliant
            {
                var a = Property3;
                throw new NotImplementedException();
            }
            set  // Noncompliant
            {
                Property3 = 42;
                throw new NotImplementedException();
            }
        }

        void ThrowsException() => throw new AggregateException();

        void ThrowsExceptionInIf()
        {
            var a = 5;
            if (a > 1)
            {
                throw new AggregateException();
            }
            else
            {
                throw new AggregateException();
            }
        }

        void CallsItselfAndThrows()            // Noncompliant
        {
            CallsItselfAndThrows();
            throw new AggregateException();
        }

        void ThrowsExceptionInIfAfterGoto()    // Noncompliant
        {
            A:
            goto A;
            var a = 5;
            if (a > 1)
            {
                throw new AggregateException();
            }
        }

        int ThrowsInIf(int a)
        {
            if (a > 1)
            {
                throw new ArgumentException();
            }
            A:
            goto A;

            return 42;
        }

        int ThrowsInTry(int a)
        {
            try
            {
                throw new ArgumentException();
            }
            catch (Exception e)
            {
                ThrowsInTry(a); // FN
                return 42;
            }

            return 42;
        }

        int ThrowsInFinally(int a)
        {
            try
            {
            }
            finally
            {
                throw new ArgumentException();
            }
        }
    }

    public interface IWithDefaultImplementation
    {
        decimal Count { get; set; }
        decimal Price { get; set; }

        decimal Total() //Noncompliant
        {
            return Count * Price + Total();
        }
    }

    abstract class One
    {
        protected One Parent;

        public virtual int Prop
        {
            get
            {
                return Parent.Prop;
            }
        }

        public virtual int Method()
        {
            return Parent.Method();
        }

        public virtual int? NullableMethod()
        {
            return Parent?.NullableMethod();
        }
    }

    class Two : One
    {
        public Two(One parent)
        {
            Parent = parent;
        }
    }

    class Three : One
    {
        public override int Prop
        {
            get => 42;
        }

        public override int Method()
        {
            return 42;
        }

        public override int? NullableMethod()
        {
            return 42;
        }
    }

    class StaticPropertyCase
    {
        private static int Prop { get => Prop; }  // Noncompliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3624
namespace Repro_3624
{
    public class Repro : Base
    {
        public string Name
        {
            get // Noncompliant
            {
                return Name;
            }
            set // Noncompliant
            {
                Name = value;
            }
        }

        public virtual string Arrow
        {
            get => Arrow;           // Noncompliant
            set => Arrow = value;   // Noncompliant
        }

        public override string Overriden
        {
            get => base.Overriden;          // Compliant
            set => base.Overriden = value;  // Compliant
        }
    }

    public class Base
    {
        public virtual string Overriden { get; set; }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/5261
public class Repro_5261
{
    public IEnumerable<T> Repeat<T>(T element)  // Noncompliant FP, it's not a recursion.
    {
        while (true)
        {
            yield return element;
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6642
public class Repro_6642
{
    private List<byte> list;

    public int this[int i]
    {
        get { return this[i]; } // Noncompliant
        set { this[i] = value; } // Noncompliant
    }

    public char this[char i] => this[i]; // Noncompliant

    public byte this[byte i]
    {
        get { return list[i]; } // Compliant
        set { list[i] = value; } // Compliant
    }

    public short this[short i] => list[1623]; // Compliant

    public string this[string i]
    {
        get { return this; } // Compliant
    }

    public static implicit operator string(Repro_6642 d) => "";
}

// https://github.com/SonarSource/sonar-dotnet/issues/6643
public class Repro_6643
{
    public static implicit operator byte(Repro_6643 d) => d; // Noncompliant

    public static explicit operator string(Repro_6643 d) => (string)d; // Noncompliant
}

// https://github.com/SonarSource/sonar-dotnet/issues/6644
public delegate bool SomeDelegate();

public class Repro_6644
{
    public event SomeDelegate SomeEvent
    {
        add { SomeEvent += value; } // Noncompliant

        remove { SomeEvent -= value; } // Noncompliant
    }

    public event SomeDelegate SomeEvent2
    {
        add { SomeEvent2 -= value; } // Noncompliant FP

        remove { SomeEvent2 += value; } // Noncompliant FP
    }

    public event SomeDelegate SomeEvent3
    {
        add
        {
            SomeEvent += value; // Compliant
        }

        remove
        {
            SomeEvent -= value; // Compliant
        }
    }
}
