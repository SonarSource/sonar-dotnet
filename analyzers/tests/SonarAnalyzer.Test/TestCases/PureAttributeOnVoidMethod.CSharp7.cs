using System.Diagnostics.Contracts;

record Record   // Error [CS0246] The type or namespace name 'record' could not be found (are you missing a using directive or an assembly reference?)
                // Error@-1 [CS0548] '<invalid-global-code>.Record': property or indexer must have at least one accessor
{
    Record()    // Error [CS1014] A get or set accessor expected
                // Error@-1 [CS1014] A get or set accessor expected
                // Error@-2 [CS1014] A get or set accessor expected
    {
        [Pure]
        void LocalFunction()
        { }
    }
}
