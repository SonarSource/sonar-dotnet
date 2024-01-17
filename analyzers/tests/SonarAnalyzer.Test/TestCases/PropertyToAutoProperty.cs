using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class MyAttribute : Attribute { }

    public class SomeException : Exception { }

    public class PropertyToAutoProperty
    {
        static public implicit operator int (PropertyToAutoProperty x)
        {
            return 1;
        }
        static public implicit operator PropertyToAutoProperty(int x)
        {
            return null;
        }

        private int field;
        public PropertyToAutoProperty Prop //Compliant
        {
            get { return field; }
            set { field = value; }
        }

        [My()]
        private int field2;
        public int Prop2 //Compliant
        {
            get { return field2; }
            set { field2 = value; }
        }

        private string _make;
        public string Make // Noncompliant
//                    ^^^^
        {
            get { return _make; }
            set { this._make = value; }
        }

        public string MakeModif
        {
            get { return _make; }
            private set { this._make = value; }
        }

        PropertyToAutoProperty instance = new PropertyToAutoProperty();
        public string Make5 // Compliant
        {
            get { return _make; }
            set { instance._make = value; }
        }

        private static string _make3;
        public static string Make3 // Noncompliant {{Make this an auto-implemented property and remove its backing field.}}
        {
            get { return _make3; }
            set { PropertyToAutoProperty._make3 = value; }
        }

        public static string Make3Modif
        {
            get { return _make3; }
            internal set { PropertyToAutoProperty._make3 = value; }
        }

        public static string Make3Attr
        {
            get { return _make3; }
            [My]
            set { PropertyToAutoProperty._make3 = value; }
        }

        public static string Make4 // Compliant
        {
            get { return _make3; }
            set { PropertyToAutoProperty._make3 += value; }
        }

        public string Make9 // Compliant, returns a static field
        {
            get { return _make3; }
            set { _make3 = value; }
        }

        public string Make2
        {
            get { return _make; }
            set
            {
                if (value == null)
                {
                    throw new SomeException();
                }
                _make = value;
            }
        }

        public int Readonly => 1;

        // C# 7 should not throw
        public int Property01 //Noncompliant
        {
            get => field;
            set => field = value;
        }
    }
}
