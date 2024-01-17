using System.Collections.Generic;

record Record
{
    Record()
    {
        OtherClass.StaticMethod(this); // Noncompliant
        OtherClass.StaticList.Add(this); // Noncompliant
        OtherClass.StaticProperty = this; // Noncompliant

        InstanceProperty = this;
        InstanceMethod(this);
    }

    public void InstanceMethod(Record r) { }

    public Record InstanceProperty { get; set; }
}

static class OtherClass
{
    public static List<Record> StaticList = new List<Record>();

    public static void StaticMethod(Record r) { }

    public static Record StaticProperty { get; set; }
}
