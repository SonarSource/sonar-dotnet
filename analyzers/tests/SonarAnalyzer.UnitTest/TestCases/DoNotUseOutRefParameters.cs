using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{

    public interface IProgram
    {
        void Method1(out string value);
//                   ^^^ {{Consider refactoring this method in order to remove the need for this 'out' modifier.}}
        void Method2(ref string value);
//                   ^^^ {{Consider refactoring this method in order to remove the need for this 'ref' modifier.}}
        void TryMethod(out string value);
//                     ^^^ {{Consider refactoring this method in order to remove the need for this 'out' modifier.}}
        bool TryMethod1(ref string value);
//                      ^^^ {{Consider refactoring this method in order to remove the need for this 'ref' modifier.}}
        bool TryMethod2(out string value);
    }

    public abstract class AbstractProgram
    {
        public abstract void Method1(out string value); // Noncompliant
        public abstract void Method2(ref string value); // Noncompliant
        public abstract void TryMethod(out string value); // Noncompliant
        public abstract bool TryMethod1(ref string value); // Noncompliant
        public abstract bool TryMethod2(out string value);
    }

    public class OverridenProgram : AbstractProgram
    {
        public override void Method1(out string value) { value = "a"; }
        public override void Method2(ref string value) { }
        public override void TryMethod(out string value) { value = "a"; }
        public override bool TryMethod1(ref string value) { return false; }
        public override bool TryMethod2(out string value) { value = "a"; return true; }
    }

    internal class InternalProgram
    {
        public void Method1(out string value) { value = "a"; }
        public void Method2(ref string value) { }
        public void TryMethod(out string value) { value = "a"; }
        public bool TryMethod1(ref string value) { return false; }
        public bool TryMethod2(out string value) { value = "a"; return true; }
    }

    public class Program
    {
        public void Method1(out string value1, out string value2) { value1 = "a"; value2 = "a"; }
//                          ^^^
//                                             ^^^ Noncompliant@-1

        public void Method1(out string value) { value = "a"; } // Noncompliant

        public void Method2(ref string value) { }// Noncompliant

        public void TryMethod(out string value) { value = "a"; } // Noncompliant

        public bool TryMethod1(ref string value) { return true; } // Noncompliant

        public bool TryMethod2(out string value) { value = "a"; return true; }
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/2344
    public class S3874 : I3874
    {
        public void SetRef(ref I3874 obj) // compliant because this is interface implementation
        {
            obj = new S3874();
        }

        public void SetOut(out I3874 obj) // compliant because this is interface implementation
        {
            obj = new S3874();
        }
    }

    public interface I3874
    {
        void SetRef(ref I3874 obj); // Noncompliant
        void SetOut(out I3874 obj); // Noncompliant
    }


    public class Person
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public Person(string fname, string mname, string lname)
        {
            FirstName = fname;
            MiddleName = mname;
            LastName = lname;
        }

        public void Deconstruct(out string fname, out string lname) // Compliant,
                                                                    // https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#user-defined-types
        {
            fname = FirstName;
            lname = LastName;
        }

        public void Deconstruct(out string fname, out string lname, string notAnOutParam) // in Deconstruct all parameters have to be out parameteres.
//                              ^^^
//                                                ^^^ Noncompliant@-1
        {
            fname = FirstName;
            lname = LastName + notAnOutParam;
        }

        public static void Deconstruct(out string foo) { foo = "foo"; } // Noncompliant
        public static int Deconstruct(ref int bar) { return bar; } // Noncompliant
        public static void Method(string foo)
        {
            LocalMethod(foo);
            void LocalMethod(string outFoo)
            {
                outFoo = "foo";
            }
        }
    }

    public static class PersonExtention
    {
        public static void Deconstruct(this Person p, out string fname, out string lname) // Compliant - it's a deconstruct method for class Person
        {
            fname = p.FirstName;
            lname = p.LastName;
        }

        public static void Deconstruct(this Person p, out string fname, out string lname, string notAnOutParameter) // in Deconstruct all parameters have to be out parameteres.
//                                                    ^^^
//                                                                      ^^^ Noncompliant@-1
        {
            fname = p.FirstName;
            lname = p.LastName + notAnOutParameter;
        }

        public static int Deconstruct(this Person p, out int bar) { bar = 1; return bar; } // Noncompliant
    }
}
