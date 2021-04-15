// version: CSharp9
using System;
using System.Reflection;

Type dynClass = Type.GetType("MyInternalClass");
BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
//                         ^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this accessibility bypass is safe here.}}
MethodInfo dynMethod = dynClass.GetMethod("Method", bindingAttr);
dynMethod.Invoke(dynClass, null);

void Foo()
{
    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static; // Noncompliant
    Type.GetType("MyInternalClass").GetMethod("Method", bindingAttr).Invoke(null, null);
}

public record Record
{
    private readonly BindingFlags bindingFlags = BindingFlags.NonPublic; // Noncompliant
    public BindingFlags GetFlags { get; } = BindingFlags.NonPublic; // Noncompliant
    public BindingFlags GetBindingFlags() => BindingFlags.NonPublic; // Noncompliant

    void Foo()
    {
        Type.GetType("MyInternalClass").GetMember("mymethod", BindingFlags.NonPublic); // Noncompliant
    }
}
