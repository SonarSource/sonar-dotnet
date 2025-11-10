record Base
{
    public virtual int MyProperty1 { get; init; }
    public virtual int MyProperty2 { get; init; }
    public virtual int MyProperty3 { get; init; }
    public virtual int MyProperty4 { get; init; }
    public virtual int MyProperty5 { get; init; }
    public virtual int MyProperty6 { init; } // Error [CS8051]
    public virtual int MyProperty7 { get; }
    public virtual int MyProperty8 { get; init; }
    public virtual int MyProperty9 { get; init; }
    public virtual int MyProperty10 { get; init; }
    public virtual int MyProperty11 { get; init; }
    public virtual int MyProperty12 { get; init; }
    public virtual int MyProperty13 { get; init; }
    public virtual void Method() { }
}

record Derived : Base
{
    bool isInitialized;
    int backingField;

    public override int MyProperty1 { get => base.MyProperty1; init => base.MyProperty1 = value; }      // Noncompliant
    public override int MyProperty2 { get => base.MyProperty2; init => base.MyProperty2 = value + 1; }
    public override int MyProperty3 { get => base.MyProperty3; init { base.MyProperty2 = value; isInitialized = true; } }
    /// <summary>
    ///  This is some documentation which should disble the issue on this property.
    /// </summary>
    public override int MyProperty4 { get => base.MyProperty1; init => base.MyProperty1 = value; }
    public override sealed int MyProperty5 { get => base.MyProperty5; init => base.MyProperty5 = value; }
    public override int MyProperty6 { get => base.MyProperty6; init => base.MyProperty6 = value; } // Error [CS0154, CS0545] The property or indexer 'property' cannot be used in this context because it lacks the get accessor
    public override int MyProperty7 { get => base.MyProperty7; init => base.MyProperty7 = value; } // Error [CS0200, CS0546] Property or indexer 'property' cannot be assigned to -- it is read only
    public override int MyProperty8 { init => base.MyProperty8 = value; } // Noncompliant
    public override int MyProperty9 { get => base.MyProperty9; } // Noncompliant
    public sealed int MyProperty10 { get => base.MyProperty10; init => base.MyProperty10 = value; } // Error [CS0238] 'member' cannot be sealed because it is not an override
    public override int MyProperty11 { get => base.MyProperty11; init => backingField = value; }
    public override int MyProperty12 { get => base.MyProperty5; init => base.MyProperty5 = value; }
    public override int MyProperty13 { get; init => base.MyProperty13 = value; }
    public override int MyProperty14 { get => base.MyProperty14; init => base.MyProperty14 = value; } // Error [CS0115, CS0117, CS0117] no suitable method found to override, 'Base' does not contain a definition for 'MyProperty14'
    public override void Method() => base.Method(); // Noncompliant
}

namespace CompilerGeneratedMethods
{
    record Base
    {
        public override string ToString() => "Some custom ToString";
    }

    record Derived : Base
    {
        public override string ToString() =>
            base.ToString(); // Compliant. Without the override the compiler generates a ToString instead of calling base.ToString

        protected override bool PrintMembers(System.Text.StringBuilder builder) =>
            base.PrintMembers(builder); // Compliant. The generated PrintMembers implementation would add the properties of Derived to the builder.

        public override bool Equals(Base other) // Error [CS0111] Type 'Derived' already defines a member called 'Equals' with the same parameter types
            => base.Equals(other);

        public override int GetHashCode() =>
            base.GetHashCode(); // Compliant.
    }

    record Underived
    {
        public override string ToString() =>
            base.ToString(); // Compliant. Prevents the compiler from generating a custom ToString method.
    }

    record struct RecordStruct
    {
        public override string ToString() =>
            base.ToString(); // Compliant. Prevents the compiler from generating a custom ToString method.
    }
}

namespace PartialProperties
{
    public partial class Partial
    {
        public virtual partial int Prop1 { get; }
        public virtual partial int Prop2 { get; set; }
    }

    public partial class Partial
    {
        public virtual partial int Prop1 => 42;

        private int _value;
        public virtual partial int Prop2
        {
            get => _value;
            set => _value = value;
        }
    }

    public class Derived : Partial
    {
        public override int Prop1 => base.Prop1; // Noncompliant

        public override int Prop2                // Noncompliant
        {
            get => base.Prop2;
            set => base.Prop2 = value;
        }
    }
}
