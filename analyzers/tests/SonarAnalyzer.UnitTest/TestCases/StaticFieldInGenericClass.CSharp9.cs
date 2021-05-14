record StaticFieldInGenericRecord<T>
    where T : class
{
    internal static string field; //Noncompliant {{A static field in a generic type is not shared among instances of different close constructed types.}}

    public static string Prop1 { get; set; } // Noncompliant

    public string Prop2 { get; set; }

    public static T Prop3 { get; set; }
}

record StaticFieldInGenericPositionalRecord<T>(int Property)
    where T : class
{
    internal static string field; //Noncompliant {{A static field in a generic type is not shared among instances of different close constructed types.}}

    public static string Prop1 { get; set; } // Noncompliant

    public string Prop2 { get; set; }

    public static T Prop3 { get; set; }
}
