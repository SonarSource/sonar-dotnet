namespace Net6
{

    // Static abstract members in interfaces
    interface I<T> where T : I<T>
    {
        static abstract void M();
        static abstract T P { get; set; }
        static abstract event Action E;
        static abstract T operator +(T l, T r);
    }

    class C : I<C>
    {
        static void I<C>.M() => Console.WriteLine("Implementation");
        static C I<C>.P { get; set; }
        static event Action I<C>.E { add { } remove { } }
        static C I<C>.operator +(C l, C r) => r;
    }

    interface I0
    {
        static sealed void M() => Console.WriteLine("Default behavior");

        static sealed int P1 { get; set; }
        static sealed event Action E1;
        static sealed event Action E2 { add => E1 += value; remove => E1 -= value; }

        static sealed I0 operator +(I0 l, I0 r) => l;
    }
}
