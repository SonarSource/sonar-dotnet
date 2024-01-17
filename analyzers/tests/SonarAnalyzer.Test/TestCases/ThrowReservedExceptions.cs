using System;

public class ThrowReservedExceptions
{
    public void Method1()
    {
        throw new Exception();                  // Noncompliant {{'System.Exception' should not be thrown by user code.}}
//            ^^^^^^^^^^^^^^^

        throw new ApplicationException();       // Noncompliant {{'System.ApplicationException' should not be thrown by user code.}}
//            ^^^^^^^^^^^^^^^^^^^^^^^^^^

        throw new SystemException();            // Noncompliant {{'System.SystemException' should not be thrown by user code.}}
//            ^^^^^^^^^^^^^^^^^^^^^

        throw new ExecutionEngineException();   // Noncompliant {{'System.ExecutionEngineException' should not be thrown by user code.}}
//            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        throw new IndexOutOfRangeException();   // Noncompliant {{'System.IndexOutOfRangeException' should not be thrown by user code.}}
//            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        throw new NullReferenceException();     // Noncompliant {{'System.NullReferenceException' should not be thrown by user code.}}
//            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        throw new OutOfMemoryException();       // Noncompliant {{'System.OutOfMemoryException' should not be thrown by user code.}}
//            ^^^^^^^^^^^^^^^^^^^^^^^^^^

        var e = new OutOfMemoryException(); // Compliant
        throw new ArgumentNullException();  // Compliant

        OutOfMemoryException e1 = (OutOfMemoryException)new ArgumentException(); // Error [CS0030] - cannot cast

        try
        {
            var a = new int[0];
            Console.WriteLine(a[1]); // Throw exception
        }
        catch (IndexOutOfRangeException)
        {
            throw; // Compliant
        }
    }

    public void Arrow() =>
        throw new Exception();          // Noncompliant

    public Action Lambda() =>
        () => throw new Exception();    // Noncompliant
}
