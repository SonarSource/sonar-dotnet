namespace ReturnTypeNamedPartial
{
    using System.Collections.Generic;

    public class Compliant
    {
        public delegate @partial Delegate();

        @partial Method() => null;

        public ReturnTypeNamedPartial.@partial Qualified() => null;

        global::ReturnTypeNamedPartial.@partial Alias() => null;

        ref @partial Refs(ref partial p) => ref p;

        unsafe partial* Pointer() => null;

        partial[] Array() => null;

        List<partial> List() => null;

        partial<int> Generic() => null;

        partial GenricMethod<T>() => null;

        public delegate partial GenericDelegate<T>();

        public ReturnTypeNamedPartial.partial Qualified<T>() => null;

        global::ReturnTypeNamedPartial.partial GenericAlias<T>() => null;

        ref partial GenericRefs<T>(ref partial p) => ref p;

        void GenericLocalFunction()
        {
            partial LocalFunction<T>() => null;
        }

        partial? Nullable() => null;

        void VoidMethod() { }

        (partial, partial) TupleMethod() => default;
    }

    public class CompilerErrors
    {
        public delegate partial Delegate(); // Error [CS1002] ; expected
                                            // Error@-1 [CS1003] Syntax error, '(' expected
                                            // Error@-2 [CS1026] ) expected
                                            // Error@-3 [CS1031] Type expected
                                            // Error@-4 [CS1525] Invalid expression term 'partial'
                                            // Error@-5 [CS0751] A partial member must be declared within a partial type
                                            // Error@-6 [CS1520] Method must have a return type

        public ReturnTypeNamedPartial.partial FirstMethod() // Error [CS0102] The type 'CompilerErrors' already contains a definition for ''
                                                            // Error@-1 [CS1002] ; expected
                                                            // Error@-2 [CS1525] Invalid expression term 'partial'
                                                            // Error@-3 [CS1525] Invalid expression term 'partial'
                                                            // Error@-4 [CS0751] A partial member must be declared within a partial type
                                                            // Error@-5 [CS1520] Method must have a return type
        {
            return new ReturnTypeNamedPartial.partial(); // Error [CS0127] Since 'CompilerErrors.CompilerErrors()' returns void, a return keyword must not be followed by an object expression
        }

        partial SecondMethod() // Error [CS0751] A partial member must be declared within a partial type
                               // Error@-1 [CS1520] Method must have a return type
                               // Error@-2 [CS9278] Partial member 'CompilerErrors.CompilerErrors()' may not have multiple implementing declarations.
        {
            return new partial(); // Error [CS0127] Since 'CompilerErrors.CompilerErrors()' returns void, a return keyword must not be followed by an object expression
        }
    }

    public class partial { }

    public class partial<T> { }
}

namespace partial
{

    class Compliant
    {
        partial.Compliant Method() => null;

        global::partial.Compliant Method2() => null;
    }
}
