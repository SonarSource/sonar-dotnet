using System.Collections.Generic;

public class Sample
{
    public void Method(List<int> list, CustomList custom, Sample otherType)
    {
        list.ForEach(x => { });  // Noncompliant {{Use 'foreach' iteration instead of 'List.ForEach'.}}
        //   ^^^^^^^
        custom.ForEach(x => { });   // Noncompliant

        ForEach();                  // Compliant
        otherType.ForEach();        // Compliant, we don't know what it does
        list.Add(1);
        list.TrueForAll(x => true);
    }

    public void Errors(List<int> list)
    {
        list.();            // Error [CS1001] Identifier expected
        list.Undefined();   // Error [CS1061] 'List<int>' does not contain a definition for 'Undefined'
        list.ForEach();     // Error [CS7036] There is no argument given that corresponds to the required parameter 'action' of 'List<int>.ForEach(Action<int>)'
        unknown.ForEach();  // Error [CS0103] The name 'unknown' does not exist in the current context
    }

    public void ErrorNoExpression()
    {   // Error [CS1513] Closing curly brace expected
        .ForEach();
    }

    public void ForEach() { }
}

public class CustomList : List<string> { }
