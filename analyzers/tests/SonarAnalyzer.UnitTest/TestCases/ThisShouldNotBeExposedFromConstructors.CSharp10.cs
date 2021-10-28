using System.Collections.Generic;

struct AStruct
{
    public int Id;

    public AStruct()
    {
        Id = 0;
        OtherClass.StaticList.Add(this); // Noncompliant
        OtherClass.StaticMethod(this); // Noncompliant
        OtherClass.StaticProperty = this; // Noncompliant
    }
}

static class OtherClass
{
    public static List<AStruct> StaticList = new List<AStruct>();

    public static void StaticMethod(AStruct r) { }

    public static AStruct StaticProperty { get; set; }
}
