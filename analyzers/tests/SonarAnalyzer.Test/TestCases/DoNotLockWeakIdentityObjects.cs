using System;
using System.Reflection;
using System.Threading;

namespace Tests.Diagnostics
{
    class DoNotLockWeakIdentityObjects
    {
        private object synchronized = new object();
        private MarshalByRefObject marshalByRefObject = new Timer(null);
        private Timer marshalByRefObjectDerivate = new Timer(null);
        private ExecutionEngineException executionEngineException = new ExecutionEngineException();
        private OutOfMemoryException outOfMemoryException = new OutOfMemoryException();
        private StackOverflowException stackOverflowException = new StackOverflowException();
        private string aString = "some value";
        private MemberInfo memberInfo = typeof(string).GetProperty("Length");
        private ParameterInfo parameterInfo = typeof(string).GetMethod("Equals").ReturnParameter;
        private Thread thread = new Thread((ThreadStart)null);

        private void Test()
        {
            lock (synchronized) { } // Compliant

            lock (marshalByRefObject) { } // Noncompliant {{Replace this lock on 'MarshalByRefObject' with a lock against an object that cannot be accessed across application domain boundaries.}}
//                ^^^^^^^^^^^^^^^^^^
            lock (marshalByRefObjectDerivate) { } // Noncompliant {{Replace this lock on 'Timer' with a lock against an object that cannot be accessed across application domain boundaries.}}
            lock (executionEngineException) { } // Noncompliant
            lock (outOfMemoryException) { } // Noncompliant
            lock (stackOverflowException) { } // Noncompliant
            lock (aString) { } // Noncompliant
            lock (memberInfo) { } // Noncompliant
            lock (parameterInfo) { } // Noncompliant
            lock (thread) { } // Noncompliant
        }
    }
}
