record struct StaticFieldInGenericRecordStruct<T>
    where T : class
{
    public StaticFieldInGenericRecordStruct() { }

    internal static string field; // Noncompliant

    public static string Prop1 { get; set; } // Noncompliant

    public string Prop2 { get; set; } = "";

    public static T Prop3 { get; set; } = null;
}

record struct StaticFieldInGenericPositionalRecordStruct<T>(int Property)
    where T : class
{
    public StaticFieldInGenericPositionalRecordStruct() : this(1) { }

    internal static string field; // Noncompliant

    public static string Prop1 { get; set; } // Noncompliant

    public string Prop2 { get; set; } = "";

    public static T Prop3 { get; set; } = null;
}
