using System;
using System.Collections.Generic;
using static Renamed = Tests.Diagnostics.Other.StaticMethod;

namespace Tests.Diagnostics
{
    class Program
    {
        public Program()
        {
            StaticMethod(null);
            StaticMethod(this);
            StaticMethod(((this)));
            StaticProperty = this;
            StaticProperty = ((this));

            Other.StaticMethod(this); // Noncompliant
            Other.StaticList.Add(this); // Noncompliant
            Other.StaticProperty = this; // Noncompliant
            ProgramsStatic.Add(this); // Noncompliant
            InstanceList.Add(this); // Noncompliant
            this.InstanceList.Add(this); // Noncompliant
            InstanceProperty = this;
            InstanceMethod(this);
            this.InstanceMethod(this);
            Renamed(this); // Compliant, False Negative

            new Program().InstanceMethod(this); // Noncompliant
        }

        public void Method()
        {
            StaticMethod(this);
            StaticMethod(((this)));
            Other.StaticMethod(this);
            Other.StaticList.Add(this);
            ProgramsStatic.Add(this);
            InstanceList.Add(this);
            InstanceMethod(this);
        }

        public static Program StaticProperty { get; set; }

        public static void StaticMethod(Program program) { }

        public static List<Program> ProgramsStatic = new List<Program>();

        public List<Program> InstanceList = new List<Program>();

        public void InstanceMethod(Program program) { }

        public Program InstanceProperty { get; set; }
    }

    static class Other
    {
        public static List<Program> StaticList = new List<Program>();

        public static void StaticMethod(Program program) { }

        public static Program StaticProperty { get; set; }
    }
}
