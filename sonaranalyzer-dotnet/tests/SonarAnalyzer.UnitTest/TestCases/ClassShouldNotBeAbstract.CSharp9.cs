using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public abstract record Empty { } // FN

    public abstract record Animal // FN
    {
        protected abstract void move();
        protected abstract void feed();
    }

    public record SomeBaseRecord { }

    public abstract record Animal2 : SomeBaseRecord //Compliant
    {
        protected abstract void move();
        protected abstract void feed();
    }

    public abstract record Color // FN
    {
        private int red = 0;
        public int getRed() => red;
    }

    public interface AnimalCompliant
    {
        void move();
        void feed();
    }

    public class ColorCompliant
    {
        private int red = 0;

        private ColorCompliant()
        { }

        public int getRed() => red;
    }

    public abstract record LampCompliant
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

    public abstract record View // FN
    {
        public abstract string Content { get; }
    }

    public abstract record View2 // Compliant, has abstract and non abstract members
    {
        public abstract string Content { get; }
        public abstract string Content1 { get; }
        public string Content2 { get; }
    }

    public abstract record Record(string X);
}

// See https://github.com/dotnet/roslyn/issues/45510
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
