using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    file abstract partial class A
    {
        public abstract void X();
    }
    file abstract partial class A
    {
        public void Y() { }
    }

    file abstract class Empty // Noncompliant {{Convert this 'abstract' class to a concrete type with a protected constructor.}}
//                      ^^^^^
    {

    }

    file abstract class Animal //Noncompliant {{Convert this 'abstract' class to an interface.}}
    {
        protected abstract void move();
        protected abstract void feed();

    }

    file class SomeBaseClass { }

    file abstract class Animal2 : SomeBaseClass //Compliant
    {
        protected abstract void move();
        protected abstract void feed();

    }

    file abstract class Color //Noncompliant {{Convert this 'abstract' class to a concrete type with a protected constructor.}}
    {
        private int red = 0;
        private int green = 0;
        private int blue = 0;

        public int getRed()
        {
            return red;
        }
    }

    file interface AnimalCompliant
    {

        void move();
        void feed();

    }

    file class ColorCompliant
    {
        private int red = 0;
        private int green = 0;
        private int blue = 0;

        private ColorCompliant()
        { }

        public int getRed()
        {
            return red;
        }
    }

    file abstract class LampCompliant
    {

        private bool switchLamp = false;

        public abstract void glow();

        public void flipSwitch()
        {
            switchLamp = !switchLamp;
            if (switchLamp)
            {
                glow();
            }
        }
    }

    file abstract class View // Noncompliant, should be an interface
    {
        public abstract string Content { get; }
    }

    file abstract class View2 // Compliant, has abstract and non abstract members
    {
        public abstract string Content { get; }
        public abstract string Content1 { get; }
        public string Content2 { get; }
    }

    file abstract class View2Derived : View2 // Compliant, still has abstract parts
    {
        public string Content3 { get; }
        public override string Content1 { get { return ""; } }
    }

    file abstract class View3Derived : SomeUnknownType // Noncompliant
                                                         // Error@-1 [CS0246]
    {
        public string Content3 { get; }
        public override int Content1 { get { return 1; } }
    }
}
