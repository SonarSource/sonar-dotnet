partial Function () => null; // Error [CS0116] A namespace cannot directly contain members such as fields, methods or statements
                             // Error@-1 [CS0201] Only assignment, call, increment, decrement, await, and new object expressions can be used as a statement

global::partial Global () => null; // Error [CS1002] ; expected
                                   // Error@-1 [CS1525] Invalid expression term 'partial'
                                   // Error@-2 [CS0116] A namespace cannot directly contain members such as fields, methods or statements
                                   // Error@-3 [CS0201] Only assignment, call, increment, decrement, await, and new object expressions can be used as a statement

global::partial GlobalGeneric<T>() => null;

@partial Function2() => null;

global::@partial Global2() => null;

partial GenericFunction<T>() => null;

partial<int> GenericFunction2() => null;

class Errors
{
    global::partial Method() => null; // Error [CS1002] ; expected
                                      // Error@-1 [CS1525] Invalid expression term 'partial'
                                      // Error@-2 [CS0751] A partial member must be declared within a partial type
                                      // Error@-3 [CS1520] Method must have a return type
                                      // Error@-4 [CS0201] Only assignment, call, increment, decrement, await, and new object expressions can be used as a statement
                                      // Error@-5 [CS1525] Invalid expression term 'partial'
                                      // Error@-6 [CS9276] Partial member 'Errors.Errors()' must have a definition part.
}

class Compliant
{
    global::@partial Method() => null;
}

class partial { }

class partial<T> { }

